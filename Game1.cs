using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ImGuiNET.SampleProgram.XNA;
using System;
using Serilog;
using RPGCreator.core.logs;
using RPGCreator.core.debug;
using RPGCreator.core.Types.Resources;
using RPGCreator.thirdparty.ImageSharp;
using SixLabors.ImageSharp;

namespace RPGCreator;

public class Game1 : Game
{

    ResourcesImages testImage;

    private GraphicsDeviceManager _graphics;
    static private GraphicsDevice _graphicsDevice;
    private SpriteBatch _spriteBatch;

    private ImGuiRenderer _imGuiRenderer;

    static public GraphicsDevice GetGraphicDevice()
    {
        return _graphicsDevice;
    }

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _imGuiRenderer = new ImGuiRenderer(this);
        _imGuiRenderer.RebuildFontAtlas();

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

        Log.Logger.Fatal("Fatal");
        Log.Logger.Error("Err");
        Log.Logger.Debug("Debug");
        Log.Logger.Verbose("Verbose");
        Log.Logger.Information("Info");
        Log.Logger.Warning("Warn");

        testImage = new("C:\\Users\\Malywan\\Pictures\\image1420.png");
        Log.Logger.Debug($"Image: {testImage}");

        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);

        _spriteBatch.Begin();

        _spriteBatch.Draw(testImage.GetTexture2D(), new Vector2(50, 50), Microsoft.Xna.Framework.Color.White);

        _spriteBatch.End();

        _imGuiRenderer.BeforeLayout(gameTime);

        ImGuiNET.ImGui.Begin("test");

        ImGuiNET.ImGui.End();

        ImDebug.Logger.RenderLogger();
        
        _imGuiRenderer.AfterLayout();

        base.Draw(gameTime);
    }
}
