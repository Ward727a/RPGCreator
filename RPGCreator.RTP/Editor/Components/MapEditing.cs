using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace RPGCreator.RTP.Editor.Components
{
    public class MapEditing
    {
        public bool ShowGridInFront { get; set; } = false;
        private SpriteBatch _sb;

        public Point _LastPreviewAt;

        private MapEditing()
        {

            RegisterEvents();
        }
        
        public MapEditing(SpriteBatch spriteBatchExtend) : this()
        {
            _sb = spriteBatchExtend;
        }

        protected static void RegisterEvents()
        {
            // EngineCore.Instance.Managers.Brush.Event.ClickedAt += Brush_ClickedAt;
            // EngineCore.Instance.Managers.Brush.Event.PreviewAt += Brush_PreviewAt;
            // EngineCore.Instance.Managers.Brush.Event.ClearPreview += Brush_ClearPreview;
        }

        private static void Brush_ClearPreview()
        {
            // if (MapInstance == null)
            // {
            //     return;
            // }
            // MapInstance.PreviewLayer.InstancedElements.Clear(); // Clear the preview layer tiles
            // _LastPreviewAt = new(-1,-1); // Reset the last preview position
            // _LastPreviewBrush = null; // Reset the last preview brush
        }

        public void Draw()
        {
            DrawMapBorder();

            if (!ShowGridInFront)
            {
                DrawGrid();
            }

            DrawLayers();

            if (ShowGridInFront) // For some like Entities, we want the grid to be drawn in front of them so we can see where they are placed
            {
                DrawGrid();
            }
        }


        public void Update(GameTime gameTime)
        {

        }

        protected static bool HasMap()
        {
            // return MapInstance != null;
            return true;
        }
        protected void DrawLayers()
        {

            if (!HasMap())
                return;
            //
            // MapInstance.Draw(_sb);
            // // Draw other map components here, such as tiles, entities, etc.
            // // foreach (var layer in Map.Layers.OrderBy(layer => layer.ZIndex))
            // // {
            // //     layer.Draw(_sb);
            // // }
            //
            // _sb.SetOpacity(0.5f); // Set opacity for the preview layer
            // MapInstance.PreviewLayer.Draw(_sb);
            // _sb.ResetOpacity(); // Reset opacity after drawing the preview layer
        }

        protected bool InBorder(Point at)
        {
            if(!HasMap())
            {
                return false;
            }
            //
            // int cellSize = MapInstance.Definition.GridParameter.CellWidth;
            // int horizontalCells = MapInstance.Definition.Size.Width;
            // int verticalCells = MapInstance.Definition.Size.Height;

            // Check if the point is within the bounds of the map
            // if (at.X < 0 || at.Y < 0 || at.X >= horizontalCells * cellSize || at.Y >= verticalCells * cellSize)
            // {
            //     return false;
            // }
            return true;
        }

        protected void DrawGrid()
        {
            if (!HasMap())
            {
                return;
            }

            // // Number of cells on the horizontal axis
            // int cellSize = MapInstance.Definition.GridParameter.CellWidth;
            //
            // int horizontalCells = MapInstance.Definition.Size.Width;
            // int verticalCells = MapInstance.Definition.Size.Height;
            //
            // int totalCells = horizontalCells * verticalCells;
            //
            // _sb.Begin();
            //
            // for (int i = 0; i < horizontalCells; i++)
            // {
            //     for (int j = 0; j < verticalCells; j++)
            //     {
            //         _sb.DrawRectangle(new Rectangle(i * cellSize + 1, j * cellSize + 1, cellSize, cellSize), MapInstance.Definition.GridParameter.CellBorderColor, 1f);
            //     }
            // }
            // _sb.End();
        }

        /// <summary>
        /// This methods is used to draw a previews of the tile that is currently selected on the grid / map editor.<br/>
        /// This should always be called at last, after all the other components have been drawn.<br/>
        /// </summary>
        protected static void DrawTilePreview()
        {

        }

        protected void DrawMapBorder()
        {
            if (!HasMap())
            {
                return;
            }
            // int cellSize = MapInstance.Definition.GridParameter.CellWidth;
            // // Draw the border of the map
            // _sb.Begin();
            // _sb.DrawRectangle(new Rectangle(0, 0, MapInstance.Definition.Size.Width * cellSize+2, MapInstance.Definition.Size.Height * cellSize+2), Color.Black, 1f);
            // _sb.End();

        }
    }
}
