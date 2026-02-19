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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.RTP.ECS.Systems;
using RPGCreator.RTP.Services;
using RPGCreator.SDK;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Systems;
using RPGCreator.SDK.Editor.Rendering;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.RuntimeService;

namespace RPGCreator.RTP.Viewport;

public class MonogameViewport : BaseMonogameViewport
{
    private static ScopedLogger _logger = Logger.ForContext<MonogameViewport>();
    
    public RenderTarget2D? RenderTarget { get; set; }
    
    private GraphicsDevice _graphicsDevice;
    private SpriteBatch _spriteBatch;
    private IEcsWorld _ecsWorld;
    
    private readonly Color _bgColor = Color.CornflowerBlue; // We do this for now, later on, it's the map selected that will determine the background color of the viewport.
    
    public MonogameViewport(RenderTarget2D renderTarget)
    {
        RenderTarget = renderTarget;
    }

    public void LoadContent(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
    {
        _graphicsDevice = graphicsDevice;
        _spriteBatch = spriteBatch;
        _ecsWorld = EngineServices.ECS.CreateWorld();
            
        RuntimeServices.MapService = new MapService();
        RuntimeServices.LayerService = new LayerService();
        RuntimeServices.ChunkService = new ChunkService();
        RuntimeServices.CameraService = new CameraService();
        RuntimeServices.RenderService = new RenderService(graphicsDevice, _spriteBatch);
        RuntimeServices.PlayerController = new BasePlayerController();
        RuntimeServices.GameSession = new DefaultGameSession();
        RuntimeServices.GameSession.ActiveEcsWorld = _ecsWorld;
        RuntimeServices.CameraService.SetCameraEntity(_ecsWorld.EntityManager.CreateCameraEntity().Id);
        
        _ecsWorld.SystemManager.AddSystem(new CameraSystem());
        _ecsWorld.SystemManager.AddSystem(new MapDrawingSystem(graphicsDevice));
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
        RuntimeServices.ChunkService.UpdateLoadedChunk();
        _ecsWorld.Update(deltaTime);
    }

    #region InternalMethods - DO NOT TOUCH
    
    private IntPtr? _bitmapControlAddress;
    private uint[]? _internalBuffer;

    public override void LoadContent(object graphicsDevice, object spriteBatch)
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
        if(RenderTarget == null) return;
        if(_bitmapControlAddress == null || !_bitmapControlAddress.HasValue) return;
        _inDrawing = true;

        var address = _bitmapControlAddress.Value;
        
        int totalPixels = (Size.Width * Size.Height);
        
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