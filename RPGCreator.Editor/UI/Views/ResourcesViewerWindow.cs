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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.Services;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.Resources;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace RPGCreator.Editor.UI.Views;

public class ResourcesViewerUiWindow : SimpleEditorUiWindow, IDisposable
{
    private readonly IntPtr _windowHandle;
    private readonly IResourceManager _resourceManager;
    private readonly IThumbnailCacheService _thumbnailCacheService;
    public ImGuiWindowFlags Flags => ImGuiWindowFlags.NoCollapse;

    private bool _needRefresh = true;
    private IEnumerable<IResourceManager.ResourceObject> _resources = [];

    private string _searchTerm = "";

    private bool _searchImage = true;
    private bool _searchAudio = true;
    private bool _searchObject = true;

    public ResourcesViewerUiWindow(Game game, IResourceManager resourceManager,
        IThumbnailCacheService thumbnailCacheService, IEditorViewContext context,
        EditorViewDefinition definition) : base(context, definition)
    {
        _windowHandle = game.Window.Handle;
        _resourceManager = resourceManager;
        _thumbnailCacheService = thumbnailCacheService;
    }

    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        if (ImGui.Begin(Name, Flags))
        {
            RenderFilterBar();

            ImGui.Separator();

            RenderResourceTable();

            ImGui.End();
        }
    }

    private void RenderResourceTable()
    {
        // The table will be hard fixed to 12, until I can be certain that the size
        // of the thumbnails and item are not weird with something else.
        if (ImGui.BeginTable("ResourcesList", 12))
        {
            RefreshResources();
            var index = 0;
            foreach (var resource in _resources)
            {
                switch (resource.ResourceType)
                {
                    case IResourceManager.ResourceType.Image when !_searchImage:
                    case IResourceManager.ResourceType.Audio when !_searchAudio:
                    case IResourceManager.ResourceType.Text when !_searchObject:
                        continue;
                }

                if (!resource.ResourceName.StartsWith(_searchTerm))
                {
                    continue;
                }

                ImGui.PushID(index);
                ImGui.TableNextColumn();

                DrawResourceDetails(resource);
                ImGui.PopID();

                index++;
            }

            ImGui.EndTable();
        }
    }

    private void DrawResourceDetails(IResourceManager.ResourceObject resource)
    {
        ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(.20f, 0.20f, 0.20f, 1.0f));

        ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 1.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 4.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);

        var tileSize = ImGui.GetContentRegionAvail();

        tileSize = tileSize with { Y = tileSize.X + ImGui.CalcTextSize(resource.ResourceName, tileSize.X).Y };

        if (ImGui.BeginChild("Border", tileSize, ImGuiChildFlags.Borders,
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
        {
            if (resource.ResourceType == IResourceManager.ResourceType.Image)
            {
                var thumbnail = _thumbnailCacheService.GetThumbnail(resource.ResourceId);

                if (thumbnail.IsSuccess)
                {
                    var imageSize = ImGui.GetContentRegionAvail();
                    ImGui.Image(thumbnail.Value, imageSize with { Y = imageSize.X });
                }
                else
                {
                    ImGui.TextColoredWrapped(new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                        resource.ResourceName);
                    Logger.Error("Failed to generate thumbnail for resource: {id}",
                        resource.ResourceId);
                }
            }

            ImGui.Separator();

            ImGui.PushFont(ImGui.GetIO().FontDefault, 16);
            ImGui.CenteredText(resource.ResourceName, ImGuiExt.CenteringDirection.Horizontal, true);
            ImGui.PopFont();
        }

        ImGui.EndChild();

        ImGui.PopStyleColor();
        ImGui.PopStyleVar(3);

        if (ImGui.BeginItemTooltip())
        {
            ImGui.TextColored(ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled], "Name: ");
            ImGui.SameLine(0f);
            ImGui.Text(resource.ResourceName);
            ImGui.EndTooltip();
        }
    }

    private void RenderFilterBar()
    {
        if (ImGui.BeginTable("SearchBar", 3))
        {
            ImGui.TableNextColumn();

            ImGui.OpenFileDialogButton("testFileImport", new ImGuiExt.FileDialogParameters(_windowHandle,
                (fileSelected) =>
                {
                    var result = _resourceManager.ImportResource(fileSelected[0]);
                    if (result.IsFailure)
                        ImNotify.Show("Error while importing",
                            $"Oops, there was an error while importing: {result.Error}", NotifyType.Error, 5f);
                }));

            ImGui.SetNextItemWidth(-1f);
            ImGui.InputTextWithHint("##SearchingBar", "Search...", ref _searchTerm, 255);

            ImGui.TableNextColumn();

            if (ImGui.Button($"{ImIcon.Filter}##001"))
            {
                ImGui.OpenPopup("MonMenuPopup");
            }

            if (ImGui.BeginPopup("MonMenuPopup"))
            {
                Vector2 currentPadding = ImGui.GetStyle().FramePadding;

                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, currentPadding with { Y = currentPadding.Y - 4.0f });
                ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 4.0f);

                ImGui.Checkbox($"{ImIcon.Image} Image", ref _searchImage);

                ImGui.Spacing();

                ImGui.Checkbox($"{ImIcon.Audio} Audio", ref _searchAudio);
                if (ImGui.BeginItemTooltip())
                {
                    ImGui.Text("Audio can be: Sounds, Music, Voices.");
                    ImGui.EndTooltip();
                }

                ImGui.Spacing();

                ImGui.Checkbox($"{ImIcon.Box} Object", ref _searchObject);

                ImGui.PopStyleVar(2);

                if (ImGui.BeginItemTooltip())
                {
                    ImGui.Text("Object can be: Characters, Items, Potions, or thing like that.");
                    ImGui.EndTooltip();
                }

                ImGui.EndPopup();
            }
        }

        ImGui.EndTable();
    }

    private void RefreshResources()
    {
        if (!_needRefresh)
            return;

        _needRefresh = false;

        _resources = _resourceManager.GetAllResources();
    }

    public void Dispose()
    {
    }
}