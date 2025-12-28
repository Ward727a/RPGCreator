using Microsoft.Xna.Framework;
using MonoGame.Extended;
using RPGCreator.Core;
using RPGCreator.Core.Managers.RTP.BrushManagers;
using RPGCreator.Core.Managers.RTP.BrushManagers.Brushs;
using RPGCreator.Core.Rendering.Batching;
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
        public MapInstance? MapInstance;
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

        public MapEditing(MapInstance mapInstance, SpriteBatchExtend spriteBatchExtend) : this()
        {
            _sb = spriteBatchExtend;
            MapInstance = mapInstance;
            // Initialize components related to the provided map
            // This could include setting up layers, properties, and other map-related functionalities
        }

        protected void RegisterEvents()
        {
            // EngineCore.Instance.Managers.Brush.Event.ClickedAt += Brush_ClickedAt;
            // EngineCore.Instance.Managers.Brush.Event.PreviewAt += Brush_PreviewAt;
            // EngineCore.Instance.Managers.Brush.Event.ClearPreview += Brush_ClearPreview;
        }

        private void Brush_ClearPreview()
        {
            if (MapInstance == null)
            {
                return;
            }
            MapInstance.PreviewLayer.InstancedElements.Clear(); // Clear the preview layer tiles
            _LastPreviewAt = new(-1,-1); // Reset the last preview position
            _LastPreviewBrush = null; // Reset the last preview brush
        }

        private void Brush_ClickedAt(object? sender, ClickedAtEventArgs e)
        {
            if(MapInstance == null)
            {
                return;
            }

            // e.brush.Draw(_sb, e.At, MapInstance);
        }

        private void Brush_PreviewAt(object? sender, PreviewAtEventArgs e)
        {

            return;
            if(e.At.IsEqualTo(_LastPreviewAt) && _LastPreviewBrush == e.Brush)
            {
                return; // No need to update the preview if the position and brush are the same
            }

            if (MapInstance == null)
            {
                return;
            }

            if (MapInstance.PreviewLayer.InstancedElements.Count > 0)
            {
                MapInstance.PreviewLayer.InstancedElements.Clear(); // Clear previous preview tiles
            }

            if (
                e.Brush == null)
            {
                return;
            }

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
            return MapInstance != null;
        }
        protected void DrawLayers()
        {

            if (!HasMap())
                return;

            MapInstance.Draw(_sb);
            // Draw other map components here, such as tiles, entities, etc.
            // foreach (var layer in Map.Layers.OrderBy(layer => layer.ZIndex))
            // {
            //     layer.Draw(_sb);
            // }

            _sb.SetOpacity(0.5f); // Set opacity for the preview layer
            MapInstance.PreviewLayer.Draw(_sb);
            _sb.ResetOpacity(); // Reset opacity after drawing the preview layer
        }

        protected bool InBorder(Point at)
        {
            if(!HasMap())
            {
                return false;
            }

            int cellSize = MapInstance.Definition.GridParameter.CellWidth;
            int horizontalCells = MapInstance.Definition.Size.Width;
            int verticalCells = MapInstance.Definition.Size.Height;

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

            // Number of cells on the horizontal axis
            int cellSize = MapInstance.Definition.GridParameter.CellWidth;

            int horizontalCells = MapInstance.Definition.Size.Width;
            int verticalCells = MapInstance.Definition.Size.Height;

            int totalCells = horizontalCells * verticalCells;

            _sb.Begin();

            for (int i = 0; i < horizontalCells; i++)
            {
                for (int j = 0; j < verticalCells; j++)
                {
                    _sb.DrawRectangle(new Rectangle(i * cellSize + 1, j * cellSize + 1, cellSize, cellSize), MapInstance.Definition.GridParameter.CellBorderColor, 1f);
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
            int cellSize = MapInstance.Definition.GridParameter.CellWidth;
            // Draw the border of the map
            _sb.Begin();
            _sb.DrawRectangle(new Rectangle(0, 0, MapInstance.Definition.Size.Width * cellSize+2, MapInstance.Definition.Size.Height * cellSize+2), Color.Black, 1f);
            _sb.End();

        }
    }
}
