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
using System.Linq;
using System.Numerics;
using Hexa.NET.ImGui;
using MediatR;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Mediator.Messages;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.Services.EngineService;
using RPGCreator.Shared.Types;

namespace RPGCreator.Editor.UI.Views;

public class MainEditorUiWindow : SimpleEditorUiWindow
{
    public const string WindowGroup = "Editor";
    private readonly IMediator _mediator;
    private readonly IGamePlayerService _gamePlayerService;

    private class MenuNode
    {
        public Dictionary<string, MenuNode> SubMenus { get; } = new();
        public Dictionary<string, MenuItem> Items { get; } = new();
    }

    private class MenuItem
    {
        public int Type { get; init; }
        public URN UrnData { get; init; }
    }
    
    private static readonly ScopedLogger Logger = SDK.Common.Logging.Logger.ForContext<MainEditorUiWindow>();
    
    private IEditorUiBindableView? _gamePreviewView;
    
    private MenuNode _menuItems = new();

    public ImGuiWindowFlags Flags =>
        ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoCollapse |
        ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBringToFrontOnFocus |
        ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar |
        ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.MenuBar;

    private HashSet<URN> _openedTools;
    
    private const string OPENED_TOOLS_KEY = "opened_tools";
    
    public MainEditorUiWindow(IEditorViewContext context, EditorViewDefinition definition, IMediator mediator, IGamePlayerService gamePlayerService) : base(context, definition)
    {
        _mediator = mediator;
        _gamePlayerService = gamePlayerService;
        Store.AddOrEdit("hello", "world!");
        
        _openedTools = Store.GetOrDefault<HashSet<URN>>(OPENED_TOOLS_KEY, []);
        if(!Store.Has(OPENED_TOOLS_KEY))
            Store.AddOrEdit(OPENED_TOOLS_KEY, _openedTools);

        GlobalRouter.ActivateView("rpgc://views/game_preview")
            .OnSuccess((view)=>_gamePreviewView = view)
            .OnFailure(error =>
            {
                Logger.Critical(error);
            });
    }

    protected override void OnChildBound(BoundChild child, StringName bindId)
    {
        if (bindId == "menu_item")
        {
            if (child.Payload.TryGet<string>("menu", out var menu) && !string.IsNullOrWhiteSpace(menu))
            {
                var subsMenu = menu.Split('|');

                var lastSubMenuName = subsMenu.Last();
                if (string.IsNullOrWhiteSpace(lastSubMenuName))
                {
                    Logger.Error("Menu: {menu_name} doesn't have a valid end!", args: menu);
                    Logger.Error("Last item MUST be non-empty (not: 'menu|')");
                    return;
                }

                MenuNode rootMenu = _menuItems;
                
                foreach (var subMenuName in subsMenu)
                {
                    if(string.IsNullOrWhiteSpace(subMenuName))
                    {
                        Logger.Warning("Menu: {menu_name} has an empty item like '||'! - Ignoring it", args: menu);
                        continue;
                    }

                    if(lastSubMenuName == subMenuName)
                    {
                        continue;
                    }
                    
                    if (rootMenu.SubMenus.TryGetValue(subMenuName, out rootMenu!))
                    {
                        continue;
                    }

                    rootMenu = new MenuNode();
                    _menuItems.SubMenus.Add(subMenuName, rootMenu);
                }

                child.Payload.TryGet<int>("menuType", out var menuType);
                child.Payload.TryGet<URN>("menuClick_MessageId", out var menuClickedMessageId);
                
                if(menuType == 0)
                {
                    rootMenu.Items.Add(lastSubMenuName, new MenuItem { Type = menuType, UrnData = child.ViewId });
                }
                else
                {
                    rootMenu.Items.Add(lastSubMenuName, new MenuItem { Type = menuType, UrnData = menuClickedMessageId });
                }
            }

        }
    }
    
    private uint _dockspaceId;

    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        ImGuiViewportPtr viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(viewport.WorkPos);
        ImGui.SetNextWindowSize(viewport.WorkSize);
        ImGui.SetNextWindowViewport(viewport.ID);

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));
        var isBegan = ImGui.Begin(Name, Flags);
        ImGui.PopStyleVar();
        if (isBegan)
        {
            _dockspaceId = ImGui.GetID(Name);
            ImGui.DockSpace(_dockspaceId, new Vector2(0.0f, 0.0f), ImGuiDockNodeFlags.None);

            WindowMenuBar();
            if (_gamePreviewView != null)
            {
                _gamePreviewView.Draw();
            }
            else
            {
                ImGui.TextUnformatted("Game preview view is not loaded due to an internal error.");
            }
            
            EditorWindowManager.Draw(WindowGroup);
        }

        ImGui.End();
    }

    private bool _firstTime = true;
    
    public void WindowMenuBar()
    {
        if (ImGui.BeginMenuBar())
        {
            GenerateMenuItems(_menuItems);

            if (ImGui.MenuItem("Play(test)"))
            {
                _gamePlayerService.StartDebugGame();
            }

            ImGui.EndMenuBar();
        }

        if (_firstTime)
        {
            foreach (var openedTool in _openedTools)
            {
                GlobalRouter.ActivateView(openedTool)
                    .OnSuccess(view =>
                    {
                        if (view is IEditorUiWindow window)
                        {
                            EditorWindowManager.Open(window, WindowGroup);
                        }
                    });
            }
            _firstTime = false;
        }
    }

    private void GenerateMenuItems(in MenuNode menuItem)
    {
        foreach (var subMenu in menuItem.SubMenus)
        {
            if(ImGui.BeginMenu(subMenu.Key))
            {
                GenerateMenuItems(subMenu.Value);
                ImGui.EndMenu();
            }
        }
        
        if(menuItem.Items.Count > 0 && menuItem.SubMenus.Count > 0)
            ImGui.Separator();

        foreach (var menuEntry in menuItem.Items)
        {
            if(ImGui.MenuItem(menuEntry.Key))
            {
                var urnData = menuEntry.Value.UrnData;
                if (menuEntry.Value.Type == 0)
                {
                    GlobalRouter.ActivateView(urnData)
                        .OnSuccess(view =>
                        {
                            if (view is IEditorUiWindow window)
                            {
                                if (!window.IsOpen)
                                {
                                    _openedTools.Add(urnData);
                                    EditorWindowManager.Open(window, WindowGroup);
                                }
                                else
                                {
                                    _openedTools.Remove(urnData);
                                    EditorWindowManager.Close(window, WindowGroup);
                                }
                            }
                        });
                }
                else
                {
                    _mediator.Publish(new UiMessage.BindMessage(urnData, []));
                }
            }
        }
    }

    public void RemoveOpenedTool(URN viewId)
    {
        _openedTools.Remove(viewId);
    }
}