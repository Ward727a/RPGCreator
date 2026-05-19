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

using System.Numerics;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.GameUI.Enums;
using RPGCreator.SDK.GameUI.Events.Contexts;
using RPGCreator.SDK.GameUI.Interfaces;
using RPGCreator.SDK.GameUI.Visual;
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.GameUI.Controls;

public abstract class BaseControl
{
    public bool IsInternal { get; init; } = false;
    public bool IsPropertiesInitialized { get; private set; } = false;
    public bool IsEventsInitialized { get; private set; } = false;

    public event Action? PropertyDescriptorsExposed;
    public event Action<ControlPropertyDescriptor>? PropertyDescriptorAdded;
    public event Action<ControlPropertyDescriptor>? PropertyDescriptorRemoved;

    private Dictionary<(PipedPath category, string name), ControlEventDescriptor> _eventDescriptorsCache = new();
    private readonly List<ControlEventDescriptor> _eventDescriptors = new();

    public void EmitEvent(PipedPath category, string name, GuiEventContext context)
    {
        if (!_eventDescriptorsCache.TryGetValue((category, name), out var descriptor))
        {
            throw new ArgumentException($"No event descriptor with name '{name}' is registered in '{category}'.");
        }
        
        if(descriptor.GuiContextType.IsInstanceOfType(context))
        {
            if (descriptor.Action is { } action)
            {
                if(action.Match(context))
                    action.Execute(context);
                else
                    Logger.Error($"The action '{action.Name}' associated with event '{descriptor.Name}' in category '{descriptor.Category}' could not be executed because the context did not match the action's requirements.");
            }
        }
    }
    
    protected T RegisterEventDescriptor<T>(T descriptor, bool overrideIfExists = false) where T : ControlEventDescriptor
    {
        if (_eventDescriptorsCache.ContainsKey((descriptor.Category, descriptor.Name)))
        {
            
            if (overrideIfExists)
            {
                _eventDescriptors.Remove(_eventDescriptorsCache[(descriptor.Category, descriptor.Name)]);
                _eventDescriptorsCache.Remove((descriptor.Category, descriptor.Name));
            } else
                throw new ArgumentException($"A descriptor with name '{descriptor.Name}' is already registered in '{descriptor.Category}'.");
        }
        else
        {
            _eventDescriptorsCache.Add((descriptor.Category, descriptor.Name), descriptor);
            _eventDescriptors.Add(descriptor);
        }
        return descriptor;
    }
    
    protected void UnregisterEventDescriptor(PipedPath category, string name)
    {
        if (!_eventDescriptorsCache.TryGetValue((category, name), out var descriptor))
        {
            throw new ArgumentException($"No descriptor with name '{name}' is registered in '{category}'.");
        }
        _eventDescriptorsCache.Remove((category, name));
        _eventDescriptors.Remove(descriptor);
    }
    
    #region Exposed Events

    public ControlEventDescriptor<GuiClickEventContext> ClickedGuiEvent;
    
    #endregion
    
    private readonly Dictionary<(PipedPath path, string name), ControlPropertyDescriptor> _propertyDescriptorsCache = new();
    private readonly List<ControlPropertyDescriptor> _propertyDescriptors = new();
    
    protected T RegisterPropertyDescriptor<T>(T descriptor, bool overrideIfExists = false) where T : ControlPropertyDescriptor
    {
        if (_propertyDescriptorsCache.ContainsKey((descriptor.Path, descriptor.Name)))
        {
            
            if (overrideIfExists)
            {
                _propertyDescriptors.Remove(_propertyDescriptorsCache[(descriptor.Path, descriptor.Name)]);
                _propertyDescriptorsCache.Remove((descriptor.Path, descriptor.Name));
            } else
                throw new ArgumentException($"A descriptor with name '{descriptor.Name}' is already registered in '{descriptor.Path}'.");
        }
        
        if (!_propertyDescriptors.Contains(descriptor))
        {
            _propertyDescriptors.Add(descriptor);
            _propertyDescriptorsCache[(descriptor.Path, descriptor.Name)] = descriptor;
            PropertyDescriptorAdded?.Invoke(descriptor);
        }
        return descriptor;
    }
    
    protected void UnregisterPropertyDescriptor(PipedPath path, string name)
    {
        if (!_propertyDescriptorsCache.TryGetValue((path, name), out var descriptor))
        {
            throw new ArgumentException($"No descriptor with name '{name}' is registered in '{path}'.");
        }
        
        _propertyDescriptors.Remove(descriptor);
        _propertyDescriptorsCache.Remove((path, name));
        PropertyDescriptorRemoved?.Invoke(descriptor);
    }

    protected static readonly UrnSingleModule _urnModule = "gameUi_control".ToUrnSingleModule();
    protected static readonly PipedPath _defaultPath = "Other".ToPipedPath();
    public abstract URN Urn { get; }
    
    public virtual PipedPath Category { get; } = _defaultPath;
    
    #region ExposedProperties
    
    public EditableControlPropertyDescriptor<string> NameProperty { get; protected set; }
    public EditableControlPropertyDescriptor<BaseControl?> ParentProperty { get; protected set; }
    public EditableControlPropertyDescriptor<bool> VisibilityProperty { get; protected set; }
    public EditableControlPropertyDescriptor<float> OpacityProperty { get; protected set; }
    public EditableControlPropertyDescriptor<Vector2> PivotProperty { get; protected set; }
    public EditableControlPropertyDescriptor<EOriginUnitType> PivotXUnitProperty { get; protected set; }
    public EditableControlPropertyDescriptor<EOriginUnitType> PivotYUnitProperty { get; protected set; }
    public EditableControlPropertyDescriptor<Vector2> PositionProperty { get; protected set; }
    public EditableControlPropertyDescriptor<EPositionUnitType> PositionXUnitProperty { get; protected set; }
    public EditableControlPropertyDescriptor<EPositionUnitType> PositionYUnitProperty { get; protected set; }
    public EditableControlPropertyDescriptor<Vector2> SizeProperty { get; protected set; }
    public EditableControlPropertyDescriptor<ESizeUnitType> SizeXUnitProperty { get; protected set; }
    public EditableControlPropertyDescriptor<ESizeUnitType> SizeYUnitProperty { get; protected set; }
    public EditableControlPropertyDescriptor<bool> IsHitTestVisibleProperty { get; protected set; }
    public EditableControlPropertyDescriptor<bool> ClipHitTestToBoundsProperty { get; protected set; }
    
    #endregion

    /// <summary>
    /// Define the name of the component. This is used to display it in the UI editor.<br/>
    /// It can be renamed by the user in the UI editor.<br/>
    /// But if the user does not rename it, the name will be what is set by default.
    /// </summary>
    public virtual string Name { get; set; } = "Unnamed Control";
    
    public virtual string DisplayControlName { get; set; } = "Base Control";
    public virtual string Description { get; set; } = "A basic control that does nothing by default.";

    public abstract BaseControl Create();
    
    public bool IsVisible { get; set; } = true;
    public bool IsHitTestVisible { get; set; } = true;

    /// <summary>
    /// If true, the control and its children will only receive mouse events if the mouse is within this control's bounds.<br/>
    /// If false, the control and its children can receive mouse events even if the mouse is outside this control's bounds.
    /// </summary>
    public bool ClipHitTestToBounds { get; set; } = true;

    public BaseVisual Visual { get; protected set; }

    protected bool _isMouseOver = false;
    public bool IsMouseOver => _isMouseOver;

    protected bool _isFocused = false;
    public bool IsFocused => _isFocused;

    protected bool _isPressed = false;
    public bool IsPressed => _isPressed;

    protected bool _isDragging = false;
    public bool IsDragging => _isDragging;

    #region Inheritance and Hierarchy

    public event Action<BaseControl>? AddedChildren;
    public event Action<BaseControl>? RemovedChildren;

    protected readonly List<BaseControl> _children = new List<BaseControl>();
    public IReadOnlyList<BaseControl> Children => _children.AsReadOnly();
    public BaseControl? Parent { get; private set; }

    public void AddChild(BaseControl child)
    {
        if (child.Parent != null)
            throw new InvalidOperationException("The control already has a parent.");

        _children.Add(child);
        child.Parent = this;
        child.Visual.AddToParent(Visual);
        child.CallAddedToParent();
        AddedChildren?.Invoke(child);
    }

    public void RemoveChild(BaseControl child)
    {
        if (child.Parent != this)
            throw new InvalidOperationException("The control is not a child of this parent.");

        _children.Remove(child);
        child.Parent = null;
        child.Visual.RemoveFromParent();
        child.CallRemovedFromParent();
        RemovedChildren?.Invoke(child);
    }

    public void ClearChildren()
    {
        foreach (var child in _children)
        {
            child.Parent = null;
            child.Visual.RemoveFromParent();
            child.CallRemovedFromParent();
            RemovedChildren?.Invoke(child);
        }

        _children.Clear();
    }

    #endregion

    public ControlEventDescriptor GetExposedEvent(PipedPath category, string name)
    {
        if (!_eventDescriptorsCache.TryGetValue((category, name), out var descriptor))
            throw new ArgumentException($"No descriptor with name '{name}' is registered in '{category}'.");
        
        return descriptor;
    }
    
    public List<ControlEventDescriptor> GetExposedEvents()
    {
        if(_eventDescriptors.Count == 0)
            MakeExposedEvents();
        
        return _eventDescriptors;
    }

    protected virtual void MakeExposedEvents()
    {
        if (IsEventsInitialized)
            return;
        IsEventsInitialized = true;
        
        ClickedGuiEvent = RegisterEventDescriptor(new ControlEventDescriptor<GuiClickEventContext>("OnClicked", "Mouse".ToPipedPath(), "Triggered when the control is clicked with a mouse button."));
    }

    public ControlPropertyDescriptor GetExposedProperty(PipedPath path, string name)
    {
        if(_propertyDescriptors.Count == 0)
            MakeExposedProperties();
        
        if (!_propertyDescriptorsCache.TryGetValue((path, name), out var descriptor))
            throw new ArgumentException($"No descriptor with name '{name}' is registered in '{path}'.");
        
        return descriptor;
    }
    public List<ControlPropertyDescriptor> GetExposedProperties()
    {
        if(_propertyDescriptors.Count == 0)
            MakeExposedProperties();
        
        return _propertyDescriptors;
    }
    
    protected virtual void MakeExposedProperties()
    {
        if (IsPropertiesInitialized)
            return;
        IsPropertiesInitialized = true;
        
        NameProperty = RegisterPropertyDescriptor(new EditableControlPropertyDescriptor<string>("Name", () => Name,
            "General".ToPipedPath(), "Name of the control.", (s =>
            {
                if (string.IsNullOrWhiteSpace(s)) s = "Unnamed Control";
                Name = s;
            })));
        ParentProperty = RegisterPropertyDescriptor(new EditableControlPropertyDescriptor<BaseControl?>("Parent",
            () => Parent,
            "General".ToPipedPath(), "Parent of the control.", c =>
            {
                Parent?.RemoveChild(this);

                c?.AddChild(this);
            },
            onAskedClean: () => Parent?.RemoveChild(this),
            editorHint:"only-existing"));
        
        // VISIBILITY
        VisibilityProperty = RegisterPropertyDescriptor(new EditableControlPropertyDescriptor<bool>("Visibility", () => Visual.Visible,
            "Appearance".ToPipedPath(), "Define if the element is visible or not.", (
                b =>
                {
                    Visual.Visible = b;
                })));
        
        // OPACITY
        OpacityProperty = RegisterPropertyDescriptor(new EditableControlPropertyDescriptor<float>("Opacity", () => Visual.Opacity,
            "Appearance".ToPipedPath(), "Define the opacity of the element.", (f => Visual.Opacity = f),
            f => { return f is >= 0 and <= 1; },
            (f => { return Math.Clamp(f, 0f, 1f); })));
        
        // PIVOT - PROPERTIES
        PivotProperty = RegisterPropertyDescriptor(new EditableControlPropertyDescriptor<Vector2>("Pivot",
            () => { return new Vector2(Visual.PivotX, Visual.PivotY); },
            "Appearance".ToPipedPath().Extend("Pivots"), "Define the pivot point of the element.",
            v =>
            {
                Visual.PivotX = (int)MathF.Round(v.X);
                Visual.PivotY = (int)MathF.Round(v.Y);
            }));
        PivotXUnitProperty = RegisterPropertyDescriptor(new EditableControlPropertyDescriptor<EOriginUnitType>("Pivot X Unit",
            () => Visual.PivotXUnit,
            "Appearance".ToPipedPath().Extend("Pivots"), "Define the unit type for the pivot X coordinate.",
            (u => { Visual.PivotXUnit = u; })));
        PivotYUnitProperty = RegisterPropertyDescriptor(new EditableControlPropertyDescriptor<EOriginUnitType>("Pivot Y Unit",
            () => Visual.PivotYUnit,
            "Appearance".ToPipedPath().Extend("Pivots"), "Define the unit type for the pivot Y coordinate.",
            (u => { Visual.PivotYUnit = u; })));
        
        // POSITION - PROPERTIES
        PositionProperty = RegisterPropertyDescriptor(new EditableControlPropertyDescriptor<Vector2>("Position",
            () => { return new Vector2(Visual.X, Visual.Y); },
            "Appearance".ToPipedPath().Extend("Position"), "Define the position of the element.",
            v =>
            {
                Visual.X = (int)MathF.Round(v.X);
                Visual.Y = (int)MathF.Round(v.Y);
            }));
        PositionXUnitProperty = RegisterPropertyDescriptor(new EditableControlPropertyDescriptor<EPositionUnitType>("Position X Unit",
            () => Visual.XUnit,
            "Appearance".ToPipedPath().Extend("Position"), "Define the unit type for the X coordinate of the element's position.",
            (u => { Visual.XUnit = u; })));
        PositionYUnitProperty = RegisterPropertyDescriptor(new EditableControlPropertyDescriptor<EPositionUnitType>("Position Y Unit",
            () => Visual.YUnit,
            "Appearance".ToPipedPath().Extend("Position"), "Define the unit type for the Y coordinate of the element's position.",
            (u => { Visual.YUnit = u; })));
        
        // SIZE - PROPERTIES
        SizeProperty = RegisterPropertyDescriptor(new EditableControlPropertyDescriptor<Vector2>("Size",
            () => { return new Vector2(Visual.Width, Visual.Height); },
            "Appearance".ToPipedPath().Extend("Size"), "Define the size of the element.",
            v =>
            {
                Visual.Width = (int)MathF.Round(v.X);
                Visual.Height = (int)MathF.Round(v.Y);
            }));
        SizeXUnitProperty = RegisterPropertyDescriptor(new EditableControlPropertyDescriptor<ESizeUnitType>("Size X Unit", () => Visual.WidthUnit,
            "Appearance".ToPipedPath().Extend("Size"), "Define the unit type for the X coordinate of the element's size.",
            u => { Visual.WidthUnit = u; }));
        SizeYUnitProperty = RegisterPropertyDescriptor(new EditableControlPropertyDescriptor<ESizeUnitType>("Size Y Unit", () => Visual.HeightUnit,
            "Appearance".ToPipedPath().Extend("Size"), "Define the unit type for the Y coordinate of the element's size.",
            u => { Visual.HeightUnit = u; }));
        
        // COMPORTMENT MOUSE - PROPERTIES
        IsHitTestVisibleProperty = RegisterPropertyDescriptor(new EditableControlPropertyDescriptor<bool>("Is mouse clickable", () => IsHitTestVisible,
            "Comportment".ToPipedPath().Extend("Mouse"), "Define if the element can receive mouse click.",
            b => IsHitTestVisible = b));
        ClipHitTestToBoundsProperty = RegisterPropertyDescriptor(new EditableControlPropertyDescriptor<bool>("Can click outside bounds", () => ClipHitTestToBounds,
            "Comportment".ToPipedPath().Extend("Mouse"), "Define if the children of this element can receive mouse click even if they are outside the bounds of this element.",
            b => ClipHitTestToBounds = !b));
        PropertyDescriptorsExposed?.Invoke();
    }
    

    public BaseControl? GetControlAt(Vector2 screenPosition)
    {
        if (!IsVisible)
            return null;

        var isOverParent = Visual.HitTest(screenPosition);
        if (!isOverParent && ClipHitTestToBounds)
            return null;

        for (int i = _children.Count - 1; i >= 0; i--)
        {
            var child = _children[i];
            var result = child.GetControlAt(screenPosition);
            if (result != null)
                return result;
        }

        if (IsHitTestVisible && isOverParent)
            return this;

        return null;
    }

    /// <summary>
    /// Update the control's state (e.g., handle animations, update properties, etc.) before the visual is updated and drawn. This method is called every frame and should contain any logic that needs to be processed regularly for the control.
    /// </summary>
    public abstract void OnUpdate();

    /// <summary>
    /// Synchronise the visual state (if for example the control has properties that affect the visual, such as a button's pressed state) before the visual is updated and drawn.
    /// </summary>
    public abstract void SyncVisual();

    public virtual void UpdateControl(Rect parentBounds, Rect screenBounds)
    {
        OnUpdate();

        SyncVisual();

        Visual.UpdateVisual(parentBounds, screenBounds);

        Rect childrenArea = new Rect(0, 0, Visual.LocalBounds.Width, Visual.LocalBounds.Height);
        foreach (var child in _children)
        {
            child.UpdateControl(childrenArea, screenBounds);
        }
    }

    public virtual void DrawControl(IUiRendererContext renderer)
    {
        if (!IsVisible)
            return;

        Visual.DrawVisual(renderer);
    }

    #region Events

    #region Control Events

    /// <summary>
    /// Triggered when the control is added to a parent control.<br/>
    /// This event is called after the control has been added to the parent's children list and the Parent property has been set.<br/>
    /// It is not triggered when the control is added as a root control (i.e., without a parent).
    /// </summary>
    public event EventHandler? OnAddedToParent;

    /// <summary>
    /// Triggered when the control is removed from its parent control.<br/>
    /// This event is called after the control has been removed from the parent's children list and the Parent property has been cleared.<br/>
    /// It is not triggered when the control is removed as a root control (i.e., without a parent).
    /// </summary>
    public event EventHandler? OnRemovedFromParent;

    /// <summary>
    /// Triggered when the control is added as a root control (i.e., without a parent).<br/>
    /// This event is called after the control has been added to the UI system as a root control and is now part of the active UI hierarchy.<br/>
    /// It is not triggered when the control is added to a parent control.
    /// </summary>
    public event EventHandler? OnAddedAsRoot;

    /// <summary>
    /// Triggered when the control is removed as a root control (i.e., without a parent).<br/>
    /// This event is called after the control has been removed from the UI system as a root control and is no longer part of the active UI hierarchy.<br/>
    /// It is not triggered when the control is removed from a parent control.
    /// </summary>
    public event EventHandler? OnRemovedAsRoot;

    /// <summary>
    /// Triggered when the control is brought to the front of the UI hierarchy (i.e., becomes the topmost control).<br/>
    /// This event can only be called on controls that are a root control (i.e., without a parent) and is called after the control has been moved to the front of the UI hierarchy.
    /// </summary>
    public event EventHandler? OnBringToFront;

    /// <summary>
    /// Triggered when the control gains focus.<br/>
    /// This event is called after the control has become the focused control in the UI system.<br/>
    /// </summary>
    public event EventHandler? OnFocusGained;

    /// <summary>
    /// Triggered when the control loses focus.<br/>
    /// This event is called after the control has lost focus and is no longer the focused control in the UI system.<br/>
    /// </summary>
    public event EventHandler? OnFocusLoss;

    #region Non-Virtual Events

    public void CallAddedToParent()
    {
        OnAddedToParent?.Invoke(this, EventArgs.Empty);
        OnAddToParent();
    }

    public void CallRemovedFromParent()
    {
        OnRemovedFromParent?.Invoke(this, EventArgs.Empty);
        OnRemoveFromParent();
    }

    public void CallAddedAsRoot()
    {
        OnAddedAsRoot?.Invoke(this, EventArgs.Empty);
        OnAddAsRoot();
    }

    public void CallRemovedAsRoot()
    {
        OnRemovedAsRoot?.Invoke(this, EventArgs.Empty);
        OnRemoveAsRoot();
    }

    public void CallBringToFront()
    {
        OnBringToFront?.Invoke(this, EventArgs.Empty);
        OnMovedToFront();
    }

    public void CallFocusGain()
    {
        OnFocusGained?.Invoke(this, EventArgs.Empty);
        OnFocusGain();
    }

    public void CallFocusLost()
    {
        OnFocusLoss?.Invoke(this, EventArgs.Empty);
        OnFocusLost();
    }

    #endregion

    #region Virtual Events

    protected virtual void OnAddToParent()
    {
    }

    protected virtual void OnRemoveFromParent()
    {
    }

    protected virtual void OnAddAsRoot()
    {
    }

    protected virtual void OnRemoveAsRoot()
    {
    }

    protected virtual void OnMovedToFront()
    {
    }

    protected virtual void OnFocusGain()
    {
    }

    protected virtual void OnFocusLost()
    {
    }

    #endregion

    #endregion

    #region Mouse Events

    /// <summary>
    /// Triggered when the control is clicked with a mouse button.<br/>
    /// Returns the button that was clicked.
    /// </summary>
    public event EventHandler<MouseButton>? OnMouseClicked;

    /// <summary>
    /// Triggered when the control is double-clicked with a mouse button.<br/>
    /// Returns the button that was double-clicked.
    /// </summary>
    public event EventHandler<MouseButton>? OnMouseDoubleClicked;

    /// <summary>
    /// Triggered when a mouse button is pressed down on the control.<br/>
    /// Returns the button that was pressed.
    /// </summary>
    public event EventHandler<MouseButton>? OnMousePressed;

    /// <summary>
    /// Triggered when a mouse button is released on the control.<br/>
    /// Returns the button that was released.
    /// </summary>
    public event EventHandler<MouseButton>? OnMouseReleased;

    /// <summary>
    /// Triggered when the mouse cursor enters the control's area.
    /// </summary>
    public event EventHandler? OnMouseEntered;

    /// <summary>
    /// Triggered when the mouse cursor leaves the control's area.
    /// </summary>
    public event EventHandler? OnMouseLeaved;

    /// <summary>
    /// Triggered when the mouse cursor moves while over the control.<br/>
    /// Returns the delta position of the mouse movement since the last event.
    /// </summary>
    public event EventHandler<Vector2>? OnMouseMoved;

    /// <summary>
    /// Triggered when the mouse wheel is scrolled while over the control.<br/>
    /// Returns the amount of scroll (positive for scrolling up, negative for scrolling down).
    /// </summary>
    public event EventHandler<int>? OnMouseWheelScrolled;

    #region Non-Virtual Events

    public void CallMouseClick(MouseButton button)
    {
        OnMouseClicked?.Invoke(this, button);
        OnMouseClick(button);
    }

    public void CallMouseDoubleClick(MouseButton button)
    {
        OnMouseDoubleClicked?.Invoke(this, button);
        OnMouseDoubleClick(button);
    }

    public void CallMouseDown(MouseButton button)
    {
        OnMousePressed?.Invoke(this, button);
        _isPressed = true;
        OnMouseDown(button);
    }

    public void CallMouseUp(MouseButton button)
    {
        OnMouseReleased?.Invoke(this, button);
        _isPressed = false;
        OnMouseUp(button);
    }

    public void CallMouseEnter()
    {
        OnMouseEntered?.Invoke(this, EventArgs.Empty);
        _isMouseOver = true;
        OnMouseEnter();
    }

    public void CallMouseLeave()
    {
        OnMouseLeaved?.Invoke(this, EventArgs.Empty);
        _isMouseOver = false;
        OnMouseLeave();
    }

    public void CallMouseMove(Vector2 deltaPosition)
    {
        OnMouseMoved?.Invoke(this, deltaPosition);
        OnMouseMove(deltaPosition);
    }

    public void CallMouseWheelScroll(int scrollAmount)
    {
        OnMouseWheelScrolled?.Invoke(this, scrollAmount);
        OnMouseWheelScroll(scrollAmount);
    }

    #endregion

    #region Virtual Events

    protected virtual void OnMouseClick(MouseButton button)
    {
        EmitEvent("Mouse".ToPipedPath(), "Click", new GuiClickEventContext(this, button));
    }

    protected virtual void OnMouseDoubleClick(MouseButton button)
    {
    }

    protected virtual void OnMouseDown(MouseButton button)
    {
    }

    protected virtual void OnMouseUp(MouseButton button)
    {
    }

    protected virtual void OnMouseEnter()
    {
    }

    protected virtual void OnMouseLeave()
    {
    }

    protected virtual void OnMouseMove(Vector2 deltaPosition)
    {
    }

    protected virtual void OnMouseWheelScroll(int scrollAmount)
    {
    }

    #endregion

    #endregion

    #region Mouse Drag Events

    public event EventHandler? OnDragStarted;
    public event EventHandler? OnDragEnded;
    public event EventHandler<Vector2>? OnDragMoved;

    #region Non-Virtual Events

    public void CallDragStart()
    {
        OnDragStarted?.Invoke(this, EventArgs.Empty);
        _isDragging = true;
        OnDragStart();
    }

    public void CallDragEnd()
    {
        OnDragEnded?.Invoke(this, EventArgs.Empty);
        _isDragging = false;
        OnDragEnd();
    }

    public void CallDragMove(Vector2 deltaPosition)
    {
        OnDragMoved?.Invoke(this, deltaPosition);
        OnDragMove(deltaPosition);
    }

    #endregion

    #region Virtual Events

    protected virtual void OnDragStart()
    {
    }

    protected virtual void OnDragEnd()
    {
    }

    protected virtual void OnDragMove(Vector2 deltaPosition)
    {
    }

    #endregion

    #endregion

    #endregion
}