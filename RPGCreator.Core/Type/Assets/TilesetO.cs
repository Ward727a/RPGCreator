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
    public class Tileset : ImageAsset, ISerializable, IDeserializable
    {
        public override bool ShouldBeCached => true;
        public Dictionary<RPGCreator.Core.Type.Internal.Point, Autotiling> Autotiles = [];
        public List<AutotilesGroup> Groups = new List<AutotilesGroup>();

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

            if (tile_width > 0 && tile_height > 0)
            {
                return; // Already initialized
            }
            int dim_width = int.Parse(data.Element("tile_width")?.Value ?? "0");
            int dim_height = int.Parse(data.Element("tile_height")?.Value ?? "0");

            if (dim_width <= 0 || dim_height <= 0)
            {
                throw new Exception($"Tile dimensions are invalid.");
            }

            tile_width = dim_width;
            tile_height = dim_height;
        }

        public Tile GetTileAt(int tile_x, int tile_y)
        {
            if (tile_x < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tile_x), "Tile coordinates cannot be negative.");
            }
            if (tile_y < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tile_y), "Tile coordinates cannot be negative.");
            }

            if (tile_x * tile_width >= ImageWidth)
            {
                throw new ArgumentOutOfRangeException(nameof(tile_x), "Tile coordinates are out of bounds.");
            }
            if (tile_y * tile_height >= ImageHeight)
            {
                throw new ArgumentOutOfRangeException(nameof(tile_y), "Tile coordinates are out of bounds.");
            }

            tile_x /= tile_width;
            tile_y /= tile_height;

            Microsoft.Xna.Framework.Rectangle tile_rect = new(tile_x * tile_width, tile_y * tile_height, tile_width, tile_height);

            return new Tile(this, tile_rect);
        }

        public Tile? GetTile(int tile_col, int tile_row)
        {
            var width = Math.Max(ImageWidth, GetBitmap().Size.Width);
            var height = Math.Max(ImageHeight, GetBitmap().Size.Height);

            if (tile_col < 0 || tile_row < 0)
            {
                throw new ArgumentOutOfRangeException("Tile coordinates cannot be negative.");
            }

            if (tile_col * tile_width >= width)
            {
                return null; // Tile is out of bounds
            }
            if (tile_row * tile_height >= height)
            {
                return null; // Tile is out of bounds
            }

            Microsoft.Xna.Framework.Rectangle tile_rect = new(tile_col * tile_width, tile_row * tile_height, tile_width, tile_height);

            return new Tile(this, tile_rect);
        }

        public Autotiling? GetTile(Ulid ID)
        {
            var group = Groups.FirstOrDefault(g => g.HasTile(ID));
            return group?.GetTileById(ID);
        }

        public Autotiling? GetTileAt(RPGCreator.Core.Type.Internal.Point at)
        {
            if(Autotiles.Count != 0 && Autotiles.TryGetValue(at, out var autotile))
            {
                return autotile;
            }
            return Groups.FirstOrDefault(g => g.HasTileAt(at))?.GetTileByPosition(at);
        }
        public bool HasTileAt(RPGCreator.Core.Type.Internal.Point at)
        {
            if (Autotiles.Count != 0 && Autotiles.ContainsKey(at))
            {
                return true;
            }
            return Groups.FirstOrDefault(g => g.HasTileAt(at)) != null;
        }

        public void CombineTilesGroup()
        {
            ClearCombinedTiles();
            foreach (var autotilesGroup in Groups)
            {
                foreach (var tile in autotilesGroup.Tilings)
                {
                    if (tile.TilesetID == Unique)
                    {
                        Autotiles[tile.TilePosition] = tile;
                    }
                }
            }
        }

        private void ClearCombinedTiles()
        {
            Autotiles.Clear();
        }

        public SerializationInfo GetObjectData()
        {
            SerializationInfo info = new SerializationInfo(typeof(Tileset));
            info.AddValue("unique", Unique);
            info.AddValue("type", Type);
            info.AddValue("name", Name);
            info.AddValue("file_path", ImagePath);
            info.AddValue("tile_width", tile_width);
            info.AddValue("tile_height", tile_height);
            return info;
        }

        public void SetObjectData(SerializationInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
            }

            info.TryGetValue("unique", out Ulid unique, Ulid.Empty, "Unique identifier not found or invalid.");
            info.TryGetValue("type", out Type, TYPE.TILESETS, "Type not found or invalid (Set to Tileset by default).");
            info.TryGetValue("name", out string name, "Unnamed Tileset", "Name not found or invalid (Set to 'Unnamed Tileset' by default).");
            info.TryGetValue("file_path", out string imagePath, string.Empty, "File path not found or invalid (Set to empty string by default).");
            info.TryGetValue("tile_width", out tile_width, 32, "Tile width not found or invalid (Set to 32 by default).");
            info.TryGetValue("tile_height", out tile_height, 32, "Tile height not found or invalid (Set to 32 by default).");
            
            Unique = unique;
            Name = name;
            ImagePath = imagePath;
        }
    }
}
