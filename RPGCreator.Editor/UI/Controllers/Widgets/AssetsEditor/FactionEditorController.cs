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
using System.Numerics;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.UI.Models.Widgets.AssetsEditor;
using RPGCreator.Editor.UI.Views;
using RPGCreator.RuntimeLib.FNA;
using RPGCreator.SDK.Assets.Factions;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.Editor.UI.Controllers.Widgets.AssetsEditor;

[EditorUiController]
public class FactionEditorController(FactionEditorModel model, ImGuiRenderer imGuiRenderer, IResourceManager resourceManager, GraphicsDevice graphicsDevice, IAssetManager assetManager)
{
    public readonly FactionEditorModel Model = model;

    private ResourceSelector.ResourceSelectorArguments IconSelectorArguments =>
        new ResourceSelector.ResourceSelectorArguments
        {
            AllowImportFromOs = true,
            ResourceTypes = [IResourceManager.ResourceType.Image],
            OnResourceSelected = (resource) =>
            {
                if (resource.ResourceId == model.IconId)
                {
                    _iconSelectorView = null;
                    return;
                }
                
                if (model.IconTexture.HasValue)
                {
                    var textureInfo = imGuiRenderer.UnbindTexture(model.IconTexture.Value);
                    if (!textureInfo.IsManaged)
                    {
                        var texture2D = textureInfo.Texture;
                        texture2D.Dispose();
                    }

                    model.IconTexture = null;
                }

                resourceManager.GetResource(resource.ResourceId)
                    .OnSuccess((data) =>
                    {
                        var texture2D = Texture2D.FromMemory(graphicsDevice, data);
                        model.IconTexture = imGuiRenderer.BindTexture(texture2D);
                        model.IconId = resource.ResourceId;
                    }).OnFailure(err =>
                    {
                        Logger.Error("Failed to load faction icon from resource with id {0} path {1}",
                            args: [resource.ResourceId, resource.ResourceLocalPath]);
                    });

                _iconSelectorView = null;
            }
        };

    private IEditorUiBindableView? _iconSelectorView = null;
    public void OpenIconSelector(IEditorUiRouterLocal router)
    {
        router.ActivateView("rpgc://views/resource_selector", IconSelectorArguments).OnSuccess((selector) =>
        {
            _iconSelectorView = selector;
            ImGui.OpenPopup((_iconSelectorView as ResourceSelector)?.ModalId);
        });
    }
    
    public void TryDrawResourceSelector()
    {
        _iconSelectorView?.Draw(EDrawAs.Modal);
    }

    public void Setup(FactionDefinition faction)
    {
        Model.Name = faction.Name;
        Model.IconId = faction.IconResource;
        Model.DefaultRelation = faction.DefaultRelation;
        var color = ImGui.ColorConvertU32ToFloat4(faction.ColorRgba);
        Model.ColorRgbEditable = new Vector3(color.X, color.Y, color.Z);
        Model.Description = faction.Description;
        Model.Relations = new Dictionary<Ulid, short>(faction.Relations);
        
        Model.Faction = faction;
    }

    public void Save()
    {
        Model.Faction.Name = Model.Name;
        Model.Faction.IconResource = Model.IconId;
        Model.Faction.DefaultRelation = (short)Math.Clamp(Model.DefaultRelation, FactionDefinition.HOSTILE_MAX, FactionDefinition.FRIENDLY_MAX);
        Model.Faction.ColorRgba = ImGui.ColorConvertFloat4ToU32(new Vector4(Model.ColorRgbEditable.X, Model.ColorRgbEditable.Y, Model.ColorRgbEditable.Z, 1));
        Model.Faction.Description = Model.Description;
        Model.Faction.Relations = new Dictionary<Ulid, short>(Model.Relations);

        assetManager.SaveAsset(Model.Faction);
    }
    
    public IReadOnlyList<Ulid> GetAvailableFactions()
    {
        return [.. assetManager.ListAssets<FactionDefinition>()
            .Where(id => id != Model.Faction.AssetId && !Model.Relations.ContainsKey(id))];
    }

    public Result<FactionDefinition> GetFaction(Ulid id) => assetManager.GetAsset<FactionDefinition>(id);

    public void AddOrUpdateRelation(Ulid targetFactionId, short relationValue)
    {
        if (targetFactionId == Ulid.Empty || targetFactionId == Model.Faction.AssetId)
            return;

        Model.Relations[targetFactionId] = Math.Clamp(relationValue, FactionDefinition.HOSTILE_MAX, FactionDefinition.FRIENDLY_MAX);
    }

    public void RemoveRelation(Ulid targetFactionId)
    {
        Model.Relations.Remove(targetFactionId);
    }
}