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

using Arch.Buffer;
using Arch.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Editor.ECS.Components;
using RPGCreator.Editor.ECS.Flags;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.Scenes;
using RPGCreator.SDK.Services.Map;

namespace RPGCreator.RuntimeLib.ECS.System;

public class TileLayerSystem(
    World world,
    CommandBuffer commandBuffer,
    ISceneRender sceneRender,
    GraphicsDevice graphicsDevice) : RpgcBaseSystem(world, commandBuffer, sceneRender)
{
    private readonly QueryDescription _mapTilesetRegistryQuery =
        new QueryDescription().WithAll<MapTilesetRegistryComponent, WorldEntityFlag>();

    private readonly QueryDescription _layerQueryDescription =
        new QueryDescription().WithAll<TileLayerComponent, LayerComponent>();

    private Texture2D _whitePixel = null!;

    private bool _hasRegistry = false;
    private MapTilesetRegistryComponent _currentRegistry;

    public override void Initialize()
    {
        _whitePixel = new Texture2D(graphicsDevice, 1, 1);
        _whitePixel.SetData([Color.White]);
        _whitePixel.Name = "WhitePixelTestLayer";
    }

    public override void BeforeUpdate(in double t)
    {
        _hasRegistry = false;

        World.Query(in _mapTilesetRegistryQuery, (ref MapTilesetRegistryComponent registry) =>
        {
            _hasRegistry = true;
            _currentRegistry = registry;
        });
    }

    private double _totalTime = 0f;

    public override void Update(in double t)
    {
        if (!_hasRegistry) return;

        _totalTime += t;
        World.Query(in _layerQueryDescription,
            (Entity entity, ref TileLayerComponent tileLayerComponent, ref LayerComponent layerComponent) =>
            {
                if (!layerComponent.Visible)
                    return;
                TileLayer layer = tileLayerComponent.Layer;
                ReadOnlySpan<TileData> tiles = layer.GetAll();

                Color baseColor = Color.White;

                if (World.Has<PhantomLayerFlag>(entity))
                {
                    baseColor *= 0.5f + 0.3f * (float)Math.Sin(_totalTime * Math.PI);
                }

                for (int i = 0; i < tiles.Length; i++)
                {
                    ref readonly TileData tile = ref tiles[i];
                    if (!tile.IsNotEmpty) continue;

                    if (!_currentRegistry.Textures.TryGetValue(tile.TilesetId, out var texture) ||
                        !_currentRegistry.Definitions.TryGetValue(tile.TilesetId, out var tilesetDef))
                        continue;

                    Point gridPos = layer.GetGridPos(i);

                    int tilesPerRow = texture.Width / tilesetDef.TileWidth;
                    int srcX = (tile.TileId % tilesPerRow) * tilesetDef.TileWidth;
                    int srcY = (tile.TileId / tilesPerRow) * tilesetDef.TileHeight;

                    var sourceRect = new Rectangle(srcX, srcY, tilesetDef.TileWidth, tilesetDef.TileHeight);
                    var destPos = new Vector2(gridPos.X * tilesetDef.TileWidth, gridPos.Y * tilesetDef.TileHeight);

                    SceneRender.SubmitSprite(texture, destPos, sourceRect, baseColor, gridPos.Y * tilesetDef.TileHeight,
                        layerComponent.ZIndex);
                }
            });
    }

    public override void Dispose()
    {
        _whitePixel.Dispose();
        GC.SuppressFinalize(this);
    }
}