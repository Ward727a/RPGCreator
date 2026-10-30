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

using System.Buffers;
using Arch.Buffer;
using Arch.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Editor.ECS.Components;
using RPGCreator.Editor.ECS.Flags;
using RPGCreator.SDK.Assets.AutoTiles;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.Scenes;
using RPGCreator.SDK.Services.Map;
using SpriteEffects = Microsoft.Xna.Framework.Graphics.SpriteEffects;

namespace RPGCreator.RuntimeLib.ECS.System;

// ReSharper disable once ClassNeverInstantiated.Global
public class AutoTileLayerSystem(World world, CommandBuffer commandBuffer, ISceneRender sceneRender, GraphicsDevice graphicsDevice)
    : RpgcBaseSystem(world, commandBuffer, sceneRender)
{
    private readonly QueryDescription _mapTilesetRegistryQuery =
        new QueryDescription().WithAll<GameMaterialRegistryComponent, WorldEntityFlag>();

    private readonly QueryDescription _layerQueryDescription =
        new QueryDescription().WithAll<AutoTileLayerComponent, LayerComponent>();

    private Texture2D _whitePixel = null!;

    private bool _hasRegistry;
    private GameMaterialRegistryComponent _currentMaterialRegistry;

    public override void Initialize()
    {
        _whitePixel = new Texture2D(graphicsDevice, 1, 1);
        _whitePixel.SetData([Color.White]);
        _whitePixel.Name = "WhitePixelTestLayer";
    }

    public override void BeforeUpdate(in double t)
    {
        _hasRegistry = false;

        World.Query(in _mapTilesetRegistryQuery, (ref GameMaterialRegistryComponent registry) =>
        {
            _hasRegistry = true;
            _currentMaterialRegistry = registry;
        });
    }

    private double _totalTime;

    public override void Update(in double t)
    {
        if (!_hasRegistry) return;

        _totalTime += t;
        World.Query(in _layerQueryDescription,
            (Entity entity, ref AutoTileLayerComponent tileLayerComponent, ref LayerComponent layerComponent) =>
            {
                if (!layerComponent.Visible)
                    return;
                var layer = tileLayerComponent.Layer;
                var tiles = layer.GetAll();

                if (tileLayerComponent.Version != tileLayerComponent.LastRenderedVersion)
                {
                    tileLayerComponent.LastRenderedVersion = tileLayerComponent.Version;
                    if (tileLayerComponent.TileData.Length < tiles.Length)
                    {
                        if (tileLayerComponent.TileData.Length > 0)
                            ArrayPool<TileData>.Shared.Return(tileLayerComponent.TileData, true);
                        tileLayerComponent.TileData = ArrayPool<TileData>.Shared.Rent(tiles.Length);
                    }

                    RebuildLayer(ref tileLayerComponent, tiles);
                }

                var baseColor = Color.White;

                if (World.Has<PhantomLayerFlag>(entity))
                {
                    baseColor *= 0.5f + 0.3f * (float)Math.Sin(_totalTime * Math.PI);
                }

                for (var i = 0; i < tiles.Length; i++)
                {
                    ref readonly var tile = ref tileLayerComponent.TileData[i];
                    if (!tile.IsNotEmpty) continue;

                    var ruleset = _currentMaterialRegistry.Rulesets[tile.TilesetId];
                    if (!_currentMaterialRegistry.Textures.TryGetValue(ruleset.TargetTileset, out var texture) ||
                        !_currentMaterialRegistry.Definitions.TryGetValue(ruleset.TargetTileset, out var tilesetDef))
                        continue;

                    var gridPos = layer.GetGridPos(i);

                    var tilesPerRow = texture.Width / tilesetDef.TileWidth;
                    var srcX = (tile.TileId % tilesPerRow) * tilesetDef.TileWidth;
                    var srcY = (tile.TileId / tilesPerRow) * tilesetDef.TileHeight;

                    var sourceRect = new Rectangle(srcX, srcY, tilesetDef.TileWidth, tilesetDef.TileHeight);
                    var destPos = new Vector2(gridPos.X * tilesetDef.TileWidth, gridPos.Y * tilesetDef.TileHeight);
                    
                    var flip = tile.Flip;

                    var spriteEffectFlip = SpriteEffects.None;
                    
                    if (flip.HasFlag(TileFlipType.Horizontal))
                        spriteEffectFlip |= SpriteEffects.FlipHorizontally;
                    if (flip.HasFlag(TileFlipType.Vertical))
                        spriteEffectFlip |= SpriteEffects.FlipVertically;

                    SceneRender.SubmitSprite(texture, destPos, sourceRect, baseColor, gridPos.Y * tilesetDef.TileHeight,
                        layerComponent.ZIndex, spriteEffectFlip);
                }
            });
    }

    private enum NeighborDirection
    {
        Nw,
        N,
        Ne,
        W,
        E,
        Sw,
        S,
        Se
    }

    private void RebuildLayer(ref AutoTileLayerComponent tileLayerComponent, in ReadOnlySpan<IntGridData> tiles)
    {
        Span<MaterialId> neighbors = stackalloc MaterialId[8];

        for (int i = 0; i < tiles.Length; i++)
        {
            var tile = tiles[i];

            if (tile.MaterialId == MaterialId.Empty)
            {
                tileLayerComponent.TileData[i] = TileData.Empty;
                continue;
            }

            var ruleset = _currentMaterialRegistry.Rulesets[tile.MaterialId];

            GetNeighbors(ref neighbors, i, tiles.Length, ref tileLayerComponent, tiles);
            var tileData = ruleset.EvaluateNeighbors(neighbors[0], neighbors[1], neighbors[2], neighbors[3],
                neighbors[4], neighbors[5], neighbors[6], neighbors[7]);

            if (tileData is not { IsNotEmpty: true })
            {
                tileData = new TileData()
                {
                    TileId = ruleset.DefaultTile,
                    TilesetId = tile.MaterialId,
                    IsNotEmpty = true
                };
            }

            if (!tileData.HasValue)
                continue;
            tileLayerComponent.TileData[i] = tileData.Value;
        }

        return;

        void GetNeighbors(ref Span<MaterialId> returnArray, int currentIndex, int totalTiles,
            ref AutoTileLayerComponent tileLayerComponent, in ReadOnlySpan<IntGridData> tiles)
        {
            returnArray[0] = GetNeighbor(NeighborDirection.N, currentIndex, totalTiles, ref tileLayerComponent,
                tiles);
            returnArray[1] = GetNeighbor(NeighborDirection.E, currentIndex, totalTiles, ref tileLayerComponent,
                tiles);
            returnArray[2] = GetNeighbor(NeighborDirection.S, currentIndex, totalTiles, ref tileLayerComponent,
                tiles);
            returnArray[3] = GetNeighbor(NeighborDirection.W, currentIndex, totalTiles, ref tileLayerComponent,
                tiles);
            returnArray[4] = GetNeighbor(NeighborDirection.Ne, currentIndex, totalTiles, ref tileLayerComponent,
                tiles);
            returnArray[5] = GetNeighbor(NeighborDirection.Nw, currentIndex, totalTiles, ref tileLayerComponent,
                tiles);
            returnArray[6] = GetNeighbor(NeighborDirection.Se, currentIndex, totalTiles, ref tileLayerComponent,
                tiles);
            returnArray[7] = GetNeighbor(NeighborDirection.Sw, currentIndex, totalTiles, ref tileLayerComponent,
                tiles);
        }

        MaterialId GetNeighbor(NeighborDirection direction, int currentIndex, int totalTiles,
            ref AutoTileLayerComponent tileLayerComponent, in ReadOnlySpan<IntGridData> tiles)
        {
            var tilesByRow = tileLayerComponent.Layer.Width;
            switch (direction)
            {
                case NeighborDirection.N:
                {
                    if (currentIndex >= tilesByRow)
                    {
                        var northId = currentIndex - tilesByRow;
                        return tiles[northId].MaterialId;
                    }

                    return MaterialId.Empty;
                }
                case NeighborDirection.W:
                {
                    if (currentIndex % tilesByRow != 0)
                    {
                        var westId = currentIndex - 1;
                        return tiles[westId].MaterialId;
                    }

                    return MaterialId.Empty;
                }
                case NeighborDirection.E:
                {
                    if ((currentIndex + 1) % tilesByRow != 0)
                    {
                        var eastId = currentIndex + 1;
                        return tiles[eastId].MaterialId;
                    }

                    return MaterialId.Empty;
                }
                case NeighborDirection.S:
                {
                    if (currentIndex + tilesByRow < totalTiles)
                    {
                        var southId = currentIndex + tilesByRow;
                        return tiles[southId].MaterialId;
                    }

                    return MaterialId.Empty;
                }
                case NeighborDirection.Ne:
                {
                    if (currentIndex < tilesByRow) return MaterialId.Empty;

                    var northId = currentIndex - tilesByRow;
                    currentIndex = northId;
                    goto case NeighborDirection.E;
                }
                case NeighborDirection.Nw:
                {
                    if (currentIndex < tilesByRow) return MaterialId.Empty;

                    var northId = currentIndex - tilesByRow;
                    currentIndex = northId;
                    goto case NeighborDirection.W;
                }
                case NeighborDirection.Se:
                {
                    if (currentIndex + tilesByRow >= totalTiles) return MaterialId.Empty;
                    var southId = currentIndex + tilesByRow;
                    currentIndex = southId;
                    goto case NeighborDirection.E;
                }
                case NeighborDirection.Sw:
                {
                    if (currentIndex + tilesByRow >= totalTiles) return MaterialId.Empty;
                    var southId = currentIndex + tilesByRow;
                    currentIndex = southId;
                    goto case NeighborDirection.W;
                }
                default:
                {
                    return MaterialId.Empty;
                }
            }
        }
    }

    public override void Dispose()
    {
        _whitePixel.Dispose();
        GC.SuppressFinalize(this);
    }
}