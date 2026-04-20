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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Reactive;
using RPGCreator.SDK.Editor.Rendering;
using RPGCreator.SDK.GameUI.Controls;
using RPGCreator.SDK.GameUI.Interfaces;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Logging;
using Rect = Avalonia.Rect;
using Vector = Avalonia.Vector;

namespace RPGCreator.UI.Content.GameUiEditor.Components;


public class SelectedControlArgs(BaseControl? control) : BaseUiEventArgs
{
    public readonly BaseControl? Control = control;
}

public class StartDraggingArgs(BaseControl? control, Vector2 originalPosition) : BaseUiEventArgs
{
    public readonly BaseControl? Control = control;
    public readonly Vector2 OriginalPosition = originalPosition;
}

public class DraggingArgs(BaseControl? control, Vector2 position) : BaseUiEventArgs
{
    public readonly BaseControl? Control = control;
    public readonly Vector2 Position = position;
}

public class EndDraggingArgs(BaseControl? control, Vector2 dropPosition) : BaseUiEventArgs
{
    public readonly BaseControl? Control = control;
    public readonly Vector2 DropPosition = dropPosition;
}

public class UiEditorLayer : Canvas
{
    public event EventHandler<SelectedControlArgs>? SelectedControlChanged;
    
    public bool IsPressed { get; private set; } = false;
    public bool IsDragging { get; private set; } = false;
    
    public UiEditorContext Context { get; }
    
    private BaseControl _rootControl;
    private BaseControl? _selectedControl;
    /// <summary>
    /// The adorner, this is used to draw the selected control handle.
    /// </summary>
    private UiHandler _handler;
    public UiEditorLayer(UiEditorContext context)
    {
        InitializeIfNeeded();
        Context = context;
        Background = Brushes.Transparent;
        
        context.EditorReady += () =>
        {
            if (Context.Manager.GetRootControl(0) is not { } rootControl)
            {
                Context.RaiseRootControlAdded(this, new TestControl()
                {
                    Name = "Root"
                });
                rootControl = Context.Manager.GetRootControl(0);
            }

            _rootControl = rootControl;
            _handler = new UiHandler();

            _handler.Height = this.Height;
            _handler.Width = this.Width;
            
            _handler.PinPressed += (pinType, position) =>
            {
                IsPressed = true;
                ControlPosition = new Vector2(_selectedControl.Visual.X, _selectedControl.Visual.Y);
            };
            
            _handler.PinReleased += () =>
            {
                IsPressed = false;
                _handler.RefreshSelection();
            };

            
            context.ControlSelected += (sender, args) =>
            {
                if (Equals(sender, this)) return;
                if (args.IsCanceled) return;
                _selectedControl = args.Control;
                _handler.SelectControl(args.Control);
            };
            
            context.ControlUnselected += (sender, args) =>
            {
                if (Equals(sender, this)) return;
                if (args.IsCanceled) return;
                _selectedControl = null;
                _handler.SelectControl(null);
            };
            
            this.GetObservable(BoundsProperty).Subscribe(new AnonymousObserver<Rect>(bounds =>
            {
                _handler.Width = bounds.Width;
                _handler.Height = bounds.Height;
                _handler.InvalidateVisual();
                _handler.RefreshSelection();
            }));
            
            Children.Add(_handler);

            RegisterEvents();
        };
    }
    

    private void RegisterEvents()
    {
        PointerPressed += OnPointerPressed;
        SelectedControlChanged += (sender, args) =>
        {
            // TEST
            // args.Cancel(this);
        };
    }

    private Vector2 ControlPosition;
    // NOTE: All of the content below is commented because it is still not ready, and still need lots of work.
    // Too much for the release of the Preview01.
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        /*
        var pin = _handler.PressedRect;
        var currentMousePos = new Vector2((int)e.GetPosition(this).X, (int)e.GetPosition(this).Y);
        var delta = currentMousePos - _handler.PressedPosition;


        if (IsPressed)
        {
            if (!IsDragging && (Math.Abs(delta.X) > 20 || Math.Abs(delta.Y) > 20))
            {
                IsDragging = true;
            } else if (IsDragging && pin == UiHandler.PinType.Movement)
            {
                int theoryX = (int)(ControlPosition.X + delta.X);
                int theoryY = (int)(ControlPosition.Y - delta.Y);

                _selectedControl.Visual.X = theoryX;
                _selectedControl.Visual.Y = theoryY;

                _handler.CheckSnapping(_selectedControl.Visual, _selectedControl.Parent?.Visual.Children ?? []);

                if (_handler.SnappedX) 
                {
                    float distFromTheory = Math.Abs(_selectedControl.Visual.X - theoryX);
                    if (distFromTheory > 15) 
                    {
                        Logger.Debug($"Forcing snap exit for control {_selectedControl.Name} from {theoryX} to {_selectedControl.Visual.X}");
                        _selectedControl.Visual.X = theoryX;
                    }
                }

                if (_handler.SnappedY)
                {
                    float distFromTheory = Math.Abs(_selectedControl.Visual.Y - theoryY);
                    if (distFromTheory > 15) 
                    {
                        Logger.Debug($"Forcing snap exit for control {_selectedControl.Name} from {theoryY} to {_selectedControl.Visual.Y}");
                        _selectedControl.Visual.Y = theoryY;
                    }
                }

                _handler.InvalidateVisual();
            }
        }
        else
        {
            _handler.UiSnapLineController.DisableAll();
        }
        */
        
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs pointer)
    {
        var pos = pointer.GetPosition(this);
        var selectedControl = _rootControl.GetControlAt(new Vector2((float)pos.X, (float)pos.Y));

        if (selectedControl == null)
            return;
        
        var arg = Context.RaiseControlSelected(this, selectedControl);
        
        if(arg.IsCanceled)
        {
            Logger.Debug($"Pointer pressed canceled by {arg.CancelledBy?.GetType().ToString() ?? "Unknown"}");
            return;
        }
        
        _selectedControl = selectedControl;
        _handler.SelectControl(_selectedControl);
    }
}