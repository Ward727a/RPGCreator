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
using RPGCreator.Editor.UI.Models.Widgets.AssetsCreator;
using RPGCreator.Editor.UI.Views;
using RPGCreator.RuntimeLib.FNA;
using RPGCreator.SDK.Assets.Stats;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.Editor.UI.Controllers.Widgets.AssetsCreator;

[EditorUiController]
public class StatAssetCreatorController(
    StatAssetCreatorModel model,
    ImGuiRenderer imGuiRenderer,
    IResourceManager resourceManager,
    GraphicsDevice graphicsDevice,
    IAssetManager assetManager)
{
    public StatAssetCreatorModel Model => model;
    private IEditorUiBindableView? _iconSelectorView = null;

    public ResourceSelector.ResourceSelectorArguments ResourceSelectorArguments => new()
    {
        AllowImportFromOs = true,
        ResourceTypes = [IResourceManager.ResourceType.Image],
        OnResourceSelected = (resource) =>
        {
            if (Model.IconId != Ulid.Empty)
            {
                if (Model.IconTexture.HasValue)
                {
                    var backup = Model.IconTexture.Value;
                    Model.IconTexture = null;
                    var textureInfo = imGuiRenderer.UnbindTexture(backup);
                    if (!textureInfo?.IsManaged ?? false)
                    {
                        textureInfo.Texture.Dispose();
                    }
                }
            }

            Model.IconSourcePath = resource.ResourceLocalPath;
            Model.IconId = resource.ResourceId;

            resourceManager.GetResource(resource.ResourceId).OnSuccess(data =>
            {
                var texture2D = Texture2D.FromMemory(graphicsDevice, data);
                Model.IconTexture = imGuiRenderer.BindTexture(texture2D);

                resourceManager.GetResourceMeta<ResourceImageMeta>(resource).OnSuccess(meta =>
                {
                    var x = Math.Min(meta.ImageSize.X, 256);
                    var y = Math.Min(meta.ImageSize.Y, 256);
                    Model.IconSize = new Point(x, y);
                }).OnFailure(err =>
                {
                    Model.IconSize = new Point(32, 32);
                    Logger.Error("Couldn't load icon meta due to error: {err}", err);
                });
            }).OnFailure(err => { Logger.Error("Couldn't load icon texture due to error: {err}", err); });

            _iconSelectorView = null;
        }
    };

    public void OpenIconSelector(IEditorUiRouterLocal router)
    {
        router.ActivateView("rpgc://views/resource_selector", ResourceSelectorArguments).OnSuccess((selector) =>
        {
            _iconSelectorView = selector;
            ImGui.OpenPopup((_iconSelectorView as ResourceSelector)?.ModalId);
        });
    }

    public void TryDrawResourceSelector()
    {
        _iconSelectorView?.Draw(EDrawAs.Modal);
    }

    public void TryCreateStatAsset()
    {
        var validation = model.ValidationState;
        validation.Clear();
        
        if (string.IsNullOrWhiteSpace(model.Name))
            validation.AddError("Name", "Name cannot be empty");
        if (string.IsNullOrWhiteSpace(model.Code))
            validation.AddError("Code", "Code cannot be empty");
        if (model.IconId == Ulid.Empty)
            validation.AddWarning("Icon", "Stat icon is empty");

        if (validation.HasErrors)
        {
            ImNotify.Show("Error!", string.Join("\n", validation.Errors), NotifyType.Error, 5f);
            return;
        }

        if (validation.HasWarnings)
            ImNotify.Show("Warning!", string.Join("\n", validation.Warnings), NotifyType.Warning, 5f);

        var stat = new StatDefinition()
        {
            Code = model.Code,
            Name = model.Name,
            Description = model.Description,
            Kind = model.Kind,
            IconResource = model.IconId
        };

        var result = assetManager.SaveAsset(stat);

        if (result.IsFailure)
        {
            ImNotify.Show("Error!", result.Error, NotifyType.Error, 5f);
        }
        else
        {
            ImNotify.Show("Success!", "Stat asset created", NotifyType.Success, 2f);
        }
    }
}