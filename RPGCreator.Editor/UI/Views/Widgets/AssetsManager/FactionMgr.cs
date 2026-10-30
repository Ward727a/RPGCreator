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

using System.Globalization;
using Hexa.NET.ImGui;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.UI.Controllers.Widgets.AssetsManager;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;

namespace RPGCreator.Editor.UI.Views.Widgets.AssetsManager;

public class FactionMgr(
    IEditorViewContext context,
    EditorViewDefinition definition,
    FactionMgrController controller) : ComplexEditorUi<FactionMgrController>(context, definition, controller)
{

    private IEditorUiBindableView? _factionCreatorView = null;
    
    public override void OnActivate(object? args)
    {
        Controller.LoadFaction();
    }

    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        if (drawAs != EDrawAs.Content)
            return;

        if (ImGui.Button("New"))
        {
            _factionCreatorView = GlobalRouter.ActivateView("rpgc://views/asset_creator/faction")
                .Match(view => view, _ => null);
            if (_factionCreatorView != null)
            {
                ImModal.Show(_factionCreatorView.Name, ()=>_factionCreatorView.Draw(EDrawAs.Content), true, ()=>Controller.LoadFaction());
            }
        }
        ImGui.SameLine();
        ImGui.BeginDisabled(Controller.Model.SelectedFaction == null);
        if (ImGui.Button("Edit"))
        {
            LocalRouter.ActivateView("rpgc://views/faction_editor", Controller.Model.SelectedFaction)
                    .OnSuccess(view =>
                    {
                        if(view is IEditorUiWindow window)
                            EditorWindowManager.Open(window, MainEditorUiWindow.WindowGroup);
                    })
                    .OnFailure(err =>
                    {
                        Logger.Error("Failed to open faction editor due to error: {0}", err);
                    });
        }
        ImGui.SameLine();
        if (ImGui.Button("Delete", ImGuiExt.ButtonType.Error))
        {
            if (Controller.Model.SelectedFaction != null)
            {
                ImModal.Show("Are you sure?", () =>
                {
                    ImGui.Text("Are you sure you want to delete ");
                    ImGui.SameLine(0f);
                    ImGui.TextDisabled(Controller.Model.SelectedFaction?.Name ?? "[ERROR-UNKNOWN]");
                    ImGui.SameLine(0f);
                    ImGui.Text(" faction?");
                    ImGui.Text("This will not be recoverable!");

                    ImGui.Separator();
                    if (ImGui.Button("Yes, delete it."))
                    {
                        Controller.DeleteFaction(Controller.Model.SelectedFaction!);
                        Controller.Model.SelectedFaction = null;
                        Controller.LoadFaction();
                        ImModal.CloseCurrent();
                    }

                    ImGui.SameLine();
                    if (ImGui.Button("No, do not delete it."))
                    {
                        ImModal.CloseCurrent();
                    }
                });
            }
        }
        ImGui.EndDisabled();
        
        ImGui.Separator();

        if (ImGui.BeginTable("##FactionList", 2, ImGuiTableFlags.BordersInner))
        {
            ImGui.TableSetupColumn("Name");
            ImGui.TableSetupColumn("Default Relation");
            
            ImGui.TableHeadersRow();

            var clipper = ImGui.ImGuiListClipper();
            
            clipper.Begin(Controller.Model.Factions.Count);

            while (clipper.Step())
            {
                for(int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var faction = Controller.Model.Factions[i];
                    ImGui.TableNextColumn();
                    var pos = ImGui.GetCursorPos();
                    if (ImGui.Selectable("##" + i, faction == Controller.Model.SelectedFaction, ImGuiSelectableFlags.SpanAllColumns))
                        Controller.Model.SelectedFaction = faction;
                    ImGui.SetCursorPos(pos);
                    ImGui.Text(faction.Name);
                    ImGui.TableNextColumn();
                    ImGui.Text(faction.DefaultRelation.ToString("N0", CultureInfo.InvariantCulture));
                }
            }
            
            clipper.End();
            
            ImGui.EndTable();
        }
    }
}