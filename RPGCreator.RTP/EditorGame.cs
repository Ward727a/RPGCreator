using System;
using System.Collections.Generic;
using CommunityToolkit.HighPerformance;
using Gum.Forms.Controls;
using Gum.Wireframe;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameGum;
using RPGCreator.RTP.ECS.Systems;
using RPGCreator.RTP.Editor.Components;
using RPGCreator.RTP.Extensions;
using RPGCreator.RTP.Services;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Animations;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.ECS.Entities;
using RPGCreator.SDK.ECS.Systems;
using RPGCreator.SDK.GamePlayer;
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Resources;

// WORKING PROGRESS PART - THIS IS NOT READY YET, AND NEED **MASSIVE** REFACTORING TO WORK WITH THE NEW ECS AND RENDERING SYSTEMS.

namespace RPGCreator.RTP
{
    public class EditorGame : Game, IGamePlayer
    {
        public event Action OnInitialize;
        public event Action OnLoad;
        public event Action<TimeSpan> OnUpdate;

        public event Action<TimeSpan> OnDraw;
        
        private readonly IMouseState _mouseState;
        private readonly IKeyboardState _keyboardState;
        
        public bool CanUseMouse = false;

        //private bool CanUseMouse => IsActive && InsideEditorBox;

        public GraphicsDeviceManager _graphics;

        private IECSWorld _ecsWorld = EngineServices.ECS.CreateWorld();

        private MapEditing _mapEditing;
        
        private List<Ulid> SpawnedCharacters = new();
        private IEntity _playerEntity;

        private AnimationInstance? _spritePlayerAtlas = null;
        private AnimationInstance? _spritePlayerIdle = null;
        private SpriteBatch _spriteBatch;

        GumService Gum => GumService.Default;

        public class Texture2DLoader(GraphicsDevice graphicsDevice) : IResourceLoader
        {
            public object Load(string path)
            {
                return Texture2D.FromFile(graphicsDevice, path);
            }
        }

        public EditorGame()
        {
            EngineProviders.GameProvider = new EngineGameProvider(this);
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            EngineServices.ResourcesService.RegisterLoader<Texture2D>(new Texture2DLoader(GraphicsDevice));

            _keyboardState = EngineStates.ViewportKeyboardState;
            _mouseState = EngineStates.ViewportMouseState;
        }

        private TextBox _noMapSelectedText;
        private TextBox _mousePointerText;
        protected override void Initialize()
        {
            base.Initialize();
            OnInitialize?.Invoke();

            Gum.Initialize(this);
            
            // EngineCore.Instance.Data.EditedMapChanged += (instance) =>
            // {
            //     if (instance == null)
            //         return;
            //     _mapEditing.MapInstance = instance;
            // };

            var mainPanel = new Panel(Gum.Root);
            mainPanel.Width = GraphicalUiElement.CanvasWidth;
            mainPanel.Height = GraphicalUiElement.CanvasHeight;
            
            // Add text box to display when no map is selected.
            _noMapSelectedText = new TextBox()
            {
                IsReadOnly = true,
                Text = "No map selected. Please select a map to edit.",
            };
            mainPanel.AddChild(_noMapSelectedText);
            
            _mousePointerText = new TextBox()
            {
                IsReadOnly = true,
                Text = "Pos NONE",
                X = 0,
                Y = 30,
            };
            mainPanel.AddChild(_mousePointerText);

            //
            // EngineCore.Instance.Events.RTPKeyPressed += (sender, keyEventArgs) =>
            // {
            //
            //     // manage arrows key to move the only entity we have for now.
            //     if (SpawnedCharacters.Count != 0)
            //     {
            //         var entity = _playerEntity;
            //         ref var movementComponent = ref entity.GetComponent<MovementComponent>();
            //         ref var stateComponent = ref entity.GetComponent<CharStateComponent>();
            //
            //         switch (keyEventArgs.Key)
            //         {
            //             case Key.Up:
            //                 movementComponent.TargetDirection = new System.Numerics.Vector2(0, -1);
            //                 movementComponent.IsMoving = true;
            //                 stateComponent.CurrentState = "walk_up"; // Temporary, we only have walk_down animation for now.
            //                 break;
            //             case Key.Down:
            //                 movementComponent.TargetDirection = new System.Numerics.Vector2(0, 1);
            //                 movementComponent.IsMoving = true;
            //                 stateComponent.CurrentState = "walk_down"; // Temporary, we only have walk_down animation for now.
            //                 break;
            //             case Key.Left:
            //                 movementComponent.TargetDirection = new System.Numerics.Vector2(-1, 0);
            //                 movementComponent.IsMoving = true;
            //                 stateComponent.CurrentState = "walk_left"; // Temporary, we only have walk_down animation for now.
            //                 break;
            //             case Key.Right:
            //                 movementComponent.TargetDirection = new System.Numerics.Vector2(1, 0);
            //                 movementComponent.IsMoving = true;
            //                 stateComponent.CurrentState = "walk_right"; // Temporary, we only have walk_down animation for now.
            //                 break;
            //             default:
            //                 movementComponent.IsMoving = false;
            //                 movementComponent.TargetDirection = new System.Numerics.Vector2(0, 0);
            //                 stateComponent.CurrentState = "idle";
            //                 break;
            //         }
            //     }
            // };
            //
            // EngineCore.Instance.Events.DEBUG_RTPAnimationAtlasGenerated += (s, e) =>
            // {
            //     _spritePlayerAtlas = e.Item1;
            //     _spritePlayerIdle = e.Item2;
            // };
        }

        protected override void LoadContent()
        {
            OnLoad?.Invoke();
            GraphicsDevice.Reset();
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _mapEditing = new(_spriteBatch);
            
            RuntimeServices.MapService = new MapService();
            RuntimeServices.LayerService = new LayerService();
            RuntimeServices.ChunkService = new ChunkService();
            RuntimeServices.CameraService = new CameraService();
            RuntimeServices.RenderService = new RenderService(GraphicsDevice, _spriteBatch);

            var cam = _ecsWorld.EntityManager.CreateCameraEntity();
            
            RuntimeServices.CameraService.SetCameraEntity(cam);
            
            _ecsWorld.SystemManager.AddSystem(new CameraSystem());
            _ecsWorld.SystemManager.AddSystem(new MapDrawingSystem(GraphicsDevice));

            void OnKeyboardStateOnKeyDown(KeyboardKeys key)
            {
                ReadOnlySpan<KeyboardKeys> pressedKeys = EngineStates.KeyboardState.GetPressedKeys();

                if (pressedKeys.Length == 0)
                {
                    _noMapSelectedText.Text = "No keys pressed";
                }
                else
                {
                    var sb = new System.Text.StringBuilder();

                    for (int i = 0; i < pressedKeys.Length; i++)
                    {
                        if (i > 0) sb.Append(", ");
                        sb.Append(pressedKeys[i]);
                    }

                    if (key == KeyboardKeys.D)
                    {
                        RuntimeServices.CameraService.Drag(new System.Numerics.Vector2(32, 0));
                    }
                    
                    if (key == KeyboardKeys.Q)
                    {
                        RuntimeServices.CameraService.Drag(new System.Numerics.Vector2(-32, 0));
                    }

                    _noMapSelectedText.Text = sb.ToString();
                }
            }
            //
            // _mouseState.ButtonDown += (button) =>
            // {
            //     Logger.Info($"Mouse button {button} was pressed.");
            // };
            _keyboardState.KeyDown += OnKeyboardStateOnKeyDown;
            _keyboardState.KeyUp += OnKeyboardStateOnKeyDown;
            
            _mouseState.Moved += (x, y) =>
            {
                _mousePointerText.Text = $"Pos X:{_mouseState.X} Y:{_mouseState.Y}";
            };

            // Test loop to create multiple entities with sprite and transform components and test the sprite rendering system.
            // Very basic test - Result for now : 10k entities with simple sprites renders, no movement at ~60 FPS => 3-4ms per frame.
            // for(int i = 0; i < 10000; i++)
            // {
            //     var entity = _ecsWorld.CreateEntity();
            //
            //     ref var spriteComponent = ref entity.AddComponent<SpriteComponent>();
            //
            //     // For now we will use a hardcoded path for a test sprite found in the engine assets folder.
            //     spriteComponent.SpritePath = $"{AppContext.BaseDirectory}Assets/sprites/character/test_character.png";
            //     spriteComponent.Size = new(16, 16); // Right now the size isn't used by the sprite renderer system.
            //     
            //     ref var transformComponent = ref entity.AddComponent<TransformComponent>();
            //
            //     transformComponent.Y = 5 + i * 20;
            //     transformComponent.X = 5 + i * 20;
            // }

            //CurrentMap = new(_spriteBatch) { game = this };

            //backgroundTexture = new Texture2D(GraphicsDevice, 1, 1);
            //backgroundTexture.SetData(new[] { new Color(new Vector4(1, 1, 1, .3f)) });
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            RuntimeServices.ChunkService.UpdateLoadedChunk();
            OnUpdate?.Invoke(gameTime.ElapsedGameTime);
            // GraphicalUiElement.CanvasHeight = (_graphics.PreferredBackBufferHeight);
            // GraphicalUiElement.CanvasWidth = (_graphics.PreferredBackBufferWidth);
            
            // If the game window size changes, we need to update the Camera viewport size.
            var cameraEntity = RuntimeServices.CameraService.CameraEntity;
            if (cameraEntity != null)
            {
                ref var cameraComponent = ref cameraEntity.GetComponent<CameraComponent>();
                if (cameraComponent.ViewportSize.Width != Window.ClientBounds.Width ||
                    cameraComponent.ViewportSize.Height != Window.ClientBounds.Height)
                {
                    cameraComponent.ViewportSize = new(Window.ClientBounds.Width,
                        Window.ClientBounds.Height);
                }
            }

            _mapEditing.Update(gameTime);
            Gum.Update(gameTime);
            _ecsWorld.Update(gameTime.ElapsedGameTime);

            if (_mouseState.WasButtonJustPressed(MouseButton.Left))
            {
                Logger.Info("Left mouse button was just pressed.");
            }
            
            // if (!EngineStates.EditorState.InEditorMode)
            // {
            //     var keyboardState = Keyboard.GetState();
            //
            //     var keyPressed = keyboardState.GetPressedKeys();
            //
            //     ReadOnlySpan<KeyboardKeys> sdkMappedKeys = keyPressed.AsSpan().Cast<Keys, KeyboardKeys>();
            //
            //     var rawKeyboardData = new RawKeyboardData(sdkMappedKeys, keyboardState.CapsLock, keyboardState.NumLock);
            //     EngineProviders.KeyboardProvider.Update(rawKeyboardData);
            //
            //     var mouseState = Mouse.GetState();
            //
            //     MouseButton buttons =
            //         (mouseState.LeftButton == ButtonState.Pressed ? MouseButton.Left : MouseButton.None)
            //         | (mouseState.RightButton == ButtonState.Pressed ? MouseButton.Right : MouseButton.None)
            //         | (mouseState.MiddleButton == ButtonState.Pressed ? MouseButton.Middle : MouseButton.None)
            //         | (mouseState.XButton1 == ButtonState.Pressed ? MouseButton.XButton1 : MouseButton.None)
            //         | (mouseState.XButton2 == ButtonState.Pressed ? MouseButton.XButton2 : MouseButton.None);
            //
            //     RawMouseData rawMouseData = new()
            //     {
            //         X = mouseState.X,
            //         Y = mouseState.Y,
            //         HScroll = mouseState.HorizontalScrollWheelValue,
            //         Scroll = mouseState.ScrollWheelValue,
            //         IsInsideWindow = IsActive,
            //         Buttons = buttons
            //     };
            //     EngineProviders.MouseProvider?.Update(rawMouseData);
            // }


            //
            // var _registry = EngineCore.Instance.Managers.Assets.TryResolveRegistry("characters", out var registry) ? registry as CharacterRegistry
            //     : null;
            //
            // foreach (var data in _registry.All()) 
            // {
            //     if(!SpawnedCharacters.Contains(data.Unique))
            //     {
            //         var entity = _ecsWorld.CreateEntity();
            //
            //         ref var spriteComponent = ref entity.AddComponent<SpriteComponent>();
            //
            //         spriteComponent.Texture = new UnifiedImage(data.PortraitPath, GraphicsDevice).Game;
            //         spriteComponent.RenderSize = new(48*2, 64*2);
            //
            //         ref var charDataComponent = ref entity.AddComponent<CharDataComponent>();
            //         charDataComponent.CharacterData = data;
            //         
            //         ref var charStateComponent = ref entity.AddComponent<CharStateComponent>();
            //         charStateComponent.CurrentState = "idle";
            //         charStateComponent.CurrentDirection = EDirection.Down;
            //         
            //         ref var animationComponent = ref entity.AddComponent<AnimationComponent>();
            //         animationComponent.CurrentFrame = 0;
            //         animationComponent.ElapsedTime = 0;
            //         
            //         ref var movementComponent = ref entity.AddComponent<MovementComponent>();
            //         movementComponent.IsMoving = false;
            //         movementComponent.Mode = MovementMode.Grid4;
            //         movementComponent.Speed = 32f;
            //         movementComponent.TargetDirection = new System.Numerics.Vector2(0, 0);
            //
            //         ref var transformComponent = ref entity.AddComponent<TransformComponent>();
            //
            //         transformComponent.Y = 32*3;
            //         transformComponent.X = 32*2;
            //         _playerEntity = entity;
            //         SpawnedCharacters.Add(data.Unique);
            //     }
            // }

            //if (CanUseMouse)
            //{
            //    if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            //    {
            //        Point position = Mouse.GetState().Position;

            //        int iX = (int)(position.X / 16);
            //        int iY = (int)(position.Y / 16);

            //        int X = iX * 16;
            //        int Y = iY * 16;

            //        if (SelectedLayer != -1)
            //        {

            //            if (UsedTilesets.Count <= SelectedTileset)
            //            {
            //                return;
            //            }

            //            CurrentMap.Layers[SelectedLayer].AddTile(new(X, Y), SelectedTileset, SourceRectUV, UsedTilesets[SelectedTileset].Dimension);
            //        }

            //    }
            //    else if (Mouse.GetState().RightButton == ButtonState.Pressed)
            //    {

            //        Point position = Mouse.GetState().Position;

            //        int iX = (int)(position.X / 16);
            //        int iY = (int)(position.Y / 16);

            //        int X = iX * 16;
            //        int Y = iY * 16;


            //        if (UsedTilesets.Count <= SelectedTileset)
            //        {
            //            return;
            //        }

            //        CurrentMap.Layers[SelectedLayer].RemoveTile(new Point(X, Y));
            //    }
            //}
            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            OnDraw?.Invoke(gameTime.ElapsedGameTime);
            GraphicsDevice.Clear(Color.CornflowerBlue);
// Dans ta boucle de rendu de debug
            // _mapEditing.Draw();
            _ecsWorld.SystemManager.Draw(gameTime.ElapsedGameTime, _spriteBatch);
            
            //_spriteBatch.Begin();

            //if(HasProject)
            //{
            //    if(Project.EditingMap)
            //    {
            //        if (NoMapSelectedText.Visible)
            //            NoMapSelectedText.Visible = false;
            //        // We draw the map here.
            //        Project.EditMap.Draw(_spriteBatch);
            //    } else
            //    {
            //        NoMapSelectedText.Visible = true;
            //    }
            //}

            //for (int i = 0; i < 33; i++)
            //{
            //    if (i >= 17)
            //    {
            //        _spriteBatch.DrawLine(new(0, 16 * (i - 16)), new(16 * 16, 16 * (i - 16)), Color.Black, 1);
            //    }
            //    else
            //    {
            //        _spriteBatch.DrawLine(new(16 * (i), 0), new(16 * (i), 16*16), Color.Black, 1);
            //    }
            //}

            ////Tileset test = (Tileset)EngineCore.TESTPACK.GetAsset("tilesets/test.png");

            ////if (test != null)
            ////{
            ////    _spriteBatch.Draw(test.GetTexture(GraphicsDevice), new Vector2(0, 0), Color.White);
            ////}

            ////CurrentMap.DrawMap();

            //if(NoMapSelectedText.Visible)
            //{
            //    Rectangle backgroundSizePos = new Rectangle(0, 0, (int)GraphicalUiElement.CanvasWidth, (int)GraphicalUiElement.CanvasHeight);
            //    //, new Color(new Vector4(1, 1, 1, .3f))
            //    _spriteBatch.Draw(backgroundTexture, backgroundSizePos, Color.Black);
            //}
            //_spriteBatch.End();

            Gum.Draw();
            
            

            base.Draw(gameTime);
        }
    }
}
