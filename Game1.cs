using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Serilog;
using RPGCreator.core.logs;
using RPGCreator.core.debug;
//using RPGCreator.core.types..resources;
using RPGCreator.core;
using MonoGame.Extended.Input;
//using RPGCreator.core.types.objects.ui;
using System.Collections.Generic;
using RPGCreator.core.types;
using RPGCreator.core.helpers;
using RPGCreator.core.types.objects.resources;
using MonoGameGum.GueDeriving;
using RenderingLibrary;
using RPGCreator.thirdparty.ImGui;
using MonoGameGum.Forms.Controls;
using Gum.Wireframe;
using System;
using RPGCreator.core.interfaces.launcher;
using MonoGame.OpenGL;
using System.Runtime.InteropServices;
using ImGuiNET;
using RPGCreator.core.config;
using RPGCreator.core.interfaces.debug;
using RPGCreator.core.io.datas;

namespace RPGCreator;

public partial class Game1 : Game
{
    ResourcesImages testImage;

    StackPanel Root;

    Launcher launcher;

    Debug DebugMainMenu;

    static public Game1 Self;

    private GraphicsDeviceManager _graphics;
    static private GraphicsDevice _graphicsDevice;
    private SpriteBatchExtended _spriteBatch;

    private ImGuiRenderer _imGuiRenderer;

    private List<GameObject> GameObjects;

    static public GraphicsDevice GetGraphicDevice()
    {
        return _graphicsDevice;
    }

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Self = this;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        MonoGameGum.GumService.Default.Initialize(this);

        _graphics.PreferredBackBufferWidth = 940;
        _graphics.PreferredBackBufferHeight = 520;
        _graphics.ApplyChanges();
        launcher = new(_graphics.GraphicsDevice);
        DebugMainMenu = new(_graphics.GraphicsDevice);
        SDL_Wrapper.SetWindowMinSize(Window.Handle, 920, 517);


        Root = new();
        Root.Visual.AddToManagers();

        _imGuiRenderer = new ImGuiRenderer(this);

        BaseContent.Folders.LoadFolders(); // Loading / Creating the needed folders.

        BaseContent.LoadBaseContent(Content); // Loading basic content.
        // Adding different ImGui Font size.
        ImGui_Helper.AddFont(13);
        ImGui_Helper.AddFont(16);
        ImGui_Helper.AddFont(18);
        ImGui_Helper.AddFont(20);
        ImGui_Helper.AddFont(24);
        ImGui_Helper.AddFont(32);
        _imGuiRenderer.RebuildFontAtlas();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.ImGuiLogger()
            .CreateLogger();

        // This allows you to resize:
        Window.AllowUserResizing = true;
        // This event is raised whenever a resize occurs, allowing
        // us to perform custom logic on a resize
        Window.ClientSizeChanged += HandleClientSizeChanged;

        base.Initialize();
    }
    private void HandleClientSizeChanged(object sender, EventArgs e)
    {

        if (_graphics.GraphicsDevice.Viewport.Width < 920 || _graphics.GraphicsDevice.Viewport.Height < 517)
        {
            _graphics.PreferredBackBufferWidth = 920;
            _graphics.PreferredBackBufferHeight = 517;
            _graphics.ApplyChanges();
        }

        GraphicalUiElement.CanvasWidth = _graphics.GraphicsDevice.Viewport.Width;
        GraphicalUiElement.CanvasHeight = _graphics.GraphicsDevice.Viewport.Height;

        // Resize root
        Root.Visual.UpdateLayout();
        launcher.HandleClientSizeChanged();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatchExtended(GraphicsDevice);
        _graphicsDevice = GraphicsDevice;
        BaseContent.LoadBaseContent(Content);

        ConfigFile.plugins.GetDoc();
    }

    protected override void Update(GameTime gameTime)
    {
        MonoGameGum.GumService.Default.Update(this, gameTime, Root);

        MouseExtended.Update();
        KeyboardExtended.Update();


        if (KeyboardExtended.GetState().IsAltDown() && KeyboardExtended.GetState().IsControlDown() && KeyboardExtended.GetState().WasKeyPressed(Keys.D))
            Config.debug.b_Menu = !Config.debug.b_Menu;

        launcher.Update();
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.FromNonPremultiplied(new (0.05f, 0.05f, 0.06f, 1)));

        MonoGameGum.GumService.Default.Draw();

        _imGuiRenderer.BeforeLayout(gameTime);

        launcher.Draw();

        DebugMainMenu.Draw();

        _imGuiRenderer.AfterLayout();

        base.Draw(gameTime);
    }
}
