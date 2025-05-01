using Gum.Wireframe;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;
using MonoGameGum;
using MonoGameGum.Forms.Controls;
using MonoGameGum.Forms.Controls.Editor;
using MonoGameGum.GueDeriving;
using RPGCreator.Core;
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.Core.Type.Assets;
using RPGCreator.Core.Type.RTP;
using RPGCreator.MonoGame.Editor.Components.UI.Buttons;
using RPGCreator.MonoGame.Editor.Core.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RPGCreator.MonoGame
{
    public class EditorGame : RTP_Game
    {
        public new bool InsideEditorBox = false;
        public new bool IsActive = false;

        private bool CanUseMouse => IsActive && InsideEditorBox;

        public GraphicsDeviceManager _graphics;
        private SpriteBatchExtend _spriteBatch;

        public Map? CurrentMap;
        public int SelectedLayer = -1;

        Effect _sdfEffect;

        Panel mainPanel;
        private Texture2D _dummyTexture;
        GumService Gum => GumService.Default;

        public int SelectedTileset;
        public Vector2 PreviewPos = new(16, 16);
        public Rectangle SourceRectUV;

        private Dictionary<Point, TileData> Tiles = [];
        public List<GameTileset> UsedTilesets = [];

        public struct TileData
        {
            public int Tileset;
            public Rectangle UV;

            public TileData(int tileset, Rectangle uv)
            {
                Tileset = tileset;
                UV = uv;
            }

        }

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
            // TODO: Add your initialization logic here
            _events.OnRTPInitializing(new());

            Gum.Initialize(this);

            mainPanel = new Panel(Gum.Root);

            // Creates a button instance
            var button = new Button();
            // Adds the button as a child so that it is drawn and has its
            // events raised
            mainPanel.AddChild(button);
            // Initial button text before being clicked
            button.Text = "Click Me";
            // Makes the button wider so the text fits
            button.Visual.Width = 350;
            button.Visual.X = 790;
            // Click event can be handled with a lambda
            button.Click += (_, _) =>
                button.Text = $"Clicked at {System.DateTime.Now}";

            button.PerformClick();

            base.Initialize();

            _events.OnRTPInitialized(new());
        }

        protected override void LoadContent()
        {
            _events.OnRTPLoadingContent(new());

            GraphicsDevice.Reset();
            _spriteBatch = new(GraphicsDevice);

            CurrentMap = new(_spriteBatch) { game = this };

            _events.OnRTPLoadedContent(new());
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            _events.OnRTPUpdate(new(gameTime));

            Gum.Update(gameTime);

            if (CanUseMouse)
            {
                if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    Point position = Mouse.GetState().Position;

                    int iX = (int)(position.X / 16);
                    int iY = (int)(position.Y / 16);

                    int X = iX * 16;
                    int Y = iY * 16;

                    if (SelectedLayer != -1)
                    {

                        if (UsedTilesets.Count <= SelectedTileset)
                        {
                            return;
                        }

                        CurrentMap.Layers[SelectedLayer].AddTile(new(X, Y), SelectedTileset, SourceRectUV, UsedTilesets[SelectedTileset].Dimension);
                    }

                }
                else if (Mouse.GetState().RightButton == ButtonState.Pressed)
                {

                    Point position = Mouse.GetState().Position;

                    int iX = (int)(position.X / 16);
                    int iY = (int)(position.Y / 16);

                    int X = iX * 16;
                    int Y = iY * 16;


                    if (UsedTilesets.Count <= SelectedTileset)
                    {
                        return;
                    }

                    CurrentMap.Layers[SelectedLayer].RemoveTile(new Point(X, Y));
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _events.OnRTPDraw(new(gameTime));
            GraphicsDevice.Clear(Color.CornflowerBlue);


            // TODO: Add your drawing code here
            Gum.Draw();


            _spriteBatch.Begin();

            for (int i = 0; i < 33; i++)
            {
                if (i >= 17)
                {
                    _spriteBatch.DrawLine(new(0, 16 * (i - 16)), new(16 * 16, 16 * (i - 16)), Color.Black, 1);
                }
                else
                {
                    _spriteBatch.DrawLine(new(16 * (i), 0), new(16 * (i), 16*16), Color.Black, 1);
                }
            }

            Tileset test = (Tileset)EngineCore.TESTPACK.GetAsset("tilesets/test.png");

            if (test != null)
            {
                _spriteBatch.Draw(test.GetTexture(GraphicsDevice), new Vector2(0, 0), Color.White);
            }

            //CurrentMap.DrawMap();
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        public void AddButton(int posX, int posY, string content = "Hi")
        {
            var button = new ButtonEditor();
            button.X = posX;
            button.Y = posY;
            mainPanel.AddChild(button);
            button.Visual.Width = 100;
            button.Text = content;
        }

        public void OnNewPreviewSize()
        {
            GraphicalUiElement.CanvasHeight = (_graphics.PreferredBackBufferHeight);
            GraphicalUiElement.CanvasWidth = (_graphics.PreferredBackBufferWidth);
        }

        public bool LoadTileset(Texture2D tileset, int dimension)
        {
            if(UsedTilesets.Where(t=> t.Texture == tileset).Any())
            {
                return false;
            }
            UsedTilesets.Add(new() { Texture = tileset, Dimension = dimension });
            return true;
        }

        private void AddNewTile(int tileset, Point position, Rectangle UV)
        {
            if(UsedTilesets.Count <= tileset)
            {
                return;
            }

            Tiles[position] = new(tileset, UV);
        }
    }
}
