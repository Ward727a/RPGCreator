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
using RPGCreator.RTP.Extensions;
using RPGCreator.SDK.GameUI;

namespace RPGCreator.RTP;

public class GameUiPreviewer : Game, IGameUiRunner
{
    public event Action? OnInitialize;
    public event Action? OnLoad;
    public event Action<TimeSpan>? OnUpdate;
    public event Action<TimeSpan>? OnDraw;
    public event Action? OnUnloaded;
    public event Action? OnDisposed;
    public System.Drawing.Color BackgroundColor { get; set; } = System.Drawing.Color.CornflowerBlue;
    
    public GraphicsDeviceManager Graphics;
    private SpriteBatch _spriteBatch = null!;
    
    
    #region Default Methods
    public GameUiPreviewer()
    {
        Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();
        OnInitialize?.Invoke();
    }

    protected override void LoadContent()
    {
        base.LoadContent();
        OnLoad?.Invoke();
        GraphicsDevice.Reset();
        _spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        OnUpdate?.Invoke(gameTime.ElapsedGameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        OnDraw?.Invoke(gameTime.ElapsedGameTime);
        GraphicsDevice.Clear(BackgroundColor.ToXnaFast());
    }

    protected override void UnloadContent()
    {
        base.UnloadContent();
        OnUnloaded?.Invoke();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        OnDisposed?.Invoke();
    }

    #endregion
}