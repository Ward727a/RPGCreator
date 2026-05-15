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
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Registry;

public interface IGuiPropertyEditorRegistry : IService
{
    /// <summary>
    /// The registered property editors.
    /// </summary>
    /// <remark>
    /// The key is the type of the property, and the value is the factory function that creates the property editor.<br/>
    /// </remark>
    /// <remark>
    /// If possible, try to use <see cref="GetPropertyEditor{T}"/> or <see cref="GeneratePropertyEditor"/> instead of accessing this property directly for security reasons.
    /// </remark>
    public IReadOnlyDictionary<Type, Func<ControlPropertyDescriptor, object?, object>> PropertyEditors { get; }
    
    /// <summary>
    /// Returns the number of registered property editors.
    /// </summary>
    public int PropertyEditorCount { get; }
    
    /// <summary>
    /// Register a new property editor factory.<br/>
    /// This allows creating custom property editors inside the Editor UI.
    /// </summary>
    /// <param name="propertyType">The type of the property to register (int, float, MySuperClass, etc.)</param>
    /// <param name="propertyDescriptorFactory">The factory function that creates the property editor.</param>
    /// <param name="overrideIfExist">If true, will override any existing property editor for the given type.</param>
    /// <returns>
    /// Return a <see cref="Result"/> object indicating the success or failure of the operation.<br/>
    /// In case of failure, the <see cref="Result.Error"/> property will contain the error message.
    /// </returns>
    public Result RegisterPropertyEditor(Type propertyType, Func<ControlPropertyDescriptor, object?, object> propertyDescriptorFactory, bool overrideIfExist = false);
    
    /// <summary>
    /// Register a new property editor factory.<br/>
    /// This allows creating custom property editors inside the Editor UI.
    /// </summary>
    /// <param name="propertyDescriptorFactory">The factory function that creates the property editor.</param>
    /// <param name="overrideIfExist">If true, will override any existing property editor for the given type.</param>
    /// <typeparam name="T">The type of the property to register (int, float, MySuperClass, etc.)</typeparam>
    /// <returns>
    /// Return a <see cref="Result"/> object indicating the success or failure of the operation.<br/>
    /// In case of failure, the <see cref="Result.Error"/> property will contain the error message.
    /// </returns>
    public Result RegisterPropertyEditor<T>(Func<ControlPropertyDescriptor, object?, object> propertyDescriptorFactory, bool overrideIfExist = false);
    
    /// <summary>
    /// Unregister a property editor factory.
    /// </summary>
    /// <param name="propertyType">The type of the property to unregister.</param>
    /// <returns>
    /// Return a <see cref="Result"/> object indicating the success or failure of the operation.<br/>
    /// In case of failure, the <see cref="Result.Error"/> property will contain the error message.
    /// </returns>
    public Result UnregisterPropertyEditor(Type propertyType);
    
    /// <summary>
    /// Unregister a property editor factory.
    /// </summary>
    /// <typeparam name="T">The type of the property to unregister.</typeparam>  
    /// <returns>
    /// Return a <see cref="Result"/> object indicating the success or failure of the operation.<br/>
    /// In case of failure, the <see cref="Result.Error"/> property will contain the error message.
    /// </returns>
    public Result UnregisterPropertyEditor<T>();
    
    /// <summary>
    /// Check if a property editor factory is registered for the given type.
    /// </summary>
    /// <param name="propertyType">The type of the property to check.</param>
    /// <returns>True if a property editor is registered for the given type, false otherwise.</returns>
    public bool HasPropertyEditor(Type propertyType);
    
    /// <summary>
    /// Check if a property editor factory is registered for the given type.
    /// </summary>
    /// <typeparam name="T">The type of the property to check.</typeparam>
    /// <returns>True if a property editor is registered for the given type, false otherwise.</returns>
    public bool HasPropertyEditor<T>();
    
    /// <summary>
    /// Get the property editor from a registered factory for the given type.<br/>
    /// The type will be retrieved from the 'Type' property of the Control Property Descriptor.
    /// </summary>
    /// <param name="propertyDescriptor">The descriptor of the property to get the editor for.</param>
    /// <returns>A <see cref="Result{object}"/> object containing the property editor if found, or an error message if not found.</returns>
    public Result<object> GeneratePropertyEditor(ControlPropertyDescriptor propertyDescriptor, object? context = null);

    /// <summary>
    /// Get the property editor from a registered factory for the given type.
    /// </summary>
    /// <param name="propertyType">The type of the property to get the editor for.</param>
    /// <returns>A <see cref="Result{object}"/> object containing the property editor if found, or an error message if not found.</returns>
    public Result<Func<ControlPropertyDescriptor, object?, object>> GetPropertyEditor(Type propertyType);
    
    /// <summary>
    /// Get the property editor from a registered factory for the given type.
    /// </summary>
    /// <typeparam name="T">The type of the property to get the editor for.</typeparam>
    /// <returns>A <see cref="Result{object}"/> object containing the property editor if found, or an error message if not found.</returns>
    public Result<Func<ControlPropertyDescriptor, object?, object>> GetPropertyEditor<T>();
}