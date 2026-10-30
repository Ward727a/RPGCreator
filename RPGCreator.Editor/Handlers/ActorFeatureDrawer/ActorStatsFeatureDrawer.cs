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
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.RuntimeLib.ActorFeaturesData;
using RPGCreator.SDK.ActorFeatures;
using RPGCreator.SDK.Assets.Stats;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.Editor.Handlers.ActorFeatureDrawer;

[ActorFeatureDrawer(typeof(ActorStatsFeatureData))]
public class ActorStatsFeatureDrawer(IAssetManager assetManager) : IActorFeatureDrawer<ActorStatsFeatureData>
{
    public bool Draw(ActorStatsFeatureData feature)
    {
        bool changed = false;
        var allStats = GetAllStats();
        if (allStats.Count == 0)
        {
            ImGui.TextDisabled("No StatDefinitions exist in the project yet. Create stats in the Stats Manager first.");
            return false;
        }

        if (ImGui.SmallButton("Sync Missing Project Stats"))
        {
            SyncAllProjectStatsInto(feature);
            changed = true;
        }

        if (ImGui.BeginTable("##ActorStatsTable", 4, ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn("Included", ImGuiTableColumnFlags.WidthFixed, 60f);
            ImGui.TableSetupColumn("Stat", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Code", ImGuiTableColumnFlags.WidthFixed, 80f);
            ImGui.TableSetupColumn("Base Value", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableHeadersRow();

            foreach (var statDef in allStats)
            {
                ImGui.PushID(statDef.AssetId.ToString());
                ImGui.TableNextRow();

                bool hasStat = feature.BaseStats.TryGetValue(statDef.AssetId, out double baseVal);

                ImGui.TableNextColumn();
                if (ImGui.Checkbox("##HasStat", ref hasStat))
                {
                    if (hasStat)
                        feature.BaseStats[statDef.AssetId] = statDef.DefaultValue;
                    else
                        feature.BaseStats.Remove(statDef.AssetId);

                    changed = true;
                }

                ImGui.TableNextColumn();
                ImGui.TextEllipsis(statDef.Name);

                ImGui.TableNextColumn();
                ImGui.TextDisabled(statDef.Code);

                ImGui.TableNextColumn();
                if (hasStat)
                {
                    ImGui.SetNextItemWidth(-1f);
                    if (ImGui.InputDouble("##BaseVal", ref baseVal, 0, 0, "%.1f"))
                    {
                        feature.BaseStats[statDef.AssetId] = Math.Clamp(baseVal, statDef.MinValue, statDef.MaxValue);
                        changed = true;
                    }
                }
                else
                {
                    ImGui.TextDisabled("Not used");
                }

                ImGui.PopID();
            }

            ImGui.EndTable();
        }
        
        return changed;
    }
    
    private List<StatDefinition> allStats;
    private ushort _currentStatVersion = 0;

    public IReadOnlyList<StatDefinition> GetAllStats()
    {

        var assetStatVersion = assetManager.GetVersion<StatDefinition>();
        
        if(assetStatVersion == _currentStatVersion)
            return allStats;
        
        _currentStatVersion = assetStatVersion;
        
        allStats = new List<StatDefinition>();
        foreach (var id in assetManager.ListAssets<StatDefinition>())
        {
            assetManager.GetAsset<StatDefinition>(id).OnSuccess(allStats.Add);
        }
        return allStats;
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
}