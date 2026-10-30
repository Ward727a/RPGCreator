// RPG Creator - Open-source RPG Engine.
// (c) 2026 Ward
// 
// This file is part of RPG Creator and is distributed under the Apache 2.0 License.
// You are free to use, modify, and distribute this file under the terms of the Apache 2.0 License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence Apache 2.0.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence Apache 2.0.
// Voir LICENSE pour plus de détails.
// 
// Contact:
// => Mail: Ward727a@gmail.com
//    Please use this object: "RPG Creator [YourObject]"
// => Discord: ward727
// 
// For urgent inquiries, sending both an email and a message on Discord is highly recommended for a quicker response.

using System.Text.Json;
using Arch.Core;
using Fluxor;
using Microsoft.Xna.Framework;
using RPGCreator.Editor.ECS.Components;
using RPGCreator.Editor.ECS.Flags;
using RPGCreator.RuntimeLib.ECS.Components;
using RPGCreator.RuntimeLib.ECS.System;
using RPGCreator.SDK.Assets.Map;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.PipelineManager;
using RPGCreator.SDK.Scenes;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.SDK.Services.Map;
using RPGCreator.SDK.States;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Enums;
using RPGCreator.Shared.Types;
using Point = Microsoft.Xna.Framework.Point;
using Vector2 = System.Numerics.Vector2;

namespace RPGCreator.RuntimeLib.Scenes;

[EngineService(typeof(ISceneManager))]
public class SceneManager : ISceneManager
{
    private readonly List<IMapLayer> _activeLayers = [];
    public IReadOnlyList<IMapLayer> ActiveLayers => _activeLayers;

    private static readonly ScopedLogger Logger = SDK.Common.Logging.Logger.ForContext<SceneManager>();
    private readonly QueryDescription _cameraQuery = new QueryDescription().WithAll<CameraComponent>();
    public ISceneSimulation Simulation { get; private set; }
    public ISceneRender Render { get; private set; }

    private Ulid _currentMapId;
    public Point MapSize { get; private set; }
    private IMapLayer? _phantomLayer { get; set; } = null;
    private readonly ISerializerService _serializerService;
    private readonly IAssetManager _assetManager;
    private readonly ILayerRegistry _layerRegistry;
    private readonly IDispatcher _dispatcher;
    private readonly IPipelineManager _pipelineManager;
    private readonly IState<SceneState> _sceneState;
    private readonly IState<EngineState> _engineState;


    public SceneManager(
        ISceneSimulation simulation,
        ISerializerService serializerService,
        ISceneRender render,
        IAssetManager assetManager,
        ILayerRegistry layerRegistry,
        IDispatcher dispatcher,
        IPipelineManager pipelineManager,
        IState<SceneState> sceneState,
        IState<EngineState> engineState)
    {
        _serializerService = serializerService;
        _assetManager = assetManager;
        _layerRegistry = layerRegistry;
        _dispatcher = dispatcher;
        _pipelineManager = pipelineManager;
        _sceneState = sceneState;
        _engineState = engineState;
        Simulation = simulation;
        Render = render;
        Simulation.RegisterSystem<TileLayerSystem>(ISceneSimulation.SystemPhase.Rendering);
        Simulation.RegisterSystem<AutoTileLayerSystem>(ISceneSimulation.SystemPhase.Rendering);
        Simulation.RegisterSystem<SkyRenderSystem>(ISceneSimulation.SystemPhase.Rendering);
        Simulation.RegisterSystem<ActorLayerSystem>(ISceneSimulation.SystemPhase.Rendering);
    }

    public ValueTask LoadSceneAsync(Ulid mapId)
    {
        if (_currentMapId != Ulid.Empty)
        {
            UnloadScene();
        }

        var resultMap = _assetManager.GetAsset<MapDefinition>(mapId);

        if (resultMap.IsFailure)
            return default;

        _dispatcher.Dispatch(new SceneStateActions.MapLoadStarted());

        var map = resultMap.Value;

        var world = Simulation.CreateWorld(Render);

        foreach (var mapLayerDefinition in map.Layers)
        {
            var layerResult = _layerRegistry.CreateLayerInstance(mapLayerDefinition.DataType);

            if (layerResult.IsFailure)
            {
                Logger.Error("Failed to create layer instance for layer type: {LayerType}",
                    args: mapLayerDefinition.DataType);
                continue;
            }

            var layerInstance = layerResult.Value;

            layerInstance.Build(mapLayerDefinition);
            layerInstance.CreateEntityForEcs(world);

            if (!world.IsAlive(layerInstance.LayerEntity))
            {
                Logger.Error("Layer {layerType} failed to create entity", args: layerInstance.LayerDefinition.DataType);
            }

            _activeLayers.Add(layerInstance);
        }

        _activeLayers.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex));

        var layerUrn = URN.Empty;
        if (_activeLayers.Count > 0)
        {
            layerUrn = _activeLayers[0].LayerDefinition.DataType;
        }

        Simulation.CurrentWorld?.Create<CameraComponent, TransformComponent, EditorCameraFlag>(
            new CameraComponent(new Vector2(200, 200)));
        Simulation.CurrentWorld?.Create(new SkyComponent(map.Size, map.BackgroundColor));

        _currentMapId = mapId;
        MapSize = map.Size;

        var vt = _pipelineManager.RunAsync(new LoadSceneStepContext(Simulation, map));
        
        _dispatcher.Dispatch(new SceneStateActions.MapLoadCompleted(_currentMapId, MapSize, layerUrn));

        return default;
    }

    private readonly QueryDescription _mapTilesetRegistryQuery =
        new QueryDescription().WithAll<MapTilesetRegistryComponent, WorldEntityFlag>();


    public void UnloadScene()
    {
        _currentMapId = default;
        MapSize = default;
        _activeLayers.Clear();
        _phantomLayer = null;

        var couldDisposeRegistry = false;
        Simulation.CurrentWorld?.Query(in _mapTilesetRegistryQuery, (ref MapTilesetRegistryComponent registry) =>
        {
            foreach (var texturesValue in registry.Textures.Values)
            {
                texturesValue.Dispose();
            }

            registry.Textures.Clear();
            registry.Definitions.Clear();

            couldDisposeRegistry = true;
        });

        if (!couldDisposeRegistry)
            Logger.Error("Could not dispose map tileset registry!");

        Simulation.DestroyWorld();
    }

    public ValueTask ReloadSceneAsync()
    {
        var mapId = _currentMapId;
        UnloadScene();
        
        var activeIndex = _sceneState.Value.ActiveLayerIndex;
        var activeLayerType = _sceneState.Value.ActiveLayerType;

        var valueTask = LoadSceneAsync(mapId);

        if (_activeLayers.Count <= 0) return valueTask;
        
        if (_activeLayers.Count <= activeIndex)
        {
            activeIndex = 0;
            activeLayerType = _activeLayers[activeIndex].LayerDefinition.DataType;
        }
        _dispatcher.Dispatch(new SceneStateActions.ActiveLayerChanged(activeIndex, activeLayerType));

        return valueTask;
    }

    public void SaveSceneAsync()
    {
        if (_currentMapId == Ulid.Empty)
            return;

        var mapDefinitionResult = _assetManager.GetAsset<MapDefinition>(_currentMapId);

        if (mapDefinitionResult.IsFailure)
            return;

        var mapDefinition = mapDefinitionResult.Value;

        mapDefinition.Layers.Clear();

        foreach (var activeLayer in ActiveLayers)
        {
            var layerDefinition = activeLayer.LayerDefinition;
            var data = activeLayer.GetSaveData();
            layerDefinition.Data = JsonSerializer.SerializeToElement(data, _serializerService.JsonOptions);

            mapDefinition.Layers.Add(layerDefinition);
        }

        _assetManager.SaveAsset(mapDefinition);
    }

    public ValueTask SaveSceneAsync(string newMapPath)
    {
        throw new NotImplementedException();
    }

    public IMapLayer? GetLayer(int index)
    {
        if (index >= 0 && index < _activeLayers.Count)
        {
            return _activeLayers[index];
        }

        return null;
    }

    public TLayer? EnsureActiveLayer<TLayer>(URN awaitedLayerType, int activeIndex, out int newActiveIndex)
        where TLayer : class, IMapLayer
    {
        newActiveIndex = activeIndex;
        if (Simulation.CurrentWorld == null) return null;

        if (activeIndex >= 0 && activeIndex < _activeLayers.Count && _activeLayers[activeIndex] is TLayer existingLayer)
        {
            return existingLayer;
        }

        int nextZIndex = 0;
        if (_activeLayers.Count > 0)
        {
            int currentMax = _activeLayers[^1].LayerDefinition.ZIndex;
            nextZIndex = currentMax < int.MaxValue - 1 ? currentMax + 1 : currentMax;
        }

        var newLayer = AddNewLayer(new MapLayerDefinition
        {
            Name = "New Layer",
            Size = MapSize,
            ZIndex = nextZIndex,
            DataType = awaitedLayerType
        });

        if (newLayer == null) return null;

        _activeLayers.Sort((a, b) => a.LayerDefinition.ZIndex.CompareTo(b.LayerDefinition.ZIndex));

        newActiveIndex = _activeLayers.IndexOf(newLayer);

        _dispatcher.Dispatch(new SceneStateActions.ActiveLayerChanged(newActiveIndex, awaitedLayerType));

        return newLayer as TLayer;
    }

    public IMapLayer? EnsurePhantomLayer(URN awaitedPhantomLayer)
    {
        if (_currentMapId == Ulid.Empty)
            return null;
        if (_phantomLayer != null && _phantomLayer.LayerDefinition.DataType == awaitedPhantomLayer &&
            Simulation.CurrentWorld != null && Simulation.CurrentWorld.IsAlive(_phantomLayer.LayerEntity))
            return _phantomLayer;

        if (_phantomLayer != null)
        {
            Simulation.CurrentWorld?.Destroy(_phantomLayer.LayerEntity);
            _phantomLayer = null;
        }

        _phantomLayer = AddNewLayer(new MapLayerDefinition
        {
            Name = "Phantom Layer",
            Size = MapSize,
            ZIndex = int.MaxValue,
            DataType = awaitedPhantomLayer,
        }, true);

        return _phantomLayer;
    }

    public void DrawScene(double deltaTime)
    {
        var world = Simulation.CurrentWorld;

        if (world == null)
            return;

        Simulation.ExtractRenderData(deltaTime);

        Matrix currentViewMatrix = Matrix.Identity;
        Rectangle currentBounds = new Rectangle(0, 0, 1000, 1000);

        world.Query(_cameraQuery, (ref CameraComponent cameraComponent) =>
        {
            if (cameraComponent.IsActive)
            {
                currentViewMatrix = cameraComponent.ViewMatrix;
                currentBounds = cameraComponent.BoundingBox;
            }
        });

        Render.Render(in currentViewMatrix, in currentBounds, deltaTime);
        
        if ((_engineState.Value.Mode & EngineModeFlags.Editor) != 0)
        {
            Render.RenderGrid(in currentViewMatrix, in currentBounds, MapSize, 32);
        }
    }

    public IMapLayer? AddNewLayer(MapLayerDefinition definition, bool phantom = false)
    {
        if (Simulation.CurrentWorld == null) return null;

        IMapLayer? resultReturn = null;

        _layerRegistry.CreateLayerInstance(definition.DataType).OnSuccess((layerInstance) =>
        {
            layerInstance.Build(definition);
            layerInstance.CreateEntityForEcs(Simulation.CurrentWorld);

            if (!Simulation.CurrentWorld.IsAlive(layerInstance.LayerEntity))
            {
                Logger.Error("Layer {layerType} failed to create entity", args: layerInstance.LayerDefinition.DataType);
            }

            if (phantom)
            {
                layerInstance.SetPhantom();
                Simulation.SharedCommandBuffer?.Add(layerInstance.LayerEntity, new PhantomLayerFlag());
            }
            else
            {
                _activeLayers.Add(layerInstance);
                _dispatcher.Dispatch(new SceneStateActions.MapModified());
            }

            resultReturn = layerInstance;
        });

        return resultReturn;
    }
}