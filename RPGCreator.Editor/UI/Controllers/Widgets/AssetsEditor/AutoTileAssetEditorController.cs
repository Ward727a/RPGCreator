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
using Fluxor;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.UI.Models.Widgets.AssetsEditor;
using RPGCreator.Editor.UI.Views.EditorTools.Payload;
using RPGCreator.Editor.UI.Views.Widgets.AssetsEditor;
using RPGCreator.RuntimeLib.FNA;
using RPGCreator.SDK.Assets.AutoTiles;
using RPGCreator.SDK.Assets.Tileset;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Scenes;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.SDK.Services.Map;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;
using Vector2 = System.Numerics.Vector2;

namespace RPGCreator.Editor.UI.Controllers.Widgets.AssetsEditor;

[EditorUiController]
public class AutoTileAssetEditorController(
    AutoTileAssetEditorModel model,
    IMaterialRegistry materialRegistry,
    IAssetManager assetManager,
    IResourceManager resourceManager,
    Game game,
    ImGuiRenderer renderer,
    ISceneManager sceneManager,
    IState<SceneState> sceneState)
{
    public readonly AutoTileAssetEditorModel State = model;

    private readonly GraphicsDevice _graphicsDevice = game.GraphicsDevice;

    public bool HasSelectedRule => State.SelectedRuleIndex != -1;
    public bool HasSelectedMaterial => State.SelectedMaterialId != MaterialId.Empty;

    public void Init(AutoTileAssetEditor.AutoTileAssetEditorArguments arguments)
    {
        Clear();
        RefreshMaterials();
        
        State.RulesetDefinition = arguments.RulesetDefinition;
        
        RefreshAssignedMaterials();
        RefreshAssignedTileset(arguments.RulesetDefinition.TargetTileset);
        
        if (State.AssignedTilesetDefinition == null)
            return;

        State.TilesetTileSize = new Point(State.AssignedTilesetDefinition.TileWidth,
            State.AssignedTilesetDefinition.TileHeight);

        var resource = resourceManager
            .GetResource(State.AssignedTilesetDefinition.TextureResourcePath)
            .Match(
                data => data,
                _ => ReadOnlyMemory<byte>.Empty);
        var tilesetTexture = Texture2D.FromMemory(_graphicsDevice, resource);

        State.TilesetTexture = renderer.BindTexture(tilesetTexture);
        State.TilesetTextureSize = new Point(tilesetTexture.Width, tilesetTexture.Height);
    }

    public void Update()
    {
        RefreshMaterials();
    }

    public void Clear()
    {
        State.RulesetDefinition = null;
        State.SelectedMaterialId = MaterialId.Empty;
        State.AssignedMaterialId = MaterialId.Empty;
        State.SelectedRuleIndex = -1;
        
        if (State.TilesetTexture == null) return;
        var texture = renderer.UnbindTexture(State.TilesetTexture.Value);

        if (texture is { IsManaged: false })
        {
            texture.Texture.Dispose();
        }
    }

    private void RefreshAssignedTileset(Ulid tilesetId)
    {
        State.AssignedTilesetId = tilesetId;
        State.AssignedTilesetDefinition = assetManager
            .GetAsset<TilesetDefinition>(State.AssignedTilesetId)
            .Match(
                tileset => tileset,
                _ => null);
    }

    public void SelectToolType(CellConditionMode mode)
    {
        State.SelectedToolType = mode;
    }

    public async void Save()
    {
        try
        {
            if(State.RulesetDefinition != null)
            {
                assetManager.SaveAsset(State.RulesetDefinition);
                if (sceneState.Value.IsSceneLoaded)
                {
                    if (sceneState.Value.HasUnsavedChanges)
                    {
                        ImModal.Show("Alert!", async void () =>
                        {
                            try
                            {
                                ImGui.Text("Scene will be reloaded");
                                ImGui.Text("Every change on the map that are not saved will be lost!");
                                ImGui.Text("You can save the scene before reloading, or discard changes");
                                if (ImGui.Button("Save & reload"))
                                {
                                    sceneManager.SaveSceneAsync();
                                    await sceneManager.ReloadSceneAsync();
                                    ImModal.CloseCurrent();
                                }

                                if (ImGui.Button("Discard & reload"))
                                {
                                    await sceneManager.ReloadSceneAsync();
                                    ImModal.CloseCurrent();
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.Error(e, "Failed to reload scene");
                            }
                        }, false);
                    }
                    else
                    {
                        await sceneManager.ReloadSceneAsync();
                    }
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to save auto tile asset");
        }
    }

    public void SetTile(TileIndex tileIndex)
    {
        var tileUv = State.TileUv;

        if (State.RulesetDefinition == null)
            return;

        assetManager.GetAsset<TilesetDefinition>(State.RulesetDefinition.TargetTileset).OnSuccess((tileset) =>
        {
            tileUv = GetTileUv(tileIndex, tileset);
        });

        State.TileUv = tileUv;
        State.RulesetDefinition.Rules[State.SelectedRuleIndex] = State.SelectedRule.WithResultTileId(tileIndex).WithResultTilesetId(State.AssignedMaterialId);
        
    }

    public void UpdateFlip()
    {
        
        if (State.RulesetDefinition == null)
            return;
        var flipH = State.TileFlipH? TileFlipType.Horizontal : TileFlipType.None;
        var flipV = State.TileFlipV? TileFlipType.Vertical : TileFlipType.None;
        var finalFlip = flipH | flipV;
        State.RulesetDefinition.Rules[State.SelectedRuleIndex] = State.SelectedRule.WithResultTileFlip(finalFlip);
    }

    #region Rule Section

    public void SelectRule(int index)
    {
        State.SelectedRuleIndex = index;
        State.TileUv = GetTileUv(TileIndex.Make(State.SelectedRule.Result.TileId), State.AssignedTilesetDefinition!);
        State.TileFlipH = State.SelectedRule.Result.Flip.HasFlag(TileFlipType.Horizontal);
        State.TileFlipV = State.SelectedRule.Result.Flip.HasFlag(TileFlipType.Vertical);
    }

    public void CreateRule()
    {
        var newRule = new AutoTileRule
        {
            Result = new TileData
            {
                IsNotEmpty = true,
                TileId = State.RulesetDefinition!.DefaultTile,
                TilesetId = State.AssignedMaterialId
            }
        };
        State.RulesetDefinition.Rules.Add(newRule);
        State.SelectedRuleIndex = State.RulesetDefinition.Rules.Count - 1;
        var materialPreview = GetMaterialPreview(newRule.Result.TilesetId);
        State.TileUv = (uv0: materialPreview.defaultTileUv0, uv1: materialPreview.defaultTileUv1);
    }

    public void DeleteRule()
    {
        if (HasSelectedRule)
        {
            State.RulesetDefinition.Rules.RemoveAt(State.SelectedRuleIndex);
            State.SelectedRuleIndex = -1;
        }
    }

    public void MoveRule(int from, int to)
    {
        if (from < 0 || from >= State.RulesetDefinition.Rules.Count) return;
        if (to < 0 || to >= State.RulesetDefinition.Rules.Count) return;
        if (from == to) return;

        var ruleToMove = State.RulesetDefinition.Rules[from];

        State.RulesetDefinition.Rules.RemoveAt(from);

        State.RulesetDefinition.Rules.Insert(to, ruleToMove);

        if (State.SelectedRuleIndex == from)
        {
            State.SelectedRuleIndex = to;
        }
        else if (from < State.SelectedRuleIndex && to >= State.SelectedRuleIndex)
        {
            State.SelectedRuleIndex--;
        }
        else if (from > State.SelectedRuleIndex && to <= State.SelectedRuleIndex)
        {
            State.SelectedRuleIndex++;
        }
    }

    public void EditRule(int x, int y, bool leftClick = true)
    {
        if (!HasSelectedMaterial)
            return;
        if (!HasSelectedRule)
            return;

        if (leftClick)
        {
            SetRule(x, y * 3);
        }
        else
        {
            RemoveRule(x, y * 3);
        }
    }

    private void SetRule(int x, int y)
    {
        var rule = State.RulesetDefinition!.Rules[State.SelectedRuleIndex];
        var condition = rule.Get(x + y);
        var mask = condition.TargetMask;

        if (State.SelectedToolType != CellConditionMode.Ignore)
        {
            if (!mask.Contains(State.SelectedMaterialId))
                mask = mask.AddMaterial(State.SelectedMaterialId);
        }
        else
        {
            mask = MaterialMask.Empty;
        }

        condition = new CellCondition { Mode = State.SelectedToolType, TargetMask = mask };
        State.RulesetDefinition!.Rules[State.SelectedRuleIndex] = rule.Set(x + y, condition);
        Logger.Debug($"Set rule at {x}, {y} : {condition}");
    }

    private void RemoveRule(int x, int y)
    {
        var rule = State.RulesetDefinition!.Rules[State.SelectedRuleIndex];
        var condition = rule.Get(x + y);
        var mask = condition.TargetMask;

        if (mask.Contains(State.SelectedMaterialId))
            mask = mask.RemoveMaterial(State.SelectedMaterialId);

        if (!mask.GetMaterials().Any())
        {
            condition = condition with { Mode = CellConditionMode.Ignore };
        }

        condition = condition with { TargetMask = mask };
        State.RulesetDefinition!.Rules[State.SelectedRuleIndex] = rule.Set(x + y, condition);
        Logger.Debug($"Set rule at {x}, {y} : {condition}");
    }

    #endregion

    #region Material Section

    private void RefreshMaterials()
    {
        if (State.MaterialsVersion == materialRegistry.Version) return;
        
        State.Materials = [.. materialRegistry.GetMaterials()];
        State.MaterialsVersion = materialRegistry.Version;
    }

    private void RefreshAssignedMaterials()
    {
        State.AssignedMaterialId = GetAssignedMaterial();
        State.AssignedMaterialName = materialRegistry.GetMaterialName(State.AssignedMaterialId);
    }
    
    private MaterialId GetAssignedMaterial()
    {
        return materialRegistry.GetMaterialId(State.RulesetDefinition!.AssetId);
    }
    
    public void SelectMaterial(MaterialId id)
    {
        State.SelectedMaterialId = id;
    }

    private readonly Dictionary<MaterialId, (ImTextureRef texture, Vector2 defaultTileUv0, Vector2 defaultTileUv1)>
        _cachedMaterialPreviews =
            new();

    private readonly (ImTextureRef texture, Vector2 defaultTileUv0, Vector2 defaultTileUv1) _emptyMaterialPreview =
        (default, defaultTileUv0: Vector2.Zero, defaultTileUv1: Vector2.Zero);

    public (ImTextureRef texture, Vector2 defaultTileUv0, Vector2 defaultTileUv1) GetMaterialPreview(
        MaterialId materialId)
    {
        if (_cachedMaterialPreviews.TryGetValue(materialId, out var preview))
            return preview;

        var rulesetId = materialRegistry.GetRuleset(materialId);

        if (rulesetId == Ulid.Empty)
            return _emptyMaterialPreview;

        var ruleset = assetManager.GetAsset<AutoTileRulesetDefinition>(rulesetId)
            .Match(ruleset => ruleset, _ => null);

        if (ruleset == null)
            return _emptyMaterialPreview;


        var tileset = assetManager.GetAsset<TilesetDefinition>(ruleset.TargetTileset)
            .Match(tileset => tileset, _ => null);

        if (tileset == null)
            return _emptyMaterialPreview;

        if (!_cachedTextureRefs.TryGetValue(tileset.AssetId, out var textureRef))
        {
            var textureRefTemp = GetTextureRef(tileset);

            if (!textureRefTemp.HasValue)
                return _emptyMaterialPreview;

            textureRef = textureRefTemp.Value;
        }

        var defaultTileIndex = ruleset.DefaultTile;
        var tileUvS = GetTileUv(defaultTileIndex, tileset);

        if (tileUvS == _emptyTileUv)
            return _emptyMaterialPreview;

        _cachedMaterialPreviews.Add(materialId, (textureRef, tileUvS.uv0, tileUvS.uv1));
        return _cachedMaterialPreviews[materialId];
    }

    #endregion

    #region Helpers

    private readonly Dictionary<Ulid, ImTextureRef> _cachedTextureRefs = new();

    private ImTextureRef? GetTextureRef(TilesetDefinition tilesetDefinition)
    {
        if (_cachedTextureRefs.TryGetValue(tilesetDefinition.AssetId, out var textureRef))
        {
            return textureRef;
        }

        var resourcePath = tilesetDefinition.TextureResourcePath;

        if (string.IsNullOrWhiteSpace(resourcePath))
            return null;

        var data = resourceManager.GetResource(resourcePath)
            .Match(data => data, _ => ReadOnlyMemory<byte>.Empty);

        if (data.IsEmpty)
            return null;

        var texture = Texture2D.FromMemory(_graphicsDevice, data);
        textureRef = renderer.BindTexture(texture);
        _cachedTextureRefs[tilesetDefinition.AssetId] = textureRef;

        return textureRef;
    }

    private readonly (Vector2 uv0, Vector2 uv1) _emptyTileUv = (Vector2.Zero, Vector2.Zero);

    private (Vector2 uv0, Vector2 uv1) GetTileUv(TileIndex tileIndex, TilesetDefinition tilesetDefinition)
    {
        var imageInfoMeta = resourceManager.GetResourceMeta<ResourceImageMeta>(tilesetDefinition.TextureResourcePath)
            .Match(meta => meta, _ => null);

        if (imageInfoMeta == null)
            return _emptyTileUv;

        var imgW = imageInfoMeta.ImageSize.X;
        var imgH = imageInfoMeta.ImageSize.Y;
        var tileW = tilesetDefinition.TileWidth;
        var tileH = tilesetDefinition.TileHeight;
        var tileX = tileIndex.Index % tilesetDefinition.TilesByRow;
        var tileY = tileIndex.Index / tilesetDefinition.TilesByRow;
        var tileXSize = tileX * tileW;
        var tileYSize = tileY * tileH;


        var localUv0 = new Vector2(
            (float)tileXSize / imgW,
            (float)tileYSize / imgH
        );

        var localUv1 = new Vector2(
            (float)(tileXSize + tileW) / imgW,
            (float)(tileYSize + tileH) / imgH
        );

        return (localUv0, localUv1);
    }

    #endregion

    #region Modals

    private TilesetTilePickerArgs? _cachedArgs = null;

    public TilesetTilePickerArgs? GetTilePickerArgs()
    {
        var tileW = State.TilesetTileSize.X;
        var tileH = State.TilesetTileSize.Y;
        var imgSize = State.TilesetTextureSize;

        var tilesByRow = imgSize.X / tileW;

        var tileX = (State.SelectedRule.Result.TileId % tilesByRow);
        var tileY = (State.SelectedRule.Result.TileId / tilesByRow);

        if (!_cachedArgs.HasValue)
        {
            if (!State.TilesetTexture.HasValue)
                return null;

            _cachedArgs = new TilesetTilePickerArgs()
            {
                AllowPick = true,
                SupportMultiTile = false,
                TileSize = State.TilesetTileSize,
                ImageSize = imgSize,
                Image = State.TilesetTexture.Value,
                OnTileSelected = (start, end) =>
                {
                    SetTile(start);
                    Logger.Debug("Start: {start} | End: {end}", start, end);
                }
            };
        }

        var tileIndex = new Point(tileX, tileY);
        _cachedArgs = _cachedArgs.Value with { StartIndex = tileIndex, EndIndex = tileIndex };
        return _cachedArgs.Value;
    }

    #endregion
}