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
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Assets.Runtime;

/// <summary>
/// Just a marker interface for definitions.
/// </summary>
/// <remarks>
/// Do not use this interface directly, use <see cref="IDefinition{TRuntime}"/> and <see cref="IRuntime{TDefinition}"/> instead.
/// </remarks>
public interface IDefinition;

// ReSharper disable once UnusedTypeParameter
public interface IDefinition<TRuntime> : IDefinition where TRuntime : IRuntime;

/// <summary>
/// Just a marker interface for runtimes.
/// </summary>
/// <remarks>
/// Do not use this interface directly, use <see cref="IDefinition{TRuntime}"/> and <see cref="IRuntime{TDefinition}"/> instead.
/// </remarks>
public interface IRuntime;

public interface IRuntime<out TDefinition> : IRuntime where TDefinition : IDefinition
{
    public TDefinition Definition { get; }
};

public interface IRuntimeMaker : IService
{
    /// <summary>
    /// Register a maker for a definition and runtime.
    /// </summary>
    /// <param name="maker">The maker function to register.</param>
    /// <param name="overrideExisting">If true, any existing maker for the given definition and runtime will be overridden.</param>   
    /// <typeparam name="TDefinition">The type of the definition to register the maker for.</typeparam>
    /// <typeparam name="TRuntime">The type of the runtime to register the maker for.</typeparam>
    public void RegisterMaker<TDefinition, TRuntime>(Func<TDefinition, Result<TRuntime>> maker, bool overrideExisting = false)
        where TDefinition : class, IDefinition<TRuntime>
        where TRuntime : class, IRuntime<TDefinition>;
    
    /// <summary>
    /// Create a runtime from a definition.
    /// </summary>
    /// <param name="definition">The definition to create the runtime from.</param>
    /// <param name="createNew">If true, a new runtime will be created even if one already exists for the given definition.</param>
    /// <typeparam name="TRuntime">The type of the runtime to create.</typeparam>
    /// <typeparam name="TDefinition">The type of the definition to create the runtime from.</typeparam>
    /// <returns>
    /// The created runtime, or an existing runtime if one already exists for the given definition and createNew is false.
    /// </returns>
    public Result<TRuntime> CreateRuntime<TRuntime, TDefinition>(TDefinition definition, bool createNew = false)
        where TDefinition : class, IDefinition<TRuntime>
        where TRuntime : class, IRuntime<TDefinition>;
}