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

using System.Runtime.InteropServices;
using System.Text.Json;
using Arch.Core;
using Arch.Core.Extensions;
using Fluxor;
using Microsoft.Xna.Framework;
using RPGCreator.SDK.Assets.Actors;
using RPGCreator.SDK.Assets.Map;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.Scenes;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Services.Assets;
using Vector2 = System.Numerics.Vector2;

namespace RPGCreator.SDK.Services.Map;

public record struct ActorSpawnData(
    Ulid InstanceId,
    Ulid ActorDefinitionId,
    Point GridPosition,
    ActorDirection FacingDirection = ActorDirection.Down
)
{
    /// <summary>
    /// If a feature type is present here, it overrides the default one from ActorDefinition.
    /// </summary>
    public List<IActorFeatureData> FeatureOverrides { get; init; } = [];
};

[LayerType("rpgc", "layer_type", "actor_layer", DisplayName = "Actor Layer")]
public class ActorLayer(
    IDispatcher dispatcher,
    IAssetManager assetManager,
    ISerializerService serializerService) : IMapLayer
{
    private const int DefaultGridTileSize = 32;

    private readonly List<ActorSpawnData> _spawns = [];
    private readonly Dictionary<Ulid, Entity> _spawnedEntities = [];
    private World? _currentWorld;

    public MapLayerDefinition LayerDefinition { get; private set; } = null!;
    public Entity LayerEntity { get; private set; } = Entity.Null;

    public bool IsPhantom { get; private set; }
    public int ZIndex { get; private set; }
    public bool IsVisible { get; set; } = true;
    public int Width { get; private set; }
    public int Height { get; private set; }

    public IReadOnlyList<ActorSpawnData> Spawns => _spawns;

    public void SetPhantom()
    {
        IsPhantom = true;
    }

    public void Build(MapLayerDefinition layerDefinition)
    {
        LayerDefinition = layerDefinition;
        Width = layerDefinition.Size.X;
        Height = layerDefinition.Size.Y;
        ZIndex = layerDefinition.ZIndex;

        _spawns.Clear();

        if (layerDefinition.Data.ValueKind is not (JsonValueKind.Undefined or JsonValueKind.Null))
        {
            var loadedSpawns = layerDefinition.Data.Deserialize<List<ActorSpawnData>>(serializerService.JsonOptions);
            if (loadedSpawns != null)
            {
                _spawns.AddRange(loadedSpawns);
            }
        }
    }

    public void CreateEntityForEcs(World world)
    {
        _currentWorld = world;

        // Destroy previous entities if any
        DestroySpawnedEntities(world);
        if (world.IsAlive(LayerEntity))
        {
            world.Destroy(LayerEntity);
        }

        LayerEntity = world.Create(new ActorLayerComponent(this), new LayerComponent(ZIndex, IsVisible));

        foreach (var spawn in _spawns)
        {
            SpawnActorEntityInWorld(world, spawn);
        }
    }

    /// <summary>
    /// Places or replaces an actor at the given grid coordinates.
    /// </summary>
    public void Add(Point gridPos, in ActorSpawnData spawnData)
    {
        if (!IsValid(gridPos))
            return;

        // Remove any existing actor at the exact same grid cell first
        Remove(gridPos);

        var finalSpawn = spawnData with
        {
            InstanceId = spawnData.InstanceId == Ulid.Empty ? Ulid.NewUlid() : spawnData.InstanceId,
            GridPosition = gridPos
        };

        _spawns.Add(finalSpawn);

        if (_currentWorld != null)
        {
            SpawnActorEntityInWorld(_currentWorld, finalSpawn);
        }

        if (!IsPhantom)
            dispatcher.Dispatch(new SceneStateActions.MapModified());
    }

    public void Remove(Point gridPos)
    {
        if (!IsValid(gridPos))
            return;

        for (int i = _spawns.Count - 1; i >= 0; i--)
        {
            var spawn = _spawns[i];
            if (spawn.GridPosition != gridPos)
                continue;

            if (_currentWorld != null &&
                _spawnedEntities.Remove(spawn.InstanceId, out var entity) &&
                _currentWorld.IsAlive(entity))
            {
                _currentWorld.Destroy(entity);
            }

            _spawns.RemoveAt(i);
        }

        if (!IsPhantom)
            dispatcher.Dispatch(new SceneStateActions.MapModified());
    }

    public bool Has(Point gridPos)
    {
        if (!IsValid(gridPos))
            return false;

        foreach (var spawn in _spawns)
        {
            if (spawn.GridPosition == gridPos)
                return true;
        }

        return false;
    }

    public ActorSpawnData? Get(Point gridPos)
    {
        if (!IsValid(gridPos))
            return null;

        foreach (var spawn in _spawns)
        {
            if (spawn.GridPosition == gridPos)
                return spawn;
        }

        return null;
    }

    public ReadOnlySpan<ActorSpawnData> GetAll() => CollectionsMarshal.AsSpan(_spawns);

    public void Clear()
    {
        if (_currentWorld != null)
        {
            DestroySpawnedEntities(_currentWorld);
        }

        _spawns.Clear();
    }

    public object? GetSaveData() => _spawns;

    public void SetVisibility(ISceneSimulation sceneSimulation, bool isVisible)
    {
        IsVisible = isVisible;
        var world = sceneSimulation.CurrentWorld;
        if (world == null)
            return;

        if (world.IsAlive(LayerEntity))
        {
            LayerEntity.Set(new LayerComponent(ZIndex, isVisible));
        }

        foreach (var entity in _spawnedEntities.Values)
        {
            if (world.IsAlive(entity) && world.Has<LayerComponent>(entity))
            {
                entity.Set(new LayerComponent(ZIndex, isVisible));
            }
        }
    }

    private void SpawnActorEntityInWorld(World world, in ActorSpawnData spawn)
    {
        var worldPos = new Vector2(spawn.GridPosition.X * DefaultGridTileSize, spawn.GridPosition.Y * DefaultGridTileSize);

        var entity = world.Create(
            new TransformComponent(worldPos, new Point(DefaultGridTileSize, DefaultGridTileSize)),
            new LayerComponent(ZIndex, IsVisible),
            new ActorInstanceComponent
            {
                InstanceId = spawn.InstanceId,
                ActorDefinitionId = spawn.ActorDefinitionId,
                ParentLayer = this
            }
        );

        // Apply all IActorFeatureData attached to the ActorDefinition
        if (!IsPhantom && spawn.ActorDefinitionId != Ulid.Empty)
        {
            assetManager.GetAsset<ActorDefinition>(spawn.ActorDefinitionId).OnSuccess(actorDef =>
            {
                foreach (var feature in actorDef.Features)
                {
                    feature.ApplyToEntity(world, entity);
                }
            });
        }

        _spawnedEntities[spawn.InstanceId] = entity;
    }

    private void DestroySpawnedEntities(World world)
    {
        foreach (var entity in _spawnedEntities.Values)
        {
            if (world.IsAlive(entity))
            {
                world.Destroy(entity);
            }
        }

        _spawnedEntities.Clear();
    }

    private bool IsValid(Point gridPos) =>
        gridPos.X >= 0 && gridPos.X < Width && gridPos.Y >= 0 && gridPos.Y < Height;
}