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

using System.Diagnostics.CodeAnalysis;

namespace RPGCreator.SDK;

/// <summary>
/// Just a marker interface for services.
/// </summary>
public interface IService
{
}

public interface IServiceProvider
{
    /// <summary>
    /// Get a service of type T. Throws an exception if not found.
    /// </summary>
    /// <param name="groupName">Optional group name for the service.</param>
    /// <typeparam name="T">Type of the service.</typeparam>
    /// <returns>The service instance.</returns>
    T GetService<T>(string groupName = "") where T : class, IService;
    /// <summary>
    /// Try to get a service of type T. Returns true if found, false otherwise.
    /// </summary>
    /// <param name="service">The output service instance.</param>
    /// <param name="groupName">Optional group name for the service.</param>
    /// <typeparam name="T">Type of the service.</typeparam>
    /// <returns>True if the service was found, false otherwise.</returns>
    bool TryGetService<T>([NotNullWhen(true)] out T? service,string groupName = "") where T : class, IService;
    /// <summary>
    /// Register a service of type T with an optional group name.
    /// </summary>
    /// <param name="service">The service instance to register.</param>
    /// <param name="groupName">Optional group name for the service.</param>
    /// <typeparam name="T">Type of the service.</typeparam>
    void RegisterService<T>(T service, string groupName) where T : class, IService;
}