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
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Projektanker.Icons.Avalonia;
using RPGCreator.Core;
using RPGCreator.SDK;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Commands;
using RPGCreator.SDK.Editor;
using RPGCreator.SDK.EditorUiService;
using RPGCreator.SDK.GlobalState;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Modules.UIModule;
using RPGCreator.SDK.Types;
using RPGCreator.UI.Common.Modal.Browser;
using RPGCreator.UI.Contexts;
using Ursa.Controls;

namespace RPGCreator.UI.Content.Editor.Toolbar;


internal class RemoveButtonCommand(ToolLogic removedTool, bool wasActiveButton = false) : BaseCommand
{
    public bool WasActiveButton { get; } = wasActiveButton;
    public override string Name { get; } = $"Remove '{removedTool.DisplayName}' tool button";

    protected override void OnExecute()
    {
        EngineServices.EngineConfig.ToolsShortcuts.Remove(removedTool.ToolUrn);
    }

    protected override void OnUndo()
    {
        EngineServices.EngineConfig.ToolsShortcuts.Add(removedTool.ToolUrn);
    }
}


/*
 * Idea to implement (or not) for the editor toolbar:
 * - Allow the user to assign a keyboard shortcut to each tool button.
 * - If keyboard shortcut assignement is implemented, add (in the tooltip) the assigned keyboard shortcut for each tool button.
 * - Allow the user to reorder the tool buttons in the toolbar by - if possible - drag and drop.
 * - Allow the user to edit the order / keyboard assignement of the tool buttons in a dedicated panel (we could open this panel on right-click on a tool button).
 * Ward - 19/02/2026
 */


public class EditorToolsBar : UserControl
{

    [ExposePropToPlugin("EditorToolbar")]
    private CancellationTokenSource? JiggleCancelToken { get; set; }
    [ExposePropToPlugin("EditorToolbar")]
    private ToggleButton? ActiveButtonBeforeEditMode { get; set; }
    
    [ExposePropToPlugin("EditorToolbar")]
    private bool InEditMode { get; set; } = false;
    
    [ExposePropToPlugin("EditorToolbar")]
    private Grid Body { get; set; } = null!;
    
    [ExposePropToPlugin("EditorToolbar")]
    private ScrollViewer Scroll { get; set; } = null!;
    
    [ExposePropToPlugin("EditorToolbar")]
    private StackPanel MenuPanel { get; set; } = null!; // The menu panel is the one that will contain the 'Manage tools' button and the tool panel.
    
    [ExposePropToPlugin("EditorToolbar")]
    private Button AddToolButton { get; set; } = null!; // The add tool button is the one that will allow to add new tools to the toolbar. It will only be visible in edit mode.
    
    /// <summary>
    /// The tool panel is the one that will contain the actual tool buttons.<br/>
    /// It is managed by the <see cref="EngineConfig.ToolsShortcuts"/> list, which is a list of <see cref="ToolButtonInfo"/> objects.<br/>
    /// If you can, please avoid modifying this panel directly, or else the binding with the <see cref="EngineConfig.ToolsShortcuts"/> list will be broken and the tools buttons will not be displayed anymore.<br/>
    /// If you want to add a new tool, please add a new <see cref="ToolButtonInfo"/> object to the <see cref="EngineConfig.ToolsShortcuts"/> list, and the button will be automatically added to the panel (if the user add it inside his tool config panel).<br/>
    /// </summary>
    [ExposePropToPlugin("EditorToolbar")]
    private ItemsControl ToolPanel { get; set; } = null!; // The tool panel is the one that will contain the actual tool buttons.
    
    /// <summary>
    /// This list contains all the toggle buttons that are part of the same group, meaning that only one button can be selected at a time.<br/>
    /// If you can, don't touch this list directly, as it is used to manage the toggle state of the buttons in the toolbar.
    /// </summary>
    [ExposePropToPlugin("EditorToolbar")]
    private List<ToggleButton> Group { get; set; } = new();
    
    public EditorToolsBar()
    {
        CreateComponents();
        LinkToExtension();
    }
    
    private void CreateComponents()
    {
        Body = new Grid()
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            RowDefinitions = new RowDefinitions("*"),
            ColumnDefinitions = new ColumnDefinitions("Auto, *"),
            ColumnSpacing = 4
        };
        this.Content = Body;
        MenuPanel = new StackPanel()
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 4
        };
        Body.Children.Add(MenuPanel);
        Grid.SetColumn(MenuPanel, 0);
        
        Scroll = new ScrollViewer()
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
        };
        Body.Children.Add(Scroll);
        Grid.SetColumn(Scroll, 1);
        
        
        AddCustomButton(
            "Manage tools",
            "Allow to edit the toolbar.\nYou can add, or remove tools (by right clicking on a tool button).",
            "mdi-cog", async (obj, state) =>
            {
                if(obj is not { } button)
                    return;
                if (!state) return;
                button.IsChecked = false;
                
                InEditMode = !InEditMode;
                AddToolButton.IsVisible = InEditMode;
                AddToolButton.IsEnabled = InEditMode;

                JiggleCancelToken?.Cancel();
                
                if (InEditMode)
                {
                    JiggleCancelToken = new CancellationTokenSource();
                    var animation = CreateJiggleAnimation();
                    foreach (var toggleButton in Group)
                    {
                        
                        toggleButton.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
        
                        var rotateTransform = new RotateTransform(0);
                        toggleButton.RenderTransform = rotateTransform;
                        toggleButton.SetValue(OpacityProperty, 0.8);
                    }

                    _ = RunGlobalJiggleAsync(JiggleCancelToken.Token);
                }
                else
                {
                    foreach (var toggleButton in Group)
                    {
                        if (toggleButton.RenderTransform is not RotateTransform rotateTransform) continue;
                        toggleButton.Opacity = 1;
                        rotateTransform.Angle = 0;
                        toggleButton.RenderTransform = null;
                    }
                }
                
                if (ActiveButtonBeforeEditMode == null) return;
                
                ActiveButtonBeforeEditMode.IsChecked = !InEditMode;
            }
        );
        
        AddToolButton = AddCustomButton(
            "Add new tool",
            "Add a new available tool to the toolbar.",
            "mdi-plus",
            (obj, state) =>
            {
                if(obj is not { } button)
                    return;
                if (!state) return;
                button.IsChecked = false;
                EditorUiServices.DialogService.ShowPromptAsync("Add Tool", new ToolsBrowser(), new DialogStyle(Width: 900, Height: 500, SizeToContent:DialogSizeToContent.None));
            }
        );
        
        AddToolButton.IsVisible = false;
        AddToolButton.IsEnabled = false;
        
        MenuPanel.Children.Add(new Divider()
        {
            Orientation = Orientation.Vertical
        });
        
        #if DEBUG
        
        MenuPanel.Children.Add(CreateToolButton(new MockupTool()));
        
        #endif

        ToolPanel = new ItemsControl()
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            ItemsPanel = new FuncTemplate<Panel?>(() => new StackPanel()
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Spacing = 4
            }),
            ItemsSource = EngineServices.EngineConfig.ToolsShortcuts,
            DataTemplates = { 
                new FuncDataTemplate<URN>((urn, _) =>
                {
                    if (urn == URN.Empty) return null;
                    if(urn.Namespace.ToString() == "separator")
                    {
                        return new Divider()
                        {
                            Orientation = Orientation.Vertical
                        };
                    }
                    #if DEBUG
                    
                    // ReSharper disable once ConvertIfStatementToReturnStatement
                    if (urn == "rpgc".ToUrnNamespace().ToUrnModule("tools").ToUrn("mockup"))
                    {
                        return CreateToolButton(new MockupTool());
                    }
                    
                    #endif
                    return CreateToolButton(RegistryServices.ToolRegistry.GetTool(urn));
                }),
            }
        };
        
        Scroll.Content = ToolPanel;
    }
    
    private async Task RunGlobalJiggleAsync(CancellationToken token)
    {
        var animation = CreateJiggleAnimation();
    
        foreach (var toggleButton in Group)
        {
            if (token.IsCancellationRequested) return;

            Dispatcher.UIThread.Post(() => 
            {
                animation.RunAsync(toggleButton, token);
            }, DispatcherPriority.Background);

            await Task.Delay(new Random().Next(0, 60), token); 
        }
    }

    private void LinkToExtension()
    {
        var config = new EditorToolbarContext.Config()
        {
            AddCustomButton = AddCustomButton,
            CreateToolButton = CreateToolButton,
            RegisterButtonToGroup = RegisterButtonToGroup,
            CreateButtonTip = CreateButtonTip,
            CreateValidButton = CreateValidButton,
            CreateInvalidButton = CreateInvalidButton,
            GetBody = () => Body,
            GetMenuPanel = () => MenuPanel,
            GetToolPanel = () => ToolPanel,
            GetGroup = () => Group,
            GetScroll = () => Scroll,
            GetInEditMode = () => InEditMode,
            GetActiveButtonBeforeEditMode = () => ActiveButtonBeforeEditMode,
            GetAddToolButton = () => AddToolButton,
            GetJiggleCancelToken =  () => JiggleCancelToken,
        };
        
        var context = new EditorToolbarContext(config);
        
        EditorUiServices.ExtensionManager.ApplyExtensions(UIRegion.EditorToolbar, this, context);
    }

    [ExposeToPlugin("EditorToolbar")]
    private ToggleButton CreateValidButton()
    {
        return new ToggleButton()
        {
            MaxHeight = 32,
            MinHeight = 32,
            MinWidth = 32,
            MaxWidth = 32,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };
    }
    
    [ExposeToPlugin("EditorToolbar")]
    private ToggleButton CreateInvalidButton()
    {
        var button = CreateValidButton();
        button.Content = new Icon()
        {
            Value = "mdi-alert-circle",
            Width = 32,
            Height = 32,
            Classes = { "Danger" }
        };
        button.IsEnabled = false;
        ToolTip.SetTip(button, "Invalid tool! This tool is not available or has been removed.");
        ToolTip.SetShowOnDisabled(button, true);
        return button;
    }

    [ExposeToPlugin("EditorToolbar")]
    private TextBlock CreateButtonTip(string name, string description = "")
    {
        var tip = new TextBlock()
        {
            Inlines = new InlineCollection()
        };
        
        var nameRun = new Run(name)
        {
            FontWeight = Avalonia.Media.FontWeight.Bold,
        };
        tip.Inlines.Add(nameRun);
        
        tip.Inlines.Add(new LineBreak());
        
        var descRun = new Run(!string.IsNullOrWhiteSpace(description) ? description : "No description provided.")
        {
            FontStyle = Avalonia.Media.FontStyle.Italic,
        };
        tip.Inlines.Add(descRun);
        
        return tip;
    }
    
    [ExposeToPlugin("EditorToolbar")]
    private void RegisterButtonToGroup(ToggleButton button)
    {
        Group.Add(button);
        button.IsCheckedChanged += (s, e) =>
        {
            if (button.IsChecked != true)
                return;
            foreach (var btn in Group)
            {
                if(btn == button)
                    continue;
                
                btn.IsChecked = false;
            }
        };
    }

    [ExposeToPlugin("EditorToolbar")]
    private ToggleButton CreateToolButton(ToolLogic? tool)
    {
        if (tool == null)
        {
            return CreateInvalidButton();
        }
        
        var button = CreateValidButton();
        button.Tag = Ulid.NewUlid();
        var icon = new Icon()
        {
            Value = tool.Icon,
            Width = 32,
            Height = 32,
        };
        
        button.Content = icon;
        button.FontSize = 24;

        if (GlobalStates.ToolState.ActiveTool == tool)
        {
            button.IsChecked = true;
        }

        button.IsCheckedChanged += (s, e) =>
        {
            if (InEditMode)
            {
                button.IsChecked = false;
                return;
            }

            if (button.IsChecked != true && ActiveButtonBeforeEditMode?.Tag == button.Tag)
            {
                RegistryServices.ToolRegistry.DeactivateTool(tool);
                ActiveButtonBeforeEditMode = null;
                return;
            }
            if(button.IsChecked != true)
                return;

            RegistryServices.ToolRegistry.ActivateTool(tool);
            ActiveButtonBeforeEditMode = button;
        };

        button.PointerPressed += (s, e) =>
        {
            if (!InEditMode) return;

            if (e.GetCurrentPoint(button).Properties.IsRightButtonPressed)
            {
                e.Handled = true;
                // For now, we remove it directly, but later on, why not open a panel to config the tool shortcut?
                // We could also add a drag and drop system to reorder the tools in the toolbar.
                // But well, already have a lot of things to do, so let's keep it simple for now.
                EngineServices.UndoRedoService.ExecuteCommand(
                    new RemoveButtonCommand(tool, ActiveButtonBeforeEditMode == button)
                        .WhenUndone(
                            command =>
                            {
                                if(command is not RemoveButtonCommand removeCommand)
                                    return;
                                if (removeCommand.WasActiveButton)
                                    ActiveButtonBeforeEditMode = button;
                            }
                        ).WhenExecuted(
                            _ =>
                            {
                                if (ActiveButtonBeforeEditMode == button)
                                {
                                    ActiveButtonBeforeEditMode = null;
                                    GlobalStates.ToolState.ActiveTool = null;
                                }
                            }
                        )
                    );
                
                if(ActiveButtonBeforeEditMode == button)
                    ActiveButtonBeforeEditMode = null;
            }
        };

        var tip = CreateButtonTip(tool.DisplayName, tool.Description);
        button.PointerEntered += (s, e) =>
        {
            if (!InEditMode) return;

            ToolTip.SetTip(button, CreateButtonTip(tool.DisplayName, "Right click to remove this tool from the toolbar."));
            button.Content = new Icon()
            {
                Value = "mdi-trash-can",
                Width = 32,
                Height = 32,
            };
            button.Classes.Add("Danger");
        };

        button.PointerExited += (s, e) =>
        {
            // We check if the button has the class "Danger"
            // So even if by some reason, the button is 'lock' in the 'delete mode',
            // then it will be reseted when the user move the mouse away from it.
            //
            // Normally it should not happen, but it's better to be safe than sorry.
            if (!InEditMode && !button.Classes.Contains("Danger")) return;
            if (tip.Parent != null)
            {
                ((ToolTip)tip.Parent).Content = null;
            }
            ToolTip.SetTip(button, tip);
            button.Content = icon;
            button.Classes.Remove("Danger");
        };
        
        RegisterButtonToGroup(button);
        button.DetachedFromVisualTree += (s, e) =>
        {
            if (tip.Parent != null)
            {
                ((ToolTip)tip.Parent).Content = null;
            }
            Group.Remove(button);
        };
        
        ToolTip.SetTip(button, tip);

        if (InEditMode)
        {
            button.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);

            var rotateTransform = new RotateTransform(0);
            button.RenderTransform = rotateTransform;
            button.SetValue(OpacityProperty, 0.8);
            _ = StartJiggleAsync(CreateJiggleAnimation(), button, JiggleCancelToken?.Token ?? CancellationToken.None);
        }
        
        return button;
    }

    [ExposeToPlugin("EditorToolbar")]
    private Button AddCustomButton(string displayName, string description, string icon, Action<ToggleButton, bool> onCheckedChanged)
    {
        var button = CreateValidButton();
        button.Tag = Ulid.NewUlid();
        var iconControl = new Icon()
        {
            Value = icon,
            Width = 32,
            Height = 32,
        };
        
        button.Content = iconControl;
        button.FontSize = 24;
        
        button.IsCheckedChanged += (s, e) => onCheckedChanged?.Invoke(button, button.IsChecked == true);
        
        MenuPanel.Children.Add(button);
        
        ToolTip.SetTip(button, CreateButtonTip(displayName, description));
        return button;
    }
    
    #region Helpers
    
    private Animation CreateJiggleAnimation()
    {
        return new Animation
        {
            Duration = TimeSpan.FromMilliseconds(300),
            IterationCount = IterationCount.Infinite,
            PlaybackDirection = PlaybackDirection.Alternate,
            Easing = new SineEaseInOut(),
            Children =
            {
                new KeyFrame
                {
                    Setters = { new Setter(RotateTransform.AngleProperty, -5.0) },
                    KeyTime = TimeSpan.FromSeconds(0)
                },
                new KeyFrame
                {
                    Setters = { new Setter(RotateTransform.AngleProperty, 5.0) },
                    KeyTime = TimeSpan.FromMilliseconds(300),
                }
            },
        };
    }
    
    private async Task StartJiggleAsync(Animation animation, Visual visual, CancellationToken token)
    {
        try 
        {
            await Task.Delay(new Random().Next(0, 50), token);
            await animation.RunAsync(visual, token);
        }
        catch (OperationCanceledException) { /* Normal */ }
    }
    
    #endregion
}