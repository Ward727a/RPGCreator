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

using System.Collections.Concurrent;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.EngineClass;

namespace RPGCreator.SDK.Registry;

public class ClassesRegistry : IClassesRegistry
{
    private ConcurrentDictionary<URN, Func<EBaseClass>> _classes = new();
    private ConcurrentDictionary<Type, URN> _typeToUrn = new();
    private IReadOnlyDictionary<URN, Type> _urnToType => _typeToUrn.ToDictionary(x => x.Value, x => x.Key);
    private ConcurrentDictionary<URN, IReadOnlyDictionary<StringName, EPropertyContext>> _classesProperties = new();

    public ClassesRegistry()
    {
        // We disable the warning because we know what we are doing here
#pragma warning disable CS0618 // Type or member is obsolete
        Register<TestClass>(TestClass.ClassUrn, TestClass.Create, TestClass.GetProperties());
#pragma warning restore CS0618 // Type or member is obsolete
    }
    
    public Result Register<T>(URN urn, Func<T> constructor, IReadOnlyDictionary<StringName, EPropertyContext>? properties = null) where T : EBaseClass
    {
        if(_classes.ContainsKey(urn))
        {
            return Result.Fail($"Class with URN '{urn}' is already registered.");
        }
        
        _classes[urn] = constructor;
        _typeToUrn[typeof(T)] = urn;

        if (properties == null)
            properties = new Dictionary<StringName, EPropertyContext>();
        
        _classesProperties[urn] = properties;
        
        return Result.Ok();
    }

    public Result Unregister(URN urn)
    {
        if(!_classes.ContainsKey(urn))
        {
            return Result.Fail($"Class with URN '{urn}' is not registered.");
        }
        
        _classes.Remove(urn, out _);
        _typeToUrn.Remove(_urnToType[urn], out _);
        _classesProperties.Remove(urn, out _);
        
        return Result.Ok();
    }

    public Result<Func<EBaseClass>?> GetConstructor(URN urn)
    {
        if(_classes.TryGetValue(urn, out var ctor))
        {
            return Result<Func<EBaseClass>?>.Ok(ctor);
        }
        
        return Result.Fail($"Class with URN '{urn}' has no constructor registered.");
    }

    public Result<EBaseClass> Instantiate(URN urn)
    {
        if(_classes.TryGetValue(urn, out var ctor))
        {
            return Result<EBaseClass>.Ok(ctor());
        }
        
        return Result<EBaseClass>.Fail($"Class with URN '{urn}' has no constructor registered.");
    }

    public Result<T> Instantiate<T>() where T : EBaseClass
    {
        if(_classes.TryGetValue(_typeToUrn[typeof(T)], out var ctor))
        {
            if(ctor() is T typed)
                return Result<T>.Ok(typed);
            return Result<T>.Fail($"Failed to cast constructor result to type '{typeof(T)}'");
        }
        
        return Result<T>.Fail($"Class with URN '{_typeToUrn[typeof(T)]}' has no constructor registered.");
    }

    public Result<List<(Type type, URN urn)>> GetAllClasses()
    {
        return Result<List<(Type type, URN urn)>>.Ok(_typeToUrn.Select(c => (c.Key, c.Value)).ToList());
    }

    public Result<IReadOnlyDictionary<StringName, EPropertyContext>> GetPropertiesOfClass(URN classUrn)
    {
        if(_classesProperties.TryGetValue(classUrn, out var properties))
        {
            return Result<IReadOnlyDictionary<StringName, EPropertyContext>>.Ok(properties);
        }
        
        return Result<IReadOnlyDictionary<StringName, EPropertyContext>>.Fail($"Class with URN '{classUrn}' has no properties registered.");   
    }

    public bool HasConstructor(URN urn)
    {
        return _classes.ContainsKey(urn);
    }
    
    public bool HasConstructor<T>() where T : EBaseClass
    {
        return _typeToUrn.ContainsKey(typeof(T));
    }
}