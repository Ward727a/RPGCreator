using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ImGuiNET.SampleProgram.XNA;
using Serilog;
using RPGCreator.core.logs;
using RPGCreator.core.debug;
using RPGCreator.core.types.objects.resources;
using System.IO;
using RPGCreatorLib.ContentPipeline.TXT;
using RPGCreator.core;
using MonoGame.Extended.Input;
using RPGCreator.core.types.objects.ui;
using System.Collections.Generic;
using RPGCreator.core.types;
using RPGCreator.core.UI.components.buttons;

namespace RPGCreator;

public class Game1 : Game
{

    ResourcesImages testImage;

    static public Game1 Self;

    private BaseButton testUI;

    private GraphicsDeviceManager _graphics;
    static private GraphicsDevice _graphicsDevice;
    private SpriteBatch _spriteBatch;

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
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _graphicsDevice = GraphicsDevice;
        BaseContent.LoadBaseContent(Content);

        //testImage = new("C:\\Users\\Ward\\Pictures\\image1420.png");
        //Log.Logger.Debug($"Image: {testImage}");

        //Log.Logger.Debug($"Base Gitignore: {BaseContent.GetGitignore()}");

        testUI = new();
        testUI.SetScale(new(200, 200));
        testUI.SetPosition(new(250, 50));

        testUI.OnPressed += (object sender, System.EventArgs e) =>
        {
            Log.Logger.Debug("Button pressed!");
        };

        Log.Logger.Information(testUI.ToString());
    }

    protected override void Update(GameTime gameTime)
    {
        MouseExtended.Update();
        KeyboardExtended.Update();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        testUI._Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);

        _spriteBatch.Begin();

        //_spriteBatch.Draw(testImage.GetTexture2D(), new Vector2(50, 50), Microsoft.Xna.Framework.Color.White);
        testUI._Draw(_spriteBatch);

        _spriteBatch.End();

        _imGuiRenderer.BeforeLayout(gameTime);

        ImGuiNET.ImGui.Begin("test");

        ImGuiNET.ImGui.End();

        ImDebug.Logger.RenderLogger();
        
        _imGuiRenderer.AfterLayout();

        base.Draw(gameTime);
    }
}
