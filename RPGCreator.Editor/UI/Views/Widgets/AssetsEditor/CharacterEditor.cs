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
using System.Linq;
using System.Numerics;
using Hexa.NET.ImGui;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.UI.Controllers.Widgets.AssetsEditor;
using RPGCreator.SDK.Assets.Actors;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.Shared.Types;
using ImGuiExt = RPGCreator.Editor.ImGuiLib.ImGuiExt;

namespace RPGCreator.Editor.UI.Views.Widgets.AssetsEditor;

public class CharacterEditor(
    IEditorViewContext context,
    EditorViewDefinition definition,
    CharacterEditorController controller)
    : ComplexEditorUiWindowBase<CharacterEditorController>(context, definition, controller)
{
    public override void OnActivate(object? args)
    {
        if (args is not ActorDefinition actor)
        {
            Logger.Once.Error("Invalid actor definition provided to CharacterEditor.");
            Close();
            return;
        }

        Controller.Setup(actor);
    }

    protected override void OnClosed()
    {
        Controller.Clear();
    }

    private readonly float _footerHeight = ImGui.CalcButtonSize("Save").Y + ImGui.GetStyle().FramePadding.Y * 3f;
    
    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        if (drawAs != EDrawAs.Window)
            return;

        if (ImGui.Begin(Name, ref IsOpenInternal, Controller.Model.IsDirty ? ImGuiWindowFlags.UnsavedDocument : ImGuiWindowFlags.None))
        {

            if (ImGui.BeginTable("##CharacterEditor_MainTable", 2, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.Resizable, new Vector2(0, -_footerHeight)))
            {
                ImGui.TableSetupColumn("##LeftIdentityColumn", ImGuiTableColumnFlags.WidthFixed, 280f);
                ImGui.TableSetupColumn("##RightTabsColumn", ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextColumn();
                DrawInfoColumn();

                ImGui.TableNextColumn();
                if (ImGui.BeginTabBar("##CharacterEditorTabs"))
                {
                    if (ImGui.BeginTabItem($"Features ({Controller.Model.Features.Count})###FeaturesTab"))
                    {
                        DrawFeaturesTab();
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem($"Animations ({Controller.Model.Animations.Count})###AnimationsTab"))
                    {
                        DrawAnimationsTab();
                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar();
                }

                ImGui.EndTable();
            }

            ImGui.Separator();

            if (ImGui.Button("Save", ImGuiExt.ButtonType.Success))
            {
                Controller.Save();
            }

            ImGui.SameLine();
            if (ImGui.Button("Close"))
            {
                Close();
            }
        }
        ImGui.End();
    }

    private void DrawInfoColumn()
    {
        ImGui.SeparatorText("Character Info");

        if (Controller.Model.PortraitTexture.HasValue)
        {
            ImGui.Image(Controller.Model.PortraitTexture.Value, new Vector2(96, 96));
        }
        else
        {
            ImGui.ColorButton("##NoPortrait", new Vector4(0.26f, 0.27f, 0.29f, 1f),
                ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoBorder | ImGuiColorEditFlags.NoDragDrop,
                new Vector2(96, 96));
        }

        ImGui.SameLine();
        if (ImGui.Button("Select Portrait"))
        {
            Controller.OpenPortraitSelector(LocalRouter);
        }
        Controller.TryDrawResourceSelector();

        if (ImGui.InputText("Name", ref Controller.Model.Name, 256))
            Controller.Model.IsDirty = true;

        if (ImGui.InputTextMultiline("Description", ref Controller.Model.Description, 2048, new Vector2(-1, 140)))
            Controller.Model.IsDirty = true;
    }

    private void DrawFeaturesTab()
    {
        ImGui.TextDisabled("Configure modular capabilities attached to this character.");
        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X -
                            ImGui.CalcButtonSize("+ Add Feature").X - ImGui.GetStyle().ItemSpacing.X);

        if (ImGui.Button("+ Add Feature", ImGuiExt.ButtonType.Success))
        {
            ImModal.Show("Add Character Feature", () =>
            {
                var available = Controller.GetAvailableFeaturesToAdd().ToList();
                if (available.Count == 0)
                {
                    ImGui.TextDisabled("All registered features are already added to this character.");
                    return;
                }

                if (ImGui.BeginTable("##AddFeatureModalTable", 3, ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.RowBg))
                {
                    ImGui.TableSetupColumn("Feature", ImGuiTableColumnFlags.WidthFixed, 150f);
                    ImGui.TableSetupColumn("Category", ImGuiTableColumnFlags.WidthFixed, 110f);
                    ImGui.TableSetupColumn("Description", ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableHeadersRow();

                    foreach (var (urn, _, attr) in available)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        var rowSize = ImGui.GetContentRegionAvail();
                        if (ImGui.Selectable($"{attr.DisplayName}##{urn}", false, ImGuiSelectableFlags.SpanAllColumns, rowSize with { X = 0 }))
                        {
                            Controller.AddFeature(urn);
                            ImModal.CloseCurrent();
                        }

                        ImGui.TableNextColumn();
                        ImGui.TextDisabled(attr.Category);

                        ImGui.TableNextColumn();
                        ImGui.TextWrapped(attr.Description);
                    }

                    ImGui.EndTable();
                }
            });
        }

        ImGui.Separator();

        if (Controller.Model.Features.Count == 0)
        {
            ImGui.TextDisabled("No features attached. Click '+ Add Feature' to add capabilities (Stats, Faction, Movement...).");
            return;
        }

        ImGui.BeginChild("##FeaturesScrollRegion", new Vector2(0, -_footerHeight));

        int? featureIndexToRemove = null;

        for (int i = 0; i < Controller.Model.Features.Count; i++)
        {
            var feature = Controller.Model.Features[i];
            var meta = Controller.GetFeatureMetadata(feature);
            string title = meta != null ? $"{meta.DisplayName} [{meta.Category}]" : feature.GetType().Name;

            ImGui.PushID(i);

            bool isOpen = ImGui.CollapsingHeader($"{title}###FeatureHeader_{i}", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.AllowOverlap);

            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X -
                                ImGui.CalcButtonSize("Remove").X - ImGui.GetStyle().ItemSpacing.X);
            if (ImGui.SmallButton("Remove"))
            {
                featureIndexToRemove = i;
            }

            if (isOpen)
            {
                if (meta != null && !string.IsNullOrEmpty(meta.Description))
                {
                    ImGui.TextDisabled(meta.Description);
                }

                ImGui.Indent();
                DrawFeatureEditorBody(feature);
                ImGui.Unindent();
                ImGui.Spacing();
            }

            ImGui.PopID();
        }

        if (featureIndexToRemove.HasValue)
        {
            Controller.RemoveFeatureAt(featureIndexToRemove.Value);
        }

        ImGui.EndChild();
    }

    private void DrawFeatureEditorBody(IActorFeatureData feature)
    {
        if (Controller.DrawFeature(feature))
        {
            Controller.Model.IsDirty = true;
        }
    }

    private void DrawAnimationsTab()
    {
        ImGui.Text("Add Animation Slot (e.g. idle, walk, run, fish):");
        ImGui.SetNextItemWidth(100f);
        ImGui.InputText("##SlotNs", ref Controller.Model.NewAnimSlotNamespace, 64);
        ImGui.SameLine();
        ImGui.Text("://anim_slots/");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(140f);
        ImGui.InputText("##SlotName", ref Controller.Model.NewAnimSlotName, 64);
        ImGui.SameLine();
        if (ImGui.Button("Add Slot"))
        {
            var slotUrn = new URN(Controller.Model.NewAnimSlotNamespace, "anim_slots", Controller.Model.NewAnimSlotName);
            Controller.AddAnimationSlot(slotUrn);
        }

        ImGui.Separator();

        var allSpriteAnims = Controller.GetAllSpriteAnimations();
        URN? slotToRemove = null;

        ImGui.BeginChild("##AnimationsScrollRegion", Vector2.Zero);

        foreach (var (slotUrn, animSet) in Controller.Model.Animations)
        {
            ImGui.PushID(slotUrn.ToString());
            bool open = ImGui.CollapsingHeader($"{slotUrn}###AnimSlot_{slotUrn}", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.AllowOverlap);

            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X -
                                ImGui.CalcButtonSize("Remove").X - ImGui.GetStyle().ItemSpacing.X);
            if (ImGui.SmallButton("Remove"))
            {
                slotToRemove = slotUrn;
            }

            if (open && ImGui.BeginTable($"##DirTable_{slotUrn}", 2, ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.RowBg))
            {
                ImGui.TableSetupColumn("Direction", ImGuiTableColumnFlags.WidthFixed, 110f);
                ImGui.TableSetupColumn("Sprite Animation", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableHeadersRow();

                foreach (var dir in Enum.GetValues<ActorDirection>())
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.Text(dir.ToString());

                    ImGui.TableNextColumn();
                    animSet.Clips.TryGetValue(dir, out var currentAnimId);
                    var currentAnim = allSpriteAnims.FirstOrDefault(a => a.AssetId == currentAnimId);
                    string preview = currentAnim != null ? currentAnim.Name : "None (Fallback)";

                    ImGui.SetNextItemWidth(-1f);
                    if (ImGui.BeginCombo($"##DirAnim_{dir}", preview))
                    {
                        if (ImGui.Selectable("None (Fallback)", currentAnimId == Ulid.Empty))
                        {
                            animSet.Clips.Remove(dir);
                            Controller.Model.IsDirty = true;
                        }

                        foreach (var anim in allSpriteAnims)
                        {
                            if (ImGui.Selectable($"{anim.Name}##{anim.AssetId}", anim.AssetId == currentAnimId))
                            {
                                animSet.Clips[dir] = anim.AssetId;
                                Controller.Model.IsDirty = true;
                            }
                        }

                        ImGui.EndCombo();
                    }
                }

                ImGui.EndTable();
            }

            ImGui.PopID();
        }

        if (slotToRemove.HasValue)
        {
            Controller.RemoveAnimationSlot(slotToRemove.Value);
        }

        ImGui.EndChild();
    }
}
