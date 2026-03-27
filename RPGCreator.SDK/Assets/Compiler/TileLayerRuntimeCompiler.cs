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

using System.Runtime.CompilerServices;
using CommunityToolkit.Diagnostics;
using RPGCreator.SDK.Assets.Definitions.Maps.Chunks;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Assets.Runtime.Tilesets;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.ECS.Components.Maps;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Collections;

namespace RPGCreator.SDK.Assets.Compiler;

public readonly record struct ChunkCompilerContext(Dictionary<long, SlabItemPointer> ChunkData, long ChunkIndex) : ICompilerContext;

public class TileLayerRuntimeCompiler : BaseAssetRuntimeCompiler<TileLayerDefinition>
{
    
    public override void Compile(TileLayerDefinition source, IEcsWorld world, ICompilerContext? context = null)
    {
        var chunkCompiler = RegistryServices.RuntimeCompilerRegistry.GetRegisteredCompiler<TileChunkRuntimeCompiler, LayerChunk<ITileDef>>();
        
        Guard.IsNotNull(source);
        Guard.IsNotNull(world);
        Guard.IsNotNull(context);
        Guard.IsOfType<MapCompilerContext>(context);
        
        var mapCompilerContext = (MapCompilerContext)context;

        var layerEntity = world.CreateEntity();

        layerEntity.AddComponent(new TileLayerTagComponent());
        
        var layerHeaderComponent = new LayerHeaderComponent()
        {
            IsVisible = source.VisibleByDefault,
            LayerZIndex = source.ZIndex,
            MapId = mapCompilerContext.MapId,
            Opacity = source.Opacity
        };
        
        layerEntity.AddComponent(layerHeaderComponent);

        var chunkData = new Dictionary<long, SlabItemPointer>();

        foreach (var element in source.Chunks)
        {
            var chunkIndex = element.Key;
            var value = element.Value;
            
            var chunkContext = new ChunkCompilerContext(chunkData, chunkIndex);
            chunkCompiler.Compile(value, world, chunkContext);
        }

        var component = new LayerChunkStorageComponent()
        {
            ChunkDataPointers = chunkData,
        };
        layerEntity.AddComponent(component);
    }
}

public class TileChunkRuntimeCompiler : BaseAssetRuntimeCompiler<LayerChunk<ITileDef>>
{
    public override void Compile(LayerChunk<ITileDef> source, IEcsWorld world, ICompilerContext? context = null)
    {
        Guard.IsNotNull(source);
        Guard.IsNotNull(world);
        Guard.IsNotNull(context);
        Guard.IsOfType<ChunkCompilerContext>(context);
        
        var chunkCompilerContext = (ChunkCompilerContext)context;

        var runtimeChunk = new RuntimeChunk<RuntimeTile>();

        var elementSpan = source.GetAllElementsSpan();

        var mask = new PresenceMaskBuffer();
        
        for (int i = 0; i < elementSpan.Length; i++)
        {
            var tileDef = elementSpan[i];
            if (tileDef == null) continue;
            
            mask[i / 64] |= 1UL << (i % 64);
            
            var tilePositionInWorld = LayerChunk.GetWorldPosition(chunkCompilerContext.ChunkIndex, i);
            
            SpriteEffects effects = SpriteEffects.None;

            switch (tileDef.Flip)
            {
                case TileFlip.Both:
                    effects = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
                    break;
                case TileFlip.Horizontal:
                    effects = SpriteEffects.FlipHorizontally;
                    break;
                case TileFlip.Vertical:
                    effects = SpriteEffects.FlipVertically;
                    break;
            }
            
            runtimeChunk[i] = new RuntimeTile(tileDef)
            {
                PositionInMap = tilePositionInWorld,
                Effects = effects
            };
        }
        
        runtimeChunk.PresenceMask = mask;

        var index = world.WorldBlobManager.Register(runtimeChunk);
        chunkCompilerContext.ChunkData.Add(chunkCompilerContext.ChunkIndex, index);
    }
}