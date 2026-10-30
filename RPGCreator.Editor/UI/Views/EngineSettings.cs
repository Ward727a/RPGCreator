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

using System.Numerics;
using Hexa.NET.ImGui;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.SDK.Modules.Editor.Ui;

namespace RPGCreator.Editor.UI.Views;

public class EngineSettings(IEditorViewContext context, EditorViewDefinition definition) : SimpleEditorUi(context, definition)
{
    public bool IsOpen => _isOpen;
    private bool _isOpen = false;
    public ImGuiWindowFlags Flags { get; private set; } = ImGuiWindowFlags.NoCollapse;

    public void Open()
    {
        _isOpen = true;
    }

    public void Close()
    {
        _isOpen = false;
    }

    private bool _gotEdited;

    private bool _windowTitlebarEnabled = ImConfig.EngineConfig.Global.Gui.ShowTitlebar;

    public override void OnActivate(object? args)
    {
    }

    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        switch (_gotEdited)
        {
            case true when !Flags.HasFlag(ImGuiWindowFlags.UnsavedDocument):
                Flags |= ImGuiWindowFlags.UnsavedDocument;
                break;
            case false when Flags.HasFlag(ImGuiWindowFlags.UnsavedDocument):
                Flags &= ~ImGuiWindowFlags.UnsavedDocument;
                break;
        }

        ImGui.SetNextWindowSizeConstraints(new Vector2(300, 200), Vector2.PositiveInfinity);
        if (ImGui.Begin(Name, ref _isOpen, Flags))
        {
            if (ImGui.BeginTabBar("SettingsTabBar"))
            {
                if (ImGui.BeginTabItem("Engine Settings"))
                {
                    EngineSettingsContent();
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }

            ImGui.Separator();
            ImGui.BeginDisabled(!_gotEdited);
            if (ImGui.CenteredButton("Save Settings", ImGuiExt.CenteringDirection.Horizontal))
            {
                SaveNewSettings();
            }

            ImGui.EndDisabled();
        }

        ImGui.End();
    }

    private void SaveNewSettings()
    {
        ImConfig.EngineConfig.Global.Gui.ShowTitlebar = _windowTitlebarEnabled;

        _gotEdited = false;
    }

    private void EngineSettingsContent()
    {
        ImGui.SetNextWindowSize(ImGui.GetContentRegionAvail() with
        {
            Y = ImGui.GetContentRegionAvail().Y - ImGui.GetFrameHeightWithSpacing() * 1.2f
        });
        if (ImGui.BeginChild("EngineSettings"))
        {
            if (ImGui.Checkbox("Enable Window Titlebar", ref _windowTitlebarEnabled))
            {
                _gotEdited = true;
            }

            if (ImGui.BeginItemTooltip())
            {
                ImGui.Text(
                    "If on true, this will show a titlebar above EVERY window and widget, so you can move them as you want.");
                ImGui.Text(
                    "If on false, this will hide the titlebar above EVERY window and widget, so you won't be able to move them.");
                ImGui.EndTooltip();
            }

            ImGui.EndChild();
        }
    }
}