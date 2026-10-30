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
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.UI.Models.Widgets.AssetsCreator;
using RPGCreator.Editor.UI.Views;
using RPGCreator.RuntimeLib.FNA;
using RPGCreator.SDK.Assets.SpriteAnimation;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.Editor.UI.Controllers.Widgets.AssetsCreator;

[EditorUiController]
public class SpriteAnimationCreatorController(SpriteAnimationCreatorModel model, IAssetManager assetManager, ImGuiRenderer imGuiRenderer, GraphicsDevice graphicsDevice, IResourceManager resourceManager)
{
    public readonly SpriteAnimationCreatorModel Model = model;

    private ResourceSelector.ResourceSelectorArguments ResourceSelectorArguments =>
        new()
        {
            AllowImportFromOs = true,
            ResourceTypes = [IResourceManager.ResourceType.Image],
            OnResourceSelected = (resource) =>
            {
                if (resource.ResourceId == Model.SpriteResourceId)
                {
                    _resourceSelectorView = null;
                    return;
                }


                if (Model.Texture != null)
                {
                    var textureInfo = imGuiRenderer.UnbindTexture(Model.Texture.Value);

                    if (textureInfo is { IsManaged: false })
                    {
                        textureInfo.Texture.Dispose();
                    }
                }

                Model.Texture = null;

                resourceManager.GetResource(resource.ResourceId).OnSuccess(data =>
                {
                    var texture2D = Texture2D.FromMemory(graphicsDevice, data);
                    Model.Texture = imGuiRenderer.BindTexture(texture2D);
                    Model.SpriteResourceId = resource.ResourceId;
                }).OnFailure((err) =>
                {
                    Logger.Error("Failed to load sprite resource with id {0} due to {1}", resource.ResourceId, err);
                });
                _resourceSelectorView = null;
            }
        };
    
    private IEditorUiBindableView? _resourceSelectorView = null;

    public void OpenSpriteSheetSelector(IEditorUiRouterLocal router)
    {
        router.ActivateView("rpgc://views/resource_selector", ResourceSelectorArguments).OnSuccess((selector) =>
        {
            _resourceSelectorView = selector;
            ImGui.OpenPopup((_resourceSelectorView as ResourceSelector)?.ModalId);
        });
    }

    public void TryDrawResourceSelector()
    {
        _resourceSelectorView?.Draw(EDrawAs.Modal);
    }
    
    public void Create()
    {
        var animation = new SpriteAnimationDefinition()
        {
            Name = Model.Name,
            SpriteSheetResource = Model.SpriteResourceId
        };

        assetManager.SaveAsset(animation);
    }

    public void Clear()
    {
        Model.Name = string.Empty;
        Model.SpriteResourceId = Ulid.Empty;

        if (Model.Texture != null)
        {
            var textureInfo = imGuiRenderer.UnbindTexture(Model.Texture.Value);

            if (textureInfo is { IsManaged: false })
            {
                textureInfo.Texture.Dispose();
            }
        }

        Model.Texture = null;
    }
}