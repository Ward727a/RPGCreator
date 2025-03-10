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

namespace RPGCreator;

public class Game1 : Game
{

    ResourcesImages testImage;

    StackPanel Root;

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
        var gumProject = MonoGameGum.GumService.Default.Initialize(this);

        Root = new();
        Root.Visual.AddToManagers();

        var button = new Button();

        Root.AddChild(button);
        button.Text = "Click me";

        button.Visual.Width = 350;

        button.Click += (_, _) =>
            button.Text = $"Clicked at {System.DateTime.Now}";

        _imGuiRenderer = new ImGuiRenderer(this);
        _imGuiRenderer.RebuildFontAtlas();
        MouseExtended.WindowHandle = Mouse.WindowHandle;
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.ImGuiLogger()
            .CreateLogger();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatchExtended(GraphicsDevice);
        _graphicsDevice = GraphicsDevice;
        BaseContent.LoadBaseContent(Content);
    }

    protected override void Update(GameTime gameTime)
    {
        MonoGameGum.GumService.Default.Update(this, gameTime, Root);

        MouseExtended.Update();
        KeyboardExtended.Update();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);


        MonoGameGum.GumService.Default.Draw();


        _imGuiRenderer.BeforeLayout(gameTime);

        ImGuiNET.ImGui.Begin("test");

        ImGuiNET.ImGui.End();

        ImDebug.Logger.RenderLogger();
        
        _imGuiRenderer.AfterLayout();

        base.Draw(gameTime);
    }
}
