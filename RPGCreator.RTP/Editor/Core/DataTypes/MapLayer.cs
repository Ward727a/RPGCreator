using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.MonoGame.Editor.Core.DataTypes
{
    public class MapLayer
    {
        public int ZIndex = -1;
        public string Name { get; set; }
        public Dictionary<Point, TileData> Tiles = [];

        public MapLayer(string name)
        {
            Name = name;
        }

        public struct TileData
        {
            public int Tileset;
            public Rectangle UV;
            public int Dimension;

            public TileData(int tileset, Rectangle uv, int dimension)
            {
                Tileset = tileset;
                UV = uv;
                Dimension = dimension;
            }

        }

        public void AddTile(Point position, TileData tile)
        {
            Tiles[position] = tile;
        }

        public void AddTile(Point position, int tileset, Rectangle uv, int dimension)
        {
            Tiles[position] = new(tileset, uv, dimension);
        }

        public void RemoveTile(Point position)
        {
            Tiles.Remove(position);
        }
    }
}
