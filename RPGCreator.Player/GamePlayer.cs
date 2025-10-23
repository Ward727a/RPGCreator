using System;
using System.IO;
using Gum.Forms;
using Gum.Forms.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameGum;
using RPGCreator.Core;
using RPGCreator.Core.Rendering.Batching;
using Serilog;

namespace RPGCreator.Player;

public class GamePlayer : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatchExtend _spriteBatch;
    GumService Gum = GumService.Default;

    public enum GameFrom
    {
        Args,
        File
    }
    
    public enum GameState
    {
        Playing,
        Paused
    }
    
    ILogger logger = Log.ForContext<GamePlayer>();

    GameFrom _gameFrom;
    GameState _gameState;

    string _gameFilePath;
    
    public GamePlayer()
    {

        EngineCore.InitCore();
        
        logger.Information("Starting RPG Creator Player...");
        // Check for command line arguments or file input to determine game source
        logger.Information("Checking for game source...");
        if (Environment.GetCommandLineArgs().Length > 1)
        {
            _gameFrom = GameFrom.Args;
            
            // Check if we have the '--file' argument
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--file" && i + 1 < args.Length)
                {
                    logger.Information("Game source from command line arguments.");
                    string filePath = args[i + 1];
                    
                    // Check if the file is an .xml file
                    if (System.IO.Path.GetExtension(filePath).Equals(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        _gameFilePath = filePath;
                    }
                    else
                    {
                        logger.Error("Game source from file path doesn't match expected format.");    
                        throw new("Error: The specified file is not a valid .xml file.");
                    }
                    
                    break;
                }
            }
        }
        else
        {
            logger.Information("No command line arguments found, defaulting to GameData.xml file.");
            _gameFrom = GameFrom.File;
            
            // Get the path of the currently executing assembly
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string exeDirectory = System.IO.Path.GetDirectoryName(exePath);
            _gameFilePath = System.IO.Path.Combine(exeDirectory, "GameData.xml");
            
            if(!File.Exists(_gameFilePath))
            {
                logger.Error("Default GameData.xml file not found in executable directory.");
                // throw new("Error: No game file specified and default GameData.xml not found.");
            }
        }
        
        _graphics = new GraphicsDeviceManager(this);
        _gameState = GameState.Playing;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        Gum.Initialize(this, DefaultVisualsVersion.V2);
        
        Gum.Root.Width = _graphics.GraphicsDevice.Viewport.Width;
        Gum.Root.Height = _graphics.GraphicsDevice.Viewport.Height;
        // Create base GumUI Panel
        var mainPanel = new Panel(Gum.Root);
        
        var startButton = new Button()
        {
            Text = "Start Game",
            Width = 200,
            Height = 50,
            X = (Gum.Root.Width - 200) / 2,
            Y = (Gum.Root.Height - 50) / 2,
        };
        mainPanel.AddChild(startButton);
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatchExtend(GraphicsDevice);

        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here
        Gum.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here
        Gum.Draw();
        base.Draw(gameTime);
    }
}