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
    
    public Result Register<T>(URN urn, Func<T> constructor) where T : EBaseClass
    {
        if(_classes.ContainsKey(urn))
        {
            return Result.Fail($"Class with URN '{urn}' is already registered.");
        }
        
        _classes[urn] = constructor;
        _typeToUrn[typeof(T)] = urn;
        
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

    public Result<EBaseClass> Instantiate<T>() where T : EBaseClass
    {
        if(_classes.TryGetValue(_typeToUrn[typeof(T)], out var ctor))
        {
            return Result<EBaseClass>.Ok(ctor());
        }
        
        return Result<EBaseClass>.Fail($"Class with URN '{_typeToUrn[typeof(T)]}' has no constructor registered.");
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