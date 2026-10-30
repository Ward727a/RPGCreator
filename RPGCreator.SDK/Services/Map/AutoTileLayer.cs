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

using Arch.Core;
using Arch.Core.Extensions;
using Fluxor;
using Microsoft.Xna.Framework;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.Scenes;

namespace RPGCreator.SDK.Services.Map;

public struct IntGridData(byte materialId)
{
    public byte MaterialId { get; set; } = materialId;
    
    public static IntGridData Empty => new(byte.MinValue);
}

[LayerType("rpgc", "layer_type", "auto_tile_layer", DisplayName = "AutoTile Layer")]
public class AutoTileLayer(IDispatcher dispatcher) : MapLayer<IntGridData>
{
    public override void Remove(Point gridPos)
    {
        if (!IsValid(gridPos)) return;
        
        Tiles[GetIndex(gridPos)] = IntGridData.Empty;
        
        if(!IsPhantom) dispatcher.Dispatch(new SceneStateActions.MapModified());
    }

    public override bool Has(Point gridPos)
    {
        if (!IsValid(gridPos)) return false;
        
        return Tiles[GetIndex(gridPos)].MaterialId != 0; 
    }

    public override void CreateEntityForEcs(World world)
    {
        if(world.IsAlive(LayerEntity))
            world.Destroy(LayerEntity);

        LayerEntity = world.Create(new AutoTileLayerComponent(this), new LayerComponent(ZIndex, IsVisible));
    }

    public void UpdateEntity()
    {
        if (LayerEntity != Entity.Null)
        {
            ref var layerComponent = ref LayerEntity.Get<AutoTileLayerComponent>();
            layerComponent.Version++;
        }
    }

    public override void Add(Point gridPos, in IntGridData obj)
    {
        if (!IsValid(gridPos)) return;
        
        Tiles[GetIndex(gridPos)] = obj;
        
        if(!IsPhantom) dispatcher.Dispatch(new SceneStateActions.MapModified());
    }

    public override IntGridData Get(Point gridPos)
    {
        return !IsValid(gridPos) ? IntGridData.Empty : Tiles[GetIndex(gridPos)];
    }

    public override ReadOnlySpan<IntGridData> GetAll()
    {
        return Tiles.AsSpan();
    }
}