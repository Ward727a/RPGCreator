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

using System.Collections.Generic;
using System.Numerics;
using Fluxor;
using Hexa.NET.ImGui;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.UI.Controllers.Widgets;
using RPGCreator.EngineLib.Services.Modules.Features;
using RPGCreator.EngineLib.Services.Modules.Features.UiFeatureProvider;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.Scenes;
using RPGCreator.Shared.Types;

namespace RPGCreator.Editor.UI.Views.Widgets;

/// <summary>
/// A game preview is the widget that allow to show the game in realtime inside the Editor.
/// </summary>
public class GamePreview : ComplexEditorUi<GamePreviewController>
{
    private readonly EditorUiRegistry _editorUiRegistry;
    private readonly IState<SceneState> _sceneState;
    private ImAnimPointer<float> ImAnimTopLeftMenuId;
    private ImAnimPointer<float> ImAnimRightMenuId;
    private ImAnimPointer<float> ImAnimRightButtonId;

    public GamePreview(IEditorViewContext context, EditorViewDefinition definition, GamePreviewController controller,
        EditorUiRegistry editorUiRegistry, IState<SceneState> sceneState) :
        base(context, definition, controller)
    {
        _editorUiRegistry = editorUiRegistry;
        _sceneState = sceneState;
    }

    private static readonly StringName INSIDE_BUTTON_BIND_ID = new("inside_button");
    private const string INSIDE_BUTTON_TEXT_PARAMETER = "button_text";
    private const string INSIDE_BUTTON_DESCRIPTION_PARAMETER = "button_description";

    private static readonly StringName RIGHT_PARAMETER_SECTION_ID = new("right_section");
    private const string RIGHT_SECTION_TITLE_PARAMETER = "section_title";
    private const string RIGHT_SECTION_DESCRIPTION_PARAMETER = "section_description";

    private class ButtonItem(URN viewId, string buttonText, string buttonDescription)
    {
        public URN ViewId { get; } = viewId;
        public string ButtonText { get; } = buttonText;
        public string ButtonDescription { get; } = buttonDescription;
    }

    private class RightSectionItem(URN viewId, string sectionTitle, string sectionDescription)
    {
        public URN ViewId { get; } = viewId;
        public IEditorUiBindableView? SectionView { get; set; }
        public string SectionTitle { get; } = sectionTitle;
        public string SectionDescription { get; } = sectionDescription;
    }

    private readonly Dictionary<URN, ButtonItem> _buttonItems = [];
    private readonly Dictionary<URN, RightSectionItem> _rightSectionItems = [];

    protected override void OnChildBound(BoundChild child, StringName bindId)
    {
        if (bindId == INSIDE_BUTTON_BIND_ID)
        {
            var payload = child.Payload;
            var viewId = child.ViewId;

            if (payload.TryGet<string>(INSIDE_BUTTON_TEXT_PARAMETER, out var buttonText))
            {
                var description = "";
                if (payload.TryGet<string>(INSIDE_BUTTON_DESCRIPTION_PARAMETER, out var buttonDescription))
                {
                    description = buttonDescription;
                }

                var buttonItem = new ButtonItem(viewId, buttonText, description);

                _buttonItems.Add(viewId, buttonItem);
            }
        }

        if (bindId == RIGHT_PARAMETER_SECTION_ID)
        {
            var payload = child.Payload;
            var viewId = child.ViewId;

            if (payload.TryGet<string>(RIGHT_SECTION_TITLE_PARAMETER, out var sectionTitle))
            {
                var description = "";
                if (payload.TryGet<string>(RIGHT_SECTION_DESCRIPTION_PARAMETER, out var sectionDescription))
                {
                    description = sectionDescription;
                }

                var rightSectionItem = new RightSectionItem(viewId, sectionTitle, description);
                _rightSectionItems.Add(viewId, rightSectionItem);
            }
        }
    }

    protected override void OnChildUnbound(BoundChild child, StringName bindId)
    {
        if (bindId == RIGHT_PARAMETER_SECTION_ID)
        {
            _rightSectionItems.Remove(child.ViewId);
        }
    }

    private const int BUTTON_ITEM_WIDTH = 100;
    private const int BUTTON_ITEM_HEIGHT = 30;
    private const int BUTTON_ITEM_SPACING = 20;
    private const float BUTTON_ITEM_PADDING = 4f;

    private float MultiplierPosY => ImAnim.Get(ImAnimTopLeftMenuId);
    private float MultiplierRightMenuPosX => ImAnim.Get(ImAnimRightMenuId);
    private float MultiplierRightButtonPosX => ImAnim.Get(ImAnimRightButtonId);

    public override void OnActivate(object? args)
    {
        Controller.Init();
        ImAnimTopLeftMenuId = ImAnim.Create(1f, -1f, .2f);
        ImAnimRightMenuId = ImAnim.Create(0f, 1f, .2f);
        ImAnimRightButtonId = ImAnim.Create(0f, 1f, .2f);
        ImAnim.Stop(ImAnimTopLeftMenuId);
        ImAnim.Stop(ImAnimRightMenuId);
        ImAnim.Stop(ImAnimRightButtonId);
    }

    public override void OnDeactivate()
    {
        _buttonItems.Clear();
        ImAnim.Destroy(ImAnimTopLeftMenuId);
        ImAnim.Destroy(ImAnimRightMenuId);
        ImAnim.Destroy(ImAnimRightButtonId);
    }

    private bool _previewHovered = false;
    private uint _dockspaceId;
    
    public override unsafe void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        // The window constraints are used to prevent the window from beginning at an invalid size ([0, x] or [x, 0]).
        ImGui.SetNextWindowSizeConstraints(new Vector2(50, 50), Vector2.PositiveInfinity);
        var flag = _sceneState.Value.HasUnsavedChanges ? ImGuiWindowFlags.UnsavedDocument : ImGuiWindowFlags.None;
        
        _dockspaceId = ImGui.GetID(Name);
        if (ImGui.Begin("Game preview", flag))
        {
            var availableSize = ImGui.GetContentRegionAvail();

            ImGui.DockSpace(_dockspaceId, availableSize, ImGuiDockNodeFlags.PassthruCentralNode | ImGuiDockNodeFlags.NoDockingOverCentralNode);

            ImGuiDockNodePtr centralNode = ImGuiP.DockBuilderGetCentralNode(_dockspaceId);
            if (centralNode.Handle != null) 
            {
                Vector2 centralSize = centralNode.Size; 
                Vector2 centralScreenPos = centralNode.Pos; 
                
                Controller.ResizePreviewWindowIfNeeded(centralSize, ImGui.IsMouseDown(ImGuiMouseButton.Left));

                ImGui.SetCursorScreenPos(centralScreenPos);

                ImGui.BeginChild("###CentralWorkspace", centralSize, ImGuiChildFlags.None, 
                    ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

                float top = 0f; 
                float right = centralSize.X - 8;

                ImGui.Image(Controller.State.GamePreviewRef, centralSize);
                
                if (_previewHovered)
                {
                    int mouseDown = 0;
                    mouseDown += ImGui.IsMouseDown(ImGuiMouseButton.Left) ? 1 : 0;
                    mouseDown += ImGui.IsMouseDown(ImGuiMouseButton.Right) ? 2 : 0;
                    mouseDown += ImGui.IsMouseDown(ImGuiMouseButton.Middle) ? 4 : 0;

                    bool currentDown = mouseDown > 0;
                    Controller.HoverOnPreview(mouseDown);

                    if (!currentDown && Controller.WasClicked) Controller.ReleasedOnPreview(mouseDown);
                    if (currentDown && !Controller.WasClicked) Controller.ClickOnPreview(mouseDown);
                }
                else
                {
                    Controller.StopHoveringPreview();
                }

                _previewHovered = ImGui.IsItemHovered();

                if (_buttonItems.Count > 0)
                {
                    ImGui.SetCursorPos(new Vector2(0, top)); 
                    
                    if (ImAnim.IsActive(ImAnimTopLeftMenuId) ||
                        (ImAnim.IsReverse(ImAnimTopLeftMenuId) && ImAnim.IsAtEnd(ImAnimTopLeftMenuId)))
                    {
                        DrawQuickMenu(top);
                    }

                    if (ImAnim.IsAtEnd(ImAnimTopLeftMenuId) && !ImAnim.IsReverse(ImAnimTopLeftMenuId)) 
                        DrawDownArrow(top);
                    else 
                        DrawUpArrow(top);
                }

                if (_rightSectionItems.Count > 0 || true)
                {
                    ImGui.SetCursorPos(new Vector2(0, top)); // Reset
                    
                    if (ImAnim.IsActive(ImAnimRightMenuId) ||
                        (!ImAnim.IsReverse(ImAnimRightMenuId) && ImAnim.IsAtEnd(ImAnimRightMenuId)))
                    {
                        DrawRightSection(top, right);
                    }

                    DrawCogOpen(top, right);
                }

                ImGui.EndChild();
            }
        }
        else
        {
            ImGui.DockSpace(_dockspaceId, new Vector2(0.0f, 0.0f), ImGuiDockNodeFlags.KeepAliveOnly);
        }

        ImGui.End();
    }

    private void DrawDownArrow(float top)
    {
        ImGui.SetCursorPos(new Vector2(ImGui.GetCursorPosX() + 2, top + 2));
        var posY = ImGui.GetCursorPosY();
        var posX = ImGui.GetCursorPosX();
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.06f, 0.06f, 0.06f, .4f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.06f, 0.06f, 0.06f, .6f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.06f, 0.06f, 0.06f, .8f));
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 6f);
        if (ImGui.Button("###Down", new Vector2(20)))
        {
            ImAnim.Reverse(ImAnimTopLeftMenuId);
        }

        ImGui.SetCursorPosX(posX + 2.5f);
        ImGui.SetCursorPosY(posY + 1.5f);
        ImGui.Text(ImIcon.DownArrow);

        ImGui.PopStyleColor(3);
        ImGui.PopStyleVar();
    }

    private void DrawUpArrow(float top)
    {
        ImGui.SetCursorPos(new Vector2(ImGui.GetCursorPosX() + 2, top + 2));
        var posY = ImGui.GetCursorPosY();
        var posX = ImGui.GetCursorPosX();
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.06f, 0.06f, 0.06f, .4f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.06f, 0.06f, 0.06f, .6f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.06f, 0.06f, 0.06f, .8f));
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 6f);
        if (ImGui.Button("###Up", new Vector2(20)))
        {
            if (ImAnim.IsReverse(ImAnimTopLeftMenuId))
                ImAnim.Reverse(ImAnimTopLeftMenuId);
            else
                ImAnim.Start(ImAnimTopLeftMenuId);
        }

        ImGui.SetCursorPosX(posX + 2.5f);
        ImGui.SetCursorPosY(posY + 1.5f);
        ImGui.Text(ImIcon.UpArrow);

        ImGui.PopStyleColor(3);
        ImGui.PopStyleVar();
    }

    private void DrawQuickMenu(float top)
    {
        ImGui.SetCursorPos(new Vector2(BUTTON_ITEM_SPACING + 12, top * MultiplierPosY));
        unsafe
        {
            ImGui.PushStyleColor(ImGuiCol.ChildBg,
                (*ImGui.GetStyleColorVec4(ImGuiCol.WindowBg)) with { W = .25f });

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,
                new Vector2(BUTTON_ITEM_PADDING, BUTTON_ITEM_PADDING));

            if (ImGui.BeginChild("###inside_button_gamepreview",
                    new Vector2(BUTTON_ITEM_WIDTH + BUTTON_ITEM_PADDING * 2,
                        (BUTTON_ITEM_HEIGHT + BUTTON_ITEM_PADDING * 2) * _buttonItems.Count),
                    ImGuiChildFlags.AlwaysUseWindowPadding))
            {
                foreach (var buttonItemData in _buttonItems)
                {
                    var buttonItem = buttonItemData.Value;
                    if (ImGui.Button(buttonItem.ButtonText,
                            new Vector2(BUTTON_ITEM_WIDTH, BUTTON_ITEM_HEIGHT)))
                    {
                        _editorUiRegistry.AttachBinding(new EditorUiBinding() { });
                    }

                    if (!string.IsNullOrWhiteSpace(buttonItem.ButtonDescription))
                    {
                        ImGui.SetItemTooltip(buttonItem.ButtonDescription);
                    }
                }
            }

            ImGui.EndChild();
            ImGui.PopStyleColor();
            ImGui.PopStyleVar();
        }
    }



    private bool first_time_pressed_cog = true;
    private void DrawCogOpen(float top, float right)
    {
        ImGui.SetCursorPosX((right - ((300) * MultiplierRightButtonPosX) - 14));
        ImGui.SetCursorPosY(top + 2);
        var posY = ImGui.GetCursorPosY();
        var posX = ImGui.GetCursorPosX();
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.06f, 0.06f, 0.06f, .4f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.06f, 0.06f, 0.06f, .6f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.06f, 0.06f, 0.06f, .8f));
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 6f);
        if (ImGui.Button("###CogOpen", new Vector2(20)))
        {
            if (first_time_pressed_cog)
            {
                first_time_pressed_cog = false;
                ImAnim.Start(ImAnimRightMenuId);
                ImAnim.Start(ImAnimRightButtonId);
            }
            else
            {
                ImAnim.Reverse(ImAnimRightMenuId);
                ImAnim.Reverse(ImAnimRightButtonId);
            }
        }

        if (_previewHovered && ImGui.IsItemHovered())
            _previewHovered = false;

        ImGui.SetCursorPosX(posX + 2.5f);
        ImGui.SetCursorPosY(posY + 1.5f);
        ImGui.Text(ImIcon.Cog);

        ImGui.PopStyleColor(3);
        ImGui.PopStyleVar();
    }

    private void DrawRightSection(float top, float right)
    {
        ImGui.SetCursorPos(new Vector2((right - ((300 - 8) * MultiplierRightMenuPosX)), top));
        unsafe
        {
            ImGui.PushStyleColor(ImGuiCol.ChildBg,
                (*ImGui.GetStyleColorVec4(ImGuiCol.WindowBg)));

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,
                new Vector2(BUTTON_ITEM_PADDING, BUTTON_ITEM_PADDING));

            if (ImGui.BeginChild("###parameter_section_gamepreview",
                    new Vector2(300,
                        0),
                    ImGuiChildFlags.AlwaysUseWindowPadding))
            {
                if (_rightSectionItems.Count == 0)
                {
                    ImGui.Text("Nothing there! Try adding plugins :)");
                }
                else
                {
                    var index = 0;
                    foreach (var rightSectionData in _rightSectionItems)
                    {
                        var item = rightSectionData.Value;
                        if (ImGui.CollapsingHeader($"{item.SectionTitle}##{index}"))
                        {
                            if (item.SectionView == null)
                            {
                                GlobalRouter.ActivateView(item.ViewId).OnSuccess(view => { item.SectionView = view; });
                            }

                            item.SectionView?.Draw();
                        }

                        ImGui.SetItemTooltip(item.SectionDescription);
                        index++;
                    }
                }
            }

            ImGui.EndChild();
            ImGui.PopStyleColor();
            ImGui.PopStyleVar();
        }
    }
}