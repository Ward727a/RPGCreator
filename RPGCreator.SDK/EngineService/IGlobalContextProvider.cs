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

using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.EngineService;

public interface IGlobalContextProvider : IService
{
    public UrnSingleModule ModuleForContext { get; }
    
    /// <summary>
    /// Register a new provider in the global context. This allows you to provide a value that can be accessed globally by other services or systems.
    /// </summary>
    /// <param name="key">The key to identify the provider. This should be unique to avoid conflicts with other providers.</param>
    /// <param name="resolver">The function that will be called to resolve the value when requested. This allows for lazy loading and dynamic values.</param>
    public void RegisterProvider(URN key, Func<object> resolver);

    /// <summary>
    /// Retrieve a value from the global context using the specified key. The value will be resolved using the provider registered with the given key.
    /// </summary>
    /// <param name="key">The key to identify the provider. This should match the key used when registering the provider.</param>
    /// <typeparam name="T">The expected type of the value. The provider's resolver should return a value that can be cast to this type.</typeparam>
    /// <returns>
    /// The value resolved by the provider registered with the given key, cast to the specified type. If no provider is registered with the key or if the resolved value cannot be cast to the specified type, this method may return null or throw an exception depending on the implementation.
    /// </returns>
    public T? Get<T>(URN key);
}