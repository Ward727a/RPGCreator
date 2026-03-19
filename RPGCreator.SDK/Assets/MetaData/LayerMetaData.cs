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

using Newtonsoft.Json;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;

namespace RPGCreator.SDK.Assets.MetaData;

/// <summary>
/// A layer metadata is a simplified and light representation of a layer definition, containing only essential info.<br/>
/// This is mainly used to quickly load and display layer in, for example, combobox, list, etc...<br/>
/// In general, this should be preferably used instead of the full layer definition.<br/>
/// The only case where the full layer definition should be used is when you need to edit the layer, or show it on screen.
/// </summary>
public sealed class LayerMetaData : BaseMetaData
{
    private MapMetaData? _parentMapMetaData = null;

    public MapMetaData? GetParentMapMetaData()
    {
        if (_parentMapMetaData != null)
            return _parentMapMetaData;
        
        if (MapId == Ulid.Empty) return null;
        if (!RegistryServices.AssetsMetaDataRegistry.ContainsMetaData(MapId)) return null;
        _parentMapMetaData = RegistryServices.AssetsMetaDataRegistry.GetMetaData<MapMetaData>(MapId);
        return _parentMapMetaData;
    }
    
    public Ulid MapId { get; set; } = Ulid.Empty;
    
    public string Name { get; set; } = string.Empty;
    
    public int LayerIndex { get; set; } = 0;
    public int ZIndex { get; set; } = 0;
    
    public float Opacity { get; set; } = 1.0f;
    
    public bool IsForeground { get; set; } = false;
    public bool VisibleByDefault { get; set; } = true;
    
    public RenderingMode RenderingMode { get; set; } = RenderingMode.StaticUnder;
    
    #region HelperFields
    
    [JsonIgnore]
    public string DisplayName => string.IsNullOrWhiteSpace(Name) ? $"Layer ({UniqueId})" : Name;
    
    #endregion
    
    #region Constructors

    public LayerMetaData()
    {
    }

    public LayerMetaData(BaseLayerDef def)
    {
        Name = def.Name;
        UniqueId = def.Unique;
        LayerIndex = def.LayerIndex;
        ZIndex = def.ZIndex;
        Opacity = def.Opacity;
        IsForeground = def.IsForeground;
        VisibleByDefault = def.VisibleByDefault;
        RenderingMode = def.RenderingMode;
    }

    #endregion

    public override string DbKey => "metadata_layer";

    public override IEnumerable<Ulid> GetReferencedIds()
    {
        return [MapId];
    }
}