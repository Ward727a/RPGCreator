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

using System.Collections.Generic;
using System.Numerics;
using Hexa.NET.ImGui;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.SDK.Assets.AutoTiles;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.Services.Map;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.Editor.UI.Views.Widgets.AssetsCreator;

public class AutoTileCreator : ComplexEditorUi<AutoTileCreatorController>
{
    private float _heightTop;
    
    public AutoTileCreator(IEditorViewContext context, EditorViewDefinition definition, AutoTileCreatorController controller) : base(context, definition, controller)
    {
        _heightTop = ImGui.GetFrameHeight();
    }

    public override void OnActivate(object? args)
    {
        if(Controller.NeedClear)
            Controller.Clear();
    }

    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        ImGui.BeginChild("##auto_tile_creator", new Vector2(0, 0));

        var inTable = ImGui.InTable();
        
        ImGui.BeginTable(Name, 3, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.Resizable);
        ImGui.TableNextColumn();

        if (ImGui.BeginTable("##left_part", 1, ImGuiTableFlags.BordersInnerH))
        {
            inTable = ImGui.InTable();
            var height = ImGui.GetContentRegionAvail().Y - 200f;
            ImGui.TableNextRow(height);
            ImGui.TableSetColumnIndex(0);
            DrawMenuMaterials();

            ImGui.TableNextRow(200f);
            ImGui.TableSetColumnIndex(0);
            DrawRulesetInfo();
            
            ImGui.EndTable();
        }
        
        ImGui.TableNextColumn();
        
        if (ImGui.BeginTable("##content_part", 1, ImGuiTableFlags.BordersInnerH))
        {
            
            ImGui.TableNextRow(_heightTop);
            ImGui.TableSetColumnIndex(0);
            DrawTopBar();
            
            ImGui.TableNextColumn();
            DrawContent();
            
            ImGui.EndTable();
        }
        ImGui.TableNextColumn();
        DrawRulesList();
        
        ImGui.EndTable();
        ImGui.EndChild();
    }

    private string _newMaterialName = string.Empty;

    private void DrawMenuMaterials()
    {
        ImGui.InputText("##materialName", ref _newMaterialName, 256);
        ImGui.SameLine();
        if (ImGui.Button("Add"))
        {
            
        }

        ImGui.Separator();

        if (ImGui.BeginTable("##materials", 2))
        {
            for (byte i = 0; i < 128; i++)
            {
                if (Controller.HasRuleset(i))
                {
                    ImGui.TableNextColumn();
                    ImGui.Text($"{i}");

                    ImGui.TableNextColumn();
                    var name = Controller.GetMaterialName(i);
                    ImGui.Text(string.IsNullOrWhiteSpace(name) ? $"unnamed material" : name);
                }
            }

            ImGui.EndTable();
        }
    }

    private void DrawRulesetInfo()
    {
        ImGui.Text("Ruleset info");
    }

    private void DrawTopBar()
    {
        ImGui.Button("Required");
        ImGui.SameLine();
        ImGui.Button("Exclude");
        ImGui.SameLine();
        ImGui.Button("Ignore");
    }
    
    private void DrawContent()
    {
        ImGui.Text("Content");
    }

    private void DrawRulesList()
    {
        ImGui.Text("Rules");
    }
}

[EditorUiController]
public class AutoTileCreatorController
{
    public AutoTileCreatorModel State;
    private readonly IMaterialRegistry _registry;
    public bool NeedClear { get; private set; }
    
    public AutoTileCreatorController(AutoTileCreatorModel model, IMaterialRegistry registry)
    {
        State = model;
        _registry = registry;
    }

    public void SetNeedClear()
    {
        NeedClear = true;
    }

    public void Clear()
    {
        
    }
    
    public bool HasRuleset(MaterialId id) => _registry.HasRuleset(id);
    public string GetMaterialName(MaterialId id) => _registry.GetMaterialName(id);
}

[EditorUiModel]
public class AutoTileCreatorModel
{
}