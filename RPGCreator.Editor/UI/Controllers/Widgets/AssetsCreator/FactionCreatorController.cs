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

using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.UI.Models.Widgets.AssetsCreator;
using RPGCreator.SDK.Assets.Factions;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.Editor.UI.Controllers.Widgets.AssetsCreator;

[EditorUiController]
public class FactionCreatorController(FactionCreatorModel model, IAssetManager assetManager)
{
    public readonly FactionCreatorModel Model = model;

    public void Create()
    {
        var faction = new FactionDefinition
        {
            Name = Model.Name,
            DefaultRelation = (short)Model.DefaultRelation
        };
        assetManager.SaveAsset(faction).Finally(()=>
        {
            Model.Name = string.Empty;
            Model.DefaultRelation = 0;
            ImModal.CloseCurrent();
        });
    }

    public void Cancel()
    {
        Model.Name = string.Empty;
        Model.DefaultRelation = 0;
        ImModal.CloseCurrent();
    }
}