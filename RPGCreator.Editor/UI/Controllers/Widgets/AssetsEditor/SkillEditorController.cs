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
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.UI.Models.Widgets.AssetsEditor;
using RPGCreator.Editor.UI.Views;
using RPGCreator.RuntimeLib.FNA;
using RPGCreator.SDK.Assets.Skills;
using RPGCreator.SDK.Assets.SpriteAnimation;
using RPGCreator.SDK.Assets.Stats;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.Parser.PrattFormula;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.Editor.UI.Controllers.Widgets.AssetsEditor;

[EditorUiController]
public class SkillEditorController(
    SkillEditorModel model,
    IAssetManager assetManager,
    IResourceManager resourceManager,
    IPrattFormulaService prattFormulaService,
    GraphicsDevice graphicsDevice,
    ImGuiRenderer imGuiRenderer)
{
    public readonly SkillEditorModel Model = model;
    private IEditorUiBindableView? _resourceSelectorView = null;

    private ResourceSelector.ResourceSelectorArguments IconSelectorArguments => new()
    {
        AllowImportFromOs = true,
        ResourceTypes = [IResourceManager.ResourceType.Image],
        OnResourceSelected = (resource) =>
        {
            if (resource.ResourceId != Model.IconResourceId)
            {
                LoadIconTexture(resource.ResourceId);
                Model.IsDirty = true;
            }
            _resourceSelectorView = null;
        }
    };

    private ResourceSelector.ResourceSelectorArguments SoundSelectorArguments => new()
    {
        AllowImportFromOs = true,
        ResourceTypes = [IResourceManager.ResourceType.Audio],
        OnResourceSelected = (resource) =>
        {
            Model.HitSoundResourceId = resource.ResourceId;
            Model.HitSoundDisplayName = resource.ResourceName;
            Model.IsDirty = true;
            _resourceSelectorView = null;
        }
    };

    public void Setup(SkillDefinition skill)
    {
        Clear();

        Model.Skill = skill;
        Model.Name = skill.Name;
        Model.Description = skill.Description;
        Model.IconResourceId = skill.IconResource;
        Model.CostStatId = skill.CostStatId;
        Model.CostAmount = skill.CostAmount;
        Model.Cooldown = skill.Cooldown;
        Model.TargetType = skill.TargetType;
        Model.Range = skill.Range;

        Model.CasterAnimNamespace = string.IsNullOrEmpty(skill.CasterAnimationSlot.Namespace.ToString())
            ? "rpgc"
            : skill.CasterAnimationSlot.Namespace.ToString();
        Model.CasterAnimSlotName = string.IsNullOrEmpty(skill.CasterAnimationSlot.Name.ToString())
            ? "attack"
            : skill.CasterAnimationSlot.Name.ToString();

        Model.HitSpriteAnimationId = skill.HitSpriteAnimationId;
        Model.HitSoundResourceId = skill.HitSoundResourceId;
        Model.HitSoundDisplayName = ResolveResourceName(skill.HitSoundResourceId);

        // Deep-copy effects list so Cancel doesn't mutate the original asset
        Model.Effects = skill.Effects.Select(e => new SkillEffect
        {
            Kind = e.Kind,
            TargetStatId = e.TargetStatId,
            Formula = e.Formula,
            Variance = e.Variance,
            Chance = e.Chance,
            StatusEffectId = e.StatusEffectId,
            SignalToEmit = e.SignalToEmit
        }).ToList();

        if (Model.IconResourceId != Ulid.Empty)
            LoadIconTexture(Model.IconResourceId);

        Model.IsDirty = false;
    }

    private string ResolveResourceName(Ulid resourceId)
    {
        if (resourceId == Ulid.Empty)
            return "None";

        return resourceManager.GetResourceObject(resourceId)
            .Match(obj => obj.ResourceName, _ => resourceId.ToString());
    }

    private void LoadIconTexture(Ulid resourceId)
    {
        UnbindIconTexture();
        Model.IconResourceId = resourceId;

        resourceManager.GetResource(resourceId).OnSuccess(data =>
        {
            var texture2D = Texture2D.FromMemory(graphicsDevice, data);
            Model.IconTexture = imGuiRenderer.BindTexture(texture2D);
        }).OnFailure(err =>
        {
            Logger.Error("Failed to load skill icon {0}: {1}", resourceId, err);
        });
    }

    public void OpenIconSelector(IEditorUiRouterLocal router)
    {
        router.ActivateView("rpgc://views/resource_selector", IconSelectorArguments).OnSuccess(selector =>
        {
            _resourceSelectorView = selector;
            ImGui.OpenPopup((_resourceSelectorView as ResourceSelector)?.ModalId);
        });
    }

    public void OpenSoundSelector(IEditorUiRouterLocal router)
    {
        router.ActivateView("rpgc://views/resource_selector", SoundSelectorArguments).OnSuccess(selector =>
        {
            _resourceSelectorView = selector;
            ImGui.OpenPopup((_resourceSelectorView as ResourceSelector)?.ModalId);
        });
    }

    public void TryDrawResourceSelector()
    {
        _resourceSelectorView?.Draw(EDrawAs.Modal);
    }

    // --- Effects Management ---

    public void AddEffect()
    {
        var defaultStat = GetAllStats().FirstOrDefault(s => s.Kind == StatKind.Resource)?.AssetId ?? Ulid.Empty;
        Model.Effects.Add(new SkillEffect
        {
            Kind = SkillEffectKind.DamageStat,
            TargetStatId = defaultStat,
            Formula = "10",
            Variance = 0.1f,
            Chance = 1.0f
        });
        Model.IsDirty = true;
    }

    public void RemoveEffectAt(int index)
    {
        if (index < 0 || index >= Model.Effects.Count)
            return;

        Model.Effects.RemoveAt(index);
        Model.IsDirty = true;
    }

    public bool IsFormulaValid(string formula)
    {
        return !string.IsNullOrWhiteSpace(formula) && prattFormulaService.TryCompile(formula, out _);
    }

    // --- Asset Lookups ---

    public IReadOnlyList<StatDefinition> GetAllStats()
    {
        var list = new List<StatDefinition>();
        foreach (var id in assetManager.ListAssets<StatDefinition>())
        {
            assetManager.GetAsset<StatDefinition>(id).OnSuccess(list.Add);
        }
        return list;
    }

    public IReadOnlyList<SpriteAnimationDefinition> GetAllSpriteAnimations()
    {
        var list = new List<SpriteAnimationDefinition>();
        foreach (var id in assetManager.ListAssets<SpriteAnimationDefinition>())
        {
            assetManager.GetAsset<SpriteAnimationDefinition>(id).OnSuccess(list.Add);
        }
        return list;
    }

    // --- Save & Cleanup ---

    public void Save()
    {
        Model.Skill.Name = Model.Name;
        Model.Skill.Description = Model.Description;
        Model.Skill.IconResource = Model.IconResourceId;
        Model.Skill.CostStatId = Model.CostStatId;
        Model.Skill.CostAmount = Math.Max(0, Model.CostAmount);
        Model.Skill.Cooldown = Math.Max(0f, Model.Cooldown);
        Model.Skill.TargetType = Model.TargetType;
        Model.Skill.Range = Math.Max(0f, Model.Range);
        Model.Skill.CasterAnimationSlot = new URN(Model.CasterAnimNamespace, "anim_slots", Model.CasterAnimSlotName);
        Model.Skill.HitSpriteAnimationId = Model.HitSpriteAnimationId;
        Model.Skill.HitSoundResourceId = Model.HitSoundResourceId;
        Model.Skill.Effects = [.. Model.Effects];

        var res = assetManager.SaveAsset(Model.Skill);
        if (res.IsFailure)
        {
            ImNotify.Show("Error!", res.Error, NotifyType.Error, 5f);
            return;
        }

        Model.IsDirty = false;
        ImNotify.Show("Saved!", $"Skill '{Model.Skill.Name}' saved.", NotifyType.Success, 2f);
    }

    public void Clear()
    {
        UnbindIconTexture();
        Model.Effects.Clear();
        Model.IsDirty = false;
    }

    private void UnbindIconTexture()
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