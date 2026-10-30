using System.Collections.ObjectModel;
using System.Numerics;
using CommunityToolkit.HighPerformance;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.AssetsPre;

[EngineService(typeof(ITypesRegistry))]
public class TypesRegistryDiscriminator : ITypesRegistry
{
    private const char GenericCharSeparator = ',';
    private const char GenericCharGroupStart = '<';
    private const char GenericCharGroupEnd = '>';
    
    
    private static readonly ScopedLogger Logger = Common.Logging.Logger.ForContext<TypesRegistryDiscriminator>();
    
    /// <summary>
    /// Internal types mapping. Define types where we can't put the attribute on (like System types).
    /// </summary>
    private static readonly IReadOnlyDictionary<URN, Type> _internalTypeMapping = new Dictionary<URN, Type>()
    {
        { "rpgc://sdk/common/ulid", typeof(Ulid)},
        {"rpgc://sdk/common/int", typeof(int)},
        {"rpgc://sdk/common/float", typeof(float)},
        {"rpgc://sdk/common/string", typeof(string)},
        {"rpgc://sdk/common/bool", typeof(bool)},
        { "rpgc://sdk/common/double", typeof(double)},
        { "rpgc://sdk/common/long", typeof(long)},
        { "rpgc://sdk/common/short", typeof(short)},
        { "rpgc://sdk/common/byte", typeof(byte)},
        { "rpgc://sdk/common/decimal", typeof(decimal)},
        { "rpgc://sdk/common/sbyte", typeof(sbyte)},
        { "rpgc://sdk/common/uint", typeof(uint)},
        { "rpgc://sdk/common/ulong", typeof(ulong)},
        { "rpgc://sdk/common/ushort", typeof(ushort)},
        { "rpgc://sdk/common/char", typeof(char)},
        { "rpgc://sdk/common/vector2", typeof(Vector2)},
        { "rpgc://sdk/common/observable_collection/1", typeof(ObservableCollection<>)},
        { "rpgc://sdk/common/list", typeof(List<>)},
        { "rpgc://sdk/common/tuple/1", typeof(ValueTuple<>)},
        { "rpgc://sdk/common/tuple/2", typeof(ValueTuple<,>)},
        { "rpgc://sdk/common/tuple/3", typeof(ValueTuple<,,>)},
        { "rpgc://sdk/common/tuple/4", typeof(ValueTuple<,,,>)},
        { "rpgc://sdk/common/tuple/5", typeof(ValueTuple<,,,,>)},
        { "rpgc://sdk/common/tuple/6", typeof(ValueTuple<,,,,,>)},
        { "rpgc://sdk/common/tuple/7", typeof(ValueTuple<,,,,,,>)},
        { "rpgc://sdk/common/dictionary/2", typeof(Dictionary<,>)},
        { "rpgc://sdk/common/version", typeof(Version)}
    };
    
    private readonly Dictionary<URN, Type> _keyToType = new(_internalTypeMapping);
    // Need to change this to a better and more optimized way, but for now it's fine.
    private IReadOnlyDictionary<Type, URN> TypeToKey => _keyToType.ToDictionary(kv => kv.Value, kv => kv.Key);

    public void RegisterMapping(URN key, Type type)
    {
        _keyToType[key] = type;
        Logger.Debug("Registered type: {key} -> {typeName}", args:[key, type.FullName ?? "UNKNOWN"]);
    }
    
    internal void DebugLog()
    {
        Logger.Debug("Current type mappings: {count}", args:[_keyToType.Count]);
        foreach (var type in _keyToType)
        {
            Logger.Debug(" - {key} -> {typeName}", args:[type.Key, type.Value.FullName ?? "UNKNOWN"]);
        }
    }
    
    private List<string> SplitGenericArguments(string innerContent)
    {
        var args = new List<string>();
        int depth = 0;
        int lastStart = 0;

        for (int i = 0; i < innerContent.Length; i++)
        {
            if (innerContent[i] == GenericCharGroupStart) depth++;
            if (innerContent[i] == GenericCharGroupEnd) depth--;
        
            if (innerContent[i] == GenericCharSeparator && depth == 0)
            {
                args.Add(innerContent.Substring(lastStart, i - lastStart).Trim());
                lastStart = i + 1;
            }
        }
        
        args.Add(innerContent.Substring(lastStart).Trim());
        return args;
    }
    public Result<Type> GetType(URN key)
    {
        var stringKey = key.ToString();
        if (!stringKey.Contains(GenericCharGroupStart))
        {
            return ResolveSimpleType(key);
        }

        int firstBracket = stringKey.IndexOf(GenericCharGroupStart);
        int lastBracket = stringKey.LastIndexOf(GenericCharGroupEnd);
    
        string baseUrn = stringKey.Substring(0, firstBracket);
        string innerContent = stringKey.Substring(firstBracket + 1, lastBracket - firstBracket - 1);

        var baseTypeResult = ResolveSimpleType(new URN(baseUrn));
        if (!baseTypeResult.IsSuccess) return baseTypeResult;
        Type baseType = baseTypeResult.Value;

        List<string> argKeys = SplitGenericArguments(innerContent);
        Type[] resolvedArgs = new Type[argKeys.Count];

        for (int i = 0; i < argKeys.Count; i++)
        {
            var argResult = GetType(argKeys[i]);
            if (!argResult.IsSuccess) return argResult;
            resolvedArgs[i] = argResult.Value;
        }

        return baseType.MakeGenericType(resolvedArgs);

        Result<Type> ResolveSimpleType(URN urn)
        {
            if (urn == URN.Empty) return Result<Type>.Fail("URN cannot be empty.");
            if (_keyToType.TryGetValue(urn, out var type)) return Result<Type>.Ok(type);
            if(ClassesRegistry.HasUrn(urn)) return Result<Type>.Ok(ClassesRegistry.GetType(urn).Value);
            return Result<Type>.Fail($"Type {urn} not found in registry.");
        }
    }

    public Result<URN> GetKey(Type type) 
    {
        if (type.IsGenericType)
        {
            Type genericDefinition = type.GetGenericTypeDefinition();
        
            var baseKeyResult = GetSimpleKey(genericDefinition);
            if (!baseKeyResult.IsSuccess) return baseKeyResult;

            Type[] genericArgs = type.GetGenericArguments();
            List<string> argKeys = new List<string>();

            foreach (var arg in genericArgs)
            {
                var argKeyResult = GetKey(arg);
                if (!argKeyResult.IsSuccess) return argKeyResult;
                argKeys.Add(argKeyResult.Value);
            }

            string finalKey = $"{baseKeyResult.Value}{GenericCharGroupStart}{string.Join(GenericCharSeparator, argKeys)}{GenericCharGroupEnd}";
            return Result<URN>.Ok(finalKey);
        }

        return GetSimpleKey(type);

        Result<URN> GetSimpleKey(Type type)
        {
            if (TypeToKey.TryGetValue(type, out var urn))
            {
                return Result<URN>.Ok(urn);
            }
            
            if(ClassesRegistry.HasType(type))
                return Result<URN>.Ok(ClassesRegistry.GetUrn(type).Value);

            Logger.Error("Type {typeName} is not registered in the registry.", type.FullName ?? "UNKNOWN");
            return Result<URN>.Fail($"Type {type.FullName} not found.");
        }
    }

    public bool HasKey(URN key)
    {
        if(key.ToString().Contains(GenericCharGroupStart))
        {
            var baseKey = key.ToString().Split(GenericCharGroupStart)[0];
            var numberOfGenericArg = key.ToString().Count(GenericCharSeparator);
            return _keyToType.ContainsKey(new URN($"{baseKey}/{numberOfGenericArg}"));
        }
        return _keyToType.ContainsKey(key);
    }

    public bool HasType(Type type)
    {
        if (type.IsGenericType)
        {
            type = type.GetGenericTypeDefinition();
        }
        return TypeToKey.ContainsKey(type);
    }
}