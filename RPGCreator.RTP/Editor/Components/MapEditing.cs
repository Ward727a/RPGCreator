using Microsoft.Xna.Framework;
using MonoGame.Extended;
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.Core.Type.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.RTP.Editor.Components
{
    public class MapEditing
    {
        public bool ShowGridInFront { get; set; } = false;
        public BaseMap? Map;
        private SpriteBatchExtend _sb;

        public MapEditing(SpriteBatchExtend spriteBatchExtend)
        {
            _sb = spriteBatchExtend;
            // Initialize components related to map editing
            // This could include setting up layers, properties, and other map-related functionalities
        }

        public MapEditing(BaseMap map, SpriteBatchExtend spriteBatchExtend)
        {
            _sb = spriteBatchExtend;
            Map = map;
            // Initialize components related to the provided map
            // This could include setting up layers, properties, and other map-related functionalities
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

        protected bool HasMap()
        {
            return Map != null;
        }
        protected void DrawLayers()
        {

            if (!HasMap())
                return;

            // Draw other map components here, such as tiles, entities, etc.
            foreach (var layer in Map.Layers.OrderBy(layer => layer.ZIndex))
            {
                layer.Draw(_sb);
            }
        }

        protected void DrawGrid()
        {
            if (!HasMap())
            {
                return;
            }

            if (!Map.ShowGrid)
            {
                return;
            }

            // Number of cells on the horizontal axis
            int cellSize = Map.GridParameter.CellWidth;

            int horizontalCells = Map.Size.Width;
            int verticalCells = Map.Size.Height;

            int totalCells = horizontalCells * verticalCells;

            _sb.Begin();

            for (int i = 0; i < horizontalCells; i++)
            {
                for (int j = 0; j < verticalCells; j++)
                {
                    _sb.DrawRectangle(new Rectangle(i * cellSize + 1, j * cellSize + 1, cellSize, cellSize), Map.GridParameter.CellBorderColor, 1f);
                }
            }
            _sb.End();
        }

        /// <summary>
        /// This methods is used to draw a previews of the tile that is currently selected on the grid / map editor.<br/>
        /// This should always be called at last, after all the other components have been drawn.<br/>
        /// </summary>
        protected void DrawTilePreview()
        {

        }

        protected void DrawMapBorder()
        {
            if (!HasMap())
            {
                return;
            }
            int cellSize = Map.GridParameter.CellWidth;
            // Draw the border of the map
            _sb.Begin();
            _sb.DrawRectangle(new Rectangle(0, 0, Map.Size.Width * cellSize+2, Map.Size.Height * cellSize+2), Color.Black, 1f);
            _sb.End();

        }
    }
}
