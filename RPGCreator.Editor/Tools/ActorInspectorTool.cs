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
using System.Collections.Immutable;
using System.Linq;
using Fluxor;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.SDK.ActorFeatures;
using RPGCreator.SDK.Assets.Actors;
using RPGCreator.SDK.AssetsPre;
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.Scenes;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.SDK.Services.Map;
using RPGCreator.SDK.Tools;
using RPGCreator.Shared.Attributes;
using Vector2 = System.Numerics.Vector2;

namespace RPGCreator.Editor.Tools;

[Tool("rpgc://tools/actor_inspector")]
public class ActorInspectorTool(
    IState<SceneState> sceneState,
    ISceneManager sceneManager,
    IAssetManager assetManager,
    IActorFeaturesRegistry featuresRegistry,
    IActorFeatureDrawerRegistry featureDrawerRegistry,
    ITypesRegistry typesRegistry,
    ISerializerService serializerService,
    IDispatcher dispatcher) : Tool
{
    public override string DisplayName => "Actor Inspector";
    public override string Description => "Click on a placed actor on an Actor Layer to inspect and override its features for that specific instance.";

    public override IImmutableList<IToolParameter> GetDefaultParameters() => [];

    public override void OnActivate()
    {
        var activeLayer = sceneManager.GetLayer(sceneState.Value.ActiveLayerIndex);
        if (activeLayer is not ActorLayer)
        {
            ImNotify.Show(
                "Actor Layer Required",
                "Select an Actor Layer in the Layer Explorer to use the Actor Inspector tool.",
                NotifyType.Warning,
                4f);
        }
    }

    public override void ClickedAt(IToolLogic logic, Point gridPosition, MouseButton button = MouseButton.Left)
    {
        var activeLayerIndex = sceneState.Value.ActiveLayerIndex;
        var activeLayer = sceneManager.GetLayer(activeLayerIndex);

        if (activeLayer is not ActorLayer actorLayer)
        {
            ImNotify.Show(
                "Invalid Layer!",
                "The Actor Inspector tool can only be used on an Actor Layer.",
                NotifyType.Error,
                3f);
            return;
        }

        if (button == MouseButton.Right)
        {
            if (actorLayer.Has(gridPosition))
            {
                actorLayer.Remove(gridPosition);
            }
            return;
        }

        if (button != MouseButton.Left)
            return;

        var spawnNullable = actorLayer.Get(gridPosition);
        if (spawnNullable == null)
        {
            ImNotify.Show("No Actor Here", $"No actor placed at ({gridPosition.X}, {gridPosition.Y}).", NotifyType.Info, 2f);
            return;
        }

        var spawn = spawnNullable.Value;
        var actorDefResult = assetManager.GetAsset<ActorDefinition>(spawn.ActorDefinitionId);
        if (actorDefResult.IsFailure)
        {
            ImNotify.Show("Error!", $"Could not load ActorDefinition {spawn.ActorDefinitionId}.", NotifyType.Error, 4f);
            return;
        }

        OpenInstanceInspectorModal(actorLayer, gridPosition, spawn, actorDefResult.Value);
    }

    private void OpenInstanceInspectorModal(
        ActorLayer actorLayer,
        Point gridPosition,
        ActorSpawnData currentSpawn,
        ActorDefinition actorDef)
    {
        ImModal.Show($"Edit Actor Instance: {actorDef.Name} ({gridPosition.X}, {gridPosition.Y})", () =>
        {
            // Re-fetch current spawn data from the layer so mutations stay live
            var latestSpawnNullable = actorLayer.Get(gridPosition);
            if (latestSpawnNullable == null)
            {
                ImModal.CloseCurrent();
                return;
            }

            var spawn = latestSpawnNullable.Value;
            bool spawnModified = false;

            ImGui.TextDisabled($"Instance ID: {spawn.InstanceId} | Grid: ({gridPosition.X}, {gridPosition.Y})");
            ImGui.Separator();

            var facing = spawn.FacingDirection;
            if (ImGui.BeginCombo("Facing Direction", facing.ToString()))
            {
                foreach (var dir in Enum.GetValues<ActorDirection>())
                {
                    if (ImGui.Selectable(dir.ToString(), dir == facing))
                    {
                        spawn = spawn with { FacingDirection = dir };
                        spawnModified = true;
                    }
                }
                ImGui.EndCombo();
            }

            ImGui.SeparatorText("Instance Feature Overrides");
            ImGui.TextDisabled("Override default Actor features or add instance-specific features for this map placement only.");

            ImGui.BeginChild("##InstanceFeaturesScroll", new Vector2(560, 340), ImGuiChildFlags.Borders);

            foreach (var baseFeature in actorDef.Features)
            {
                var featureType = baseFeature.GetType();
                var meta = GetFeatureAttribute(featureType);
                string title = meta != null ? $"{meta.DisplayName} [{meta.Category}]" : featureType.Name;

                var existingOverride = spawn.FeatureOverrides.FirstOrDefault(f => f.GetType() == featureType);
                bool isOverridden = existingOverride != null;

                ImGui.PushID(featureType.FullName ?? featureType.Name);

                if (isOverridden)
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGuiExt.Colors.Warning);

                bool open = ImGui.CollapsingHeader(
                    isOverridden ? $"{title} (Overridden)###Header" : $"{title} (Default)###Header",
                    ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.AllowOverlap);

                if (isOverridden)
                    ImGui.PopStyleColor();

                ImGui.SameLine();
                string btnLabel = isOverridden ? "Reset to Default" : "Override on Instance";
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X -
                                    ImGui.CalcButtonSize(btnLabel).X - ImGui.GetStyle().ItemSpacing.X);

                if (ImGui.SmallButton(btnLabel))
                {
                    if (isOverridden)
                    {
                        spawn.FeatureOverrides.Remove(existingOverride!);
                        spawnModified = true;
                    }
                    else
                    {
                        // Clone the base feature from ActorDefinition into FeatureOverrides
                        serializerService.Serialize(baseFeature, featureType, out var json);
                        if (serializerService.Deserialize(json, featureType) is IActorFeatureData cloned)
                        {
                            spawn.FeatureOverrides.Add(cloned);
                            existingOverride = cloned;
                            isOverridden = true;
                            spawnModified = true;
                        }
                    }
                }

                if (open)
                {
                    ImGui.Indent();
                    if (isOverridden && existingOverride != null)
                    {
                        if (featureDrawerRegistry.DrawFeature(existingOverride))
                        {
                            spawnModified = true;
                        }
                    }
                    else
                    {
                        ImGui.BeginDisabled();
                        featureDrawerRegistry.DrawFeature(baseFeature);
                        ImGui.EndDisabled();
                    }
                    ImGui.Unindent();
                    ImGui.Spacing();
                }

                ImGui.PopID();
            }

            ImGui.EndChild();

            if (spawnModified)
            {
                // Re-apply the updated ActorSpawnData to the ActorLayer so the ECS entity updates!
                actorLayer.Add(gridPosition, spawn);
                dispatcher.Dispatch(new SceneStateActions.MapModified());
            }
        });
    }

    private ActorFeatureAttribute? GetFeatureAttribute(Type featureType)
    {
        var urnRes = typesRegistry.GetKey(featureType);
        if (urnRes.IsFailure)
            return null;

        return featuresRegistry.GetFeature(urnRes.Value)?.attribute;
    }
}