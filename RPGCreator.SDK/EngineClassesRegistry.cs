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

using System.Buffers;
using System.Reflection;
using RPGCreator.SDK.Common.Attributes;
using RPGCreator.SDK.Common.Debug;
using RPGCreator.SDK.Common.Exceptions;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Internals;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK;

public static class ClassesRegistry
{
    private static bool _initialized = false;
    private static readonly Lock Lock = new();

    /// <summary>
    /// Description of a class in the engine.
    /// </summary>
    public readonly struct EngineClass(Type classType, IEngineClassFactory? factory = null)
    {
        public Type ClassType => classType;
        public StringName Name { get; init; }
        public URN Urn { get; init; }
        public EngineClassMeta Meta { get; init; }
        public IEngineClassFactory Factory { get; init; } = factory ?? new EmptyEngineClassFactory();
        
        // ReSharper disable once MemberCanBePrivate.Global
        public string DisplayName => Meta.DisplayName ?? Name;
        public string Icon => Meta.Icon ?? "default";
        public string SerializationFolder => Meta.SerializationFolder ?? Name;

        public override string ToString()
        {
            return
                $"Class: {Name}({Urn}), DisplayName: {DisplayName}, Icon: {Icon}, SerializationFolder: {SerializationFolder}, Description: {Meta.Description}, Tags: [{string.Join(", ", Meta.Tags ?? [])}]";
        }
    }

    /// <summary>
    /// Define the meta of a class.
    /// </summary>
    public readonly record struct EngineClassMeta
    {
        public string? DisplayName { get; init; }
        public string? Description { get; init; }
        public string? Icon { get; init; }
        public string? SerializationFolder { get; init; }
        public StringName[]? Tags { get; init; }
    }

    /// <summary>
    /// Define the factory of a class.
    /// </summary>
    public interface IEngineClassFactory
    {
        public Result<object?> DefaultConstructor();
        public Result<object?> CreateInstance(object? arg);
    }

    public class GenericFactory<T> : IEngineClassFactory where T : class, IEngineClass, new()
    {
        public Result<object?> DefaultConstructor() => Result<object?>.Success(new T());
        public Result<object?> CreateInstance(object? arg) => Result<object?>.Success(new T());
    }
    
    public class DefaultEngineClassFactory(
        Func<Result<object?>>? defaultConstructor = null,
        Func<Result<object?>, object?>? createInstance = null) : IEngineClassFactory
    {
        public Result<object?> DefaultConstructor()
        {
            return defaultConstructor?.Invoke() ?? Result<object?>.Failure("Default constructor is null");
        }

        public Result<object?> CreateInstance(object? arg)
        {
            return createInstance?.Invoke(arg) ?? Result<object?>.Failure("Create instance is null");
        }
    }

    private class EmptyEngineClassFactory : IEngineClassFactory
    {
        public Result<object?> DefaultConstructor() => Result<object?>.Fail("No class factory defined for this class.");

        public Result<object?> CreateInstance(object? arg) =>
            Result<object?>.Success("No class factory defined for this class.");
    }
    
    private class AbstractEngineClassFactory : IEngineClassFactory
    {
        public Result<object?> DefaultConstructor() => Result<object?>.Fail("An abstract class cannot be instantiated.");
        
        public Result<object?> CreateInstance(object? arg) => Result<object?>.Fail("An abstract class cannot be instantiated.");
    }
    
    private const int InitialCapacity = 64;
    
    private static EngineClass[] EngineClasses { get; set; } = ArrayPool<EngineClass>.Shared.Rent(InitialCapacity);
    private static Dictionary<URN, int> EngineClassIndex { get; } = new();
    private static Dictionary<Type, int> EngineClassTypes { get; } = new();
    public static int TotalCount { get; private set; }
    
    private static Span<EngineClass> GetEngineClassesSpan() => EngineClasses.AsSpan(0, TotalCount);

    public static void ForEachClass(Action<EngineClass> action)
    {
        lock (Lock)
        {
            foreach (var item in GetEngineClassesSpan())
            {
                action(item);
            }
        }
    }

    public static IReadOnlyList<EngineClass> GetEngineClassesWithTag(StringName tag)
    {
        lock (Lock)
        {
            var result = new List<EngineClass>();
            foreach (var item in GetEngineClassesSpan())
            {
                if (item.Meta.Tags?.Contains(tag) ?? false)
                {
                    result.Add(item);
                    if (DebugBuildSettings.EngineTypesRegistry)
                    {
                        Logger.Debug("Found = " + item.Meta.Tags);
                    }

                    continue;
                }
                if (DebugBuildSettings.EngineTypesRegistry)
                {
                    Logger.Debug("Urn not match = " + item.Meta.Tags + " compared to " + tag);
                }
            }
            return result;
        }
    }

    public static IReadOnlyList<EngineClass> GetEngineClassesWithUrnPrefix(string urnPrefix, URN.SearchComparisionType comparisionType = URN.SearchComparisionType.Full)
    {
        int foundCount = 0;
        int totalCount = TotalCount;
        Logger.Info("Search for classes with URN {urnSearch} (comparision type: {comparisionType})", urnPrefix, comparisionType);
        lock (Lock)
        {
            var result = new List<EngineClass>();
            foreach (var item in GetEngineClassesSpan())
            {
                if (item.Urn.StartsWith(urnPrefix, comparisionType))
                {
                    result.Add(item);
                    foundCount++;
                    Logger.Debug("Match: {itemName} ({itemUrn})", item.Name, item.Urn);
                    if (DebugBuildSettings.EngineTypesRegistry)
                    {
                        Logger.Debug("Found = " + item.Urn);
                    }

                    continue;
                }
                if (DebugBuildSettings.EngineTypesRegistry)
                {
                    Logger.Debug("Urn not match = " + item.Urn + " compared to " + urnPrefix);
                }
            }
            Logger.Info("Found {foundCount} classes out of {totalCount} classes.", foundCount, totalCount);
            return result;
        }
    }
    
    public static Result<EngineClass> GetEngineClass(URN urn)
    {
        if (urn == URN.Empty)
            return Result<EngineClass>.Failure("URN cannot be empty.");

        lock (Lock)
        {
            if (EngineClassIndex.TryGetValue(urn, out int index))
            {
                return Result<EngineClass>.Success(EngineClasses[index]);
            }
        }

        return Result<EngineClass>.Failure($"No class with name {urn} found.");
    }

    public static Result<EngineClass> GetEngineClass(Type type)
    {
        if (type == null)
            return Result<EngineClass>.Failure("Type cannot be null.");
        lock (Lock)
        {
            if (EngineClassTypes.TryGetValue(type, out int index))
            {
                return Result<EngineClass>.Success(EngineClasses[index]);
            }
        }

        return Result<EngineClass>.Failure($"No class with type {type} found.");
    }

    private static void EnsureSize(int elementToAdd = 1)
    {
        if (elementToAdd <= 0)
            return;
        
        if (TotalCount + elementToAdd > EngineClasses.Length)
        {
            int newSize = EngineClasses.Length * 2;
            var newArray = ArrayPool<EngineClass>.Shared.Rent(newSize);

            EngineClasses.AsSpan(0, TotalCount).CopyTo(newArray);

            ArrayPool<EngineClass>.Shared.Return(EngineClasses);
            EngineClasses = newArray;
        }
        
    }
    
    public static bool HasUrn(URN urn) => EngineClassIndex.ContainsKey(urn);
    public static bool HasType(Type type) => EngineClassTypes.ContainsKey(type);
    
    public static Result<Type> GetType(URN urn)
    {
        if (!EngineClassIndex.TryGetValue(urn, out int index))
            return Result<Type>.Failure($"Type not found in registry: {urn}");
        
        return Result<Type>.Success(EngineClasses[index].ClassType);
    }

    public static Result<URN> GetUrn(Type type)
    {
        if (!EngineClassTypes.TryGetValue(type, out int index))
            return Result<URN>.Failure($"Type not found in registry: {type.FullName ?? type.Name}");
        
        return Result<URN>.Success(EngineClasses[index].Urn);
    }
    
    public static void RegisterEngineClass(EngineClass engineClass)
    {
        if (engineClass.Urn == URN.Empty)
            throw new ArgumentException("Class URN cannot be empty.", nameof(engineClass));

        lock (Lock)
        {
            if (HasUrn(engineClass.Urn))
            {
                throw new CriticalEngineException(
                    $"Class URN(Unified Resource Name) {engineClass.Urn} is already used by another class. [E-Code: 1]");
            }

            EnsureSize();
            EngineClasses[TotalCount] = engineClass;
            EngineClassIndex[engineClass.Urn] = TotalCount;
            EngineClassTypes[engineClass.ClassType] = TotalCount;
            TotalCount++;
            if (DebugBuildSettings.EngineTypesRegistry)
            {
                Logger.Debug("New size = " + TotalCount);
            }
        }
    }
    private static void _OverrideEngineClass(EngineClass engineClass)
    {
        if (engineClass.Urn == URN.Empty)
            throw new ArgumentException("Class URN cannot be empty.", nameof(engineClass));

        lock (Lock)
        {
            if (!EngineClassIndex.TryGetValue(engineClass.Urn, out var index))
            {
                throw new CriticalEngineException(
                    $"Class URN(Unified Resource Name) {engineClass.Urn} is not registered.");
            }

            EngineClasses[index] = engineClass;
        }
    }

    public static void RegisterOrOverrideEngineClass(EngineClass engineClass)
    {
        if (!HasUrn(engineClass.Urn))
        {
            RegisterEngineClass(engineClass);
            return;
        }
        _OverrideEngineClass(engineClass);
    }

    public static void RemoveEngineClass(URN urn)
    {
        _RemoveEngineClass(urn);
    }

    public static void RemoveEngineClass(EngineClass engineClass)
    {
        _RemoveEngineClass(engineClass.Urn);
    }

    private static void _RemoveEngineClass(URN urn)
    {
        lock (Lock)
        {
            if (!EngineClassIndex.TryGetValue(urn, out var index))
            {
                throw new CriticalEngineException($"Class URN {urn} is not registered.");
            }

            int lastIndex = TotalCount - 1;

            if (index < lastIndex)
            {
                EngineClass lastClass = EngineClasses[lastIndex];
            
                EngineClasses[index] = lastClass;
            
                EngineClassIndex[lastClass.Urn] = index;
            }

            EngineClasses[lastIndex] = default;
            EngineClassIndex.Remove(urn);
            TotalCount--;
        }
    }
    
    private static List<int> _analyzedAsm = new List<int>();
    public static Result AnalyzeAsm(Assembly asm)
    {
        var hash = asm.GetHashCode();

        if (_analyzedAsm.Contains(hash))
            return Result.Fail("ASM already analyzed");
        
        _analyzedAsm.Add(hash);

        var typesWithAttributes =
            asm.GetTypes().Where(t => t.GetCustomAttributes<EngineClassAttribute>() is { } attributes && attributes.Any());

        foreach (var type in typesWithAttributes)
        {
            var attr = type.GetCustomAttribute<EngineClassAttribute>()!;
            var meta = new EngineClassMeta()
            {
                DisplayName = attr.DisplayName,
                Description = attr.Description,
                Icon = attr.Icon,
                SerializationFolder = attr.SerializationFolder,
                Tags = attr.Tags?.Select(t => new StringName(t)).ToArray()
            };

            Type factoryType;
            if (type.IsAbstract)
            {
                factoryType = typeof(AbstractEngineClassFactory);
            }
            else
            {
                try
                {
                    factoryType = typeof(GenericFactory<>).MakeGenericType(type);
                }
                catch (Exception err)
                {
                    Logger.Error(err, "Failed to create factory for class: {0}", type.Name);
                    factoryType = typeof(EmptyEngineClassFactory);
                }
            }

            var factory = Activator.CreateInstance(factoryType) as IEngineClassFactory;
            
            var engineClass = new EngineClass(type, factory)
            {
                Name = new StringName(type.Name),
                Urn = new URN(attr.Urn),
                Meta = meta
            };
            
            RegisterEngineClass(engineClass);
            if (DebugBuildSettings.EngineTypesRegistry_Analyzer)
            {
                Logger.Debug("Added class: {name} (data: {data_class})", type.Name, engineClass);
            }
        }
        
        return Result.Ok();
    }
}
