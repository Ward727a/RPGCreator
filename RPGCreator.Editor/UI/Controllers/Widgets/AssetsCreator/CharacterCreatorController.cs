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
using RPGCreator.Editor.UI.Models.Widgets.AssetsCreator;
using RPGCreator.Editor.UI.Views;
using RPGCreator.RuntimeLib.ActorFeaturesData;
using RPGCreator.RuntimeLib.FNA;
using RPGCreator.SDK.ActorFeatures;
using RPGCreator.SDK.Assets.Actors;
using RPGCreator.SDK.Assets.Stats;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.Editor.UI.Controllers.Widgets.AssetsCreator;

[EditorUiController]
public class CharacterCreatorController(
    CharacterCreatorModel model,
    IAssetManager assetManager,
    IActorFeaturesRegistry featuresRegistry,
    IResourceManager resourceManager,
    GraphicsDevice graphicsDevice,
    ImGuiRenderer imGuiRenderer)
{
    public readonly CharacterCreatorModel Model = model;
    private IEditorUiBindableView? _resourceSelectorView = null;

    // Standard built-in feature URNs for presets
    private static readonly URN StatsUrn = new("rpgc", "sdk", "actor_features/stats");
    private static readonly URN FactionUrn = new("rpgc", "sdk", "actor_features/faction");
    private static readonly URN MovementUrn = new("rpgc", "sdk", "actor_features/movement");
    private static readonly URN ColliderUrn = new("rpgc", "sdk", "actor_features/collider");
    private static readonly URN PlayerControllerUrn = new("rpgc", "sdk", "actor_features/player_controller");

    private ResourceSelector.ResourceSelectorArguments ResourceSelectorArguments => new()
    {
        AllowImportFromOs = true,
        ResourceTypes = [IResourceManager.ResourceType.Image],
        OnResourceSelected = (resource) =>
        {
            if (resource.ResourceId == Model.PortraitResourceId)
            {
                _resourceSelectorView = null;
                return;
            }

            UnbindTexture();

            resourceManager.GetResource(resource.ResourceId).OnSuccess(data =>
            {
                var texture2D = Texture2D.FromMemory(graphicsDevice, data);
                Model.PortraitTexture = imGuiRenderer.BindTexture(texture2D);
                Model.PortraitResourceId = resource.ResourceId;
            }).OnFailure(err =>
            {
                Logger.Error("Failed to load portrait resource with id {0} due to {1}", resource.ResourceId, err);
            });

            _resourceSelectorView = null;
        }
    };

    public void InitializeDefaults()
    {
        ApplyPreset(Model.Preset);
    }

    /// <summary>
    /// Automatically selects the recommended features for the chosen preset.
    /// </summary>
    public void ApplyPreset(CharacterPreset preset)
    {
        Model.Preset = preset;
        Model.SelectedFeatureUrns.Clear();

        switch (preset)
        {
            case CharacterPreset.PlayableHero:
                TrySelectFeature(StatsUrn);
                TrySelectFeature(FactionUrn);
                TrySelectFeature(MovementUrn);
                TrySelectFeature(ColliderUrn);
                TrySelectFeature(PlayerControllerUrn);
                break;

            case CharacterPreset.EnemyMonster:
                TrySelectFeature(StatsUrn);
                TrySelectFeature(FactionUrn);
                TrySelectFeature(MovementUrn);
                TrySelectFeature(ColliderUrn);
                break;

            case CharacterPreset.TownNpc:
                TrySelectFeature(FactionUrn);
                TrySelectFeature(MovementUrn);
                TrySelectFeature(ColliderUrn);
                break;

            case CharacterPreset.Custom:
                break;
        }
    }

    private void TrySelectFeature(URN urn)
    {
        if (featuresRegistry.IsFeatureRegistered(urn))
            Model.SelectedFeatureUrns.Add(urn);
    }

    public IEnumerable<(URN Urn, Type FeatureType, ActorFeatureAttribute Attribute)> GetRegisteredFeatures()
    {
        if (featuresRegistry is RPGCreator.EngineLib.Registry.ActorFeaturesRegistry concreteRegistry)
        {
            foreach (var (urn, entry) in concreteRegistry.FeatureRegistry)
                yield return (urn, entry.featureType, entry.attribute);
        }
    }

    public void ToggleFeature(URN urn, bool enabled)
    {
        Model.Preset = CharacterPreset.Custom;
        if (enabled)
            Model.SelectedFeatureUrns.Add(urn);
        else
            Model.SelectedFeatureUrns.Remove(urn);
    }

    public void OpenPortraitSelector(IEditorUiRouterLocal router)
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
            ImNotify.Show("Validation Error", "Character name cannot be empty.", NotifyType.Error, 4f);
            return false;
        }

        var actor = new ActorDefinition
        {
            Name = Model.Name,
            Description = Model.Description,
            Category = ActorCategory.Character,
            IconResource = Model.PortraitResourceId
        };

        // Instantiate all selected features
        foreach (var featureUrn in Model.SelectedFeatureUrns)
        {
            var entry = featuresRegistry.GetFeature(featureUrn);
            if (entry == null)
                continue;

            if (Activator.CreateInstance(entry.Value.featureType) is IActorFeatureData featureInstance)
            {
                // Pre-populate base stats with existing StatDefinition default values if Stats feature is added
                if (featureInstance is ActorStatsFeatureData statsFeature)
                {
                    foreach (var statId in assetManager.ListAssets<StatDefinition>())
                    {
                        assetManager.GetAsset<StatDefinition>(statId).OnSuccess(statDef =>
                        {
                            statsFeature.BaseStats[statId] = statDef.DefaultValue;
                        });
                    }
                }

                actor.Features.Add(featureInstance);
            }
        }

        var result = assetManager.SaveAsset(actor);
        if (result.IsFailure)
        {
            ImNotify.Show("Error!", result.Error, NotifyType.Error, 5f);
            return false;
        }

        ImNotify.Show("Success!", $"Character '{actor.Name}' created.", NotifyType.Success, 2f);
        return true;
    }

    public void Clear()
    {
        Model.Name = string.Empty;
        Model.Description = string.Empty;
        Model.PortraitResourceId = Ulid.Empty;
        Model.SelectedFeatureUrns.Clear();
        Model.Preset = CharacterPreset.PlayableHero;
        UnbindTexture();
    }

    private void UnbindTexture()
    {
        if (!Model.PortraitTexture.HasValue)
            return;

        var textureInfo = imGuiRenderer.UnbindTexture(Model.PortraitTexture.Value);
        if (textureInfo is { IsManaged: false })
        {
            textureInfo.Texture.Dispose();
        }

        Model.PortraitTexture = null;
    }
}