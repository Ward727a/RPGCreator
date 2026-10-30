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

using System;
using System.Collections.Generic;
using System.Linq;
using Arch.Core;
using Fluxor;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Editor.ECS.Components;
using RPGCreator.Editor.ECS.Flags;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.Tools.ToolLogics;
using RPGCreator.Editor.UI.Models.EditorTools.Payload;
using RPGCreator.Editor.UI.Views.EditorTools.Payload;
using RPGCreator.RuntimeLib.FNA;
using RPGCreator.SDK.Assets.Map;
using RPGCreator.SDK.Assets.Tileset;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Scenes;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.SDK.States;
using RPGCreator.SDK.Tools;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;
using Vector2 = System.Numerics.Vector2;

namespace RPGCreator.Editor.UI.Controllers.EditorTools.Payload;

[EditorUiController]
public class TilesetPayloadController(
    TilesetPayloadModel state,
    IAssetManager assetManager,
    ISceneManager sceneManager,
    IState<SceneState> sceneState,
    IResourceManager resourceManager,
    GraphicsDevice graphicsDevice,
    ImGuiRenderer renderer,
    IDispatcher dispatcher,
    IState<TileLayerToolState> testLayerToolState,
    IState<ToolState> toolState)
{
    public readonly TilesetPayloadModel State = state;

    private readonly QueryDescription _queryRegistry =
        new QueryDescription().WithAll<WorldEntityFlag, MapTilesetRegistryComponent>();

    private bool _isTextureInMap;
    private Ulid _textureTilesetId;
    private Texture2D? _texture;
    private ImTextureRef _textureRef;

    public IEnumerable<Ulid> GetTilesets()
    {
        return assetManager.ListAssets<TilesetDefinition>();
    }

    public TilesetDefinition? GetTileset(Ulid id)
    {
        return assetManager.GetAsset<TilesetDefinition>(id).Match(asset => asset, (err) =>
        {
            Logger.Error(err);
            return null;
        });
    }

    public bool IsTilesetAlreadyInMap(Ulid id)
    {
        return assetManager.GetAsset<MapDefinition>(sceneState.Value.CurrentMapId).Match(
            (asset) => asset.Header.TilesetMappingsReverse.ContainsKey(id),
            (err) =>
            {
                Logger.Error(err);
                return false;
            });
    }

    public byte GetTilesetIndex(Ulid id)
    {
        return assetManager.GetAsset<MapDefinition>(sceneState.Value.CurrentMapId).Match(
            (asset) => asset.Header.TilesetMappingsReverse[id],
            (err) =>
            {
                Logger.Error(err);
                return (byte)0;
            });
    }

    public byte AddTilesetInMap(Ulid id)
    {
        return assetManager.GetAsset<MapDefinition>(sceneState.Value.CurrentMapId).Match((asset) =>
            {
                var index = asset.Header.GetNextAvailableTilesetId();

                asset.Header.TilesetMappings.Add(index, id);
                _isTextureInMap = true;

                sceneManager.Simulation.CurrentWorld?.Query(in _queryRegistry,
                    (ref MapTilesetRegistryComponent registry) =>
                    {
                        var tilesetResult = assetManager.GetAsset<TilesetDefinition>(id);

                        if (tilesetResult.IsFailure)
                        {
                            Logger.Error(tilesetResult.Error);
                            return;
                        }

                        var tileset = tilesetResult.Value;

                        var textureDataResult = resourceManager.GetResource(tileset.TextureResourcePath);

                        if (textureDataResult.IsFailure)
                        {
                            Logger.Error(textureDataResult.Error);
                            return;
                        }

                        var textureData = textureDataResult.Value;

                        var texture2D = Texture2D.FromMemory(graphicsDevice, textureData);
                        registry.Textures.Add(index, texture2D);
                        registry.Definitions.Add(index, tileset);
                    });

                return index;
            },
            err =>
            {
                Logger.Error(err);
                return byte.MaxValue;
            });
    }

    public void RemoveTilesetInMap(Ulid id)
    {
        assetManager.GetAsset<MapDefinition>(sceneState.Value.CurrentMapId)
            .OnSuccess((asset) =>
            {
                var tilesetId = asset.Header.TilesetMappingsReverse[id];
                asset.Header.TilesetMappings.Remove(asset.Header.TilesetMappingsReverse[id]);
                _isTextureInMap = false;

                sceneManager.Simulation.CurrentWorld?.Query(in _queryRegistry,
                    (ref MapTilesetRegistryComponent registry) =>
                    {
                        if (!registry.Textures.Remove(tilesetId, out var texture)) return;

                        texture.Dispose();
                        registry.Definitions.Remove(tilesetId);
                    });
            }).OnFailure(err => { Logger.Error(err); });
    }

    public void SelectTileset(TilesetDefinition? tileset)
    {
        if (tileset != null)
        {
            if (!_isTextureInMap && HasSelectedTileset())
            {
                assetManager.ClearAssetFromCache(GetSelectedTileset());
            }

            State.Tileset = tileset;
            State.ImageSize = resourceManager.GetResourceMeta<ResourceImageMeta>(State.Tileset.TextureResourcePath)
                .Match(meta => meta.ImageSize, _ => Point.Zero);

            if (IsTilesetAlreadyInMap(tileset.AssetId))
            {
                byte tilesetId = GetTilesetIndex(tileset.AssetId);
                var tilesByRow = State.ImageSize.Value.X / State.Tileset.TileWidth;
                dispatcher.Dispatch(new TileLayerToolActions.SetTilesetAction(tilesetId, tilesByRow, TileIndex.Zero, TileIndex.Zero));
            }
        }
    }

    public bool HasSelectedTileset() => State.Tileset != null;
    public Ulid GetSelectedTileset() => State.Tileset?.AssetId ?? Ulid.Empty;
    public string GetSelectedTilesetName() => State.Tileset?.Name ?? "None";

    public Texture2D? LoadTilesetTexture()
    {
        if (State.Tileset == null)
            return null;

        var resource = State.Tileset.TextureResourcePath;

        return resourceManager.GetResource(resource).Match((data) =>
            {
                _isTextureInMap = IsTilesetAlreadyInMap(State.Tileset.AssetId);
                _textureTilesetId = GetSelectedTileset();
                var value = Texture2D.FromMemory(graphicsDevice, data);
                value.Name = "TilesetPayloadControllerTexture";
                return value;
            },
            (err) =>
            {
                Logger.Error(err);
                return null;
            });
    }

    public ImTextureRef GetTextureRef(out bool success)
    {
        if (_textureTilesetId == GetSelectedTileset() && _texture != null)
        {
            success = true;
            return _textureRef;
        }

        success = false;
        if (State.Tileset == null)
            return default;

        CleanTextureIfNeeded();
        _texture = LoadTilesetTexture();

        if (_texture == null)
            return default;

        _textureRef = renderer.BindTexture(_texture);
        success = true;
        return _textureRef;
    }

    public void CleanTextureIfNeeded()
    {
        if (_texture != null)
        {
            renderer.UnbindTexture(_textureRef);
            _texture.Dispose();
            _textureRef = default;
            _texture = null;
        }
    }

    public TilesetTilePickerArgs? _pickerArgsCache;

    public TilesetTilePickerArgs? GetArgs(ImTextureRef textureRef, out bool fromCache)
    {
        fromCache = false;
        var supportMultiTile = toolState.Value.ActiveTool?.Capabilities.Contains(RpgcToolCapabilities.MultiTileSelection) ?? false;

        if (!State.ImageSize.HasValue || State.Tileset == null)
            return null;

        if (_pickerArgsCache.HasValue)
        {
            if (textureRef.TexID == _pickerArgsCache.Value.Image.TexID && supportMultiTile == _pickerArgsCache.Value.SupportMultiTile)
            {
                fromCache = true;
                return _pickerArgsCache.Value;
            }
        }

        Point? startIndex = null, endIndex = null;

        if (testLayerToolState.Value.StartTileIndex != TileIndex.Zero)
        {
            var startTileIndex = testLayerToolState.Value.StartTileIndex;
            startIndex = new (startTileIndex.X,  startTileIndex.Y);
        }

        if (testLayerToolState.Value.EndTileIndex != TileIndex.Zero)
        {
            var endTileIndex = testLayerToolState.Value.EndTileIndex;
            endIndex = new (endTileIndex.X,  endTileIndex.Y);
        }

        var arg = new TilesetTilePickerArgs(textureRef, State.ImageSize.Value,
            new(State.Tileset.TileWidth, State.Tileset.TileHeight),
            (selectedStartIndex, selectedEndIndex) =>
            {
                Logger.Debug("Selected from {startIndex} to {endIndex}", selectedStartIndex, selectedEndIndex);
                
                dispatcher.Dispatch(new TileLayerToolActions.SetSelectedTileIndexAction(selectedStartIndex, selectedEndIndex));

                if (testLayerToolState.Value.TilesByRow == -1)
                {
                    var tilesByRow = State.ImageSize.Value.X / State.Tileset.TileWidth;
                    dispatcher.Dispatch(new TileLayerToolActions.SetTilesByRowAction(tilesByRow));
                }
            }, supportMultiTile, startIndex, endIndex);
        _pickerArgsCache = arg;
        return arg;
    }
}