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
using Hexa.NET.ImGui;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.UI.Controllers.Widgets.AssetsEditor;
using RPGCreator.SDK.Assets.SpriteAnimation;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;
using ImGuiExt = RPGCreator.Editor.ImGuiLib.ImGuiExt;

namespace RPGCreator.Editor.UI.Views.Widgets.AssetsEditor;

public class SpriteAnimationEditor(
    IEditorViewContext context,
    EditorViewDefinition definition,
    SpriteAnimationEditorController controller)
    : ComplexEditorUiWindowBase<SpriteAnimationEditorController>(context, definition, controller)
{
    public override void OnActivate(object? args)
    {
        if (args is not SpriteAnimationDefinition animation)
        {
            Logger.Once.Error("Invalid sprite animation definition provided.");
            Close();
            return;
        }

        Controller.Setup(animation);
    }

    protected override void OnClosed()
    {
        Controller.Clear();
    }

    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        if (drawAs != EDrawAs.Window)
            return;

        if (ImGui.Begin(Name, ref IsOpenInternal, Controller.Model.IsDirty ? ImGuiWindowFlags.UnsavedDocument : ImGuiWindowFlags.None))
        {
            Controller.UpdatePreview(ImGui.GetIO().DeltaTime);

            float footerHeight = ImGui.CalcButtonSize("Save").Y + ImGui.GetStyle().FramePadding.Y * 3f;
            if (ImGui.BeginTable("##SpriteAnimEditor_MainTable", 3, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.Resizable, new Vector2(0, -footerHeight)))
            {
                ImGui.TableSetupColumn("##SettingsAndPreview", ImGuiTableColumnFlags.WidthFixed, 250f);
                ImGui.TableSetupColumn("##SpriteSheetGrid", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("##FramesSequence", ImGuiTableColumnFlags.WidthFixed, 300f);

                ImGui.TableNextColumn();
                DrawSettingsAndPreviewColumn();

                ImGui.TableNextColumn();
                DrawSpriteSheetPickerColumn(footerHeight);

                ImGui.TableNextColumn();
                DrawFramesSequenceColumn();

                ImGui.EndTable();
            }

            ImGui.Separator();
            
            if (ImGui.Button("Save", ImGuiExt.ButtonType.Success))
            {
                Controller.Save();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                Close();
            }
        }
        ImGui.End();
    }

    private void DrawSettingsAndPreviewColumn()
    {
        ImGui.SeparatorText("Animation Settings");

        if (ImGui.InputText("Name", ref Controller.Model.Name, 256))
            Controller.Model.IsDirty = true;

        if (ImGui.Button("Change SpriteSheet"))
        {
            Controller.OpenSpriteSheetSelector(LocalRouter);
        }
        Controller.TryDrawResourceSelector();

        if (ImGui.InputInt("Frame Width", ref Controller.Model.FrameWidth))
        {
            Controller.Model.FrameWidth = Math.Max(1, Controller.Model.FrameWidth);
            Controller.Model.IsDirty = true;
        }

        if (ImGui.InputInt("Frame Height", ref Controller.Model.FrameHeight))
        {
            Controller.Model.FrameHeight = Math.Max(1, Controller.Model.FrameHeight);
            Controller.Model.IsDirty = true;
        }

        if (ImGui.SliderFloat("FPS", ref Controller.Model.Fps, 0.5f, 60f, "%.1f"))
            Controller.Model.IsDirty = true;

        if (ImGui.BeginCombo("Playback", Controller.Model.PlaybackMode.ToString()))
        {
            foreach (var mode in Enum.GetValues<AnimationPlaybackMode>())
            {
                if (ImGui.Selectable(mode.ToString(), mode == Controller.Model.PlaybackMode))
                {
                    Controller.Model.PlaybackMode = mode;
                    Controller.RestartPreview();
                    Controller.Model.IsDirty = true;
                }
            }
            ImGui.EndCombo();
        }

        ImGui.SeparatorText("Live Preview");

        if (ImGui.Button(Controller.Model.IsPlaying ? "Pause" : "Play"))
        {
            Controller.Model.IsPlaying = !Controller.Model.IsPlaying;
        }
        ImGui.SameLine();
        if (ImGui.Button("Restart"))
        {
            Controller.RestartPreview();
        }

        if (Controller.Model.Texture.HasValue && Controller.Model.Frames.Count > 0)
        {
            var idx = Math.Clamp(Controller.Model.CurrentPreviewFrameIndex, 0, Controller.Model.Frames.Count - 1);
            var frame = Controller.Model.Frames[idx];
            var (uv0, uv1) = Controller.GetTileUvs(frame.TileIndex);

            ImGui.TextDisabled($"Frame {idx + 1}/{Controller.Model.Frames.Count} (Tile #{frame.TileIndex})");

            // Scale preview proportionally up to 128x128
            var aspect = (float)Controller.Model.FrameWidth / Math.Max(1, Controller.Model.FrameHeight);
            var previewSize = aspect >= 1f ? new Vector2(128f, 128f / aspect) : new Vector2(128f * aspect, 128f);

            ImGui.Image(Controller.Model.Texture.Value, previewSize, uv0, uv1);
        }
        else
        {
            ImGui.TextDisabled("Add frames from the sprite sheet to preview.");
        }
    }

    private void DrawSpriteSheetPickerColumn(float footerHeight)
    {
        ImGui.SeparatorText("Sprite Sheet (Click a cell to add frame)");

        if (!Controller.Model.Texture.HasValue)
        {
            ImGui.TextDisabled("No sprite sheet selected.");
            return;
        }

        ImGui.BeginChild("##SpriteSheetScrollRegion", new(0, -footerHeight), ImGuiChildFlags.Borders, ImGuiWindowFlags.HorizontalScrollbar);

        var texSize = new Vector2(Controller.Model.TextureSize.X, Controller.Model.TextureSize.Y);
        var origin = ImGui.GetCursorScreenPos();

        // Draw the sprite sheet image
        ImGui.Image(Controller.Model.Texture.Value, texSize);

        var drawList = ImGui.GetWindowDrawList();
        int frameW = Math.Max(1, Controller.Model.FrameWidth);
        int frameH = Math.Max(1, Controller.Model.FrameHeight);
        int cols = Math.Max(1, Controller.Model.TextureSize.X / frameW);
        int rows = Math.Max(1, Controller.Model.TextureSize.Y / frameH);

        uint gridColor = ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 0.35f));
        uint hoverColor = ImGui.GetColorU32(new Vector4(0.2f, 0.8f, 0.3f, 0.45f));

        Vector2 mousePos = ImGui.GetMousePos();
        bool isImageHovered = ImGui.IsItemHovered();

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                ushort tileIndex = (ushort)(r * cols + c);
                Vector2 cellMin = origin + new Vector2(c * frameW, r * frameH);
                Vector2 cellMax = cellMin + new Vector2(frameW, frameH);

                drawList.AddRect(cellMin, cellMax, gridColor);

                if (isImageHovered &&
                    mousePos.X >= cellMin.X && mousePos.X < cellMax.X &&
                    mousePos.Y >= cellMin.Y && mousePos.Y < cellMax.Y)
                {
                    drawList.AddRectFilled(cellMin, cellMax, hoverColor);
                    ImGui.SetTooltip($"Click to add Tile #{tileIndex} ({c}, {r})");

                    if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                    {
                        Controller.AddFrame(tileIndex);
                    }
                }
            }
        }

        ImGui.EndChild();
    }

    private void DrawFramesSequenceColumn()
    {
        ImGui.SeparatorText($"Frames ({Controller.Model.Frames.Count})");

        if (Controller.Model.Frames.Count == 0)
        {
            ImGui.TextDisabled("No frames yet.\nClick cells on the sprite sheet to add frames.");
            return;
        }

        if (ImGui.BeginTable("##FramesSequenceTable", 4, ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY))
        {
            ImGui.TableSetupColumn("##Thumb", ImGuiTableColumnFlags.WidthFixed, 36f);
            ImGui.TableSetupColumn("Tile", ImGuiTableColumnFlags.WidthFixed, 45f);
            ImGui.TableSetupColumn("Speed x", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("##Actions", ImGuiTableColumnFlags.WidthFixed, 75f);
            ImGui.TableHeadersRow();

            int? indexToRemove = null;
            (int Index, int Dir)? moveAction = null;

            for (int i = 0; i < Controller.Model.Frames.Count; i++)
            {
                ImGui.PushID(i);
                var frame = Controller.Model.Frames[i];
                float multiplier = frame.DurationMultiplier;

                ImGui.TableNextRow();

                // Frame Thumbnail
                ImGui.TableNextColumn();
                if (Controller.Model.Texture.HasValue)
                {
                    var (uv0, uv1) = Controller.GetTileUvs(frame.TileIndex);
                    ImGui.Image(Controller.Model.Texture.Value, new Vector2(32, 32), uv0, uv1);
                }

                // Tile Index
                ImGui.TableNextColumn();
                ImGui.Text($"#{frame.TileIndex}");

                // Duration Multiplier
                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(-1f);
                if (ImGui.InputFloat("##Mult", ref multiplier, 0f, 0f, "%.2f"))
                {
                    Controller.Model.Frames[i] = frame with { DurationMultiplier = Math.Max(0.05f, multiplier) };
                    Controller.Model.IsDirty = true;
                }
                ImGui.SetItemTooltip("Duration multiplier (1.0 = normal speed, 2.0 = stays twice as long).");

                ImGui.TableNextColumn();
                ImGui.BeginDisabled(i == 0);
                if (ImGui.SmallButton(ImIcon.UpArrow)) moveAction = (i, -1);
                ImGui.EndDisabled();

                ImGui.SameLine(0f, 2f);
                ImGui.BeginDisabled(i == Controller.Model.Frames.Count - 1);
                if (ImGui.SmallButton(ImIcon.DownArrow)) moveAction = (i, 1);
                ImGui.EndDisabled();

                ImGui.SameLine(0f, 4f);
                if (ImGui.SmallButton(ImIcon.TrashBin)) indexToRemove = i;

                ImGui.PopID();
            }

            if (moveAction.HasValue)
                Controller.MoveFrame(moveAction.Value.Index, moveAction.Value.Dir);
            if (indexToRemove.HasValue)
                Controller.RemoveFrameAt(indexToRemove.Value);

            ImGui.EndTable();
        }
    }
}