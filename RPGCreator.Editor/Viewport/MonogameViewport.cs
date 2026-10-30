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

using System;
using Fluxor;
using MediatR;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.EditorLib.Mediator.Services;
using RPGCreator.RTP.ECS.Systems;
using RPGCreator.RTP.Services;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Maps.Chunks;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Systems;
using RPGCreator.SDK.Editor.Rendering;
using RPGCreator.SDK.Factories;
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.Services.EngineService;
using RPGCreator.SDK.Services.RuntimeService;
using RPGCreator.SDK.States;
using RPGCreator.SDK.States.Actions;
using RPGCreator.SDK.Types;
using MapDefinition = RPGCreator.SDK.Assets.Maps.MapDefinition;
using Vector2 = System.Numerics.Vector2;

namespace RPGCreator.RTP.Viewport;

public class MonogameViewport : BaseMonogameViewport
{
    private bool startCameraDragging = false;

    private static ScopedLogger _logger = Logger.ForContext<MonogameViewport>();

    public RenderTarget2D? RenderTarget { get; set; }

    private GraphicsDevice _graphicsDevice;
    private IEcsWorld _ecsWorld;
    private readonly IInputsService _inputsService;
    private readonly IGameSimulationService _gameSimulationService;
    private readonly IBaseSubscriberFactory _baseSubscriberFactory;
    private readonly Dispatcher _stateDispatcher;
    private readonly ICameraService _cameraService;
    private readonly IChunkService _chunkService;
    private readonly IMapService _mapService;
    private readonly IRenderService _renderService;

    private readonly Color
        _bgColor = Color
            .CornflowerBlue; // We do this for now, later on, it's the map selected that will determine the background color of the viewport.

    public MonogameViewport(RenderTarget2D renderTarget, IInputsService inputsService,
        IGameSimulationService gameSimulationService, IBaseSubscriberFactory baseSubscriberFactory,
        Dispatcher stateDispatcher,
        ICameraService cameraService, IChunkService chunkService, IMapService mapService, IRenderService renderService)
    {
        _inputsService = inputsService;
        _gameSimulationService = gameSimulationService;
        _baseSubscriberFactory = baseSubscriberFactory;
        _stateDispatcher = stateDispatcher;
        _cameraService = cameraService;
        _chunkService = chunkService;
        _mapService = mapService;
        _renderService = renderService;
        RenderTarget = renderTarget;
        _inputsService.MouseState.Moved += OnMouseMoved;
        _inputsService.MouseState.WheelScrolled += OnMouseWheelScrolled;
    }

    private float _baseZoomLevel = 1;

    private void OnMouseWheelScrolled(int obj)
    {
        var isCtrlPressed = _inputsService.KeyboardState.IsKeyPressed(KeyboardKeys.RightControl) ||
                            _inputsService.KeyboardState.IsKeyPressed(KeyboardKeys.LeftControl);

        if (!isCtrlPressed)
            return;

        if (_inputsService.MouseState.InObject is not ("MonoGameImage"))
        {
            return;
        }

        if (obj > 0)
            _baseZoomLevel += .1f;
        else
            _baseZoomLevel -= .1f;

        _baseZoomLevel = Math.Clamp(_baseZoomLevel, .2f, 2);

        _cameraService.SetZoomLevel(_baseZoomLevel);
    }

    private void OnMouseMoved(Vector2 delta)
    {
        var isCtrlPressed = _inputsService.KeyboardState.IsKeyPressed(KeyboardKeys.RightControl) ||
                            _inputsService.KeyboardState.IsKeyPressed(KeyboardKeys.LeftControl);

        if (isCtrlPressed)
        {
            var reversedDelta = new Vector2(-delta.X, -delta.Y);
            _cameraService.Drag(reversedDelta);
        }
    }

    private void LoadContent(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
    {
        _graphicsDevice = graphicsDevice;

        _cameraService.SetCameraEntity(_ecsWorld.EntityManager.CreateCameraEntity().Id);

        _stateDispatcher.Dispatch(new GameSessionActions.StartNewSessionAction());
        _ecsWorld = _gameSimulationService.World;

        _ecsWorld.SystemManager.AddSystem(new CameraSystem(_baseSubscriberFactory, _cameraService));
        _ecsWorld.SystemManager.AddSystem(new MapDrawingSystem(_cameraService, _mapService, _renderService, _chunkService));
    }

    public override void UpdateAvaloniaControl(IntPtr bitmapControlAddress)
    {
        _bitmapControlAddress = bitmapControlAddress;
    }

    public void Draw(TimeSpan deltaTime)
    {
        _graphicsDevice.SetRenderTarget(RenderTarget);
        _graphicsDevice.Clear(_bgColor);

        _ecsWorld.SystemManager.Draw(deltaTime);
    }

    public void Update(TimeSpan deltaTime)
    {
        _chunkService.UpdateLoadedChunk();
        _ecsWorld.Update(deltaTime);
    }

    #region InternalMethods - DO NOT TOUCH

    private IntPtr? _bitmapControlAddress;
    private uint[]? _internalBuffer;

    public override void LoadContent(object graphicsDevice, object spriteBatch, object? shapeBatch = null)
    {
        if (graphicsDevice is GraphicsDevice gd && spriteBatch is SpriteBatch sb)
        {
            LoadContent(gd, sb);
        }
        else
        {
            throw new ArgumentException("Invalid types for LoadContent. Expected GraphicsDevice and SpriteBatch.");
        }
    }

    public override void SetNewRendertarget(object newRenderTarget)
    {
        if (newRenderTarget is RenderTarget2D rt)
        {
            RenderTarget?.Dispose();
            RenderTarget = rt;
        }
    }

    protected override void UpdatingFrame(TimeSpan deltaTime)
    {
        Draw(deltaTime);
        if (DrawFrameByFrame) return;
        DoNewFrame();
    }

    protected override void UpdatingLoop(TimeSpan deltaTime)
    {
        Update(deltaTime);
    }

    protected override void AskForNewFrame()
    {
        if (RenderTarget == null) return;
        if (_bitmapControlAddress == null || !_bitmapControlAddress.HasValue) return;
        _inDrawing = true;

        var address = _bitmapControlAddress.Value;

        int totalPixels = ((int)Size.Width * (int)Size.Height);

        if (_internalBuffer == null || _internalBuffer.Length != totalPixels)
        {
            _internalBuffer = new uint[totalPixels];
        }

        RenderTarget.GetData(_internalBuffer);

        try
        {
            unsafe
            {
                fixed (uint* pSource = _internalBuffer)
                {
                    long bytesToCopy = (long)totalPixels * sizeof(uint);
                    Buffer.MemoryCopy(pSource, address.ToPointer(),
                        bytesToCopy,
                        bytesToCopy);
                }
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions that may occur during memory copy
            _logger.Error($"Error copying frame data: {ex.Message}");
        }
        finally
        {
            OnceUpdatedAction?.Invoke();
            _inDrawing = false;
        }
    }

    protected override void Disposing()
    {
        RenderTarget?.Dispose();
        _internalBuffer = null;
        _bitmapControlAddress = null;
    }

    #endregion
}