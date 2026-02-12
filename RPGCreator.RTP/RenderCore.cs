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
using RPGCreator.RTP.GameUI;
using RPGCreator.RTP.GameUI.BaseControls;
using RPGCreator.RTP.GameUI.BaseControls.Box;
using RPGCreator.RTP.Services;
using RPGCreator.RTP.Viewport;
using RPGCreator.SDK;
using RPGCreator.SDK.Editor.Rendering;
using SpriteFontPlus;

namespace RPGCreator.RTP;

public sealed class RenderCore : Game, IGameRenderCore
{
    public event Action? OnInitialize;
    public event Action? OnLoad;
    public event Action<TimeSpan>? OnUpdate;
    public event Action<TimeSpan>? OnDraw;
    
    public event Action? DeviceReady;
    
    private MonogameViewportService _parentService;
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _pixelTexture;

    private UiRenderer _uiRenderer = new UiRenderer();
    
    public RenderCore(MonogameViewportService parentService)
    {
        _parentService = parentService;
        Graphics = new GraphicsDeviceManager(this);
        // Sous DesktopGL (OpenGL), on peut souvent cacher la fenêtre via le handle
        this.IsMouseVisible = true;
        RuntimeServices.GameRunner = this;
        
    }

    protected override void Initialize()
    {
        base.Initialize();
        OnInitialize?.Invoke();
    }

    public GraphicsDeviceManager Graphics { get; set; }

    protected override void LoadContent()
    {
        base.LoadContent();
        GraphicsDevice.Reset();
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _uiRenderer.Initialize(GraphicsDevice);
        SdfColorBox.LoadSdfEffect(GraphicsDevice);
        OnLoad?.Invoke();
    }


    public RenderTarget2D CreateNewRenderTarget2D(int width, int height)
    {
        return new RenderTarget2D(GraphicsDevice, width, height);
    }

    public void LoadContent(GameViewport viewport)
    {
        viewport.LoadContent(GraphicsDevice, _spriteBatch, this, _uiRenderer);
    }

    protected override void Draw(GameTime gameTime)
    {
        DeviceReady?.Invoke();
        DeviceReady = null;
        OnDraw?.Invoke(gameTime.ElapsedGameTime);
        base.Draw(gameTime);
        
        if (GraphicsDevice == null) return;
        var viewports = _parentService.GetAllViewports();
        
        for (int i = 0; i < viewports.Length; i++)
        {
            var viewport = viewports[i];
            
            if(viewport is { IsDrawingPaused: true, FrameAsked: false } || viewport.InternalIsDrawingPaused) continue;

            if (viewport is GameViewport gv && gv.RenderTarget != null)
            {
                GraphicsDevice.SetRenderTarget(gv.RenderTarget);
                viewport.DrawViewport(gameTime.ElapsedGameTime);
            }
        }
        GraphicsDevice.SetRenderTarget(null);
    }

    protected override void Update(GameTime gameTime)
    {
        OnUpdate?.Invoke(gameTime.ElapsedGameTime);
        var viewports = _parentService.GetAllViewports();
        var deltaTime = gameTime.ElapsedGameTime;

        for (int i = 0; i < viewports.Length; i++)
        {
            var viewport = viewports[i];

            if (viewport.IsUpdatingPaused || viewport.InternalIsUpdatingPaused) continue;

            viewport.UpdateViewport(deltaTime);
        }

        base.Update(gameTime);
    }
    
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}