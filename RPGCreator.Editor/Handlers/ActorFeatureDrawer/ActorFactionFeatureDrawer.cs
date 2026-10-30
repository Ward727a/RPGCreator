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
using System.Numerics;
using Hexa.NET.ImGui;
using RPGCreator.RuntimeLib.ActorFeaturesData;
using RPGCreator.SDK.ActorFeatures;
using RPGCreator.SDK.Assets.Factions;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.Editor.Handlers.ActorFeatureDrawer;

[ActorFeatureDrawer(typeof(ActorFactionFeatureData))]
public class ActorFactionFeatureDrawer(IAssetManager assetManager) : IActorFeatureDrawer<ActorFactionFeatureData>
{
    public bool Draw(ActorFactionFeatureData feature)
    {
        bool changed = false;
        string currentName = "None";

        if (feature.FactionId != Ulid.Empty)
        {
            assetManager.GetAsset<FactionDefinition>(feature.FactionId)
                .OnSuccess(f => currentName = f.Name);
        }

        if (ImGui.BeginCombo("Faction", currentName))
        {
            if (ImGui.Selectable("None", feature.FactionId == Ulid.Empty))
            {
                feature.FactionId = Ulid.Empty;
                changed = true;
            }

            foreach (var factionId in assetManager.ListAssets<FactionDefinition>())
            {
                var res = assetManager.GetAsset<FactionDefinition>(factionId);
                if (res.IsFailure) continue;
                var faction = res.Value;

                var fColor = ImGui.ColorConvertU32ToFloat4(faction.ColorRgba);
                ImGui.ColorButton($"##col_{faction.AssetId}", fColor, ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoBorder, new Vector2(14, 14));
                ImGui.SameLine();

                if (ImGui.Selectable($"{faction.Name}##{faction.AssetId}", faction.AssetId == feature.FactionId))
                {
                    feature.FactionId = faction.AssetId;
                    changed = true;
                }
            }

            ImGui.EndCombo();
        }

        return changed;
    }
}