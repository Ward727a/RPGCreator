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

using System.Collections.Generic;
using RPGCreator.Editor.UI.Models.Widgets.AssetsManager;
using RPGCreator.SDK.Assets.Factions;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.Editor.UI.Controllers.Widgets.AssetsManager;

[EditorUiController]
public class FactionMgrController(FactionMgrModel model, IAssetManager assetManager)
{
    public FactionMgrModel Model = model;
    
    private ushort _currentFactionVersion = 0;

    public void LoadFaction(bool forceRefresh = false)
    {
        var factionVersion = assetManager.GetVersion<FactionDefinition>();
        if (_currentFactionVersion == factionVersion && !forceRefresh)
            return;

        _currentFactionVersion = factionVersion;
        
        List<FactionDefinition> factions = [];
        var factionIds = assetManager.ListAssets<FactionDefinition>();

        foreach (var factionId in factionIds)
        {
            var faction = assetManager.GetAsset<FactionDefinition>(factionId)
                .Match(faction => faction,
                    err =>
                    {
                        Logger.Once.Error("Failed to load faction with id {id} due to error: {err}", args: [factionId, err]);
                        return null;
                    });

            if (faction == null)
                continue;
            
            factions.Add(faction);            
        }
        
        Model.Factions = factions;
    }
    
    public void DeleteFaction(FactionDefinition faction)
    {
        assetManager.DeleteAsset(faction.AssetId);
    }
}