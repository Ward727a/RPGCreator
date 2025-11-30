#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
// 
// This file is part of RPG Creator and is distributed under the MIT License.
// You are free to use, modify, and distribute this file under the terms of the MIT License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence MIT.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence MIT.
// Voir LICENSE pour plus de détails.
// 
// Contact:
// => Mail: Ward727a@gmail.com
//    Please use this object: "RPG Creator [YourObject]"
// => Discord: ward727
// 
// For urgent inquiries, sending both an email and a message on Discord is highly recommended for a quicker response.
// 
// 
#endregion
using RPGCreator.Core.Types.Internal;
using RPGCreator.Core.Managers.AssetsManager.Factories;
using RPGCreator.Core.Types.Editor.Context;

namespace RPGCreator.Core.Managers.RTP.BrushManagers.Brushs
{
    public class SimpleBrush : IBrush, IBrushResizeFeature, IBrushPreviewFeature
    {
        private TileFactory _tiles => EngineCore.Instance.Managers.Assets.TileFactory;
        int Size { get; set; } = 1; // Default size of the brush
        public int Step => 1;
        public int MaxSize => 6;
        public int MinSize => 1;

        private bool _isPreviewEnabled = true;
        public bool IsPreviewEnabled { get => _isPreviewEnabled; set => _isPreviewEnabled = value; }

        public void Draw(Point clickPos, MapEditorContext context)
        {
            var target = context.GetActivePaintTarget();
            
            object objectToPaint = context.SelectedObjectToPaint;

            if (target == null || objectToPaint == null) return;

            if (Size > 1)
            {
                int halfSize = Size / 2;
                for (int x = -halfSize; x <= halfSize; x++)
                {
                    for (int y = -halfSize; y <= halfSize; y++)
                    {
                        int gridX = clickPos.X + (x * target.GridWidth);
                        int gridY = clickPos.Y + (y * target.GridHeight);
                        var paintPos = new Point(gridX, gridY);

                        if (target.IsValidPosition(paintPos))
                        {
                            target.PaintAt(paintPos, objectToPaint);
                        }
                    }
                }
            }
            else
            {
                if (target.IsValidPosition(clickPos))
                {
                    target.PaintAt(clickPos, objectToPaint);
                }
            }
        }

        public int GetBrushSize()
        {
            return Size; 
        }

        public void ResizeBrush(int newSize)
        {
            Size = newSize; 
        }

        public void ShowPreview(Point at, MapEditorContext context)
        {
            if(!_isPreviewEnabled)
            {
                return; // If preview is disabled, do not show anything
            }

            if (context.MapInstance == null)
            {
                return;
            }
            var instance = context.MapInstance;

            var layer = instance.PreviewLayer?.Definition;

            if (layer == null)
            {
                return;
            }
            layer.ClearElements();

            var tile = context.SelectedTile;

            if (tile == null)
            {
                return; // No tile selected, nothing to add
            }

            if (!IBrush.InBorder(at, instance))
            {
                return; // Clicked outside the map border, do not add tile
            }

            // Manage the size of the brush, for example, if the size is 2, we will add a tile at (at.X, at.Y) and (at.X + tile.Width, at.Y + tile.Height) in a square pattern
            // The center of the brush will be at the point 'at', and the tiles will be added around it based on the size of the brush.
            if (Size > 1)
            {
                // Calculate the range of tiles to add based on the brush size
                for (int x = -Size / 2; x <= Size / 2; x++)
                {
                    for (int y = -Size / 2; y <= Size / 2; y++)
                    {
                        Point tilePosition = new Point(at.X + x * tile.TilesetDef.TileWidth, at.Y + y * tile.TilesetDef.TileHeight);
                        if (IBrush.InBorder(tilePosition, instance))
                        {
                            layer.AddElement(tile,tilePosition); // Add tile at the calculated position
                        }
                    }
                }
            }
            else
            {
                // If size is 1, just add the tile at the specified point
                layer.AddElement(tile,at); // Add tile at the calculated position
            }
        }
    }
}
