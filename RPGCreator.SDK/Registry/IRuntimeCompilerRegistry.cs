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

using RPGCreator.SDK.Assets.Compiler;
using RPGCreator.SDK.ECS;

namespace RPGCreator.SDK.Registry;

public interface IRuntimeCompilerRegistry : IService
{

    /// <summary>
    /// Register a runtime compiler.
    /// </summary>
    /// <param name="compiler">The compiler to register.</param>
    /// <param name="overrideIfExist">If a compiler with the same type is already registered, should it be overridden?</param>
    /// <typeparam name="TSource">The type of the source asset.</typeparam>
    /// <returns>
    /// Return a unique identifier for the compiler.<br/>
    /// This can be used to unregister the compiler later.<br/>
    /// For example, if you create a module that needs to compile assets, you can register the compiler with the module, and unregister it when the module is unloaded.
    /// </returns>
    public Ulid RegisterRuntimeCompiler<TSource>(IAssetRuntimeCompiler<TSource> compiler, bool overrideIfExist = false)
        where TSource : class;

    /// <summary>
    /// Compile the given source asset into a runtime representation.<br/>
    /// This method is asking for a <paramref name="sourceType"/> parameter so we can use the GetType method to determine the type of the source asset.<br/>
    /// For example of usage, check the <see cref="MapRuntimeCompiler"/> class.
    /// <code>
    /// interface MyInterface { }
    /// 
    /// public class MyClass : MyInterface { }
    /// public class MyClass2 : MyInterface { }
    /// 
    /// public class MyClassUsingCompiler {
    ///
    ///     public List&lt;MyInterface&gt; listOf_MyClass_And_MyClass2 = new();
    ///
    ///     public MyClassUsingCompiler()
    ///     {
    ///         listOf_MyClass_And_MyClass2.Add(new MyClass());
    ///         listOf_MyClass_And_MyClass2.Add(new MyClass2());
    ///
    ///         foreach (var item in listOf_MyClass_And_MyClass2)
    ///         {
    ///             // Here, on the first item, the compiler will be called with the type of MyClass (because it was added first)
    ///             // Then, on the second item, the compiler will be called with the type of MyClass2 (because it was added second)
    ///             RegistryServices.RuntimeCompilerRegistry.Compile(item.GetType(), item, world);
    ///         }
    /// 
    ///     }
    /// 
    /// }
    /// </code>
    /// </summary>
    /// <param name="sourceType">The type of the source asset.</param>
    /// <param name="source">The source asset to compile.</param>
    /// <param name="world">The ECS world to compile the asset into.</param>
    /// <param name="context">
    /// The compiler context - can be null.<br/>
    /// For more details, check the <see cref="BaseAssetRuntimeCompiler{TSource}"/> documentation.
    /// </param>
    public void Compile(Type sourceType, object source, IEcsWorld world, ICompilerContext? context = null);
    
    /// <summary>
    /// Compile the given source asset into a runtime representation.
    /// </summary>
    /// <param name="source">The source asset to compile.</param>
    /// <param name="world">The ECS world to compile the asset into.</param>
    /// <param name="context">
    /// The compiler context - can be null.<br/>
    /// For more details, check the <see cref="BaseAssetRuntimeCompiler{TSource}"/> documentation.
    /// </param>
    /// <typeparam name="TSource">The type of the source asset.</typeparam>
    public void Compile<TSource>(TSource source, IEcsWorld world, ICompilerContext? context = null) where TSource : class;
    
    /// <summary>
    /// Compile the given source asset into a runtime representation.<br/>
    /// This method is used to compile assets using a specific compiler identified by its unique ID.
    /// </summary>
    /// <param name="compilerId">The unique ID of the compiler to use.</param>
    /// <param name="source">The source asset to compile.</param>
    /// <param name="world">The ECS world to compile the asset into.</param>
    /// <param name="context">
    /// The compiler context - can be null.<br/>
    /// For more details, check the <see cref="BaseAssetRuntimeCompiler{TSource}"/> documentation.
    /// </param>
    public void Compile(Ulid compilerId, object source, IEcsWorld world, ICompilerContext? context = null);
    
    /// <summary>
    /// Return the registered compiler for the given type.<br/>
    /// This method can be used to retrieve a specific compiler for a specific type and skipping the need to search for the compiler by type.<br/>
    /// It should be used as much as possible for performance reasons.
    /// </summary>
    /// <typeparam name="TCompiler">The type of the compiler to retrieve.</typeparam>   
    /// <typeparam name="TSource">The type of the source asset.</typeparam>
    /// <returns>
    /// The registered compiler for the given type.
    /// </returns>
    public TCompiler GetRegisteredCompiler<TCompiler, TSource>() where TSource : class;
    
    /// <summary>
    /// Unregister a runtime compiler.<br/>
    /// This method can be used by modules to unregister their compiler when they are unloaded.
    /// </summary>
    /// <param name="uniqueId">The unique ID of the compiler to unregister.</param>  
    /// <returns>
    /// True if the compiler was successfully unregistered; false if the compiler was not found.
    /// </returns>
    public bool UnregisterRuntimeCompiler(Ulid uniqueId);
    
    /// <summary>
    /// Check if a compiler is registered.
    /// </summary>
    /// <param name="uniqueId">The unique ID of the compiler to check.</param> 
    /// <returns>
    /// True if the compiler is registered; false otherwise.
    /// </returns>
    public bool HasRegisteredCompiler(Ulid uniqueId);
    
    /// <summary>
    /// Check if a compiler is registered for the given type.
    /// </summary>
    /// <param name="sourceType">The type of the source asset.</param>
    /// <returns>
    /// True if the compiler is registered; false otherwise.
    /// </returns>
    public bool HasRegisteredCompiler(Type sourceType);
}