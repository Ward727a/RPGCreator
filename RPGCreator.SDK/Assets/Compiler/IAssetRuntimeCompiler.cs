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

using CommunityToolkit.Diagnostics;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.Registry;

namespace RPGCreator.SDK.Assets.Compiler;

public interface ICompilerContext
{
}

public interface IAssetRuntimeCompiler
{
    void Compile(object source, IEcsWorld world, ICompilerContext? context = null);
}

public interface IAssetRuntimeCompiler<in TSource> : IAssetRuntimeCompiler
    where TSource : class
{
    void Compile(TSource source, IEcsWorld world, ICompilerContext? context = null);
}

/// <summary>
/// The base implementation of <see cref="IAssetRuntimeCompiler{TSource}"/>.<br/>
/// An asset runtime compiler is a class that can compile an asset into a runtime representation that is less resource-intensive than the original asset.<br/>
/// For example, the <see cref="MapRuntimeCompiler"/> compiles a <see cref="MapDefinition"/> by converting each <see cref="BaseLayerDef"/> into an entity.<br/>
/// For more information, check the <see cref="MapRuntimeCompiler"/>, <see cref="TileLayerRuntimeCompiler"/> and <see cref="TileChunkRuntimeCompiler"/> for a real implementation.
/// </summary>
/// <typeparam name="TSource">
/// The type of the source asset to compile.<br/>
/// For example, the <see cref="MapDefinition"/> for the <see cref="MapRuntimeCompiler"/>.
/// </typeparam>
public abstract class BaseAssetRuntimeCompiler<TSource> : IAssetRuntimeCompiler<TSource>
    where TSource : class
{
    /// <summary>
    /// Compile the given source asset into a runtime representation.<br/>
    /// This method should be implemented by subclasses to perform the actual compilation.
    /// </summary>
    /// <param name="source">The source asset to compile.</param>
    /// <param name="world">The ECS world to compile the asset into.</param>
    /// <param name="context">
    /// The compiler context - can be null.<br/>
    /// This allows the compiler to get or set additional information that might be useful for compilation.<br/>
    /// For proper usage, you can check the <see cref="TileLayerRuntimeCompiler"/> and <see cref="TileChunkRuntimeCompiler"/>.<br/>
    /// In those 2 classes, the TileLayerCompiler creates a context then passed to the TileChunkCompiler.<br/>
    /// Without this context, the TileChunkCompiler will not have access to the necessary information to compile the asset correctly.<br/>
    /// <br/>
    /// Note: We are using the <see cref="ICompilerContext"/> interface so that we can have a generic implementation of this method.<br/>
    /// In normal cases, the class implementing this method will cast the context to the appropriate type.
    /// </param>
    public abstract void Compile(TSource source, IEcsWorld world, ICompilerContext? context = null);
    
    /// <summary>
    /// A generic implementation of <see cref="IAssetRuntimeCompiler.Compile(object, IEcsWorld, ICompilerContext?)"/>.<br/>
    /// This implementation has been made to work with the <see cref="IRuntimeCompilerRegistry"/> interface (real implementation in RPGCreator.Core.RuntimeCompilerRegistry class).<br/>
    /// Generally, you should not use this method directly.
    /// </summary>
    /// <param name="source">The source asset to compile.</param>
    /// <param name="world">The ECS world to compile the asset into.</param>
    /// <param name="context">The compiler context - can be null.</param>
    public void Compile(object source, IEcsWorld world, ICompilerContext? context = null)
    {
        Guard.IsOfType<TSource>(source);
        Compile((TSource)source, world, context);
    }
}