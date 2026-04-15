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

using RPGCreator.SDK.GameUI;
using RPGCreator.SDK.Registry;
using RPGCreator.SDK.Types;

namespace RPGCreator.Core.Registry;

/// <summary>
/// A class to register and manage property editors for controls.<br/>
/// This class is used to create the corresponding UI editor for each type of <see cref="ControlPropertyDescriptor"/>, and can be used to register custom one.<br/>
/// This is used for the engine editor UI (not the game UI!).<br/>
/// <br/>
/// For example of <b>usage</b>, you can see the "PropertyEditorSelector" in the "RPGCreator.UI" project.
/// For example of <b>registration</b>, you can see the "App.axaml.cs" in the "RPGCreator.UI" project.
/// </summary>
public class GuiPropertyEditorRegistry : IGuiPropertyEditorRegistry
{
    private readonly Dictionary<Type, Func<ControlPropertyDescriptor, object>> _propertyEditors = new();

    public IReadOnlyDictionary<Type, Func<ControlPropertyDescriptor, object>> PropertyEditors => _propertyEditors.AsReadOnly();
    public int PropertyEditorCount => _propertyEditors.Count;

    public Result RegisterPropertyEditor(Type propertyType, Func<ControlPropertyDescriptor, object> propertyDescriptorFactory, bool overrideIfExist = false)
    {
        if (propertyType == null) return Result.Fail("Property type cannot be null");
        if (propertyDescriptorFactory == null) return Result.Fail("Property descriptor factory cannot be null");
        
        if (_propertyEditors.ContainsKey(propertyType) && !overrideIfExist)
        {
            return Result.Fail($"Property editor for type: '{propertyType.FullName}' already registered and overrideIfExist is false");
        }
        
        _propertyEditors[propertyType] = propertyDescriptorFactory;
        return Result.Success();
    }

    public Result RegisterPropertyEditor<T>(Func<ControlPropertyDescriptor, object> propertyDescriptorFactory, bool overrideIfExist = false)
    {
        if (propertyDescriptorFactory == null) return Result.Fail("Property descriptor factory cannot be null");
        
        return RegisterPropertyEditor(typeof(T), propertyDescriptorFactory, overrideIfExist);
    }

    public Result UnregisterPropertyEditor(Type propertyType)
    {
        if (propertyType == null) return Result.Fail("Property type cannot be null");
        
        if (_propertyEditors.Remove(propertyType))
        {
            return Result.Success();
        }
        
        return Result.Fail($"Property editor not found for type: {propertyType.FullName}");
    }

    public Result UnregisterPropertyEditor<T>()
    {
        return UnregisterPropertyEditor(typeof(T));
    }

    public bool HasPropertyEditor(Type propertyType)
    {
        return _propertyEditors.ContainsKey(propertyType);
    }

    public bool HasPropertyEditor<T>()
    {
        return HasPropertyEditor(typeof(T));
    }

    public Result<object> GeneratePropertyEditor(ControlPropertyDescriptor propertyDescriptor)
    {
        if (propertyDescriptor == null) return Result.Fail("Property descriptor cannot be null");

        var propertyType = propertyDescriptor.Type;
        
        if (_propertyEditors.TryGetValue(propertyType, out var editor))
        {
            return editor(propertyDescriptor);
        }
        
        return Result<object>.Fail($"Property editor not found for type: {propertyType.FullName}");
    }
    
    public Result<Func<ControlPropertyDescriptor, object>> GetPropertyEditor(Type propertyType)
    {
        if (_propertyEditors.TryGetValue(propertyType, out var editor))
        {
            return Result<Func<ControlPropertyDescriptor, object>>.Ok(editor);
        }
        
        return Result<Func<ControlPropertyDescriptor, object>>.Fail($"Property editor not found for type: {propertyType.FullName}");
    }

    public Result<Func<ControlPropertyDescriptor, object>> GetPropertyEditor<T>()
    {
        return GetPropertyEditor(typeof(T));
    }
}