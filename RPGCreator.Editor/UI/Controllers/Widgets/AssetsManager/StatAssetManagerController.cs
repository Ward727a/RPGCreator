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
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.UI.Models.Widgets.AssetsManager;
using RPGCreator.RuntimeLib.FNA;
using RPGCreator.SDK.Assets.Stats;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.Editor.UI.Controllers.Widgets.AssetsManager;

[EditorUiController]
public class StatAssetManagerController(StatAssetManagerModel model, IAssetManager assetManager, IResourceManager resourceManager, GraphicsDevice graphicsDevice, ImGuiRenderer imGuiRenderer)
{
    private static readonly ScopedLogger Logger = SDK.Common.Logging.Logger.ForContext<StatAssetManagerController>();
    public StatAssetManagerModel Model => model;

    private ushort _currentStatVersion = 0;

    public void LoadStatsIfNeeded(bool forceRefresh = false)
    {
        var statVersion = assetManager.GetVersion<StatDefinition>();
        if(_currentStatVersion == statVersion && !forceRefresh)
            return;
        
        _currentStatVersion = statVersion;
        _textureRefs.Clear();
        
        var stats = new List<StatDefinition>();
        var statIds = assetManager.ListAssets<StatDefinition>();

        foreach (var statId in statIds)
        {
            var stat = assetManager.GetAsset<StatDefinition>(statId).Match(
                stat => stat, 
                err =>
            {
                Logger.Error("Failed to load stat asset with id {StatId} due to {Error}", args: [statId, err]);
                return null;
            });
            
            if(stat == null)
                continue;
            
            stats.Add(stat);
        }

        Model.Stats = stats;
    }

    private readonly Dictionary<Ulid, ImTextureRef?> _textureRefs = new();
    
    public ImTextureRef? GetTexture(Ulid iconId)
    {
        if (!_textureRefs.ContainsKey(iconId))
        {
            resourceManager.GetResource(iconId).OnSuccess(data =>
            {
                var texture2D = Texture2D.FromMemory(graphicsDevice, data);
                _textureRefs[iconId] = imGuiRenderer.BindTexture(texture2D);
            }).OnFailure(err =>
            {
                _textureRefs[iconId] = null;
                Logger.ErrorOnce("Failed to load texture asset with id {IconId} due to {Error}", args: [iconId, err]);
            });
        }
        
        return _textureRefs.TryGetValue(iconId, out var textureRef) ? textureRef : null;
    }

    public Result DeleteStat(StatDefinition? modelSelectedStat)
    {
        if(modelSelectedStat == null)
            return Result.Success();

        return assetManager.DeleteAsset(modelSelectedStat.AssetId);
    }
}