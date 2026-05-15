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

using System.ComponentModel;
using CommunityToolkit.Diagnostics;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.GameUI;

public abstract class ControlPropertyDescriptor : INotifyPropertyChanged, INotifyPropertyChanging
{
    protected static readonly PipedPath _defaultPath = "Default".ToPipedPath();

    public PipedPath Path { get; private init; }
    /// <summary>
    /// A hint that can help the editor to display a more appropriate UI for the property.
    /// </summary>
    public StringName EditorHint { get; set; } = string.Empty;
    /// <summary>
    /// The name shown in the editor.
    /// </summary>
    public string Name { get; private init; } = string.Empty;
    public Type Type { get; private init; }
    public string Description { get; private init; }
    protected readonly Func<object?> _getter;
    public object? Value => Get();

    public ControlPropertyDescriptor(string name, Type type, Func<object?> getter, PipedPath? path = null,
        string description = "", string editorHint = "")
    {
        Path = path ?? _defaultPath;
        Name = name;
        Type = type;
        Description = description;
        EditorHint = editorHint;
        _getter = getter;
    }

    public virtual object? Get() => _getter();
    
    public T? Get<T>()
    {
        var val = _getter();
        return val is T typedVal ? typedVal : default;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event PropertyChangingEventHandler? PropertyChanging;
    
    protected void RaisePropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    protected void RaisePropertyChanging(string propertyName) => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
}

public class ReadOnlyControlPropertyDescriptor : ControlPropertyDescriptor
{
    public ReadOnlyControlPropertyDescriptor(string name, Type type, Func<object?> getter, PipedPath? path = null, string description = "", string editorHint = "") : base(name, type, getter, path, description, editorHint)
    {
        Guard.IsNotNull(getter);
    }
}

public class ReadOnlyControlPropertyDescriptor<TValue> : ReadOnlyControlPropertyDescriptor
{
    public ReadOnlyControlPropertyDescriptor(string name, Func<TValue?> getter, PipedPath path = default, string description = "", string editorHint = "") : base(name, typeof(TValue), () => getter(), path, description, editorHint)
    {
    }
    
    public new TValue? Get()
    {
        var val = _getter();
        return val is TValue typedVal ? typedVal : default;
    }
}

public class EditableControlPropertyDescriptor : ControlPropertyDescriptor
{
    /// <summary>
    /// Raised when the value of the property changes.<br/>
    /// Properties:<br/>
    /// - newValue: The new value of the property.
    /// - oldValue: The old value of the property.
    /// </summary>
    public event Action<object?, object?>? ValueChanged;

    /// <summary>
    /// Raised when the value of the property is validated.<br/>
    /// Properties:<br/>
    /// - newValue: The new value of the property.
    /// - isValid: True if the value is valid, false otherwise.
    /// </summary>
    public event Action<object?, bool>? Validated;

    /// <summary>
    /// Raised when the value of the property is coerced.<br/>
    /// Properties:<br/>
    /// - newValue: The new value of the property.
    /// - coercedValue: The coerced value of the property.
    /// </summary>
    public event Action<object?, object?>? Coerced;

    public event Action ValueCleaned;

    protected readonly Action<object?> _setter;
    protected readonly Func<object?, bool> _validate;
    protected readonly Func<object?, object?> _coerce;
    protected readonly Action _onCleanAsked;

    public EditableControlPropertyDescriptor(
        string name,
        Type type,
        Func<object?> getter,
        PipedPath? path = null,
        string description = "",
        Action<object?>? setter = null,
        Func<object?, bool>? validate = null,
        Func<object?, object?>? coerce = null,
        Action? onAskedClean = null,
        string editorHint = ""
    ) : base(name, type, getter, path, description, editorHint)
    {
        _setter = setter ?? (_ => { });
        _validate = validate ?? (_ => true);
        _coerce = coerce ?? (o => o);
        _onCleanAsked = onAskedClean ?? (() => { });
    }

    public virtual void Set(object? value)
    {
        if (Validate(value))
        {
            RaisePropertyChanging(nameof(Value));
            var currentValue = _getter();
            _setter(Coerce(value));
            RaiseValueChanged(value, currentValue);
            RaisePropertyChanged(nameof(Value));
        }
    }

    public virtual bool Validate(object? value)
    {
        var isValid = _validate(value);
        RaiseValidated(value, isValid);
        return isValid;
    }

    public virtual object? Coerce(object? value)
    {
        var coercedValue = _coerce(value);
        RaiseCoerced(value, coercedValue);
        return coercedValue;
    }

    public virtual void AskClean()
    {
        _onCleanAsked();
        ValueCleaned?.Invoke();
    }

    protected void RaiseValueChanged(object? newValue, object? oldValue) => ValueChanged?.Invoke(newValue, oldValue);
    protected void RaiseValidated(object? value, bool isValid) => Validated?.Invoke(value, isValid);
    protected void RaiseCoerced(object? value, object? coercedValue) => Coerced?.Invoke(value, coercedValue);
}

public class EditableControlPropertyDescriptor<TValue> : EditableControlPropertyDescriptor
{
    public EditableControlPropertyDescriptor(string name, Func<TValue?> getter, PipedPath path = default,
        string description = "", Action<TValue?>? setter = null,
        Func<TValue, bool>? validate = null, Func<TValue, TValue>? coerce = null, Action? onAskedClean = null, string editorHint = "") : base(name, typeof(TValue),
        () => getter(),
        path,
        description,
        (o) =>
        {
            if (o is TValue value)
            {
                setter?.Invoke(value);
            }
            else
                throw new ArgumentException("Invalid type for setter", nameof(o));
        }, (o) =>
        {
            if (o is not TValue value) return false;
            if (validate == null)
                return true;
            return validate(value);
        }, (o) =>
        {
            if (o is not TValue value) return o;
            if (coerce != null)
                return coerce(value);
            return o;
        },
        onAskedClean ?? (() => { }),
        editorHint)
    {
    }

    public void Set(TValue? value)
    {
        object? currentValue = _getter();
        if (!this.Validate(value)) return;

        RaisePropertyChanging(nameof(Value));
        _setter(this.Coerce(value));

        RaiseValueChanged(value, currentValue);
        RaisePropertyChanged(nameof(Value));
    }

    public new TValue? Get()
    {
        var val = _getter();

        if (val == null)
            return default;

        if (val is not TValue typedValue)
            throw new InvalidCastException("Invalid type for getter");

        return typedValue;
    }

    public bool Validate(TValue? value)
    {
        var isValid = _validate(value);
        RaiseValidated(value, isValid);
        return isValid;
    }

    public TValue? Coerce(TValue? value)
    {
        var coercedValue = _coerce(value);

        if (coercedValue is not TValue typedCoercedValue)
            throw new ArgumentException("Invalid type for coerce", nameof(value));

        RaiseCoerced(value, coercedValue);
        return typedCoercedValue;
    }
}