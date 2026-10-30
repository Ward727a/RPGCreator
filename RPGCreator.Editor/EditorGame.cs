using System;
using Gum.Forms.Controls;
using Gum.Wireframe;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using RPGCreator.RTP.ECS.Systems;
using RPGCreator.RTP.Extensions;
using RPGCreator.RTP.Services;
using RPGCreator.SDK;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.ECS.Entities;
using RPGCreator.SDK.ECS.Systems;
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.RuntimeService;

namespace RPGCreator.RTP
{
    public class EditorGame : Game, IGameRunner
    {
        public event Action? OnInitialize;
        public event Action? OnLoad;
        public event Action<TimeSpan>? OnUpdate;

        public event Action<TimeSpan>? OnDraw;
        
        private readonly IMouseState _mouseState;
        private readonly IKeyboardState _keyboardState;

        private IEntity? _cameraEntity;
        
        public GraphicsDeviceManager Graphics;
        public GraphicsDevice GraphicsDevice => base.GraphicsDevice;

        private IEcsWorld _ecsWorld = EngineServices.ECS.CreateWorld();

        private SpriteBatch _spriteBatch = null!;

        GumService Gum => GumService.Default;

        public EditorGame()
        {
            RuntimeServices.GameRunner = this;
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            

            _keyboardState = EngineStates.ViewportKeyboardState;
            _mouseState = EngineStates.ViewportMouseState;
            
            OnUpdate += OnUpdateInputService;
            EngineServices.InputsService.RegisterAction("forward", () =>
            {
                RuntimeServices.CameraService.Drag(new System.Numerics.Vector2(0, -32));
            }, true);
            EngineServices.InputsService.RegisterAction("backward", () =>
            {
                RuntimeServices.CameraService.Drag(new System.Numerics.Vector2(0, 32));
            }, true);
            EngineServices.InputsService.RegisterAction("left", () =>
            {
                RuntimeServices.CameraService.Drag(new System.Numerics.Vector2(-32, 0));
            }, true);
            EngineServices.InputsService.RegisterAction("right", () =>
            {
                RuntimeServices.CameraService.Drag(new System.Numerics.Vector2(32, 0));
            }, true);
            EngineServices.InputsService.RegisterAction("zoom_in", () =>
            {
                RuntimeServices.CameraService.ZoomBy(0.1f);
                _zoomLevelText.Text = $"Zoom: {RuntimeServices.CameraService.ZoomLevel:F2}";
            }, false);
            EngineServices.InputsService.RegisterAction("zoom_out", () =>
            {
                RuntimeServices.CameraService.ZoomBy(- 0.1f);
                _zoomLevelText.Text = $"Zoom: {RuntimeServices.CameraService.ZoomLevel:F2}";
            }, false);
            EngineServices.InputsService.RegisterAction("left_click", () =>
            {
                EngineStates.BrushState.IsDrawing = true;
                var mousePos = _mouseState.Position;
                var worldPos = RuntimeServices.CameraService.ScreenToWorld(mousePos);

                EngineServices.BrushManager.DrawAt(worldPos);
                //
                // var layer = EngineStates.EditorState.CurrentLayer;
                // var map = EngineStates.EditorState.CurrentMap;
                // var tile = EngineStates.BrushState.CurrentObjectToPaint;
                //
                // if(layer == null || map == null || tile == null || tile is not ITileDef tileDef)
                //     return;
                //
                // var mapPos = RuntimeServices.MapService.WorldToMapCoordinates(worldPos);
                // Logger.Info("Left click action triggered at {pos}.", args: mapPos);
                //
                // RuntimeServices.MapService.PlaceTileAt((int)mapPos.X, (int)mapPos.Y, tileDef);
            }, false);
            EngineServices.InputsService.RegisterAction("right_click", () =>
            {
                var mousePos = _mouseState.Position;
                var worldPos = RuntimeServices.CameraService.ScreenToWorld(mousePos);
                Logger.Info("Right click action triggered at {pos}.", args: worldPos);
            }, false);
            EngineServices.InputsService.RegisterAction("middle_click", () =>
            {
                var mousePos = _mouseState.Position;
                var worldPos = RuntimeServices.CameraService.ScreenToWorld(mousePos);
                Logger.Info("Middle click action triggered at {pos}.", args: worldPos);
            }, false);
        }

        private void OnUpdateInputService(TimeSpan _)
        {
            EngineServices.InputsService.Update(EngineStates.ViewportKeyboardState, EngineStates.ViewportMouseState);
        }

        protected override void Dispose(bool disposing)
        {
            OnUpdate -= OnUpdateInputService;
            base.Dispose(disposing);
        }

        private TextBox _noMapSelectedText = null!;
        private TextBox _mousePointerText = null!;
        private TextBox _zoomLevelText = null!;
        protected override void Initialize()
        {
            base.Initialize();
            OnInitialize?.Invoke();
        }

        protected override void LoadContent()
        {
            OnLoad?.Invoke();
            GraphicsDevice.Reset();
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            EngineServices.ResourcesService.RegisterLoader<Texture2D>(new Texture2DLoader(GraphicsDevice));
            
            RuntimeServices.MapService = new MapService();
            RuntimeServices.LayerService = new LayerService();
            RuntimeServices.ChunkService = new ChunkService();
            RuntimeServices.CameraService = new CameraService();
            RuntimeServices.RenderService = new RenderService(GraphicsDevice, _spriteBatch);
            RuntimeServices.PlayerController = new BasePlayerController();
            RuntimeServices.GameSession = new DefaultGameSession();
            RuntimeServices.GameSession.ActiveEcsWorld = _ecsWorld;

            var cam = _ecsWorld.EntityManager.CreateCameraEntity();
            
            RuntimeServices.CameraService.SetCameraEntity(cam.Id);
            
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

                    // switch (key)
                    // {
                    //     case KeyboardKeys.D:
                    //         RuntimeServices.CameraService.Drag(new System.Numerics.Vector2(32, 0));
                    //         break;
                    //     case KeyboardKeys.Q:
                    //         RuntimeServices.CameraService.Drag(new System.Numerics.Vector2(-32, 0));
                    //         break;
                    // }

                    _noMapSelectedText.Text = sb.ToString();
                }
            }
            
            _keyboardState.KeyDown += OnKeyboardStateOnKeyDown;
            _keyboardState.KeyUp += OnKeyboardStateOnKeyDown;
            
            _mouseState.Moved += (x, y) =>
            {
                Vector2 mousePos = _mouseState.Position;
                Vector2 worldPos = RuntimeServices.CameraService.ScreenToWorld(new System.Numerics.Vector2(mousePos.X, mousePos.Y));
                _mousePointerText.Text = $"Pos X:{worldPos.X:F3} Y:{worldPos.Y:F3}";
            };
            
        }
        protected override void Update(GameTime gameTime)
        {
            RuntimeServices.ChunkService.UpdateLoadedChunk();
            OnUpdate?.Invoke(gameTime.ElapsedGameTime);
            
            // If the game window size changes, we need to update the Camera viewport size.
            if (_cameraEntity != null)
            {
                ref var cameraComponent = ref _cameraEntity.GetComponent<CameraComponent>();
                var viewportSize = cameraComponent.ViewportSize;
                var windowSize = Window.ClientBounds;
                if (Math.Abs(viewportSize.Width - windowSize.Width) > 0.1 ||
                    Math.Abs(viewportSize.Height - windowSize.Height) > 0.1)
                {
                    cameraComponent.ViewportSize = new(Window.ClientBounds.Width,
                        Window.ClientBounds.Height);
                }
            }

            _ecsWorld.Update(gameTime.ElapsedGameTime);
            
            Gum.Update(gameTime);

            if (_mouseState.WasButtonJustPressed(MouseButton.Left))
            {
                Logger.Info("Left mouse button was just pressed.");
            }
            
            base.Update(gameTime);
            EngineStates.MouseState.ResetDeltas();
            EngineStates.ViewportMouseState.ResetDeltas();
        }


        protected override void Draw(GameTime gameTime)
        {
            OnDraw?.Invoke(gameTime.ElapsedGameTime);
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _ecsWorld.SystemManager.Draw(gameTime.ElapsedGameTime);
            
            Gum.Draw();
            
            base.Draw(gameTime);
        }
    }
}
