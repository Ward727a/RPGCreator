// RPG Creator - Open-source RPG Engine.
// (c) 2026 Ward
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

using LiteDB;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Assets.MetaData;

/// <summary>
/// The map metadata is a simplified and light representation of the map asset, containing only essential info.<br/>
/// This is mainly used to quickly load and display map in, for example, combobox, list, etc...<br/>
/// In general, this should be preferably used instead of the full map definition.<br/>
/// The only case where the full map definition should be used is when you need to edit the map, or show it on screen.
/// </summary>
public sealed class MapMetaData : BaseMetaData
{
    public Ulid ParentId { get; set; } = Ulid.Empty;
    public override string DbKey => "metadata_map";
    public List<Ulid> ChildMapIds { get; set; } = [];
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public List<Ulid> TileLayerIds { get; set; } = [];
    
    // Need to check if this is really needed, as we use a chunk based system, and the size of the map is not really relevant now...
    public Size MapSize { get; set; } = new Size(100, 100);
    
    public GridParameter GridParameter { get; set; } = new GridParameter();
    public Color BackgroundColor { get; set; } = Color.DeepSkyBlue;
    
    #region HelperFields
    
    [BsonIgnore]
    public bool HasParent => ParentId != Ulid.Empty;
    
    [BsonIgnore]
    public bool HasChildMaps => ChildMapIds.Count > 0;
    [BsonIgnore]
    public bool HasTileLayers => TileLayerIds.Count > 0;
    
    [BsonIgnore]
    public int ChildMapCount => ChildMapIds.Count;
    [BsonIgnore]
    public int TileLayerCount => TileLayerIds.Count;
    
    [BsonIgnore]
    public string DisplayName => string.IsNullOrWhiteSpace(Name) ? $"Map ({Unique})" : Name;
    
    #endregion
    
    #region Constructors

    public MapMetaData()
    {
    }

    public MapMetaData(IMapDef mapDef)
    {
        Unique = mapDef.Unique;
        ChildMapIds = mapDef.MapDefs.ToArray().Select(m => m.Unique).ToList();
        Name = mapDef.Name;
        Description = mapDef.Description;
        TileLayerIds = mapDef.TileLayers.ToArray().Select(l => l.Unique).ToList();
        MapSize = mapDef.Size;
        GridParameter = mapDef.GridParameter;
        BackgroundColor = mapDef.BackgroundColor;
    }
    
    #endregion

    public override IEnumerable<Ulid> GetReferencedIds()
    {
        return [..ChildMapIds, ..TileLayerIds];
    }
}