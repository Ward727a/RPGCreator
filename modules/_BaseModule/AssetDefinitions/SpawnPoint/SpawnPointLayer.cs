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

using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps.Chunks;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Editor;
using RPGCreator.SDK.Types;

namespace _BaseModule.AssetDefinitions.SpawnPoint;

[SerializingType("SpawnPointLayer")]
public class SpawnPointLayer() : LayerWithElements<SpawnPointData>
{
    public override UrnSingleModule UrnModule => "spawn_point_layer".ToUrnSingleModule();
    
    private SpawnPointLayerTarget? _paintTargetCache;
    private readonly SpawnPointLayerRenderer _renderer = new();
    public override IPaintTarget? GetPaintTarget()
    {
        if(GlobalStates.MapState.CurrentMapDef == null)
            return null;
        var mapDef = GlobalStates.MapState.CurrentMapDef;
        
        if(_paintTargetCache != null && _paintTargetCache.MapDef == mapDef)
            return _paintTargetCache;
        
        _paintTargetCache = new SpawnPointLayerTarget(this, mapDef, (int)mapDef.GridParameter.CellWidth, (int)mapDef.GridParameter.CellHeight);
        return _paintTargetCache;
    }

    public override ILayerRenderer GetRenderer() => _renderer;

    public override bool CanPaintObject(object? objectToPaint)
    {
        return (objectToPaint is SpawnPointData);
    }

    protected override LayerChunk<SpawnPointData> CreateChunkInstance() => new SpawnPointLayerChunk();
}