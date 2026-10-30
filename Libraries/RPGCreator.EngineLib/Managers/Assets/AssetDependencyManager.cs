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

using System.Linq.Expressions;
using RPGCreator.SDK.Assets.Actors;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.EngineLib.Managers.Assets;

[EngineService(typeof(IAssetDependencyManager))]
public class AssetDependencyManager : IAssetDependencyManager
{
    private static readonly ScopedLogger Logger = SDK.Common.Logging.Logger.ForContext<AssetDependencyManager>();
    private readonly Dictionary<Type, Action<IAssetDto, List<Ulid>>> _compiledExtractors = new();

    private readonly Dictionary<Ulid, HashSet<Ulid>> _dependencies = new();
    private readonly Dictionary<Ulid, HashSet<Ulid>> _dependents = new();
    
    public void RegisterType(Type assetType)
    {
        if (_compiledExtractors.ContainsKey(assetType))
        {
            return;
        }
        
        var assetParam = Expression.Parameter(typeof(IAssetDto), "asset");
        var dependenciesParam = Expression.Parameter(typeof(List<Ulid>), "dependencies");
        var castedAsset = Expression.Convert(assetParam, assetType);
        
        var methodCalls = new List<Expression>();
        var addMethod = typeof(List<Ulid>).GetMethod(nameof(List<>.Add));
        var addRangeMethod = typeof(List<Ulid>).GetMethod(nameof(List<>.AddRange));

        ScanProperties(assetType, castedAsset, methodCalls);
        
        if (methodCalls.Count == 0)
        {
            _compiledExtractors[assetType] = (_, _) => { };
            return;
        }
        
        var block = Expression.Block(methodCalls);
        var lambda = Expression.Lambda<Action<IAssetDto, List<Ulid>>>(block, assetParam, dependenciesParam);
        
        _compiledExtractors[assetType] = lambda.Compile();
        Logger.Debug("Registered {number} propertiesGetter for type {type}", args: [methodCalls.Count, assetType]);

        return;
        void ScanProperties(Type currentType, Expression currentInstance, List<Expression> currentBlock)
        {
            var properties = currentType.GetProperties();

            foreach (var prop in properties)
            {
                if (prop.GetIndexParameters().Length > 0)
                    continue;

                bool isContainer = Attribute.IsDefined(prop, typeof(AssetContainerAttribute));
                bool isReference = Attribute.IsDefined(prop, typeof(AssetReferenceAttribute));

                if (!isContainer && !isReference)
                    continue;

                var propertyAccess = Expression.Property(currentInstance, prop);
                var propType = prop.PropertyType;

                if (isContainer)
                {
                    // Case A: The container property is a Dictionary<TKey, TValue> -> iterate over .Values
                    if (propType.IsGenericType && (
                            propType.GetGenericTypeDefinition() == typeof(Dictionary<,>) ||
                            propType.GetGenericTypeDefinition() == typeof(IDictionary<,>) ||
                            propType.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)))
                    {
                        var valueType = propType.GetGenericArguments()[1];
                        var valuesProp = propType.GetProperty("Values")!;
                        var valuesAccess = Expression.Property(propertyAccess, valuesProp);

                        AddEnumerableContainerLoop(valuesAccess, valueType, propertyAccess, propType, currentBlock);
                        continue;
                    }

                    // Case B: The container property is a List<T> / IEnumerable<T> -> iterate over elements
                    var enumInterface = propType.GetInterfaces()
                        .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                    if (enumInterface != null)
                    {
                        var elementType = enumInterface.GetGenericArguments()[0];
                        AddEnumerableContainerLoop(propertyAccess, elementType, propertyAccess, propType, currentBlock);
                        continue;
                    }

                    // Case C: Standard single object container
                    var innerCalls = new List<Expression>();
                    ScanProperties(propType, propertyAccess, innerCalls);

                    if (innerCalls.Count > 0)
                    {
                        var nullCheck = Expression.NotEqual(propertyAccess, Expression.Constant(null, propType));
                        var block = Expression.Block(innerCalls);
                        currentBlock.Add(Expression.IfThen(nullCheck, block));
                    }
                    continue;
                }

                if (!Attribute.IsDefined(prop, typeof(AssetReferenceAttribute)))
                    continue;

                var extractionBlocks = new List<Expression>();

                if (propType == typeof(Ulid))
                {
                    extractionBlocks.Add(Expression.Call(dependenciesParam, addMethod!, propertyAccess));
                }
                else if (propType.IsGenericType && (
                         propType.GetGenericTypeDefinition() == typeof(Dictionary<,>) ||
                         propType.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>) ||
                         propType.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
                {
                    var genericArgs = propType.GetGenericArguments();
                    if (genericArgs[0] == typeof(Ulid))
                    {
                        var keysProp = propType.GetProperty("Keys");
                        extractionBlocks.Add(Expression.Call(dependenciesParam, addRangeMethod!, Expression.Property(propertyAccess, keysProp!)));
                    }
                    if (genericArgs[1] == typeof(Ulid))
                    {
                        var valuesProp = propType.GetProperty("Values");
                        extractionBlocks.Add(Expression.Call(dependenciesParam, addRangeMethod!, Expression.Property(propertyAccess, valuesProp!)));
                    }
                }
                else if (typeof(IEnumerable<Ulid>).IsAssignableFrom(propType))
                {
                    extractionBlocks.Add(Expression.Call(dependenciesParam, addRangeMethod!, propertyAccess));
                }

                if (extractionBlocks.Count > 0)
                {
                    if (propType.IsValueType)
                    {
                        currentBlock.AddRange(extractionBlocks);
                    }
                    else
                    {
                        var nullCheck = Expression.NotEqual(propertyAccess, Expression.Constant(null, propType));
                        var block = Expression.Block(extractionBlocks);
                        currentBlock.Add(Expression.IfThen(nullCheck, block));
                    }
                }
            }
        }
        
        void AddEnumerableContainerLoop(Expression enumerableExpr, Type elementType, Expression rootPropExpr, Type rootPropType, List<Expression> targetBlock)
        {
            var loopVar = Expression.Variable(elementType, "item");
            var innerCalls = new List<Expression>();
            ScanProperties(elementType, loopVar, innerCalls);

            if (innerCalls.Count == 0)
                return;

            var enumeratorType = typeof(IEnumerator<>).MakeGenericType(elementType);
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);
            var enumeratorVar = Expression.Variable(enumeratorType, "enumerator");
            var getEnumeratorCall = Expression.Call(enumerableExpr, enumerableType.GetMethod("GetEnumerator")!);
            var moveNextCall = Expression.Call(enumeratorVar, typeof(System.Collections.IEnumerator).GetMethod("MoveNext")!);
            var breakLabel = Expression.Label("LoopBreak");

            var loopBody = Expression.Block(
                [loopVar],
                Expression.Assign(loopVar, Expression.Property(enumeratorVar, "Current")),
                Expression.Block(innerCalls)
            );

            var loop = Expression.Block(
                [enumeratorVar],
                Expression.Assign(enumeratorVar, getEnumeratorCall),
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.Equal(moveNextCall, Expression.Constant(true)),
                        loopBody,
                        Expression.Break(breakLabel)
                    ),
                    breakLabel
                )
            );

            var nullCheck = Expression.NotEqual(rootPropExpr, Expression.Constant(null, rootPropType));
            targetBlock.Add(Expression.IfThen(nullCheck, loop));
        }

    
    }

    public void UnregisterType(Type assetType)
    {
        _compiledExtractors.Remove(assetType);
        Logger.Warning("Asset type {assetType} has been unregistered from the dependency manager.", args: [assetType]);
    }

    public void AutoFillDependencies(IAssetDto asset)
    {
        asset.Dependencies.Clear();
        
        if (_compiledExtractors.TryGetValue(asset.GetType(), out var extractor))
        {
            extractor(asset, asset.Dependencies);

            if (asset is ActorDefinition actor)
            {
                foreach (var actorFeatureData in actor.Features)
                {
                    actorFeatureData.CollectDependencies(asset.Dependencies);
                }
            }
        }
    }

    public void RegisterAssetDependencies(Ulid assetId, List<Ulid> dependencies)
    {
        if (!_dependencies.TryGetValue(assetId, out var deps))
        {
            deps = [];
            _dependencies[assetId] = deps;
        }

        foreach (var dependencyId in dependencies)
        {
            deps.Add(dependencyId);

            if (!_dependents.TryGetValue(dependencyId, out var reverseDeps))
            {
                reverseDeps = [];
                _dependents[dependencyId] = reverseDeps;
            }

            reverseDeps.Add(assetId);
        }
    }

    public void ClearAssetDependencies(Ulid assetId)
    {
        if (!_dependencies.Remove(assetId, out var dependencies))
            return;

        foreach (var dependencyId in dependencies)
        {
            _dependents[dependencyId].Remove(assetId);
        }
    }

    public void LoadHeader(AssetHeader assetHeader)
    {
        if (assetHeader.Dependencies.Count == 0)
            return;

        RegisterAssetDependencies(assetHeader.AssetId, assetHeader.Dependencies);
    }

    public bool HasDependency(Ulid assetId, Ulid dependencyId)
    {
        return _dependencies.TryGetValue(assetId, out var deps) && deps.Contains(dependencyId);
    }

    public bool IsUsedBy(Ulid assetId, Ulid dependentId)
    {
        return _dependents.TryGetValue(dependentId, out var reverseDeps) && reverseDeps.Contains(assetId);
    }

    public IReadOnlyCollection<Ulid> GetDependencies(Ulid assetId)
    {
        return _dependencies.TryGetValue(assetId, out var deps) ? deps : [];
    }

    public IReadOnlyCollection<Ulid> GetDependents(Ulid assetId)
    {
        return _dependents.TryGetValue(assetId, out var reverseDeps) ? reverseDeps : [];
    }
}