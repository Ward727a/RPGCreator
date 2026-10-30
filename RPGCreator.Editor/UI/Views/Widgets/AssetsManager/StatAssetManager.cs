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

public class StatAssetManager(
    IEditorViewContext context,
    EditorViewDefinition definition,
    StatAssetManagerController controller)
    : ComplexEditorUi<StatAssetManagerController>(context, definition, controller)
{

    private IEditorUiBindableView? creatorView = null;
    
    public override void OnActivate(object? args)
    {
        controller.LoadStatsIfNeeded();
    }

    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        if (drawAs != EDrawAs.Content)
            return;

        if (ImGui.Button("New"))
        {
            creatorView = GlobalRouter.ActivateView("rpgc://views/asset_creator/stats").Match(view => view, _ => null);
            
            if(creatorView != null)
                ImModal.Show(creatorView.Name, () => creatorView.Draw(EDrawAs.Content), true, () =>
                {
                    Controller.LoadStatsIfNeeded();
                });
        }
        ImGui.SetItemTooltip("Create a new stat.");

        ImGui.SameLine();

        var combinedWidth = ImGui.CalcButtonSize("Edit").X + ImGui.CalcButtonSize("Delete").X + ImGui.GetStyle().ItemSpacing.X;
        var fullRightX = ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - combinedWidth;
        
        ImGui.SetCursorPosX(fullRightX);
        
        ImGui.BeginDisabled(Controller.Model.SelectedStat == null);
        if (ImGui.Button("Edit"))
        {
            LocalRouter.ActivateView("rpgc://views/stat_asset_editor", Controller.Model.SelectedStat).OnSuccess(view =>
            {
                if (view is IEditorUiWindow window)
                {
                    EditorWindowManager.Open(window, MainEditorUiWindow.WindowGroup);
                }
            });
        }
        ImGui.SameLine();
        if (ImGui.Button("Delete", ImGuiExt.ButtonType.Error))
        {
            ImModal.Show("Delete Stat", () =>
            {
                ImGui.Text("Are you sure you want to delete this stat? You will not be able to recover it!");
                if (ImGui.Button("No, DO NOT delete it", ImGuiExt.ButtonType.Success))
                {
                    ImModal.CloseCurrent();
                }
                ImGui.SameLine();
                if (ImGui.Button("Yes, delete it", ImGuiExt.ButtonType.Error))
                {
                    var result = Controller.DeleteStat(Controller.Model.SelectedStat);

                    if (result.IsFailure)
                    {
                        ImNotify.Show("Error!", "Could not delete stat! " + result.Error, NotifyType.Error, 5f);
                    }
                    else
                    {
                        ImModal.CloseCurrent();
                    }
                    Controller.LoadStatsIfNeeded();
                    Controller.Model.SelectedStat = null;
                }
            }, true);
        }
        ImGui.EndDisabled();
        
        ImGui.Separator();

        if (Controller.Model.Stats.Count > 0)
        {
            ImGui.BeginTable("StatsTable", 4, ImGuiTableFlags.BordersInner | ImGuiTableFlags.Resizable);
            ImGui.TableSetupColumn("##icon", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 32f);
            ImGui.TableSetupColumn("Code");
            ImGui.TableSetupColumn("Name");
            ImGui.TableSetupColumn("Kind");
            
            ImGui.TableHeadersRow();
            
            var clipper = new ImGuiListClipper();

            clipper.Begin(Controller.Model.Stats.Count);

            while (clipper.Step())
            {
                for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var stat = Controller.Model.Stats[i];
                    
                    ImGui.TableNextColumn();
                    var startY = ImGui.GetCursorPosY();
                    if (ImGui.Selectable($"##{stat.AssetId.ToString()}",
                            Controller.Model.SelectedStat?.AssetId == stat.AssetId, ImGuiSelectableFlags.SpanAllColumns,
                            new Vector2(0, 32)))
                    {
                        Controller.Model.SelectedStat = stat;
                    }
                    
                    ImGui.SetCursorPosY(startY);
                    var texture = Controller.GetTexture(stat.IconResource);
                    if(texture.HasValue)
                        ImGui.Image(texture.Value, new Vector2(32f, 32f));
                    else
                        ImGui.TextEllipsis("NoImg");
                    ImGui.TableNextColumn();
                    ImGui.TextEllipsis(stat.Code);
                    ImGui.TableNextColumn();
                    ImGui.TextEllipsis(stat.Name);
                    ImGui.TableNextColumn();
                    ImGui.TextEllipsis(stat.Kind.ToString());
                }
            }

            clipper.End();
            ImGui.EndTable();
        }
        else
        {
            ImGui.Text("No stats found.");
        }
    }
}