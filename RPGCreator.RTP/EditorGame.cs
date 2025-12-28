using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.VectorDraw;
using RPGCreator.Core;
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.Core.Types.Assets;
using RPGCreator.Core.Types.RTP;
using RPGCreator.RTP.Editor.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia.Input;
using RPGCreator.Core.ECS;
using RPGCreator.Core.ECS.Components.Display;
using RPGCreator.Core.ECS.Components.Display.Animation;
using RPGCreator.Core.ECS.Systems;
using RPGCreator.Core.Managers.AssetsManager.Registries;
using RPGCreator.Core.Runtimes;
using RPGCreator.Core.Runtimes.ECS;
using RPGCreator.Core.Runtimes.ECS.Components.Actor;
using RPGCreator.Core.Runtimes.ECS.Components.Display;
using RPGCreator.Core.Types;
using RPGCreator.Core.Types.Assets.Characters;
using RPGCreator.SDK.Assets.Definitions.Animations;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.ECS.Entities;
using Serilog;
using Size = RPGCreator.Core.Types.Internal.Size;

namespace RPGCreator.MonoGame
{
    public class EditorGame : RTP_Game
    {
        public bool CanUseMouse = false;

        //private bool CanUseMouse => IsActive && InsideEditorBox;

        public GraphicsDeviceManager _graphics;
        private SpriteBatchExtend _spriteBatch;

        private ECSWorld _ecsWorld;

        private MapEditing _mapEditing;
        
        private List<Ulid> SpawnedCharacters = new();
        private IEntity _playerEntity;

        private AnimationInstance? _spritePlayerAtlas = null;
        private AnimationInstance? _spritePlayerIdle = null;

        // GumService Gum => GumService.Default;

        public EditorGame()
        {
            EngineCore.Instance.Events.OnRTPCreating(new());
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            EngineCore.Instance.Events.OnRTPCreated(new(this));
        }

        protected override void Initialize()
        {
            _events.OnRTPInitializing(new());

            // Gum.Initialize(this);

            EngineCore.Instance.Data.EditedMapChanged += (instance) =>
            {
                if (instance == null)
                    return;
                _mapEditing.MapInstance = instance;
            };

            // var mainPanel = new Panel(Gum.Root);

            base.Initialize();

            EngineCore.Instance.Events.RTPKeyPressed += (sender, keyEventArgs) =>
            {

                // manage arrows key to move the only entity we have for now.
                if (SpawnedCharacters.Count != 0)
                {
                    var entity = _playerEntity;
                    ref var movementComponent = ref entity.GetComponent<MovementComponent>();
                    ref var stateComponent = ref entity.GetComponent<CharStateComponent>();

                    switch (keyEventArgs.Key)
                    {
                        case Key.Up:
                            movementComponent.TargetDirection = new System.Numerics.Vector2(0, -1);
                            movementComponent.IsMoving = true;
                            stateComponent.CurrentState = "walk_up"; // Temporary, we only have walk_down animation for now.
                            break;
                        case Key.Down:
                            movementComponent.TargetDirection = new System.Numerics.Vector2(0, 1);
                            movementComponent.IsMoving = true;
                            stateComponent.CurrentState = "walk_down"; // Temporary, we only have walk_down animation for now.
                            break;
                        case Key.Left:
                            movementComponent.TargetDirection = new System.Numerics.Vector2(-1, 0);
                            movementComponent.IsMoving = true;
                            stateComponent.CurrentState = "walk_left"; // Temporary, we only have walk_down animation for now.
                            break;
                        case Key.Right:
                            movementComponent.TargetDirection = new System.Numerics.Vector2(1, 0);
                            movementComponent.IsMoving = true;
                            stateComponent.CurrentState = "walk_right"; // Temporary, we only have walk_down animation for now.
                            break;
                        default:
                            movementComponent.IsMoving = false;
                            movementComponent.TargetDirection = new System.Numerics.Vector2(0, 0);
                            stateComponent.CurrentState = "idle";
                            break;
                    }
                }
            };

            EngineCore.Instance.Events.DEBUG_RTPAnimationAtlasGenerated += (s, e) =>
            {
                _spritePlayerAtlas = e.Item1;
                _spritePlayerIdle = e.Item2;
            };

            _events.OnRTPInitialized(new());
        }

        protected override void LoadContent()
        {
            _events.OnRTPLoadingContent(new());

            //GraphicsDevice.Reset();
            _spriteBatch = new(GraphicsDevice);
            _mapEditing = new(_spriteBatch);

            _ecsWorld = new();
            _ecsWorld.AddSystem(new SpriteRenderSystem(_ecsWorld.ComponentManager, GraphicsDevice));
            _ecsWorld.AddSystem(new AnimationSystem(_ecsWorld.ComponentManager, GraphicsDevice));
            _ecsWorld.AddSystem(new MovementSystem(_ecsWorld.ComponentManager));

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

            _events.OnRTPLoadedContent(new());
            //backgroundTexture = new Texture2D(GraphicsDevice, 1, 1);
            //backgroundTexture.SetData(new[] { new Color(new Vector4(1, 1, 1, .3f)) });
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            // GraphicalUiElement.CanvasHeight = (_graphics.PreferredBackBufferHeight);
            // GraphicalUiElement.CanvasWidth = (_graphics.PreferredBackBufferWidth);
            _events.OnRTPUpdate(new(gameTime));

            _mapEditing.Update(gameTime);
            // Gum.Update(gameTime);
            _ecsWorld.Update(gameTime);

            var _registry = EngineCore.Instance.Managers.Assets.TryResolveRegistry("characters", out var registry) ? registry as CharacterRegistry
                : null;
            
            foreach (var data in _registry.All()) 
            {
                if(!SpawnedCharacters.Contains(data.Unique))
                {
                    var entity = _ecsWorld.CreateEntity();

                    ref var spriteComponent = ref entity.AddComponent<SpriteComponent>();

                    spriteComponent.Texture = new UnifiedImage(data.PortraitPath, GraphicsDevice).Game;
                    spriteComponent.RenderSize = new(48*2, 64*2);

                    ref var charDataComponent = ref entity.AddComponent<CharDataComponent>();
                    charDataComponent.CharacterData = data;
                    
                    ref var charStateComponent = ref entity.AddComponent<CharStateComponent>();
                    charStateComponent.CurrentState = "idle";
                    charStateComponent.CurrentDirection = EDirection.Down;
                    
                    ref var animationComponent = ref entity.AddComponent<AnimationComponent>();
                    animationComponent.CurrentFrame = 0;
                    animationComponent.ElapsedTime = 0;
                    
                    ref var movementComponent = ref entity.AddComponent<MovementComponent>();
                    movementComponent.IsMoving = false;
                    movementComponent.Mode = MovementMode.Grid4;
                    movementComponent.Speed = 32f;
                    movementComponent.TargetDirection = new System.Numerics.Vector2(0, 0);

                    ref var transformComponent = ref entity.AddComponent<TransformComponent>();

                    transformComponent.Y = 32*3;
                    transformComponent.X = 32*2;
                    _playerEntity = entity;
                    SpawnedCharacters.Add(data.Unique);
                }
            }

            if (CanUseMouse)
            {
                if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    //Console.WriteLine("Left mouse button pressed inside preview.");
                }
            }

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
            _events.OnRTPDraw(new(gameTime));
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _mapEditing.Draw();
            _ecsWorld.Draw(gameTime);
            
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

            // Gum.Draw();

            base.Draw(gameTime);
        }
    }
}
