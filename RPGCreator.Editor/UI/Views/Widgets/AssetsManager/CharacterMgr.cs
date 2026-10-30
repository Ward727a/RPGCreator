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
using RPGCreator.Editor.UI.Controllers.Widgets.AssetsManager;
using RPGCreator.SDK.Modules.Editor.Ui;

namespace RPGCreator.Editor.UI.Views.Widgets.AssetsManager;

public class CharacterMgr(
    IEditorViewContext context,
    EditorViewDefinition definition,
    CharacterMgrController controller)
    : ComplexEditorUi<CharacterMgrController>(context, definition, controller)
{
    public override void OnActivate(object? args)
    {
        Controller.LoadCharacters();
    }

    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        if (drawAs != EDrawAs.Content)
            return;

        if (ImGui.Button("New"))
        {
            LocalRouter.ActivateView("rpgc://views/asset_creator/character_creator").OnSuccess(view =>
            {
                ImModal.Show(view.Name, () => view.Draw(EDrawAs.Modal), true, () => Controller.LoadCharacters());
            });
        }
        ImGui.SameLine();
        
        ImGui.BeginDisabled(Controller.Model.SelectedCharacter == null);
        
        if (ImGui.Button("Edit"))
        {
            GlobalRouter.ActivateView("rpgc://views/asset_editor/character_editor", Controller.Model.SelectedCharacter)
                .OnSuccess(view =>
                {
                    if (view is IEditorUiWindow window)
                        EditorWindowManager.Open(window, MainEditorUiWindow.WindowGroup);
                });
        }
        ImGui.SameLine();
        if(ImGui.Button("Delete", ImGuiExt.ButtonType.Error))
        {
            ImModal.Show("Are you sure?", () =>
            {
                ImGui.Text("Are you sure that you want to delete the selected character?");
                ImGui.TextDisabled("Selected:");
                ImGui.SameLine(0f);
                ImGui.Text(Controller.Model.SelectedCharacter?.Name ?? "[!!ERROR!!]");
                
                if(ImGui.Button("Yes, delete it.", ImGuiExt.ButtonType.Error))
                {
                    Controller.DeleteCharacter(Controller.Model.SelectedCharacter!);
                    Controller.LoadCharacters();
                    ImModal.CloseCurrent();
                }
                
                ImGui.SetItemTooltip("This will permanently delete the selected character from the project!");
                
                ImGui.SameLine();
                if(ImGui.Button("No"))
                    ImModal.CloseCurrent();
            });
        }
        
        ImGui.EndDisabled();
        
        ImGui.Separator();

        if(Controller.Model.Characters.Count <= 0)
            ImGui.TextDisabled("No characters found. Create a new character to see it appear here.");
        else
        {
            if (ImGui.BeginTable("##CharactersList", 2, ImGuiTableFlags.BordersInner | ImGuiTableFlags.RowBg))
            {
                ImGui.TableSetupColumn("Portrait", ImGuiTableColumnFlags.WidthFixed, 32f);
                ImGui.TableSetupColumn("Name");

                ImGui.TableHeadersRow();

                var clipper = ImGui.ImGuiListClipper();

                clipper.Begin(Controller.Model.Characters.Count);

                while (clipper.Step())
                {

                    for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                    {
                        var character = Controller.Model.Characters[i];
                        
                        ImGui.TableNextColumn();
                        
                        var pos = ImGui.GetCursorPos();
                        
                        if(ImGui.Selectable("##"+character.AssetId, Controller.Model.SelectedCharacter == character, ImGuiSelectableFlags.SpanAllColumns, new Vector2(0, 32)))
                            Controller.Model.SelectedCharacter = character;
                        ImGui.SetCursorPos(pos);
                        var texture = Controller.GetTexture(character);
                        if(texture.HasValue)
                            ImGui.Image(texture.Value, new Vector2(32));
                        else
                            ImGui.ColorButton("##NoImg", new Vector4(0.26f, 0.27f, 0.29f, 1f),
                                ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoBorder | ImGuiColorEditFlags.NoDragDrop, new Vector2(32));
                        ImGui.TableNextColumn();
                        ImGui.Text(character.Name);
                    }
                }
                clipper.End();
                ImGui.EndTable();
            }
        }

    }
}