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
using RPGCreator.Editor.UI.Controllers.Widgets.AssetsManager;
using RPGCreator.SDK.Modules.Editor.Ui;

namespace RPGCreator.Editor.UI.Views.Widgets.AssetsManager;


public partial class AutoTileAssetManager
{
    public class TilePicker(
        IEditorViewContext viewContext,
        EditorViewDefinition definition,
        AutoTileAssetManagerController.TilePickerController controller)
        : ComplexEditorUiWindowBase<AutoTileAssetManagerController.TilePickerController>(viewContext, definition, controller)
    {
        public override void OnActivate(object? args)
        {
            if (args is AutoTileAssetManagerController.TilePickerArgs arg)
            {
                Controller.Init(arg);
            }
        }

        protected override void OnOpened()
        {
            ImGui.OpenPopup(ImGuiId);
        }

        public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
        {
            ImGui.SetNextWindowSize(new Vector2(300, 350));
            if (ImGui.BeginPopupModal(Name, ImGuiWindowFlags.NoResize))
            {
                if (ImGui.Button("Confirm"))
                {
                    Controller.ConfirmTileIndexSelection();
                    ImGui.CloseCurrentPopup();
                }

                ImGui.SameLine();

                if (ImGui.Button("Cancel"))
                {
                    ImGui.CloseCurrentPopup();
                }

                ImGui.Separator();

                if (ImGui.BeginChild("##tile_pick"))
                {
                    Controller.TilePicker?.Draw();
                    ImGui.EndChild();
                }

                ImGui.EndPopup();
            }
        }
    }
}
