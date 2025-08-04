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

using RPGCreator.Core.Type.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Media.Imaging;
using MonoGame.Extended.Tiled;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;
using ITileset = RPGCreator.Core.Type.Interfaces.ITileset;
using Point = RPGCreator.Core.Type.Internal.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace RPGCreator.Core.Type.Assets
{


    [Serializable]
    public class Autotile_rule() : ISerializable, IDeserializable
    {
        public Ulid ID { get; private set; } = Ulid.NewUlid();
        public string Name = "";
        public string Description = "";
        public ERuleType Type = ERuleType.WHITELIST;
        public ERulePos Side = ERulePos.TOP_LEFT; // Default position, can be changed later
        public List<string> Tags = [];

        public SerializationInfo GetObjectData()
        {
            SerializationInfo info = new SerializationInfo(typeof(Autotile_rule));
            info.AddValue("ID", ID);
            info.AddValue("Name", Name);
            info.AddValue("Description", Description);
            info.AddValue("Type", Type);
            info.AddValue("Side", Side);
            info.AddValue("Tags", Tags);
            return info;
        }

        public void SetObjectData(SerializationInfo info)
        {
            info.TryGetValue("ID", out Ulid ID);
            info.TryGetValue("Name", out Name);
            info.TryGetValue("Description", out Description);
            info.TryGetValue("Type", out Type);
            info.TryGetValue("Side", out Side);
            info.TryGetList("Tags", out Tags);
            
            this.ID = ID;
        }
    }

    public class Autotiling() : ISerializable, IDeserializable
    {
        public RPGCreator.Core.Type.Internal.Point TilePosition = new RPGCreator.Core.Type.Internal.Point(0, 0);
        public AutotilesGroup? Group;
        public Ulid? TilesetID;
        public Ulid ID = Ulid.NewUlid();
        public bool IsBase = false;
        public Autotiling? BasedOn = null;
        public List<string> Tags = [];
        public List<Autotile_rule> Rules = new();

        public Autotiling(bool isBase = false, Autotiling? basedOn = null) : this()
        {
            IsBase = isBase;
            BasedOn = basedOn;
        }

        public void AddRule(ERulePos pos, ERuleType type, List<string> awaitedTags)
        {
            Rules.Add(new Autotile_rule { Side = pos, Type = type, Tags = awaitedTags });
        }

        public void AddRule(Autotile_rule rule)
        {
            Rules.Add(rule);
        }

        public Autotile_rule GetRule(ERulePos pos)
        {
            return Rules.FirstOrDefault(e => e.Side == pos);
        }

        public bool HasRule(ERulePos pos)
        {
            return Rules.Any(e => e.Side == pos);
        }

        public bool RespectRules(TileLayer layer, Point position, out List<Tile> tilesToAlert)
        {

            List<bool> tags = [];
            tilesToAlert = [];
            foreach (var rule in Rules)
            {

                var isWhitelist = rule.Type == ERuleType.WHITELIST;
                
                // Check the rule based on the position
                // For this we need to get the position of the tile in the layer
                // And add the offset based on the rule position
                // So: TopLeft = position + new Point(1, 1), Top = position + new Point(0, 1), etc.
                Point offset = rule.Side switch
                {
                    ERulePos.TOP_LEFT => new Point(-1, -1),
                    ERulePos.TOP => new Point(0, -1),
                    ERulePos.TOP_RIGHT => new Point(1, -1),
                    ERulePos.LEFT => new Point(-1, 0),
                    ERulePos.RIGHT => new Point(1, 0),
                    ERulePos.BOTTOM_LEFT => new Point(-1, 1),
                    ERulePos.BOTTOM => new Point(0, 1),
                    ERulePos.BOTTOM_RIGHT => new Point(1, 1),
                    _ => throw new ArgumentOutOfRangeException(nameof(rule.Side), "Invalid rule position")
                };
                
                if(Group == null)
                    throw new InvalidOperationException("Autotiling group is not set. Cannot respect rules without a group.");
                
                // Multiply the offset by the layer tile size
                offset.X *= Group.Tileset.tile_width;
                offset.Y *= Group.Tileset.tile_height;
                
                Point rulePosition = position + offset;
                
                // Now we need to check if the tile at the rule position has the tags we are looking for
                // if (layer.TryGetTileAt(rulePosition, out Tile? tile))
                // {
                //
                //     if (!tile.IsAutotiling)
                //     {
                //         if (!isWhitelist)
                //         {
                //             tags.Add(true);
                //         }
                //         continue; // If the tile is not an autotiling, we skip it
                //     }
                //     
                //     var autotilingData = tile.Autotiling;
                //     
                //     // If the rule is a whitelist, we check if the tile has the tags we are looking for
                //     if (isWhitelist)
                //     {
                //         // If the tile has all the tags we are looking for, we return true
                //         if (rule.Tags.All(tag => autotilingData.Tags.Contains(tag)))
                //         {
                //             tags.Add(true);
                //             tilesToAlert.Add(tile);
                //             continue;
                //         }
                //         tags.Add(false);
                //     }
                //     else
                //     {
                //         // If the rule is a blacklist, we check if the tile has any of the tags we are looking for
                //         if (rule.Tags.Any(tag => autotilingData.Tags.Contains(tag)))
                //         {
                //             tags.Add(false); // If it has any of the tags, we return false
                //             continue;
                //         }
                //         tags.Add(true);
                //         tilesToAlert.Add(tile);
                //     }
                //     continue;
                // }
                //
                // if (!isWhitelist)
                // {
                //     tags.Add(true);
                //     continue;
                // }
                // tags.Add(false);
                
            }

            if(tags.All(b => b == true))
            {
                return true; // All rules respected
            }

            return false;
        }

        /// <summary>
        /// A method to refresh the autotiling, this is used to update the appearance of the autotiling based on the rules and tags.<br/>
        /// Example: If the autotiling has a new tile added on the left, it will refresh the autotiling to update the appearance of the tile based on the rules and tags.<br/>
        /// </summary>
        public void RefreshAutotiling(TileLayer layer, Point position)
        {
            if (RespectRules(layer, position, out _))
                return;
            
            // If the autotiling does not respect the rules, we need to update the autotiling appearance
            var newTile = Group.GetTileByRule(layer, position, out _);


            // if (layer.TryGetTileAt(position, out var tile))
            // {
            //     if (tile == null)
            //         return;
            //     
            //     layer.RemoveTile(tile);
            // }
            //
            // if (newTile == null)
            // {
            //     // If no tile is found, we can just return
            //     return;
            // }
            //
            // layer.AddTileAt(new Tile(
            //     Group.Tileset,
            //     new Rectangle(
            //         newTile.TilePosition.X * Group.Tileset.tile_width,
            //         newTile.TilePosition.Y * Group.Tileset.tile_height,
            //         Group.Tileset.tile_width,
            //         Group.Tileset.tile_height
            //     )
            // )
            // {
            //     Autotiling = newTile
            // }, position);
            
        }

        public bool IsValid()
        {
            return (IsBase || BasedOn != null);
        }

        public override string ToString()
        {
            return $"{(IsBase ? "Base" : "Autotile")} - {ID}";
        }

        public void SetObjectData(SerializationInfo info)
        {
            
            if(info.ObjectType != typeof(Autotiling))
                throw new ArgumentException("Invalid serialization info type. Expected Autotiling.", nameof(info));
            
            info.TryGetValue("ID", out ID, Ulid.Empty, "Field 'ID' not found in the serialization info.");
            info.TryGetValue("TilePosition", out TilePosition, new RPGCreator.Core.Type.Internal.Point(0, 0), "Field 'TilePosition' not found in the serialization info.");
            info.TryGetValue("IsBase", out IsBase, false, "Field 'IsBase' not found in the serialization info.");
            info.TryGetValue("BasedOnID", out BasedOn, null, "Field 'BasedOnID' not found in the serialization info.");
            info.TryGetList("Tags", out Tags, new List<string>(), "Field 'Tags' not found in the serialization info.");
            info.TryGetList("Rules", out Rules, new List<Autotile_rule>(), "Field 'Rules' not found in the serialization info.");
        }

        public SerializationInfo GetObjectData()
        {
            SerializationInfo info = new SerializationInfo(typeof(Autotiling));
            info.AddValue("ID", ID);
            info.AddValue("TilePosition", TilePosition);
            info.AddValue("IsBase", IsBase);
            info.AddValue("BasedOnID", BasedOn?.ID ?? Ulid.Empty);
            info.AddValue("Tags", Tags);
            info.AddValue("Rules", Rules);
            return info;
        }

        // Pour ça j'aurais besoin de reworker la MapLayer et le Tile pour qu'ils puissent gérer les règles et les tags par défaut d'autotiling
        // Probablement besoin d'aussi retravailler le système de Tileset (genre ajouter un truc Dictionary<Point, List<Tags>> pour pouvoir appliquer les tags par défaut sur les tiles quand on les ajoute)
        // public bool RespectRules(MapLayer layer, Point position)
        // {
        //     foreach (var rule in Rules)
        //     {
        //         if(layer.TryGetTileAt(position + GetPoint(rule.Key), out Tile? tile))
        //         {
        //
        //         }
        //     }
        //     return true; // All rules respected
        // }

    }

    public class AutotilesGroup() : ISerializable, IDeserializable
    {
        public Ulid ID = Ulid.NewUlid();
        private Ulid _tilesetId = Ulid.Empty; // This is used to store the tileset ID, so we can retrieve it later when the project is loaded.
        
        public string Name;
        public List<Autotiling> Tilings = [];
        public List<string> Tags = []; // Group tags, those are applied to all autotilings in the group (like a "global" tag).
        public Dictionary<string, List<Ulid>> PresentTags = []; // Tags that are present in the group, used to quickly get the list of tags, so we don't have to iterate through all autotilings to get the tags.
        public Autotiling? BaseTile;
        public Tileset Tileset; // For a later version, we might want to have a group of tileset, but for now, we only have one tileset per group (for simplicity).
        public AutotilesGroup(string name, Tileset tileset, Autotiling? baseTile = null) : this()
        {
            Name = name;
            Tileset = tileset ?? throw new ArgumentNullException(nameof(tileset), "Tileset cannot be null");
            BaseTile = baseTile;
            _tilesetId = tileset.Unique;
        }

        public void SetBase(Autotiling autotiling)
        {
            if (BaseTile == autotiling)
                return;
            
            if (!HasTile(autotiling.ID))
            {
                Tilings.Add(autotiling);
            }

            if (BaseTile != null)
            {
                BaseTile.IsBase = false;
                BaseTile.BasedOn = autotiling;

                foreach (var tile in Tilings)
                {
                    tile.BasedOn = autotiling;
                }
                
            }
            
            BaseTile = autotiling;
            autotiling.IsBase = true;
            autotiling.BasedOn = null;
        }

        public Autotiling CreateNewTile(bool isBase = false)
        {
            var tile = new Autotiling(false, BaseTile);

            if (isBase)
            {
                SetBase(tile);
            }
            
            Tilings.Add(tile);
            tile.Group = this;

            return tile;
        }
        
        public void RemoveTile(Autotiling autotiling)
        {
            if (autotiling.IsBase)
            {
                throw new InvalidOperationException("Cannot remove a base tile. Set another tile as base first.");
            }
            
            if(!HasTile(autotiling.ID))
                throw new InvalidOperationException("Tile not found in the group.");

            Tilings.Remove(autotiling);
        }
        
        public void AddTile(Autotiling autotiling)
        {
            autotiling.Group = this;
            Tilings.Add(autotiling);
        }
        
        public bool HasTile(Ulid id)
        {
            return Tilings.Any(a => a.ID == id);
        }
        public bool HasTileAt(Point at)
        {
            return Tilings.Any(t => t.TilePosition.X == at.X && t.TilePosition.Y == at.Y);
        }
        
        public Autotiling? GetTileById(Ulid id)
        {
            return Tilings.FirstOrDefault(a => a.ID == id);
        }
        public Autotiling? GetTileByPosition(RPGCreator.Core.Type.Internal.Point position)
        {
            return Tilings.FirstOrDefault(a => a.TilePosition.X == position.X && a.TilePosition.Y == position.Y);
        }
        public Autotiling? GetTileByTile(Tile tile)
        {
            return Tilings.FirstOrDefault(a => a.TilePosition.X == tile.Position.X && a.TilePosition.Y == tile.Position.Y);
        }

        /// <summary>
        /// This method retrieves the correct autotiling based on the rules defined in the autotiling group.
        /// </summary>
        /// <param name="layer">The layer where the tile will be added (will be used to compare the rule)</param>
        /// <param name="position">The position where the till will be added</param>
        /// <returns></returns>
        public Autotiling? GetTileByRule(TileLayer layer, Point position, out List<Tile> tilesToAlert)
        {
            tilesToAlert = [];
            int lastNumberOfChecks = -1;
            Autotiling? lastTiling = null;
            // We need to check the rules of the autotilings in the group
            foreach (var autotiling in Tilings)
            {
                // If the autotiling is a base tile, we skip it
                if (autotiling.IsBase)
                    continue;

                // Check if the autotiling respects the rules
                if (autotiling.RespectRules(layer, position, out tilesToAlert))
                {
                    if(lastNumberOfChecks < autotiling.Rules.Count)
                    {
                        lastNumberOfChecks = autotiling.Rules.Count;
                        lastTiling = autotiling;
                    }
                }
            }
            
            if(lastTiling != null)
            {
                // If we found a valid autotiling, return it
                return lastTiling;
            }
            
            // If the base tile is set, return it
            if (BaseTile != null && BaseTile.IsValid())
            {
                return BaseTile;
            }

            return null;
        }
        
        //public Autotiling? GetAutotilingByTile(Tile tile)
        //{
        //    return Autotilings.FirstOrDefault(a => a.Tile.ID == tile.ID);
        //}
        //public Autotiling? GetAutotilingById(Ulid id)
        //{
        //    return Autotilings.FirstOrDefault(a => a.Tile.ID == id);
        //}
        public Autotiling? GetBaseTile()
        {
            return BaseTile;
        }
        public List<Autotiling> GetAutotilingsByTag(string tag)
        {
            return Tilings.Where(a => a.Tags.Contains(tag)).ToList();
        }
        public bool HasBaseTile()
        {
            return BaseTile != null && BaseTile.IsValid();
        }

        public bool IsValid()
        {
            return HasBaseTile() && Tilings.Count > 0;
        }

        public void CombineGroupTags()
        {
            foreach (var autotiling in Tilings)
            {
                autotiling.Tags.AddRange(Tags);
            }
        }
        
        public void UncombineGroupTags()
        {
            foreach (var autotiling in Tilings)
            {
                autotiling.Tags.RemoveAll(tag => Tags.Contains(tag));
            }
        }

        public SerializationInfo GetObjectData()
        {
            SerializationInfo info = new SerializationInfo(typeof(AutotilesGroup));
            info.AddValue("ID", ID);
            info.AddValue("Name", Name);
            info.AddValue("TilesetID", _tilesetId);
            info.AddValue("BaseTileID", BaseTile?.ID ?? Ulid.Empty);
            info.AddValue("Tags", Tags);
            info.AddValue("Tilings", Tilings);
            return info;
        }

        public void SetObjectData(SerializationInfo info)
        {
            if(info.ObjectType != typeof(AutotilesGroup))
                throw new ArgumentException("Invalid serialization info type. Expected AutotilesGroup.", nameof(info));
            
            info.TryGetValue("ID", out ID, Ulid.Empty, "Field 'ID' not found in the serialization info.");
            info.TryGetValue("Name", out Name, string.Empty, "Field 'Name' not found in the serialization info.");
            info.TryGetValue("TilesetID", out _tilesetId, Ulid.Empty, "Field 'TilesetID' not found in the serialization info.");
            info.TryGetValue("BaseTileID", out Ulid baseTileId, Ulid.Empty, "Field 'BaseTileID' not found in the serialization info.");
            info.TryGetList("Tags", out Tags, new List<string>(), "Field 'Tags' not found in the serialization info.");
            info.TryGetList("Tilings", out Tilings, new List<Autotiling>(), "Field 'Tilings' not found in the serialization info.");
            
            foreach (var autotiling in Tilings)
            {
                autotiling.Group = this; // Set the group for each autotiling
            }

            void OnEditedProjectOnOnProjectLoaded()
            {
                Tileset = EngineCore.Instance.Data.EditedProject.GetAssetsType<Tileset>(BaseAsset.TYPE.TILESETS)
                    .Find(t => t.Unique == _tilesetId) ?? throw new InvalidOperationException($"Tileset with ID {_tilesetId} not found.");

                BaseTile = GetTileById(baseTileId);
                
                EngineCore.Instance.Data.EditedProject.OnProjectLoaded -= OnEditedProjectOnOnProjectLoaded;
            }

            EngineCore.Instance.Data.EditedProject.OnProjectLoaded += OnEditedProjectOnOnProjectLoaded;
        }
    }

    public class Autotiles : ImageAsset, ISerializable, IDeserializable
    {
        public List<AutotilesGroup> Autotilings = [];
        private Bitmap? _bitmap = null;

        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        
        private Dictionary<Point, int> _baseTilesOwner = [];
        
        public Autotiles() : base()
        {
            Type = TYPE.AUTOTILES;
        }

        public Autotiles(string Name) : base(Name)
        {
            Type = TYPE.AUTOTILES;
        }

        public Autotiles(string Name, List<AutotilesGroup> autotilings) : base(Name)
        {
            Type = TYPE.AUTOTILES;
            Autotilings = autotilings;
        }

        public void AddGroup(AutotilesGroup autotiling)
        {
            Autotilings.Add(autotiling);
        }

        /// <summary>
        /// This method generates an image preview of the autotiles.<br/>
        /// This preview image is used to display the autotiles base tile in the editor.<br/>
        /// Then, it can be used to select tile in the map editor to draw them.
        /// </summary>
        /// <returns></returns>
        public string GeneratePreviewImage()
        {
            int MaxWidth = 256;
            int width = 0;
            int height = 0;
            
            if(string.IsNullOrEmpty(ImagePath))
                ImagePath = Path.Combine(Pack.AssetsFolder, $"{Unique}.png");
            
            List<AutotilesGroup> groups = new List<AutotilesGroup>();
            foreach (var group in Autotilings)
            {
                var baseTile = group.GetBaseTile();

                if (baseTile == null || !baseTile.IsValid()) continue;
                groups.Add(group);

                Tileset tileset = group.Tileset;

                height = Math.Max(height, tileset.tile_height);
                if (width + tileset.tile_width > MaxWidth)
                {
                    // If the width exceeds the maximum width, we need to reset the current X position and increase the Y position
                    height += tileset.tile_height;
                }
                else
                {
                    width += tileset.tile_width;
                }
            }
            
            // Image<Rgba64> image = new Image<Rgba64>(width, height);
            int currentY = 0;
            int currentX = 0;
            int index = 0;
            
            using var finalBitmap = new SKBitmap(width, height);
            
            
            using (var canvas = new SKCanvas(finalBitmap))
            {
                canvas.Clear(SKColors.Transparent);
            
                foreach (var group in groups)
                {
                    var baseTile = group.GetBaseTile();
                    if (baseTile == null || !baseTile.IsValid()) continue;

                    // Draw the base tile on the image
                    var tileset = group.Tileset;
                    var tilesetImage = tileset.GetBitmap();
                    var rect = new PixelRect(
                        baseTile.TilePosition.X * tileset.tile_width,
                        baseTile.TilePosition.Y * tileset.tile_height,
                        tileset.tile_width,
                        tileset.tile_height
                    );
                    Stream xxStream = new MemoryStream();
                    tilesetImage.Save(xxStream);
                    xxStream.Seek(0, SeekOrigin.Begin);
                    var skBitmapSource = SKBitmap.Decode(xxStream);

                    var source = new SKRect(rect.X, rect.Y, rect.Width+ rect.X, rect.Height+ rect.Y);
                    var dest = new SKRect(currentX, currentY, rect.Width + currentX , rect.Height + currentY);

                    canvas.DrawBitmap(skBitmapSource, source, dest);
                    
                    _baseTilesOwner[new Point(currentX, currentY)] = index;
                    
                    // Update the current position

                    if(currentX + tileset.tile_width >= MaxWidth)
                    {
                        // If the width exceeds the maximum width, we need to reset the current X position and increase the Y position
                        currentX = 0;
                        currentY += tileset.tile_height;
                    }
                    else
                    {
                        currentX += tileset.tile_width;
                    }

                    index++;

                }
            }

            // Sauvegarder en PNG
            try
            {
                using var rimage = SKImage.FromBitmap(finalBitmap);
                using var data = rimage.Encode(SKEncodedImageFormat.Png, 100);
                using var fs = File.OpenWrite(ImagePath);
                data.SaveTo(fs);
                Console.WriteLine($"Preview image generated successfully at {ImagePath}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating preview image: {ex.Message}");
                return "Error generating preview image.";
            }

            return ImagePath;
        }

        public override Bitmap GetBitmap(bool forceReload = false)
        {
            if (_bitmap != null && !forceReload) return _bitmap;
            
            if (string.IsNullOrEmpty(ImagePath) || forceReload)
            {
                GeneratePreviewImage();
            }

            if (!File.Exists(ImagePath))
            {
                throw new FileNotFoundException($"Preview image not found at {ImagePath}");
            }
            
            _bitmap = new Bitmap(ImagePath);

            return _bitmap;
        }

        public SerializationInfo GetObjectData()
        {
            SerializationInfo info = new SerializationInfo(typeof(Autotiles));
            info.AddValue("Unique", Unique);
            info.AddValue("Name", Name);
            info.AddValue("Type", Type);
            info.AddValue("Autotilings", Autotilings);
            info.AddValue("ImagePath", ImagePath);
            return info;
        }

        public void SetObjectData(SerializationInfo info)
        {
            if(info.ObjectType != typeof(Autotiles))
                throw new ArgumentException("Invalid serialization info type. Expected Autotiles.", nameof(info));

            info.TryGetValue("Unique", out var unique, Ulid.Empty, "Field 'Unique' not found in the serialization info.");
            info.TryGetValue("Name", out var name, string.Empty, "Field 'Name' not found in the serialization info.");
            info.TryGetValue("Type", out TYPE type, TYPE.UNKNOWN, "Field 'Type' not found in the serialization info.");
            info.TryGetValue("ImagePath", out var imagePath, string.Empty, "Field 'ImagePath' not found in the serialization info.");
            ImagePath = imagePath;
            Unique = unique;
            Name = name;
            Type = type;
            info.TryGetList("Autotilings", out Autotilings, new List<AutotilesGroup>(), "Field 'Autotilings' not found in the serialization info.");
        }

        public Tile? GetTile(int row, int column)
        {
            var autilingIndex = _baseTilesOwner[new Point(row, column)];
            if (autilingIndex < 0 || autilingIndex >= Autotilings.Count)
            {
                return null; // Index out of range
            }
            var autotiling = Autotilings[autilingIndex];
            if (autotiling.BaseTile == null || !autotiling.BaseTile.IsValid())
            {
                return null; // No valid base tile found
            }

            var pos = autotiling.GetBaseTile()?.TilePosition ?? new  Point(0, 0);
            
            return autotiling.Tileset.GetTileAt(pos.X, pos.Y);
        }

        public Tile? GetTileAt(Point at)
        {
            if (at.X < 0 || at.Y < 0)
            {
                return null; // Invalid position
            }

            int row = at.X / TileWidth;
            int column = at.Y / TileHeight;

            return GetTile(row, column);
        }

        public bool HasTile(int row, int column)
        {
            if (row < 0 || column < 0)
            {
                return false; // Invalid position
            }

            var point = new Point(row, column);
            return _baseTilesOwner.ContainsKey(point) && _baseTilesOwner[point] >= 0 && _baseTilesOwner[point] < Autotilings.Count;
        }
    }
}
