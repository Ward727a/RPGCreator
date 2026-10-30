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
using RPGCreator.SDK.ActorFeatures;
using RPGCreator.SDK.Assets.Actors;
using RPGCreator.SDK.Assets.Factions;
using RPGCreator.SDK.Assets.SpriteAnimation;
using RPGCreator.SDK.Assets.Stats;
using RPGCreator.SDK.AssetsPre;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.Editor.UI.Controllers.Widgets.AssetsEditor;

[EditorUiController]
public class CharacterEditorController(
    CharacterEditorModel model,
    IAssetManager assetManager,
    IActorFeaturesRegistry featuresRegistry,
    ITypesRegistry typesRegistry,
    ISerializerService serializerService,
    IResourceManager resourceManager,
    GraphicsDevice graphicsDevice,
    ImGuiRenderer imGuiRenderer,
    IActorFeatureDrawerRegistry drawerRegistry)
{
    public readonly CharacterEditorModel Model = model;
    private IEditorUiBindableView? _resourceSelectorView = null;

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

            LoadPortraitTexture(resource.ResourceId);
            Model.IsDirty = true;
            _resourceSelectorView = null;
        }
    };

    public void Setup(ActorDefinition actor)
    {
        Clear();

        Model.Actor = actor;
        Model.Name = actor.Name;
        Model.Description = actor.Description;
        Model.PortraitResourceId = actor.IconResource;

        Model.Animations = [];
        foreach (var (slotUrn, animSet) in actor.Animations)
        {
            var clonedSet = new DirectionalAnimationSet();
            foreach (var (dir, clipId) in animSet.Clips)
                clonedSet.Clips[dir] = clipId;
            Model.Animations[slotUrn] = clonedSet;
        }

        Model.Features = [];
        foreach (var feature in actor.Features)
        {
            serializerService.Serialize(feature, feature.GetType(), out var json);
            if (serializerService.Deserialize(json, feature.GetType()) is IActorFeatureData clonedFeature)
            {
                Model.Features.Add(clonedFeature);
            }
        }

        if (Model.PortraitResourceId != Ulid.Empty)
        {
            LoadPortraitTexture(Model.PortraitResourceId);
        }

        Model.IsDirty = false;
    }

    private void LoadPortraitTexture(Ulid resourceId)
    {
        UnbindPortraitTexture();
        Model.PortraitResourceId = resourceId;

        resourceManager.GetResource(resourceId).OnSuccess(data =>
        {
            var texture2D = Texture2D.FromMemory(graphicsDevice, data);
            Model.PortraitTexture = imGuiRenderer.BindTexture(texture2D);
        }).OnFailure(err =>
        {
            Logger.Error("Failed to load character portrait {0}: {1}", resourceId, err);
        });
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

    // --- Features Management ---

    public IEnumerable<(URN Urn, Type FeatureType, ActorFeatureAttribute Attribute)> GetAvailableFeaturesToAdd()
    {
        var existingTypes = Model.Features.Select(f => f.GetType()).ToHashSet();

        foreach (var (featureType, attr) in featuresRegistry.Features)
        {
            if (existingTypes.Contains(featureType))
                continue;

            var urnRes = typesRegistry.GetKey(featureType);
            if (urnRes.IsSuccess)
                yield return (urnRes.Value, featureType, attr);
        }
    }

    public ActorFeatureAttribute? GetFeatureMetadata(IActorFeatureData feature)
    {
        var urnRes = typesRegistry.GetKey(feature.GetType());
        if (urnRes.IsFailure)
            return null;

        return featuresRegistry.GetFeature(urnRes.Value)?.attribute;
    }

    public void AddFeature(URN featureUrn)
    {
        var instance = featuresRegistry.CreateFeatureInstance(featureUrn);
        if (instance == null)
            return;

        if (instance is RPGCreator.RuntimeLib.ActorFeaturesData.ActorStatsFeatureData statsFeature)
        {
            SyncAllProjectStatsInto(statsFeature);
        }

        Model.Features.Add(instance);
        Model.IsDirty = true;
    }

    public void RemoveFeatureAt(int index)
    {
        if (index < 0 || index >= Model.Features.Count)
            return;

        Model.Features.RemoveAt(index);
        Model.IsDirty = true;
    }

    public void SyncAllProjectStatsInto(RPGCreator.RuntimeLib.ActorFeaturesData.ActorStatsFeatureData statsFeature)
    {
        foreach (var statId in assetManager.ListAssets<StatDefinition>())
        {
            if (statsFeature.BaseStats.ContainsKey(statId))
                continue;

            assetManager.GetAsset<StatDefinition>(statId).OnSuccess(statDef =>
            {
                statsFeature.BaseStats[statId] = statDef.DefaultValue;
            });
        }
    }
    
    public bool DrawFeature(IActorFeatureData feature)
    {
        return drawerRegistry.DrawFeature(feature);
    }

    // --- Asset Lookups (Stats, Factions, SpriteAnimations) ---

    public IReadOnlyList<StatDefinition> GetAllStats()
    {
        var list = new List<StatDefinition>();
        foreach (var id in assetManager.ListAssets<StatDefinition>())
        {
            assetManager.GetAsset<StatDefinition>(id).OnSuccess(list.Add);
        }
        return list;
    }

    public IReadOnlyList<FactionDefinition> GetAllFactions()
    {
        var list = new List<FactionDefinition>();
        foreach (var id in assetManager.ListAssets<FactionDefinition>())
        {
            assetManager.GetAsset<FactionDefinition>(id).OnSuccess(list.Add);
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

    // --- Animations Management ---

    public void AddAnimationSlot(URN slotUrn)
    {
        if (Model.Animations.ContainsKey(slotUrn))
            return;

        Model.Animations[slotUrn] = new DirectionalAnimationSet();
        Model.IsDirty = true;
    }

    public void RemoveAnimationSlot(URN slotUrn)
    {
        if (Model.Animations.Remove(slotUrn))
            Model.IsDirty = true;
    }

    // --- Save & Cleanup ---

    public void Save()
    {
        Model.Actor.Name = Model.Name;
        Model.Actor.Description = Model.Description;
        Model.Actor.IconResource = Model.PortraitResourceId;
        Model.Actor.Animations = new Dictionary<URN, DirectionalAnimationSet>(Model.Animations);
        Model.Actor.Features = [.. Model.Features];

        var res = assetManager.SaveAsset(Model.Actor);
        if (res.IsFailure)
        {
            ImNotify.Show("Error!", res.Error, NotifyType.Error, 5f);
            return;
        }

        Model.IsDirty = false;
        ImNotify.Show("Saved!", $"Character '{Model.Actor.Name}' saved.", NotifyType.Success, 2f);
    }

    public void Clear()
    {
        UnbindPortraitTexture();
        Model.Animations.Clear();
        Model.Features.Clear();
        Model.IsDirty = false;
    }

    private void UnbindPortraitTexture()
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