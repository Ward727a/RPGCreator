using Gum.Wireframe;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.VectorDraw;
using MonoGameGum;
using MonoGameGum.Forms.Controls;
using MonoGameGum.Forms.Controls.Editor;
using MonoGameGum.GueDeriving;
using RPGCreator.Core;
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.Core.Type.Assets;
using RPGCreator.Core.Type.Map;
using RPGCreator.Core.Type.RTP;
using RPGCreator.RTP.Editor.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RPGCreator.MonoGame
{
    public class EditorGame : RTP_Game
    {
        public bool CanUseMouse = false;

        //private bool CanUseMouse => IsActive && InsideEditorBox;

        public GraphicsDeviceManager _graphics;
        private SpriteBatchExtend _spriteBatch;

        private MapEditing _mapEditing;

        GumService Gum => GumService.Default;

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

            Gum.Initialize(this);

            EngineCore.Instance.Data.EditedMapChanged += (s, e) =>
            {
                _mapEditing.MapInstance = EngineCore.Instance.Managers.Assets.MapFactory.Create(EngineCore.Instance.Data.EditedMap);
            };

            var mainPanel = new Panel(Gum.Root);

            base.Initialize();

            _events.OnRTPInitialized(new());
        }

        protected override void LoadContent()
        {
            _events.OnRTPLoadingContent(new());

            //GraphicsDevice.Reset();
            _spriteBatch = new(GraphicsDevice);
            _mapEditing = new(_spriteBatch);

            //CurrentMap = new(_spriteBatch) { game = this };

            _events.OnRTPLoadedContent(new());
            //backgroundTexture = new Texture2D(GraphicsDevice, 1, 1);
            //backgroundTexture.SetData(new[] { new Color(new Vector4(1, 1, 1, .3f)) });
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            GraphicalUiElement.CanvasHeight = (_graphics.PreferredBackBufferHeight);
            GraphicalUiElement.CanvasWidth = (_graphics.PreferredBackBufferWidth);
            _events.OnRTPUpdate(new(gameTime));

            _mapEditing.Update(gameTime);
            Gum.Update(gameTime);

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
