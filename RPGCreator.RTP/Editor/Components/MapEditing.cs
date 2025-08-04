using Microsoft.Xna.Framework;
using MonoGame.Extended;
using RPGCreator.Core;
using RPGCreator.Core.Managers.RTP.BrushManagers;
using RPGCreator.Core.Managers.RTP.BrushManagers.Brushs;
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

        public Point _LastPreviewAt;
        public IBrushPreviewFeature? _LastPreviewBrush;

        private MapEditing()
        {

            RegisterEvents();
        }

        public MapEditing(SpriteBatchExtend spriteBatchExtend) : this()
        {
            _sb = spriteBatchExtend;
            // Initialize components related to map editing
            // This could include setting up layers, properties, and other map-related functionalities
        }

        public MapEditing(BaseMap map, SpriteBatchExtend spriteBatchExtend) : this()
        {
            _sb = spriteBatchExtend;
            Map = map;
            // Initialize components related to the provided map
            // This could include setting up layers, properties, and other map-related functionalities
        }

        protected void RegisterEvents()
        {
            EngineCore.Instance.Managers.Brush.Event.ClickedAt += Brush_ClickedAt;
            EngineCore.Instance.Managers.Brush.Event.PreviewAt += Brush_PreviewAt;
            EngineCore.Instance.Managers.Brush.Event.ClearPreview += Brush_ClearPreview;
        }

        private void Brush_ClearPreview()
        {
            if (Map == null)
            {
                return;
            }
            Map.PreviewLayer.Elements.Clear(); // Clear the preview layer tiles
            _LastPreviewAt = new(-1,-1); // Reset the last preview position
            _LastPreviewBrush = null; // Reset the last preview brush
        }

        private void Brush_ClickedAt(object? sender, ClickedAtEventArgs e)
        {
            if(Map == null)
            {
                return;
            }

            e.brush.Draw(_sb, e.At, Map);
        }

        private void Brush_PreviewAt(object? sender, PreviewAtEventArgs e)
        {

            if(_LastPreviewAt == e.At && _LastPreviewBrush == e.Brush)
            {
                return; // No need to update the preview if the position and brush are the same
            }

            if (Map == null)
            {
                return;
            }

            if (Map.PreviewLayer.Elements.Count > 0)
            {
                Map.PreviewLayer.Elements.Clear(); // Clear previous preview tiles
            }

            if (
                e.Brush == null)
            {
                return;
            }

            e.Brush.ShowPreview(_sb, e.At, Map);
            _LastPreviewAt = e.At;
            _LastPreviewBrush = e.Brush;
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

            _sb.SetOpacity(0.5f); // Set opacity for the preview layer
            Map.PreviewLayer.Draw(_sb);
            _sb.ResetOpacity(); // Reset opacity after drawing the preview layer
        }

        protected bool InBorder(Point at)
        {
            if(!HasMap())
            {
                return false;
            }

            int cellSize = Map.GridParameter.CellWidth;
            int horizontalCells = Map.Size.Width;
            int verticalCells = Map.Size.Height;

            // Check if the point is within the bounds of the map
            if (at.X < 0 || at.Y < 0 || at.X >= horizontalCells * cellSize || at.Y >= verticalCells * cellSize)
            {
                return false;
            }
            return true;
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
