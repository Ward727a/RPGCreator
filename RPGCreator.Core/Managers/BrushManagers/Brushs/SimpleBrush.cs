#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
// 
// This file is part of RPG Creator and is distributed under the Apache 2.0 License.
// You are free to use, modify, and distribute this file under the terms of the Apache 2.0 License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence Apache 2.0.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence Apache 2.0.
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

using System.Numerics;
using CommunityToolkit.Diagnostics;
using RPGCreator.Core.Managers.AssetsManager.Factories;
using RPGCreator.Core.Types.Editor.Context;
using RPGCreator.SDK.Editor.Brushes;
using RPGCreator.SDK.Types;

namespace RPGCreator.Core.Managers.BrushManagers.Brushs
{
    public class SimpleBrush : IBrush, IBrushResizeFeature, IBrushPreview
    {
        private TileFactory _tiles => EngineCore.Instance.Managers.Assets.TileFactory;
        int Size { get; set; } = 1; // Default size of the brush
        public int Step => 1;
        public int MaxSize => 6;
        public int MinSize => 1;

        private bool _isPreviewEnabled = true;
        public bool IsPreviewEnabled { get => _isPreviewEnabled; set => _isPreviewEnabled = value; }

        public void Draw(Vector2 clickPos)
        {
            var target = "null";
            if (target == null)
                return;
            Guard.IsNotNull(target, nameof(target));
            // object? objectToPaint = MapEditorContext.SelectedObjectToPaint;
            //
            // if (objectToPaint == null) return;
            //
            // if (Size > 1)
            // {
            //     int halfSize = Size / 2;
            //     for (int x = -halfSize; x <= halfSize; x++)
            //     {
            //         for (int y = -halfSize; y <= halfSize; y++)
            //         {
            //             float gridX = clickPos.X + (x * target.GridWidth);
            //             float gridY = clickPos.Y + (y * target.GridHeight);
            //             var paintPos = new Vector2(gridX, gridY);
            //
            //             target.PaintAt(paintPos, objectToPaint);
            //         }
            //     }
            // }
            // else
            // {
            //     target.PaintAt(clickPos, objectToPaint);
            // }
        }

        public int GetBrushSize()
        {
            return Size; 
        }

        public void ResizeBrush(int newSize)
        {
            Size = newSize; 
        }

        public void ShowPreview(Vector2 at)
        {
            if(!_isPreviewEnabled)
            {
                return; // If preview is disabled, do not show anything
            }

            return;

            // if (MapEditorContext.MapInstance == null)
            // {
            //     return;
            // }
            // var instance = MapEditorContext.MapInstance;
            //
            // var layer = instance.PreviewLayer?.Definition;
            //
            // if (layer == null)
            // {
            //     return;
            // }
            // layer.ClearElements();
            //
            // var tile = MapEditorContext.SelectedTile;
            //
            // if (tile == null)
            // {
            //     return; // No tile selected, nothing to add
            // }
            //
            // if (!IBrush.InBorder(at, instance))
            // {
            //     return; // Clicked outside the map border, do not add tile
            // }
            //
            // // Manage the size of the brush, for example, if the size is 2, we will add a tile at (at.X, at.Y) and (at.X + tile.Width, at.Y + tile.Height) in a square pattern
            // // The center of the brush will be at the point 'at', and the tiles will be added around it based on the size of the brush.
            // if (Size > 1)
            // {
            //     // Calculate the range of tiles to add based on the brush size
            //     for (int x = -Size / 2; x <= Size / 2; x++)
            //     {
            //         for (int y = -Size / 2; y <= Size / 2; y++)
            //         {
            //             Vector2 tilePosition = new Vector2(at.X + x * tile.TilesetDef.TileWidth, at.Y + y * tile.TilesetDef.TileHeight);
            //             if (IBrush.InBorder(tilePosition, instance))
            //             {
            //                 layer.AddElement(tile,tilePosition); // Add tile at the calculated position
            //             }
            //         }
            //     }
            // }
            // else
            // {
            //     // If size is 1, just add the tile at the specified point
            //     layer.AddElement(tile,at); // Add tile at the calculated position
            // }
        }

        public URN UniqueName => new URN("rpgcreator", "brush","simple_brush");
        public string Name => "Simple Brush";
        public string Description => "A simple brush that paints single tiles or larger areas based on brush size.";
    }
}
