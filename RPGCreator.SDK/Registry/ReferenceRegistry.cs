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

public class ReferenceRegistry : IReferencesRegistry
{
    private ConcurrentDictionary<EBaseClass, List<Ulid>> _references = new();
    private ConcurrentDictionary<Ulid, EBaseClass> _idToClass = new();
    
    public Result RegisterReference(EBaseClass @class)
    {
        var id = @class.Id;
        if (!_idToClass.TryAdd(id, @class))
        {
            return Result.Fail("Reference already exists.");
        }

        if (!_references.TryGetValue(@class, out var value))
        {
            value = [];
            _references[@class] = value;
        }

        value.Add(id);
        return Result.Success();
    }

    public Result UnregisterReference(Ulid id)
    {
        if (!_idToClass.TryRemove(id, out var @class) || !_references[@class].Remove(id))
        {
            return Result.Fail("Reference does not exist.");
        }

        return Result.Success();
    }

    public Result<EBaseClass?> GetReference(Ulid id)
    {
        if (!_idToClass.TryGetValue(id, out var @class))
        {
            return Result<EBaseClass?>.Fail("Reference does not exist.");
        }

        return Result<EBaseClass?>.Success(@class);
    }

    public bool HasReference(Ulid id)
    {
        return _idToClass.ContainsKey(id);
    }

    public bool HasReference(EBaseClass @class)
    {
        return _references.ContainsKey(@class);
    }
}