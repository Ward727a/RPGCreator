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
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.UI.Models.Widgets.AssetsManager;
using RPGCreator.RuntimeLib.FNA;
using RPGCreator.SDK.Assets.AutoTiles;
using RPGCreator.SDK.Assets.Tileset;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.SDK.Services.Map;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;
using Vector2 = System.Numerics.Vector2;

namespace RPGCreator.Editor.UI.Controllers.Widgets.AssetsManager;


public partial class AutoTileAssetManagerController
{
    [EditorUiController]
    public class MainWindowController
    {
        public AutoTileAssetManagerModel.MainWindowModel State { get; }

        private readonly Dictionary<Ulid, (MaterialId materialId, string materialName)> _rulesetMaterials = new();
        private readonly Dictionary<Ulid, (ImTextureRef texture, Vector2 uv0, Vector2 uv1)> _textures = [];
        private readonly List<AutoTileRulesetDefinition> _rulesets = [];
        
        private readonly IAssetManager _assetManager;
        private readonly IResourceManager _resourceManager;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly ImGuiRenderer _imGuiRenderer;
        private readonly IMaterialRegistry _materialRegistry;
        
        public MainWindowController(
            AutoTileAssetManagerModel.MainWindowModel managerModel,
            IAssetManager assetManager,
            IResourceManager resourceManager,
            Game editor,
            ImGuiRenderer imGuiRenderer,
            IMaterialRegistry materialRegistry)
        {
            _imGuiRenderer = imGuiRenderer;
            _materialRegistry = materialRegistry;
            _assetManager = assetManager;
            _resourceManager = resourceManager;
            State = managerModel;
            State.RulesetMaterials = _rulesetMaterials;
            State.Rulesets = _rulesets;
            _graphicsDevice = editor.GraphicsDevice;
        }

        private AutoTileMakerModalArgs? _makerModalArgs = null;
        public AutoTileMakerModalArgs MakeMakerModalArgs()
        {
            _makerModalArgs ??= new AutoTileMakerModalArgs(RefreshList);
            return _makerModalArgs;
        }

        public void RefreshList()
        {
            _rulesets.Clear();
            _rulesetMaterials.Clear();

            foreach (var texture in _textures.Values)
            {
                var texture2D = _imGuiRenderer.UnbindTexture(texture.texture)?.Texture ?? null;
                texture2D?.Dispose();
            }

            _textures.Clear();

            var list = _assetManager.ListAssets<AutoTileRulesetDefinition>();

            foreach (var rulesetId in list)
            {
                _assetManager.GetAsset<AutoTileRulesetDefinition>(rulesetId).OnSuccess(ruleset =>
                    {
                        _rulesets.Add(ruleset);

                        var materialId = _materialRegistry.GetMaterialId(rulesetId);

                        var materialName = ruleset.Name;
                        if (materialId != MaterialId.Empty)
                        {
                            materialName = _materialRegistry.GetMaterialName(materialId);
                        }

                        _rulesetMaterials.Add(rulesetId, (materialId, materialName));
                    })
                    .OnFailure(err =>
                        Logger.Error("Couldn't load ruleset {id} definition due to error: {err}", rulesetId, err));
            }
        }

        public void SelectRuleset(AutoTileRulesetDefinition ruleset)
        {
            State.SelectedRuleset = ruleset;
        }
        
        public bool HasSelectedRuleset => State.SelectedRuleset != null;

        public (MaterialId id, string name) GetMaterialInfo(Ulid tilesetId)
        {
            return _rulesetMaterials[tilesetId];
        }

        public (Vector2 uv0, Vector2 uv1) GetTileUv(TileIndex tileIndex, AutoTileRulesetDefinition def)
        {
            var tileset = _assetManager.GetAsset<TilesetDefinition>(def.TargetTileset)
                .Match(tileset => tileset, err => null);
            if (tileset == null)
                return default;

            var imageMeta = _resourceManager.GetResourceMeta<ResourceImageMeta>(tileset.TextureResourcePath);
            if (imageMeta.IsFailure)
                return default;

            var boundsTile = new Rectangle(tileIndex.X * tileset.TileWidth, tileIndex.Y * tileset.TileHeight,
                tileset.TileWidth, tileset.TileHeight);

            var localUv0 = new Vector2(
                (float)boundsTile.X / imageMeta.Value.ImageSize.X,
                (float)boundsTile.Y / imageMeta.Value.ImageSize.Y
            );

            var localUv1 = new Vector2(
                (float)(boundsTile.X + boundsTile.Width) / imageMeta.Value.ImageSize.X,
                (float)(boundsTile.Y + boundsTile.Height) / imageMeta.Value.ImageSize.Y
            );

            return (localUv0, localUv1);
        }

        public (ImTextureRef? texture, Vector2 uv0, Vector2 uv1) GetTilesetTexture(AutoTileRulesetDefinition def)
        {
            if (_textures.TryGetValue(def.AssetId, out var textureInfo))
            {
                return (textureInfo.texture, textureInfo.uv0, textureInfo.uv1);
            }

            var tileset = _assetManager.GetAsset<TilesetDefinition>(def.TargetTileset)
                .Match(tileset => tileset, err => null);
            if (tileset == null)
                return (null, Vector2.Zero, Vector2.Zero);

            var textureData = _resourceManager.GetResource(tileset.TextureResourcePath).Match(data => data, err => default);
            if (textureData.IsEmpty)
                return (null, Vector2.Zero, Vector2.Zero);

            var texture2D = Texture2D.FromMemory(_graphicsDevice, textureData);
            var textureRef = _imGuiRenderer.BindTexture(texture2D);
            var defaultUv = GetTileUv(def.DefaultTile, def);
            _textures[def.AssetId] = (texture: textureRef, defaultUv.uv0, defaultUv.uv1);
            return _textures[def.AssetId];
        }
    }
}