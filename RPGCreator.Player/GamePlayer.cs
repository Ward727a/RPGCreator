using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Gum.Forms;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using RPGCreator.Core;
using RPGCreator.Player.ECS.Systems;
using RPGCreator.Player.Services;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers.EntityLayer;
using RPGCreator.SDK.Assets.Definitions.Stats;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.Exceptions;
using RPGCreator.SDK.GameRunner;
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.RuntimeService;
using RPGCreator.SDK.Types;
using Color = Microsoft.Xna.Framework.Color;

namespace RPGCreator.Player;

public class GamePlayer : Game, IGameRunner
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
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
    
    ScopedLogger logger = Logger.ForContext<GamePlayer>();

    GameFrom _gameFrom;
    GameState _gameState;

    string _gameFilePath;
    IGameData _gameData;
    
    public GamePlayer()
    {

        EngineCore.InitCore(EngineCore.EEngineMode.PlayerMode);
        
        
        _graphics = new GraphicsDeviceManager(this);
        _gameState = GameState.Playing;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }
    
    private TextRuntime _healthTextInstance;

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        Gum.Initialize(this, DefaultVisualsVersion.V2);
        OnInitialize?.Invoke();
        
        logger.Info("Starting RPG Creator Player...");
        
        // Check for command line arguments or file input to determine game source
        logger.Info("Checking for game source...");
        var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;

        if (string.IsNullOrEmpty(exePath))
            throw new CriticalEngineException("[GamePlayer] Unable to determine executable path.",
                System.Reflection.Assembly.GetExecutingAssembly());

        if (!Directory.Exists("Modules"))
        {
            Directory.CreateDirectory("Modules");
        }
        
        if (Environment.GetCommandLineArgs().Length > 1)
        {
            _gameFrom = GameFrom.Args;
            
            // Check if we have the '--file' argument
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--file" && i + 1 < args.Length)
                {
                    logger.Info("Game source from command line arguments.");
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
            logger.Info("No command line arguments found, defaulting to GameData.json file.");
            _gameFrom = GameFrom.File;
            
            // Get the path of the currently executing assembly
            var exeDirectory = System.IO.Path.GetDirectoryName(exePath);
            _gameFilePath = System.IO.Path.Combine(exeDirectory, "GameData.json");
            
            if(!File.Exists(_gameFilePath))
            {
                logger.Error("Default GameData.json file not found in executable directory.");
                // throw new("Error: No game file specified and default GameData.xml not found.");
            }
            var data = File.ReadAllText(_gameFilePath);
            try
            {
                EngineServices.SerializerService.Deserialize(data, out DefaultGameData gameData);
                _gameData = gameData;
            }
            catch (Exception ex)
            {
                logger.Critical("Failed to deserialize GameData.json: {Message}", args: ex.Message);
                throw new("Error: Failed to load game data from GameData.json.", ex);
            }
        }
        
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
        startButton.Click += (_, _) =>
        {
            RuntimeServices.OnceServiceReady((IGameSession gameSession) =>
            {
                gameSession.IsPaused = false;
                startButton.IsVisible = false;
            });
        };

        _healthTextInstance = new TextRuntime()
        {
            Text = "Health: 100",
            X = 10,
            Y = 10,
        };
        mainPanel.AddChild(_healthTextInstance);
        mainPanel.AddChild(startButton);
        
        EngineServices.ResourcesService.RegisterLoader<Texture2D>(new Texture2DLoader(GraphicsDevice));
        
        RuntimeServices.MapService = new MapService();
        RuntimeServices.MapService.OnMapLoaded += (mapId) =>
        {
            var def = RuntimeServices.MapService.CurrentLoadedMapDefinition;

            if (def == null)
                return;

            foreach (var baseLayerDef in def.TileLayers.Where(l => l is EntityLayerDefinition))
            {
                var layerDef = (EntityLayerDefinition)baseLayerDef;
                foreach (var chunkData in layerDef.Chunks)
                {
                    var chunkId = chunkData.Key;
                    var spawners = layerDef.GetElements(chunkId);
                    for (int i = 0; i < spawners.Length; i++)
                    {
                        var position = layerDef.GetElementWorldPosition(chunkId, i);
                        var spawner = spawners[i];
                        if(spawner == null)
                            continue;
                        
                        var entity = RuntimeServices.GameSession.ActiveEcsWorld.CreateEntity();
                        RuntimeServices.GameSession.ActiveEcsWorld.EntityFactory.InitializeEntity(entity, spawner.EntityDefinition, position);
                    }
                }
            }
        };
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        RuntimeServices.LayerService = new LayerService();
        RuntimeServices.ChunkService = new ChunkService();
        RuntimeServices.CameraService = new CameraService();
        RuntimeServices.RenderService = new RenderService(_spriteBatch);
        RuntimeServices.PlayerController = new BasePlayerController();
        RuntimeServices.GameSession = new DefaultGameSession();

        LoadingValue = EngineCore.LoadGameData(_gameData);
        
        RuntimeServices.GameSession.ActiveEcsWorld = EngineServices.ECS.CreateWorld();
        if(!LoadingValue.success)
            throw new CriticalEngineException("[GamePlayer] Failed to load game data.", this);
        var cameraEntity = RuntimeServices.GameSession.ActiveEcsWorld.EntityManager.CreateCameraEntity();
        RuntimeServices.CameraService.SetCameraEntity(cameraEntity.Id);
        
        RuntimeServices.GameSession.ActiveEcsWorld.SystemManager.AddSystem(new MapDrawingSystem());
        ((RenderService)RuntimeServices.RenderService).AddSystemToWorld(RuntimeServices.GameSession.ActiveEcsWorld);
        RuntimeServices.GameSession.ActiveEcsWorld.SystemManager.AddSystem(new MapForegroundSystem());

        
        
        new BaseSubscriber(new URN("rpgc", "events", "on_stat_changed"), 0,
        @event =>
        {
            var statId = @event.Data.GetAsOrDefault("statDefId", Ulid.Empty);
            var statFinalValue = @event.Data.GetAsOrDefault("finalValue", 0d);
            var statActualValue = @event.Data.GetAsOrDefault("actualValue", 0d);
            var statDef = EngineServices.AssetsManager.TryResolveAsset(statId, out BaseStatDefinition? StatDef) ? StatDef : null;
            if (statDef != null && statDef.Name == "Health")
            {
                _healthTextInstance.Text =
                    $"Stat: (Health) {statActualValue} / {statFinalValue}";
                Logger.Debug("Received stat changed event: stat new value: {Value}", args:
                [
                    @event.Data.GetAsOrDefault<double>("value", 0d).ToString(CultureInfo.InvariantCulture)
                ]);
            }
        }).Subscribe();
        
        base.Initialize();
    }

    private (bool success, Ulid mapId) LoadingValue;
    
    protected override void LoadContent()
    {
        RuntimeServices.MapService.LoadMap(LoadingValue.mapId);
        OnLoad?.Invoke();
    }
    
    public void UpdateKeyboard()
    {
        var mgState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
        var pressedKeys = mgState.GetPressedKeys();

        Span<KeyboardKeys> sdkKeys = stackalloc KeyboardKeys[pressedKeys.Length];

        for (int i = 0; i < pressedKeys.Length; i++)
        {
            sdkKeys[i] = (KeyboardKeys)(int)pressedKeys[i];
        }

        // On crée la donnée brute
        var rawData = new RawKeyboardData(
            sdkKeys, 
            mgState.CapsLock, 
            mgState.NumLock
        );

        EngineProviders.KeyboardProvider?.Update(rawData);
    }
    
    private TimeSpan _30Seconds = TimeSpan.FromSeconds(30);
    private double _accumulatedTime = 0;
    
    protected override void Update(GameTime gameTime)
    {
        OnUpdate?.Invoke(gameTime.ElapsedGameTime);
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (Keyboard.GetState().IsKeyDown(Keys.G))
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        
        _accumulatedTime += gameTime.ElapsedGameTime.TotalSeconds;
        if (_accumulatedTime >= 10)
        {
            _accumulatedTime = 0;
            RuntimeServices.GameSession.ActiveEcsWorld?.EventBus.Publish(new URN("rpgc", "events", "test"));
            
        }
        
        UpdateKeyboard();
        EngineServices.InputsService.Update();
        
        RuntimeServices.GameSession.ActiveEcsWorld?.Update(gameTime.ElapsedGameTime);
        Gum.Update(gameTime);
        
        EngineServices.InputsService.ResetInputAxis();
        RuntimeServices.GameSession.ActiveEcsWorld?.EventBus.TickEndOfFrame();
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        OnDraw?.Invoke(gameTime.ElapsedGameTime);

        RuntimeServices.GameSession.ActiveEcsWorld.Draw(gameTime.ElapsedGameTime);
        Gum.Draw();
        base.Draw(gameTime);
    }

    public event Action OnInitialize;
    public event Action OnLoad;
    public event Action<TimeSpan> OnUpdate;
    public event Action<TimeSpan> OnDraw;
}