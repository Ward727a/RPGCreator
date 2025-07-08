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
using RPGCreator.Core.Type.Internal;
using RPGCreator.Core.Type.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.Core.Type.Assets
{

    public enum ERulePos
    {
        TOP_LEFT,
        TOP,
        TOP_RIGHT,
        LEFT,
        RIGHT,
        BOTTOM_LEFT,
        BOTTOM,
        BOTTOM_RIGHT,
    }

    public enum ERuleType
    {
        WHITELIST,
        BLACKLIST,
    }

    public struct Autotile_rule()
    {
        public ERuleType Type = ERuleType.WHITELIST;
        public List<string> AwaitedTag = [];
    }

    public class Autotiling
    {
        public RPGCreator.Core.Type.Internal.Point TilePosition = new RPGCreator.Core.Type.Internal.Point(0, 0);
        public AutotilesGroup? Group;
        public Ulid? TilesetID;
        public Ulid ID = Ulid.NewUlid();
        public bool IsBase = false;
        public Autotiling? BasedOn = null;
        public List<string> Tags = [];
        public Dictionary<ERulePos, Autotile_rule> Rules = new();

        public Autotiling(bool isBase = false, Autotiling? basedOn = null)
        {
            IsBase = isBase;
            BasedOn = basedOn;
        }

        public void AddRule(ERulePos pos, ERuleType type, List<string> awaitedTags)
        {
            Rules[pos] = new Autotile_rule { Type = type, AwaitedTag = awaitedTags };
        }

        public void AddRule(ERulePos pos, Autotile_rule rule)
        {
            Rules[pos] = rule;
        }

        public Autotile_rule GetRule(ERulePos pos)
        {
            return Rules.ContainsKey(pos) ? Rules[pos] : new Autotile_rule();
        }

        public bool HasRule(ERulePos pos)
        {
            return Rules.ContainsKey(pos);
        }

        public bool IsValid()
        {
            return (IsBase || BasedOn != null);
        }

        public override string ToString()
        {
            return $"{(IsBase ? "Base" : "Autotile")} - {ID}";
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

    public class AutotilesGroup
    {
        public Ulid ID = Ulid.NewUlid();
        public string Name;
        public List<Autotiling> Tilings = [];
        public List<string> Tags = []; // Group tags, those are applied to all autotilings in the group (like a "global" tag).
        public Dictionary<string, List<Ulid>> PresentTags = []; // Tags that are present in the group, used to quickly get the list of tags, so we don't have to iterate through all autotilings to get the tags.
        public Autotiling? BaseTile;
        public Tileset Tileset; // For a later version, we might want to have a group of tileset, but for now, we only have one tileset per group (for simplicity).
        public AutotilesGroup(string name, Tileset tileset, Autotiling? baseTile = null)
        {
            Name = name;
            Tileset = tileset ?? throw new ArgumentNullException(nameof(tileset), "Tileset cannot be null");
            BaseTile = baseTile;
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

        //public Tile? GetTileByRule()
    }

    public class Autotiles : BaseAsset
    {
        public List<AutotilesGroup> Autotilings = [];

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

    }
}
