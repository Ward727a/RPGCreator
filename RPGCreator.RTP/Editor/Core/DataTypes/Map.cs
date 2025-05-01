using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.MonoGame.Editor.Core.DataTypes
{
    public class Map
    {

        const UInt32 MapMagic = 0xA1B300F1;

        private SpriteBatch _SB;

        public Guid Guid { get; private set; } = Guid.NewGuid();
        public string Name { get; set; } = "Default map";

        public List<MapLayer> Layers { get; private set; } = [];

        public EditorGame? game;

        public List<Texture2D> UsedTilesets { get; private set; } = [];

        public Map(SpriteBatch sb) { 
            _SB = sb;
        }

        public Map(SpriteBatch sb, string name)
        {
            _SB = sb;
            Name = name;
        }

        public void AddLayer(string layerName = "Default Layer")
        {
            MapLayer layer = new(layerName);

            int current_z_index = 0;

            foreach(MapLayer childLayer in Layers)
            {
                if(childLayer.ZIndex >= current_z_index)
                {
                    current_z_index = childLayer.ZIndex + 1;
                }

            }

            layer.ZIndex = current_z_index;

            Layers.Add(layer);
        }

        public void DrawMap()
        {
            Layers.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex));
            foreach (MapLayer layer in Layers)
            {
                foreach(KeyValuePair<Point, MapLayer.TileData> tile in layer.Tiles)
                {
                    if (game?.UsedTilesets.Count <= tile.Value.Tileset)
                        break;

                    _SB.Draw(game?.UsedTilesets[tile.Value.Tileset].Texture, tile.Key.ToVector2(), tile.Value.UV, Color.White);
                }
            }
        }

        //public void LoadTileset(Texture2D newTileset)
        //{
        //    if (UsedTilesets.Contains(newTileset)) return;
        //    UsedTilesets.Add(newTileset);
        //}

        struct UVTileBIN // UV data for 1 tile
        {
            public byte X; // 8 Bits 0000 0000
            public byte Y; // 8 Bits 0000 0000
        } // Total: 16 Bits

        struct MapTileBinData // full data for 1 tile
        {
            public ushort X; // 16 Bits 0000 0000 0000 0000 // 2o
            public ushort Y; // 16 Bits 0000 0000 0000 0000 // 2o
            public UVTileBIN TileID; // 16 Bits 0000 0000 0000 0000 // 2o
            public byte TilesetID; // 8 Bits 0000 0000 // 1o
            public byte RTL; // 8 Bits 0000 0000 // 1o
        } // Total: 64 Bits | 8 octets

        public void SaveMap(string path)
        {

            string filename = $"{Name}.cmap";
            string filepath = Path.Combine(path, filename);

            if(File.Exists(filepath))
            {
                if (File.Exists(Path.Combine(path, $"{Name}.cmap.bak")))
                    File.Delete(Path.Combine(path, $"{Name}.cmap.bak"));
                File.Copy(filepath, Path.Combine(path, $"{Name}.cmap.bak"));
            }

            File.Delete(filepath);

            MemoryStream tempSaveFile = new();
            FileStream saveFile = File.OpenWrite(filepath);

            BinaryWriter saveWriter = new(saveFile);
            BinaryWriter tempWriter = new(tempSaveFile);

            List<uint> layersSize = [];

            foreach (MapLayer layer in Layers)
            {

                uint totalLayerSize = 0;

                // Write layer header

                //writer.Write((byte)0xFF);
                //writer.Write((byte)0xFF);
                //writer.Write((byte)0xFF);
                //writer.Write((byte)0xFF);
                //writer.Write((byte)0x01); // For now this is a template, but it could be used to define different type
                //writer.Write((byte)0xFF);
                //writer.Write((byte)0xFF);
                //writer.Write((byte)0xFF);


                MapTileBinData tileBinCurrent = new MapTileBinData();

                byte current_repeat = 0;
                MapLayer.TileData current_tile = new();

                int current_y = 0;
                int last_x = 0;
                MapLayer.TileData value = new();
                Point point = new();

                var sortedDict = layer.Tiles
                    .OrderBy(kvp => kvp.Key.Y)
                    .ThenBy(kvp => kvp.Key.X)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                uint dataStart = (uint)tempSaveFile.Position;

                foreach (KeyValuePair<Point, MapLayer.TileData> tile in sortedDict)
                {
                    value = tile.Value;
                    point = tile.Key;

                    // If the new tile is:
                    //      ON SAME HEIGHT   &&   ON THE RIGHT SIDE   &&                          SAME TILE DATA
                    if (current_y == point.Y && last_x == point.X / value.Dimension - 1 && (value.UV == current_tile.UV && value.Tileset == current_tile.Tileset) && current_repeat != 255)
                    {
                        // Then we add 1 repeat
                        current_repeat++;
                        last_x = point.X / value.Dimension;
                        continue;
                    }
                    // ELSE
                    // First we save the data if the repeat is more than 0 (This is 0 only for the first tile of each layer)
                    if (current_repeat >= 1)
                    {
                        tileBinCurrent.RTL = current_repeat;
                        tileBinCurrent.X = (ushort)point.X;
                        tileBinCurrent.Y = (ushort)point.Y;
                        tileBinCurrent.TileID = new() { X = (byte)(point.X / value.Dimension), Y = (byte)(point.Y / value.Dimension) };
                        tileBinCurrent.TilesetID = (byte)value.Tileset;
                        tempWriter.Write(tileBinCurrent.X);
                        tempWriter.Write(tileBinCurrent.Y);
                        tempWriter.Write(tileBinCurrent.TileID.X);
                        tempWriter.Write(tileBinCurrent.TileID.Y);
                        tempWriter.Write(tileBinCurrent.TilesetID);
                        tempWriter.Write(tileBinCurrent.RTL);
                        totalLayerSize += 8;
                    }
                    // We reset the repeat, and start creating the next one
                    current_repeat = 1;
                    current_y = point.Y;
                    last_x = point.X / value.Dimension;
                    current_tile = value;
                }

                if (current_repeat >= 1)
                {
                    tileBinCurrent.RTL = current_repeat;
                    tileBinCurrent.X = (ushort)point.X;
                    tileBinCurrent.Y = (ushort)point.Y;
                    tileBinCurrent.TileID = new() { X = (byte)(value.UV.X / value.Dimension), Y = (byte)(value.UV.Y / value.Dimension) };
                    tileBinCurrent.TilesetID = (byte)value.Tileset;

                    tempWriter.Write(tileBinCurrent.X);
                    tempWriter.Write(tileBinCurrent.Y);
                    tempWriter.Write(tileBinCurrent.TileID.X);
                    tempWriter.Write(tileBinCurrent.TileID.Y);
                    tempWriter.Write(tileBinCurrent.TilesetID);
                    tempWriter.Write(tileBinCurrent.RTL);
                    totalLayerSize += 8;
                }

                layersSize.Add((uint)tempSaveFile.Position - dataStart);
            }

            saveWriter.Write(MapMagic);

            int index = 0;
            foreach(uint layerSize in layersSize)
            {
                saveWriter.Write(layerSize);
                saveWriter.Write(Layers[index].Name);
                if (index >= layersSize.Count - 1)
                    saveWriter.Write(0xFFFFFFFE); // End Delimiter
                else
                    saveWriter.Write(0xFFFFFFFF); // Delimiter
                index++;
            }

            tempSaveFile.Seek(0, SeekOrigin.Begin);
            tempSaveFile.CopyTo(saveFile);

            tempWriter.Close();
            saveWriter.Close();
        }

        public void LoadMap(string path, string mapName, bool backup = false)
        {

            string filename = $"{Name}.cmap";

            if (backup)
                filename = $"{Name}.cmap.bak";

            string filepath = Path.Combine(path, filename);

            if (!File.Exists(filepath))
                return;

            FileStream fs = File.OpenRead(filepath);

            long filesize = fs.Length;

            BinaryReader br = new(fs);

            UInt32 magic = br.ReadUInt32();

            if (magic != MapMagic)
            {
                throw new Exception("Map magic not matching.");
            }

            uint headersSize = 0;
            List<uint> layersSize = new List<uint>();

            // We get the header part
            while(br.BaseStream.Position < br.BaseStream.Length)
            {
                uint size = br.ReadUInt32();
                long startPosition = br.BaseStream.Position;
                string name = br.ReadString();
                long endPosition = br.BaseStream.Position;
                uint delimiter = br.ReadUInt32();
                long stringByteLength = endPosition - startPosition;

                if (delimiter == 0xFFFFFFFE)
                {
                    layersSize.Add(size);
                    AddLayer(name);
                    headersSize += 8 + (uint)stringByteLength;
                    break;
                }
                else if (delimiter != 0xFFFFFFFF)
                {
                    throw new Exception("Invalid delimiter.");
                }

                AddLayer(name);
                layersSize.Add(size);
                headersSize += 8 + (uint)stringByteLength;
            }

            // We check if the size data is valid.
            long expectedDataSize = layersSize.Sum(s => (long)s);
            long expectedTotalSize = headersSize + expectedDataSize + 4 /*Magic number size*/;

            if (filesize != expectedTotalSize)
            {
                throw new Exception($"File size isn't valid! Size = {filesize}, Expected = {expectedTotalSize}.");
            }

            List<List<MapTileBinData>> BinaryLayers = [];
            foreach (uint layer in layersSize)
            {
                uint currentLayerSize = 0;
                List<MapTileBinData> BinaryLayer = [];
                while (currentLayerSize < layer)
                {
                    currentLayerSize += 8;

                    // First we create each structure
                    UVTileBIN UVBin = new UVTileBIN();
                    MapTileBinData MapBin = new MapTileBinData();

                    MapBin.X = br.ReadUInt16();
                    MapBin.Y = br.ReadUInt16();

                    UVBin.X = br.ReadByte();
                    UVBin.Y = br.ReadByte();
                    MapBin.TileID = UVBin;

                    MapBin.TilesetID = br.ReadByte();
                    MapBin.RTL = br.ReadByte();
                    BinaryLayer.Add(MapBin);
                }
                BinaryLayers.Add(BinaryLayer);
            }

            int index = 0;
            foreach (List<MapTileBinData> layer_data in BinaryLayers)
            {
                if(Layers.Count > index)
                {

                    foreach(MapTileBinData Bin in layer_data)
                    {
                        for (int i = 0; i < Bin.RTL; i++)
                        {

                            int dim = game.UsedTilesets[Bin.TilesetID].Dimension;

                            int X = Bin.X + (dim * i);

                            Layers[index].AddTile(new Point(X, Bin.Y), Bin.TilesetID, new Rectangle(Bin.TileID.X* dim, Bin.TileID.Y* dim, dim, dim), dim);
                        }
                    }
                    index++;
                }
            }
        }
    }
}
