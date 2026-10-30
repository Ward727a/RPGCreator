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
using RPGCreator.Editor.UI.Controllers;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.Shared.Types;

namespace RPGCreator.Editor.UI.Views;

public class AssetsManagerUiWindow(IEditorViewContext context, EditorViewDefinition definition, AssetsManagerController controller)
    : ComplexEditorUiWindowBase<AssetsManagerController>(context, definition, controller)
{
    public static ImGuiWindowFlags Flags => ImGuiWindowFlags.NoCollapse;

    private struct MenuItem(string name, string description, URN viewId)
    {
        public readonly string Name = name;
        public readonly string Description = description;
        public readonly URN ViewId = viewId;
    }
    
    private readonly List<MenuItem> _menuButtons = [];

    private IEditorUiBindableView? _shownContent = null;
    private string _selectedMenu = "";

    protected override void OnChildBound(BoundChild child, StringName bindId)
    {
        switch (bindId)
        {
            case "Menu":
                if (child.Payload.TryGet<string>("MenuName", out var menuName) && child.Payload.TryGet<string>("MenuDescription", out var menuDescription))
                {
                    _menuButtons.Add(new MenuItem(menuName, menuDescription, child.ViewId));
                }

                break;
        }
    }

    public override void OnActivate(object? args)
    {
    }

    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        if (ImGui.Begin(Name, ref IsOpenInternal, Flags))
        {
            ImGui.SetNextItemWidth(-1f);
            if (ImGui.BeginTable("AssetsManagerContentsTable", 2))
            {
                ImGui.TableSetupColumn("##Menu", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("##Content", ImGuiTableColumnFlags.WidthStretch);
                
                ImGui.TableNextColumn();
                {
                    ImGui.BeginGroup();
                    foreach (var button in _menuButtons)
                    {
                        var isSelected = _selectedMenu == button.Name;

                        if (isSelected)
                        {
                            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                            ImGui.BeginDisabled();
                        }

                        var pressed = ImGui.Button(button.Name, new Vector2(200, 0f));
                        if (isSelected)
                        {
                            ImGui.EndDisabled();
                            ImGui.PopStyleColor();
                        }

                        if(pressed)
                        {
                            GlobalRouter.ActivateView(button.ViewId).OnSuccess(view => { _shownContent = view; });
                            _selectedMenu = button.Name;
                        }

                        if (string.IsNullOrWhiteSpace(button.Description)) continue;
                        if (!ImGui.BeginItemTooltip()) continue;
                        
                        ImGui.TextUnformatted(button.Description);
                        ImGui.EndTooltip();

                    }

                    ImGui.EndGroup();
                }

                ImGui.TableNextColumn();
                {
                    ImGui.BeginGroup();

                    if (_shownContent != null)
                    {
                        _shownContent.Draw(EDrawAs.Content);
                    }
                    else
                    {
                        ImGui.Text("No content selected");
                    }

                    ImGui.EndGroup();
                }

                ImGui.EndTable();
            }
        }

        ImGui.End();
    }
}