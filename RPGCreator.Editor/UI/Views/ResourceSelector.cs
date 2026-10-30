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
using Fluxor;
using Hexa.NET.ImGui;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.UI.Controllers;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.Resources;

namespace RPGCreator.Editor.UI.Views;

public class ResourceSelector(IEditorViewContext context, EditorViewDefinition definition, ResourceSelectorController controller)
    : ComplexEditorUi<ResourceSelectorController>(context, definition, controller)
{
    public class ResourceSelectorArguments
    {
        /// <summary>
        /// Allowed resource types to select from.
        /// </summary>
        /// <remarks>
        /// If no resource types are specified, all resource types will be allowed (except for <see cref="IResourceManager.ResourceType.Unknown">Unknown</see>).
        /// </remarks>
        public IResourceManager.ResourceType[] ResourceTypes = [];
        /// <summary>
        /// Action to perform when a resource is selected.<br/>
        /// This action is called with the selected resource's ID.
        /// </summary>
        public Action<IResourceManager.ResourceObject> OnResourceSelected = _ => { };
        /// <summary>
        /// If true, allows importing resources from the operating system (File browser).
        /// </summary>
        public bool AllowImportFromOs = true;
    }
    
    ImGuiExt.FileDialogParameters FileDialogParameters;
    
    public override void OnActivate(object? args)
    {
        if (args is ResourceSelectorArguments arguments)
        {
            Controller.SetArguments(arguments);
            FileDialogParameters = Controller.GetFileDialogParameters((files) =>
            {
                if (files.Length <= 0)
                    return;

                Logger.Debug($"Importing {files.Length} files");
                
                foreach (var file in files)
                {
                    Controller.Import(file);
                }

                Controller.RefreshResourceList();
            });
        }
    }

    public string ModalId => "###resource_selector";

    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        if (!ImGui.IsPopupOpen(ModalId))
        {
            if(Controller.NeedCleanup)
                Controller.Cleanup();
        }
        
        ImGui.SetWindowSize(new Vector2(600, 400), ImGuiCond.Appearing);
        if (ImGui.BeginPopupModal("RPG Creator - Resource Selector###resource_selector"))
        {

            ImGui.Text("Search:");
            ImGui.SameLine();
            if (ImGui.InputTextWithHint("##SearchResource", "", ref Controller.State.SearchText, 255))
            {
                Controller.RefreshResourceList();
            }

            if (Controller.State.AllowImportFromOs)
            {
                ImGui.SameLine();
                ImGui.OpenFileDialogButton("Import from OS", FileDialogParameters);
                ImGui.SetItemTooltip("Import a resource from the operating system");
            }

            if (ImGui.BeginTable("##resource_selector_table", 4, ImGuiTableFlags.BordersInner))
            {
                ImGui.TableSetupColumn("Name");
                ImGui.TableSetupColumn("Type");
                ImGui.TableSetupColumn("Path");
                ImGui.TableSetupColumn("Preview", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableHeadersRow();
                foreach (var resource in Controller.State.Resources)
                {
                    ImGui.TableNextColumn();
                    float startY = ImGui.GetCursorPosY();

                    if (ImGui.Selectable($"##{resource.ResourceId}", false, ImGuiSelectableFlags.SpanAllColumns, new Vector2(0, 64)))
                    {
                        Controller.State.OnResourceSelected?.Invoke(resource);
                    }

                    ImGui.SetCursorPosY(startY);

                    ImGui.TextWrapped(resource.ResourceName);

                    ImGui.TableNextColumn();
                    ImGui.TextWrapped(resource.ResourceType.ToString());

                    ImGui.TableNextColumn();
                    ImGui.TextWrapped(resource.ResourceLocalPath);

                    ImGui.TableNextColumn();
                    if (resource.ResourceType == IResourceManager.ResourceType.Image)
                    {
                        ImGui.Image(Controller.GetImage(resource), new Vector2(64));
                    }
                    else
                    {
                        ImGui.CenteredText(IResourceManager.ResourceIcons[resource.ResourceType], ImGuiExt.CenteringDirection.Horizontal);
                    }
                }

                ImGui.EndTable();
            }

            if(ImGui.Button("Close"))
            {
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }
    }
}