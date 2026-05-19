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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Diagnostics;
using Microsoft.Xna.Framework;
using RPGCreator.SDK;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Editor.Rendering;
using RPGCreator.SDK.GameUI;
using RPGCreator.SDK.GameUI.Controls;
using RPGCreator.SDK.GameUI.Interfaces;
using RPGCreator.SDK.Services.EditorUiService;
using RPGCreator.UI.Common.Bridge;
using RPGCreator.UI.Content.GameUiEditor.Components;
using RPGCreator.UI.Content.GameUiEditor.Components.Explorer;
using Color = Avalonia.Media.Color;
using Size = RPGCreator.Shared.Types.Size;

namespace RPGCreator.UI.Content.GameUiEditor;


public abstract class BaseUiEventArgs
{
    public bool Handled { get; private set; }
    public object? HandledBy { get; set; }
    public bool IsCanceled { get; private set; }
    public object? CancelledBy { get; set; }

    /// <summary>
    /// Mark this event has handled.
    /// </summary>
    /// <param name="source">What object handled the event.</param>
    /// <returns>
    /// Return true if the event was handled, false if the event was already handled.
    /// </returns>
    public bool Handle(object? source)
    {
        if(Handled)
            return false;
        
        Handled = true;
        HandledBy = source;
        return true;
    }
    
    /// <summary>
    /// Mark this event as canceled.
    /// </summary>
    /// <param name="source">What object canceled the event.</param>
    /// <returns>
    /// Return true if the event was canceled, false if the event was already canceled.
    /// </returns>
    public bool Cancel(object? source)
    {
        if(IsCanceled)
            return false;
        
        IsCanceled = true;
        CancelledBy = source;
        return true;
    }
}

public class UiEditorContext 
{
    public UserControl EditorControl => _editorControl;
    public class ControlEventArgs : BaseUiEventArgs
    {
        public readonly BaseControl? Control;
        public ControlEventArgs(BaseControl? control)
        {
            Control = control;
        }
    }
    
    public event Action? EditorReady;
    public event EventHandler<ControlEventArgs>? ControlSelected;
    public event EventHandler<ControlEventArgs>? ControlUnselected;
    public event EventHandler<ControlEventArgs>? RootControlAdded;
    public event EventHandler<ControlEventArgs>? RootControlRemoved;
    public event EventHandler<ControlEventArgs>? ControlAdded;
    public event EventHandler<ControlEventArgs>? ControlRemoved;
    public event EventHandler<ControlEventArgs>? ControlMoved;
    
    public BaseControl? SelectedControl { get; private set; }
    private List<BaseControl> _rootControls { get; set; } = new();
    
    public IReadOnlyList<BaseControl> RootControls => _rootControls;
    
    public BaseUiViewport Viewport => _editorControl.UiViewport;
    
    /// <summary>
    /// If possible, try to not use this property.<br/>
    /// It's available if you really need it, but it could cause desynchronization issues if not used carefully.
    /// </summary>
    public IUiManager Manager => Viewport.UiManager;
    
    private readonly UiEditorWindowControl _editorControl;

    public UiEditorContext(UiEditorWindowControl editorControl)
    {
        _editorControl = editorControl;
        
        _editorControl.EditorReady += OnEditorReady;
    }

    private void OnEditorReady()
    {
        var manager = _editorControl.UiViewport.UiManager;
        _rootControls = manager.GetRootControls();
        manager.RootControlAdded += (control) =>
        {
            if(control == null || _rootControls.Contains(control))
                return;
            
            RaiseRootControlAdded(this, control);
        };
        manager.RootControlRemoved += (control) =>
        {
            if(control == null || !_rootControls.Contains(control))
                return;
            
            RaiseRootControlRemoved(this, control);
        };
        EditorReady?.Invoke();
    }
    
    public ControlEventArgs RaiseControlSelected(object? caller, BaseControl control)
    {
        if (SelectedControl != null)
        {
            var arg = RaiseControlUnselected(this);
            if (arg.IsCanceled)
                return arg;
        }
        var old = SelectedControl;
        SelectedControl = control;
        ControlEventArgs args = new ControlEventArgs(control);
        ControlSelected?.Invoke(caller, args);
        if (args.IsCanceled)
        {
            SelectedControl = old;
        }
        return args;
    }

    public ControlEventArgs RaiseControlUnselected(object? caller)
    {
        if (SelectedControl == null)
            return new ControlEventArgs(null);
        ControlEventArgs args = new ControlEventArgs(SelectedControl);
        ControlUnselected?.Invoke(caller, args);
        if(args.IsCanceled)
            return args;
        SelectedControl = null;
        return args;
    }

    public ControlEventArgs RaiseRootControlAdded(object? caller, BaseControl control)
    {
        ControlEventArgs args = new ControlEventArgs(control);
        RootControlAdded?.Invoke(caller, args);
        
        if(args.IsCanceled)
            return args;
        
        if (caller != this)
        {
            Manager.AddRootControl(control);
        }
        
        return args;
    }

    public ControlEventArgs RaiseRootControlRemoved(object? caller, BaseControl control)
    {
        if(control == null)
            return new ControlEventArgs(null);
        
        ControlEventArgs args = new ControlEventArgs(control);
        RootControlRemoved?.Invoke(caller, args);
        
        if(args.IsCanceled)
            return args;
        
        if (caller != this)
            Manager.RemoveRootControl(control);
        
        return args;
    }
    
    public ControlEventArgs RaiseControlAdded(object? caller, BaseControl control)
    {
        if(control == null)
            return new ControlEventArgs(null);
        
        ControlEventArgs args = new ControlEventArgs(control);
        ControlAdded?.Invoke(caller, args);
        return args;
    }

    public ControlEventArgs RaiseControlRemoved(object? caller, BaseControl control)
    {
        if(_rootControls.Contains(control))
        {
            return RaiseRootControlRemoved(caller, control);
        }
        ControlEventArgs args = new ControlEventArgs(control);
        ControlRemoved?.Invoke(caller, args);
        return args;
    }

    public ControlEventArgs RaiseControlMoved(object? caller, BaseControl control)
    {
        ControlEventArgs args = new ControlEventArgs(control);
        ControlMoved?.Invoke(caller, args);
        return args;
    }
}

public sealed class UiEditorWindowControl : UserControl
{
    public event Action? EditorReady;

    public readonly UiEditorContext Context;
    
    private const int ExplorerCol = 0;
    private const int PreviewerCol = ExplorerCol + 2;
    private const int PropertiesRow = PreviewerCol + 2;

    private readonly AvaloniaKeyboardBridge _keyboardBridge = new();
    private readonly AvaloniaMouseBridge _mouseBridge = new();

    public BaseUiViewport UiViewport;
    private IGameUiRunner _uiPreviewer;

    public UiTopMenu TopMenu { get; set; } = null!;
    private GridSplitter LeftSplitter { get; set; } = null!;
    public UiExplorer Explorer { get; set; } = null!;
    public UiEditorLayer _editorLayer;


    private Grid MainBody { get; set; } = null!;
    private Grid ContentGrid { get; set; } = null!;

    private WriteableBitmap _uiPreviewWriteableBitmap = new(new PixelSize(1172, 827), new Vector(96, 96),
        Avalonia.Platform.PixelFormat.Rgba8888, Avalonia.Platform.AlphaFormat.Premul);

    private Image _previewerImage;
    private Grid _previewerGrid;

    public UiEditorWindowControl(IGameUiRunner previewer)
    {
        Context = new UiEditorContext(this);
        _uiPreviewer = previewer;
        Guard.IsAssignableToType<Game>(_uiPreviewer);

        CreateComponents();
        RegisterEvents();
        CreateViewport();
        EditorReady?.Invoke();
    }

    private void CreateComponents()
    {
        MainBody = new Grid()
        {
            RowDefinitions = new RowDefinitions("Auto, *"),
            ColumnDefinitions = new ColumnDefinitions("*")
        };
        Content = MainBody;

        TopMenu = new UiTopMenu(this);
        MainBody.Children.Add(TopMenu);
        Grid.SetRow(TopMenu, 0);

        ContentGrid = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("350, 2, *"),
        };
        MainBody.Children.Add(ContentGrid);
        Grid.SetRow(ContentGrid, 1);

        Explorer = new UiExplorer(Context);
        ContentGrid.Children.Add(Explorer);
        Grid.SetColumn(Explorer, ExplorerCol);

        LeftSplitter = new GridSplitter()
        {
            Width = 2,
            Background = Brushes.Gray,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        };
        ContentGrid.Children.Add(LeftSplitter);
        Grid.SetColumn(LeftSplitter, ExplorerCol + 1);

        var backgroundGridBrush = new DrawingBrush()
        {
            Drawing = new GeometryDrawing()
            {
                Geometry = StreamGeometry.Parse("M0,0 L0,1 0.03,1 0.03,0.03 1,0.03 1,0 Z"),
                Brush = new SolidColorBrush(Color.Parse("#33333b"))
            },
            TileMode = TileMode.Tile,
            DestinationRect = RelativeRect.Parse("0,0,20,20")
        };
        
        _previewerGrid = new Grid()
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            Background = backgroundGridBrush,
        };

        _previewerImage = new Image()
        {
            Source = _uiPreviewWriteableBitmap,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            Focusable = true,
            Name = "UiPreviewerImage",
        };
        _previewerGrid.Children.Add(_previewerImage);
        
        _editorLayer = new UiEditorLayer(Context);
        _previewerGrid.Children.Add(_editorLayer);
        
        ContentGrid.Children.Add(_previewerGrid);
        Grid.SetColumn(_previewerGrid, PreviewerCol);


    }

    private void RegisterEvents()
    {
        _mouseBridge.RegisterEvents(_previewerImage);
        _keyboardBridge.RegisterEvents(_previewerImage);
        DetachedFromVisualTree += (_,_) =>
        {
            Logger.Debug("UiEditorWindowControl detached from visual tree.");
            EditorUiServices.MonogameViewport.DestroyViewport("UI Preview Viewport");
        };
    }

    private void CreateViewport()
    {
        using (var buf = _uiPreviewWriteableBitmap.Lock())
        {
            var viewport = EditorUiServices.MonogameViewport.CreateNewViewport("UI Preview Viewport", buf.Address,
                new Size(1172, 827), ViewportType.Ui);

            if (viewport is BaseUiViewport uiViewport)
            {
                UiViewport = uiViewport;
            }
            else
            {
                throw new Exception("Viewport is not a BaseUiViewport.");
            }
            
            viewport?.LockToImageControl("UiPreviewerImage");

            _previewerGrid.SizeChanged += (_, _) =>
            {
                _uiPreviewWriteableBitmap = new WriteableBitmap(
                    new PixelSize((int)_previewerGrid.Bounds.Width, (int)_previewerGrid.Bounds.Height),
                    new Vector(96, 96), Avalonia.Platform.PixelFormat.Rgba8888, Avalonia.Platform.AlphaFormat.Premul);
                viewport?.Resize(
                    new Size((int)_previewerGrid.Bounds.Width, (int)_previewerGrid.Bounds.Height));
                _previewerImage.Source = _uiPreviewWriteableBitmap;
            };

            viewport?.OnceUpdatedDo(() =>
            {
                Dispatcher.UIThread.Post(() => { _previewerImage.InvalidateVisual(); },
                    priority: DispatcherPriority.Render);
            });

            viewport?.DoNewFrameAction += () =>
            {
                using (var buf = _uiPreviewWriteableBitmap.Lock())
                {
                    return buf.Address;
                }
            };
            
            
        }
    }
}