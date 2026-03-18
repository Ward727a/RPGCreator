using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RPGCreator.Core;
using RPGCreator.Core.Types.Project;
using RPGCreator.Player.ECS.Systems;
using RPGCreator.Player.Services;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps.Chunks;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers.EntityLayer;
using RPGCreator.SDK.Assets.Definitions.Stats;
using RPGCreator.SDK.Debug;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Systems;
using RPGCreator.SDK.Exceptions;
using RPGCreator.SDK.GameRunner;
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.RuntimeService;
using RPGCreator.SDK.Types;
using Color = Microsoft.Xna.Framework.Color;
using Vector2 = System.Numerics.Vector2;

namespace RPGCreator.Player;

public class GamePlayer : Game, IGameRunner
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;

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

    string _gameFilePath = null!;
    bool _fromArgs = false;
    IGameData _gameData = null!;
    
    public GamePlayer()
    {

        EngineCore.InitCore(EEngineMode.Player);
        
        
        _graphics = new GraphicsDeviceManager(this);
        _gameState = GameState.Playing;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }
    

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
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
            
            // Check if we have the '--project' argument
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--project" && i + 1 < args.Length)
                {
                    logger.Info("Game source from command line arguments.");
                    string filePath = args[i + 1];
                    
                    // Check if the file is an .xml file
                    if (System.IO.Path.GetExtension(filePath).Equals(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        _gameFilePath = filePath;
                        _fromArgs = true;
                    }
                    else
                    {
                        logger.Error("Game source from file path doesn't match expected format.");
                        logger.Error("Provided file path: {FilePath}", args: filePath);
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
            logger.Info("GameData.json path found: {path}", args: _gameFilePath);
            
            if(!File.Exists(_gameFilePath))
            {
                logger.Error("Default GameData.json file not found in executable directory.");
                // throw new("Error: No game file specified and default GameData.xml not found.");
            }
            var data = File.ReadAllText(_gameFilePath);
            try
            {
                EngineServices.SerializerService.Deserialize(data, out DefaultGameData? gameData);

                if (gameData == null)
                {
                    logger.Error("Failed to deserialize GameData.json: Deserialized data is null.");
                    throw new("Error: Failed to load game data from GameData.json.");
                }
                
                _gameData = gameData;
            }
            catch (Exception ex)
            {
                logger.Critical("Failed to deserialize GameData.json: {Message}", args: ex.Message);
                throw new("Error: Failed to load game data from GameData.json.", ex);
            }
        }

        if (_fromArgs)
        {
            var projectData = File.ReadAllText(_gameFilePath);
            try
            {
                EngineServices.SerializerService.Deserialize(projectData, out BaseProject? project);
                if (project == null)
                {
                    logger.Error("Failed to deserialize project file: Deserialized data is null.");
                    throw new("Error: Failed to load game data from specified project file.");
                }

                var data = new DefaultGameData();
                data.SetProjectPath(_gameFilePath);
                data.SetModulesHashes(project.Modules);
                data.SetMainMapId(project.MainMapId);
                _gameData = data;
            }
            catch (Exception ex)
            {
                logger.Critical("Failed to deserialize project file: {Message}", args: ex.Message);
                throw new("Error: Failed to load game data from specified project file.", ex);
            }
        }

        GlobalStates.OnceStateReady((IGameSession gameSession) =>
        {
            gameSession.IsPaused = false;
        });
        
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
                        
                        var entity = GlobalStates.GameSession.ActiveEcsWorld.CreateEntity();
                        GlobalStates.GameSession.ActiveEcsWorld.EntityFactory.InitializeEntity(entity, spawner.EntityDefinition, position);
                    }
                }
            }
        };
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        RuntimeServices.LayerService = new LayerService();
        RuntimeServices.ChunkService = new ChunkService();
        RuntimeServices.CameraService = new CameraService(_graphics);
        RuntimeServices.RenderService = new RenderService(GraphicsDevice, _spriteBatch);
        RuntimeServices.PlayerController = new BasePlayerController();
        GlobalStates.GameSession = new DefaultGameSession();

        LoadingValue = EngineCore.LoadGameData(_gameData);
        
        GlobalStates.GameSession.ActiveEcsWorld = EngineServices.ECS.CreateWorld();
        if(!LoadingValue.success)
            throw new CriticalEngineException("[GamePlayer] Failed to load game data.", this);
        var cameraEntity = GlobalStates.GameSession.ActiveEcsWorld.EntityManager.CreateCameraEntity();
        RuntimeServices.CameraService.SetCameraEntity(cameraEntity.Id);
        
        GlobalStates.GameSession.ActiveEcsWorld.SystemManager.AddSystem(new CameraSystem());
        GlobalStates.GameSession.ActiveEcsWorld.SystemManager.AddSystem(new MapDrawingSystem());
        ((RenderService)RuntimeServices.RenderService).AddSystemToWorld(GlobalStates.GameSession.ActiveEcsWorld);
        GlobalStates.GameSession.ActiveEcsWorld.SystemManager.AddSystem(new MapForegroundSystem());

        
        new BaseSubscriber(new URN("rpgc", "events", "on_stat_changed"), 0,
        @event =>
        {
            var statId = @event.Data.GetAsOrDefault("statDefId", Ulid.Empty);
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
            GlobalStates.GameSession.ActiveEcsWorld?.EventBus.Publish(new URN("rpgc", "events", "test"));
            if (GlobalStates.GameSession.CurrentPlayerId != -1)
            {
                RuntimeServices.CameraService.LinkToEntity(GlobalStates.GameSession.CurrentPlayerId);
                logger.Info("Player entity found with ID {PlayerId}. Camera linked to player.", args: GlobalStates.GameSession.CurrentPlayerId);
            }
            else
            {
                if(!RuntimeServices.CameraService.IsLinkedToEntity)
                    logger.Warning("No player entity found in the loaded game data. The game may not function correctly without a player entity.");
            }
            
        }
        
        UpdateKeyboard();
        EngineServices.InputsService.Update();
        
        GlobalStates.GameSession.ActiveEcsWorld?.Update(gameTime.ElapsedGameTime);
        
        EngineServices.InputsService.ResetInputAxis();
        GlobalStates.GameSession.ActiveEcsWorld?.EventBus.TickEndOfFrame();
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        OnDraw?.Invoke(gameTime.ElapsedGameTime);

        GlobalStates.GameSession.ActiveEcsWorld.Draw(gameTime.ElapsedGameTime);
        
        base.Draw(gameTime);
        
        // var elements = RuntimeServices.MapService.CurrentLoadedMapDefinition.CollisionChunk.Elements;
        RuntimeServices.RenderService.PrepareDrawing();
        // foreach (var element in elements)
        // {
        //     var chunkId = element.Key;
        //     var chunk = element.Value;
        //     if (chunk.IsEmpty) continue;
        //     var index = 0;
        //     foreach (var colDataList in chunk.GetAllElementsSpan())
        //     {
        //         if (!colDataList.HasValue)
        //         {
        //                     
        //             index++;
        //             continue;
        //         };
        //         Vector2 worldPos = LayerChunk.GetWorldPosition(chunkId, index);
        //         foreach (var data in colDataList.Value.Collisions)
        //         {
        //             RuntimeServices.RenderService.DrawDebugRect(
        //                 worldPos + data.Position,
        //                 data.Size,
        //                 SDK.Types.Color.Red * 0.5f,
        //                 2f
        //             );
        //         }
        //                 
        //         index++;
        //     }
        // }

        if (DebugMemory.Get<Rect>("map.collision_rectangle") is var collisionRectangle)
        {
            RuntimeServices.RenderService.DrawDebugRect(
                collisionRectangle.Position,
                collisionRectangle.Size,
                SDK.Types.Color.Green * 0.5f,
                1f
            );
        }
        
        if(DebugMemory.Get<Rect>("map.world_collision_rectangle") is var worldCollisionRect)
        {
            RuntimeServices.RenderService.DrawDebugRect(
                worldCollisionRect.Position,
                worldCollisionRect.Size,
                SDK.Types.Color.Blue * 0.5f,
                1f
            );
        }
        RuntimeServices.RenderService.FinishDrawing();
    }

    public event Action OnInitialize = null!;
    public event Action OnLoad = null!;
    public event Action<TimeSpan> OnUpdate = null!;
    public event Action<TimeSpan> OnDraw = null!;
}