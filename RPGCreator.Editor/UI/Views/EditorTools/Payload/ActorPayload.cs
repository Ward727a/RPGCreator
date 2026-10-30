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
using System.Numerics;
using Fluxor;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.Tools.ToolLogics;
using RPGCreator.RuntimeLib.FNA;
using RPGCreator.SDK.Assets.Actors;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.Editor.UI.Views.EditorTools.Payload;

public class ActorPayload(
    IEditorViewContext context,
    EditorViewDefinition definition,
    ActorPayloadController controller)
    : ComplexEditorUi<ActorPayloadController>(context, definition, controller)
{
    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        if (drawAs != EDrawAs.Content)
            return;

        Controller.LoadActorsIfNeeded();

        var currentDir = Controller.State.FacingDirection;
        if (ImGui.BeginCombo("Facing", currentDir.ToString()))
        {
            foreach (var dir in Enum.GetValues<ActorDirection>())
            {
                if (ImGui.Selectable(dir.ToString(), dir == currentDir))
                {
                    Controller.SelectFacingDirection(dir);
                }
            }
            ImGui.EndCombo();
        }

        string filterLabel = Controller.State.CategoryFilter?.ToString() ?? "All Categories";
        if (ImGui.BeginCombo("Filter", filterLabel))
        {
            if (ImGui.Selectable("All Categories", Controller.State.CategoryFilter == null))
            {
                Controller.State.CategoryFilter = null;
            }

            foreach (var cat in Enum.GetValues<ActorCategory>())
            {
                if (ImGui.Selectable(cat.ToString(), Controller.State.CategoryFilter == cat))
                {
                    Controller.State.CategoryFilter = cat;
                }
            }
            ImGui.EndCombo();
        }

        ImGui.Separator();

        if (Controller.State.Actors.Count == 0)
        {
            ImGui.TextDisabled("No actors found in project.");
            return;
        }

        if (ImGui.BeginTable("##ActorPayloadTable", 2, ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn("##Icon", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 28f);
            ImGui.TableSetupColumn("Actor", ImGuiTableColumnFlags.WidthStretch);

            foreach (var actor in Controller.State.Actors)
            {
                if (Controller.State.CategoryFilter.HasValue && actor.Category != Controller.State.CategoryFilter.Value)
                    continue;

                ImGui.PushID(actor.AssetId.ToString());
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                var pos = ImGui.GetCursorPos();
                bool isSelected = Controller.State.SelectedActorId == actor.AssetId;

                if (ImGui.Selectable("##SelectActor", isSelected, ImGuiSelectableFlags.SpanAllColumns, new Vector2(0, 24)))
                {
                    Controller.SelectActor(actor.AssetId);
                }

                ImGui.SetCursorPos(pos);
                var tex = Controller.GetTexture(actor);
                if (tex.HasValue)
                {
                    ImGui.Image(tex.Value, new Vector2(24, 24));
                }
                else
                {
                    ImGui.ColorButton("##NoIcon", new Vector4(0.26f, 0.27f, 0.29f, 1f),
                        ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoBorder | ImGuiColorEditFlags.NoDragDrop,
                        new Vector2(24, 24));
                }

                ImGui.TableNextColumn();
                ImGui.TextEllipsis(actor.Name);

                ImGui.PopID();
            }

            ImGui.EndTable();
        }
    }
}

[EditorUiController]
public class ActorPayloadController(
    ActorPayloadModel state,
    IAssetManager assetManager,
    IResourceManager resourceManager,
    GraphicsDevice graphicsDevice,
    ImGuiRenderer imGuiRenderer,
    IState<ActorLayerToolState> toolState,
    IDispatcher dispatcher)
{
    public readonly ActorPayloadModel State = state;
    private ushort _currentActorVersion = 0;
    private readonly Dictionary<Ulid, ImTextureRef?> _textures = [];

    public void LoadActorsIfNeeded()
    {
        State.ToolState = toolState;

        var latestVersion = assetManager.GetVersion<ActorDefinition>();
        if (_currentActorVersion == latestVersion)
            return;

        ClearTextures();
        _currentActorVersion = latestVersion;

        List<ActorDefinition> actors = [];
        foreach (var id in assetManager.ListAssets<ActorDefinition>())
        {
            assetManager.GetAsset<ActorDefinition>(id).OnSuccess(actors.Add);
        }

        State.Actors = actors;
    }

    public void SelectActor(Ulid actorId)
    {
        dispatcher.Dispatch(new ActorLayerToolActions.SetActor(actorId));
    }

    public void SelectFacingDirection(ActorDirection direction)
    {
        dispatcher.Dispatch(new ActorLayerToolActions.SetFacingDirection(direction));
    }

    public ImTextureRef? GetTexture(ActorDefinition actor)
    {
        var resourceId = actor.IconResource;
        if (resourceId == Ulid.Empty)
            return null;

        if (_textures.TryGetValue(resourceId, out var tex))
            return tex;

        resourceManager.GetResource(resourceId)
            .OnSuccess(data =>
            {
                var tex2D = Texture2D.FromMemory(graphicsDevice, data);
                _textures[resourceId] = imGuiRenderer.BindTexture(tex2D);
            })
            .OnFailure(err =>
            {
                Logger.Once.Error("Failed to load actor icon {0}: {1}", resourceId, err);
                _textures[resourceId] = null;
            });

        return _textures.GetValueOrDefault(resourceId);
    }

    private void ClearTextures()
    {
        foreach (var tex in _textures.Values)
        {
            if (!tex.HasValue) continue;
            var info = imGuiRenderer.UnbindTexture(tex.Value);
            if (info is { IsManaged: false })
                info.Texture.Dispose();
        }
        _textures.Clear();
    }
}

[EditorUiModel]
public class ActorPayloadModel
{
    public IState<ActorLayerToolState> ToolState = null!;
    public List<ActorDefinition> Actors = [];
    public ActorCategory? CategoryFilter = null;

    public Ulid SelectedActorId => ToolState.Value.SelectedActorId;
    public ActorDirection FacingDirection => ToolState.Value.FacingDirection;
}