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
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.Core.Type.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.Core.Managers.RTP.BrushManagers.Brushs
{
    public class SimpleBrush : IBrush, IBrushResizeFeature
    {
        int Size { get; set; } = 1; // Default size of the brush
        public int Step => 1;
        public int MaxSize => 6;
        public int MinSize => 1;

        public void Draw(SpriteBatchExtend sb, Point at, Type.Map.BaseMap map)
        {
            if (map == null)
            {
                return;
            }

            var layer = EngineCore.Instance.Data.SelectedLayer;

            if (layer == null)
            {
                return;
            }

            var tile = EngineCore.Instance.Data.SelectedTile;

            if (tile == null)
            {
                return; // No tile selected, nothing to add
            }

            if (!IBrush.InBorder(at, map))
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
                        Point tilePosition = new Point(at.X + x * tile.Tileset.tile_width, at.Y + y * tile.Tileset.tile_height);
                        if (IBrush.InBorder(tilePosition, map))
                        {
                            layer.AddTileAt(tile, tilePosition); // Add tile at the calculated position
                        }
                    }
                }
            }
            else
            {
                // If size is 1, just add the tile at the specified point
                layer.AddTileAt(tile, at);
            }
        }

        public int GetBrushSize()
        {
            return Size; // Return the current size of the brush
        }

        public void ResizeBrush(int newSize)
        {
            Size = newSize; // Update the brush size
        }
    }
}
