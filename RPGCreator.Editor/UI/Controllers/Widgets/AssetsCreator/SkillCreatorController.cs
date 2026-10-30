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
using System.Linq;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.UI.Models.Widgets.AssetsCreator;
using RPGCreator.Editor.UI.Views;
using RPGCreator.RuntimeLib.FNA;
using RPGCreator.SDK.Assets.Skills;
using RPGCreator.SDK.Assets.Stats;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.Editor.UI.Controllers.Widgets.AssetsCreator;

[EditorUiController]
public class SkillCreatorController(
    SkillCreatorModel model,
    IAssetManager assetManager,
    IResourceManager resourceManager,
    GraphicsDevice graphicsDevice,
    ImGuiRenderer imGuiRenderer)
{
    public readonly SkillCreatorModel Model = model;
    private IEditorUiBindableView? _resourceSelectorView = null;

    private ResourceSelector.ResourceSelectorArguments ResourceSelectorArguments => new()
    {
        AllowImportFromOs = true,
        ResourceTypes = [IResourceManager.ResourceType.Image],
        OnResourceSelected = (resource) =>
        {
            if (resource.ResourceId == Model.IconResourceId)
            {
                _resourceSelectorView = null;
                return;
            }

            UnbindTexture();

            resourceManager.GetResource(resource.ResourceId).OnSuccess(data =>
            {
                var texture2D = Texture2D.FromMemory(graphicsDevice, data);
                Model.IconTexture = imGuiRenderer.BindTexture(texture2D);
                Model.IconResourceId = resource.ResourceId;
            }).OnFailure(err =>
            {
                Logger.Error("Failed to load skill icon resource with id {0} due to {1}", resource.ResourceId, err);
            });

            _resourceSelectorView = null;
        }
    };

    public void ApplyPreset(SkillPreset preset)
    {
        Model.Preset = preset;
        Model.TargetType = preset switch
        {
            SkillPreset.DamageSkill => SkillTargetType.SingleEnemy,
            SkillPreset.HealSkill => SkillTargetType.SingleAlly,
            SkillPreset.SelfBuff => SkillTargetType.Self,
            _ => Model.TargetType
        };
    }

    public void OpenIconSelector(IEditorUiRouterLocal router)
    {
        router.ActivateView("rpgc://views/resource_selector", ResourceSelectorArguments).OnSuccess(selector =>
        {
            _resourceSelectorView = selector;
            ImGui.OpenPopup((_resourceSelectorView as ResourceSelector)?.ModalId);
        });
    }

    public void TryDrawResourceSelector()
    {
        _resourceSelectorView?.Draw(EDrawAs.Modal);
    }

    public bool Create()
    {
        if (string.IsNullOrWhiteSpace(Model.Name))
        {
            ImNotify.Show("Validation Error", "Skill name cannot be empty.", NotifyType.Error, 4f);
            return false;
        }

        var skill = new SkillDefinition
        {
            Name = Model.Name,
            Description = Model.Description,
            TargetType = Model.TargetType,
            IconResource = Model.IconResourceId
        };

        // Try to find a default Resource stat (like HP) to pre-fill the initial effect
        Ulid defaultResourceStatId = FindDefaultResourceStatId();

        switch (Model.Preset)
        {
            case SkillPreset.DamageSkill:
                skill.Effects.Add(new SkillEffect
                {
                    Kind = SkillEffectKind.DamageStat,
                    TargetStatId = defaultResourceStatId,
                    Formula = "10",
                    Variance = 0.1f,
                    Chance = 1.0f
                });
                break;

            case SkillPreset.HealSkill:
                skill.Effects.Add(new SkillEffect
                {
                    Kind = SkillEffectKind.HealStat,
                    TargetStatId = defaultResourceStatId,
                    Formula = "25",
                    Variance = 0.1f,
                    Chance = 1.0f
                });
                break;

            case SkillPreset.SelfBuff:
                skill.Effects.Add(new SkillEffect
                {
                    Kind = SkillEffectKind.ApplyStatusEffect,
                    Chance = 1.0f
                });
                break;

            case SkillPreset.Blank:
                break;
        }

        var result = assetManager.SaveAsset(skill);
        if (result.IsFailure)
        {
            ImNotify.Show("Error!", result.Error, NotifyType.Error, 5f);
            return false;
        }

        ImNotify.Show("Success!", $"Skill '{skill.Name}' created.", NotifyType.Success, 2f);
        return true;
    }

    private Ulid FindDefaultResourceStatId()
    {
        foreach (var statId in assetManager.ListAssets<StatDefinition>())
        {
            var res = assetManager.GetAsset<StatDefinition>(statId);
            if (res.IsSuccess && res.Value.Kind == StatKind.Resource)
                return statId;
        }

        return assetManager.ListAssets<StatDefinition>().FirstOrDefault();
    }

    public void Clear()
    {
        Model.Name = string.Empty;
        Model.Description = string.Empty;
        Model.IconResourceId = Ulid.Empty;
        Model.Preset = SkillPreset.DamageSkill;
        Model.TargetType = SkillTargetType.SingleEnemy;
        UnbindTexture();
    }

    private void UnbindTexture()
    {
        if (!Model.IconTexture.HasValue)
            return;

        var textureInfo = imGuiRenderer.UnbindTexture(Model.IconTexture.Value);
        if (textureInfo is { IsManaged: false })
        {
            textureInfo.Texture.Dispose();
        }

        Model.IconTexture = null;
    }
}