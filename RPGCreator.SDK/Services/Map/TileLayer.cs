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
using Fluxor;
using Microsoft.Xna.Framework;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.Scenes;

namespace RPGCreator.SDK.Services.Map;

[Flags]
public enum TileFlipType : byte
{
    None = 0,
    Horizontal = 1,
    Vertical = 2,
    Both = Horizontal | Vertical,
}

public struct TileData
{
    public bool IsNotEmpty { get; init; }
    public byte TilesetId { get; init; }
    public ushort TileId { get; init; }
    public byte Collision { get; init; }
    public TileFlipType Flip { get; init; }
    
    public static TileData Empty => new();

    public TileData SetTile(ushort tileId)
    {
        return this with { TileId = tileId };
    }

    public TileData SetTileset(byte tilesetId)
    {
        return this with { TilesetId = tilesetId };
    }
    
    public TileData SetCollision(byte collision)
    {
        return this with { Collision = collision };
    }
    
    public TileData SetFlip(TileFlipType flip)
    {
        return this with { Flip = flip };
    }
}

[LayerType("rpgc", "layer_type", "tile_layer", DisplayName = "Tile Layer")]
public class TileLayer(IDispatcher dispatcher) : MapLayer<TileData> 
{
    public override void Remove(Point gridPos)
    {
        if (!IsValid(gridPos))
            return;
        
        Tiles[GetIndex(gridPos)] = TileData.Empty;
        
        if(!IsPhantom)
            dispatcher.Dispatch(new SceneStateActions.MapModified());
    }

    public override bool Has(Point gridPos)
    {
        if (!IsValid(gridPos))
            return false;
        
        return Tiles[GetIndex(gridPos)].IsNotEmpty;
    }

    public override void Add(Point gridPos, in TileData obj)
    {
        if (!IsValid(gridPos))
            return;
        
        Tiles[GetIndex(gridPos)] = obj with { IsNotEmpty = true };
        
        if(!IsPhantom)
            dispatcher.Dispatch(new SceneStateActions.MapModified());
    }

    public override TileData Get(Point gridPos)
    {
        return !IsValid(gridPos) ? TileData.Empty : Tiles[GetIndex(gridPos)];
    }

    public override ReadOnlySpan<TileData> GetAll()
    {
        return Tiles.AsSpan();
    }

    public override void CreateEntityForEcs(World world)
    {
        if(world.IsAlive(LayerEntity))
            world.Destroy(LayerEntity);
        
        LayerEntity = world.Create(new TileLayerComponent(this), new LayerComponent(ZIndex, IsVisible));
    }
}