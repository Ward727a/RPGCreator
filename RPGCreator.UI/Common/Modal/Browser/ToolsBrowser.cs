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
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Threading;
using Projektanker.Icons.Avalonia;
using RPGCreator.SDK;
using RPGCreator.SDK.Commands;
using RPGCreator.SDK.GlobalState;
using RPGCreator.SDK.Modules.UIModule;

namespace RPGCreator.UI.Common.Modal.Browser;

public class ToolsBrowser : UserControl
{

    private Grid? _body;
    private AutoCompleteBox? _searchBox;
    private ToggleSwitch? _showEvenAddedToolsToggle;
    private ScrollViewer? _scrollViewer;
    private ItemsControl? _itemsControl;
    private WrapPanel? _wrapPanel;
    
    private readonly ObservableCollection<ToolLogic> _toolsSortedByName;
    
    public ToolsBrowser()
    {
        _toolsSortedByName = new ObservableCollection<ToolLogic>(RegistryServices.ToolRegistry.RegisteredTools);
        CreateComponents();
        RegisterEvents();
        LinkToExtension();
    }

    private void CreateComponents()
    {
        _body = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto, Auto, *")
        };

        _searchBox = new AutoCompleteBox
        {
            Margin = new Thickness(5),
            Watermark = "Search tools...",
            ItemsSource = RegistryServices.ToolRegistry.RegisteredTools,
            ItemTemplate = new FuncDataTemplate<ToolLogic>((tool, scope) =>
            {
                if(EngineServices.EngineConfig.ToolsShortcuts.Contains(tool.ToolUrn))
                {
                    return null;
                }
                var textBlock = new TextBlock
                {
                    Text = tool.DisplayName,
                    Margin = new Thickness(5)
                };
                ToolTip.SetTip(textBlock, tool.Description);
                return textBlock;
            })
        };
        Grid.SetRow(_searchBox, 0);
        _body.Children.Add(_searchBox);
        
        _showEvenAddedToolsToggle = new ToggleSwitch
        {
            Margin = new Thickness(5),
            Content = "Show tools already in shortcuts"
        };
        Grid.SetRow(_showEvenAddedToolsToggle, 1);
        _body.Children.Add(_showEvenAddedToolsToggle);

        _scrollViewer = new ScrollViewer
        {
            Margin = new Thickness(5),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };
        Grid.SetRow(_scrollViewer, 2);
        _body.Children.Add(_scrollViewer);

        _itemsControl = new ItemsControl()
        {
            ItemsSource = _toolsSortedByName,
            ItemsPanel = new FuncTemplate<Panel?>(() =>
            {
                _wrapPanel = new WrapPanel
                {
                    Orientation = Orientation.Horizontal,
                    ItemWidth = 200,
                    ItemHeight = 100
                };
                return _wrapPanel;
            }),
            ItemTemplate = new FuncDataTemplate<ToolLogic>(MakeToolItem)
        };
        _scrollViewer.Content = _itemsControl;

        Content = _body;
    }

    private void RegisterEvents()
    {
        _searchBox?.TextChanged += (s, e) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                _toolsSortedByName.Clear();
                foreach (var tool in RegistryServices.ToolRegistry.RegisteredTools)
                {
                    // We don't want to show tools that are already in the shortcuts, as they can be accessed from there.
                    if (!_showEvenAddedToolsToggle?.IsChecked == true && EngineServices.EngineConfig.ToolsShortcuts.Contains(tool.ToolUrn))
                        continue;
                    if (tool.DisplayName.Contains(_searchBox?.Text ?? "", StringComparison.OrdinalIgnoreCase))
                    {
                        _toolsSortedByName.Add(tool);
                    }
                }
            }, DispatcherPriority.Background);
        };
        _showEvenAddedToolsToggle?.IsCheckedChanged += (s, e) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                _toolsSortedByName.Clear();
                foreach (var tool in RegistryServices.ToolRegistry.RegisteredTools)
                {
                    if (!_showEvenAddedToolsToggle?.IsChecked == true && EngineServices.EngineConfig.ToolsShortcuts.Contains(tool.ToolUrn))
                        continue;
                    if (tool.DisplayName.Contains(_searchBox?.Text ?? "", StringComparison.OrdinalIgnoreCase))
                    {
                        _toolsSortedByName.Add(tool);
                    }
                }
            }, DispatcherPriority.Background);
        };
    }

    private void LinkToExtension()
    {
        EditorUiServices.ExtensionManager.ApplyExtensions(UIRegion.ToolsExplorer, this);
    }

    private Control? MakeToolItem(ToolLogic tool, INameScope scope)
    {
        return new ToolItemControl(tool);
    }
}

// This allows to undo/redo the addition of a tool button in the toolbar.
internal class AddButtonCommand(ToolLogic addedTool) : BaseCommand
{
    internal ToolLogic AddedTool { get; } = addedTool;
    internal bool WasActivatedBefore { get; set; }
    public override string Name => $"Add '{AddedTool.DisplayName}' tool button";
    protected override void OnExecute()
    {
        EngineServices.EngineConfig.ToolsShortcuts.Add(AddedTool.ToolUrn);
    }

    protected override void OnUndo()
    {
        EngineServices.EngineConfig.ToolsShortcuts.Remove(AddedTool.ToolUrn);
    }
}

public class ToolItemControl : UserControl
{
    public ToolLogic Tool { get; }
    
    private Grid? _body;
    private Icon? _icon;
    private TextBlock? _nameText;
    private Button? _addToToolbarButton;
    
    public ToolItemControl(ToolLogic tool)
    {
        Tool = tool;
        CreateComponents();
        RegisterEvents();
        LinkToExtension();
    }

    private void CreateComponents()
    {
        _body = new Grid
        {
            Margin = new Thickness(5),
            RowDefinitions = new RowDefinitions("Auto, Auto"),
            ColumnDefinitions = new ColumnDefinitions("Auto, *")
        };

        _icon = new Icon
        {
            Value = Tool.Icon,
            Width = 32,
            Height = 32,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        Grid.SetRow(_icon, 0);
        Grid.SetColumn(_icon, 0);
        _body.Children.Add(_icon);

        _nameText = new TextBlock
        {
            Text = Tool.DisplayName,
            Margin = new Thickness(5, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        Grid.SetRow(_nameText, 0);
        Grid.SetColumn(_nameText, 1);
        _body.Children.Add(_nameText);

        _addToToolbarButton = new Button
        {
            Content = "Add to Toolbar",
            Margin = new Thickness(5),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top
        };
        Grid.SetRow(_addToToolbarButton, 1);
        Grid.SetColumnSpan(_addToToolbarButton, 2);
        _body.Children.Add(_addToToolbarButton);
        ToolTip.SetShowOnDisabled(_addToToolbarButton, true);

        if (EngineServices.EngineConfig.ToolsShortcuts.Contains(Tool.ToolUrn))
        {
            _addToToolbarButton.IsEnabled = false;
            ToolTip.SetTip(_addToToolbarButton, "This tool is already in the shortcuts.");
        }

        ToolTip.SetTip(_body, Tool.Description);
        Content = _body;
    }
    
    private void RegisterEvents()
    {
        _addToToolbarButton?.Click += (s, e) =>
        {
            EngineServices.UndoRedoService.ExecuteCommand(
                new AddButtonCommand(Tool).WhenExecuted((baseCmd) =>
                {
                    if(baseCmd is AddButtonCommand { WasActivatedBefore: true } command)
                    {
                        RegistryServices.ToolRegistry.ActivateTool(command.AddedTool);
                    }
                    
                    _addToToolbarButton.IsEnabled = false;
                    ToolTip.SetTip(_addToToolbarButton, "This tool is already in the shortcuts.");
                }).WhenUndone((baseCmd) =>
                {
                    if (baseCmd is AddButtonCommand command)
                    {
                        command.WasActivatedBefore = GlobalStates.ToolState.ActiveTool == command.AddedTool;
                        if (command.WasActivatedBefore)
                        {
                            RegistryServices.ToolRegistry.DeactivateTool(command.AddedTool);
                        }
                    }
                    _addToToolbarButton.IsEnabled = true;
                    ToolTip.SetTip(_addToToolbarButton, null);
                })
                );
        };
    }
    
    private void LinkToExtension()
    {
        EditorUiServices.ExtensionManager.ApplyExtensions(UIRegion.ToolsExplorerItem, this);
    }
}