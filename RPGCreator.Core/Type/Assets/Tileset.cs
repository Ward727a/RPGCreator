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
using Avalonia.Media;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Core.Type.Map;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RPGCreator.Core.Type.Assets
{
    public class Tileset : ImageAsset
    {

        public override bool ShouldBeCached => true;

        public int tile_width;
        public int tile_height;

        public Tileset() : base()
        {
            Type = TYPE.TILESETS;
        }

        public Tileset(string Name) : base(Name)
        {
            Type = TYPE.TILESETS;
        }

        public Tileset(string Name, int tile_width, int tile_height, string image_path) : base(Name)
        {
            Type = TYPE.TILESETS;
            this.tile_width = tile_width;
            this.tile_height = tile_height;
            ImagePath = image_path;
        }

        public override void FromBase()
        {
            base.FromBase();

            XElement data = AssetData;

            int dim_width = int.Parse(data.Element("tile_width")?.Value ?? "0");
            int dim_height = int.Parse(data.Element("tile_height")?.Value ?? "0");

            if (dim_width <= 0 || dim_height <= 0)
            {
                throw new Exception($"Tile dimensions are invalid.");
            }

            tile_width = dim_width;
            tile_height = dim_height;
        }

        public Tile GetTileAt(int tile_x, int tile_y, GraphicsDevice device)
        {
            if (tile_x < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tile_x), "Tile coordinates cannot be negative.");
            }
            if(tile_y < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tile_y), "Tile coordinates cannot be negative.");
            }

            if (tile_x * tile_width >= Width)
            {
                throw new ArgumentOutOfRangeException(nameof(tile_x), "Tile coordinates are out of bounds.");
            }
            if (tile_y * tile_height >= Height)
            {
                throw new ArgumentOutOfRangeException(nameof(tile_y), "Tile coordinates are out of bounds.");
            }

            tile_x /= tile_width;
            tile_y /= tile_height;

            Microsoft.Xna.Framework.Rectangle tile_rect = new(tile_x * tile_width, tile_y * tile_height, tile_width, tile_height);

            // Assuming you have a method to create a texture from a rectangle
            return new Tile(this, tile_rect);
        }

    }
}
