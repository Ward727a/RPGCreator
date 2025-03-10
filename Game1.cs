using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Serilog;
using RPGCreator.core.logs;
using RPGCreator.core.debug;
//using RPGCreator.core.types..resources;
using System.IO;
using RPGCreatorLib.ContentPipeline.TXT;
using RPGCreator.core;
using MonoGame.Extended.Input;
//using RPGCreator.core.types.objects.ui;
using System.Collections.Generic;
using RPGCreator.core.types;
using RPGCreator.core.UI.components.buttons;
using RPGCreator.core.UI.containers;
using RPGCreator.core.types.Math.Transform;
using RPGCreator.core.helpers;
using RPGCreator.core.types.objects.resources;
using System;
using MonoGameGum.GueDeriving;
using RenderingLibrary;
using RPGCreator.thirdparty.ImGui;
using ImGuiNET;

namespace RPGCreator;

public class Game1 : Game
{

    ResourcesImages testImage;

    static public Game1 Self;

    private BaseButton testUI;
    private ScrollContainer Container;

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

        var rectangle = new ColoredRectangleRuntime();
        rectangle.Width = 100;
        rectangle.Height = 100;
        rectangle.Color = Microsoft.Xna.Framework.Color.White;

        rectangle.AddToManagers(SystemManagers.Default, null);
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

        //testImage = new("C:\\Users\\Ward\\Pictures\\image1420.png");
        //Log.Logger.Debug($"Image: {testImage}");

        //Log.Logger.Debug($"Base Gitignore: {BaseContent.GetGitignore()}");

        Container = new(new Scale(100, 50));
        Container.SetPosition(new Position(150, 150));
        Container.allowYScroll = true;
        Container.allowXScroll = true;

        testUI = new();
        testUI.SetScale(new(100, 50));
        testUI.SetPosition(new(0, 0));
        testUI.OnPressed += (object sender, EventArgs e) =>
        {
            ((BaseButton)sender).SetScale(new Scale(((BaseButton)sender).GetScale().X + 5, ((BaseButton)sender).GetScale().Y + 5));
        };
        Container.AddChild(testUI);
        testUI = new();
        testUI.SetScale(new(100, 50));
        testUI.SetPosition(new(0, 0));

        Container.AddChild(testUI);

        testUI.OnPressed += (object sender, System.EventArgs e) =>
        {
            Container.SetPosition(new Position(0, 0));
        };

        Log.Logger.Information(testUI.ToString());
    }

    protected override void Update(GameTime gameTime)
    {
        MonoGameGum.GumService.Default.Update(this, gameTime);
        MouseExtended.Update();
        KeyboardExtended.Update();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        Container._Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);

        MonoGameGum.GumService.Default.Draw();

        _spriteBatch.Begin();
        //testUI._Draw(_spriteBatch);

        _spriteBatch.End();

        Container._Draw(_spriteBatch);

        _imGuiRenderer.BeforeLayout(gameTime);

        ImGuiNET.ImGui.Begin("test");

        ImGuiNET.ImGui.End();

        ImDebug.Logger.RenderLogger();
        
        _imGuiRenderer.AfterLayout();

        base.Draw(gameTime);
    }
}
