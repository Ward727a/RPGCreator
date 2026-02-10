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
using System.Runtime.InteropServices;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;

namespace RPGCreator.Core;

[SerializingType("EngineGlobalPathData")]
public class EngineGlobalPathData : IGlobalPathData, ISerializable, IDeserializable
{
    private Dictionary<URN, Ulid> _pathToValue = new();
    private Dictionary<URN, HashSet<Ulid>> _tagToIds = new();
    private Dictionary<Ulid, URN> _idToTag = new();
    
    public void RegisterPath(URN pathToValue, Ulid idValue, URN? tag = null)
    {
        _pathToValue[pathToValue] = idValue;
        if (tag != null)         
            RegisterTag(tag.Value, idValue);
    }

    public void RegisterPaths(List<(URN pathToValue, Ulid idValue)> pathsToValues, URN? tag = null)
    {
        foreach (var (pathToValue, idValue) in pathsToValues)
        {
            RegisterPath(pathToValue, idValue, tag);
        }
    }

    public bool TryGetValues(URN tag, [NotNullWhen(true)] out IEnumerable<Ulid>? path)
    {
        if (_tagToIds.TryGetValue(tag, out var paths))
        {
            path = paths;
            return true;
        }

        path = null;
        return false;
    }

    public void RemovePath(URN pathToValue)
    {
        var value = _pathToValue[pathToValue];
        _pathToValue.Remove(pathToValue);
        
        if (_idToTag.Remove(value, out var tag))
        {
            if (_tagToIds.TryGetValue(tag, out var paths))
            {
                paths.Remove(value);
            }
        }
    }
    
    private static HashSet<Ulid> GetOrCreateTagToId(Dictionary<URN, HashSet<Ulid>> dict, URN key)
    {
        ref var set = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out bool exists);
        if (!exists) set = new HashSet<Ulid>();
        return set!;
    }

    public void RegisterTag(URN tag)
    {
        
        if (!_tagToIds.ContainsKey(tag))
        {
            _tagToIds[tag] = new HashSet<Ulid>();
        }
    }

    public void RegisterTag(URN tag, Ulid value)
    {
        var tagToId = GetOrCreateTagToId(_tagToIds, tag);
        if (tagToId.Add(value))
        {
            _idToTag[value] = tag;
        }
    }

    public void RegisterTag(URN tag, List<Ulid> values)
    {
        if (!_tagToIds.ContainsKey(tag))
        {
            _tagToIds[tag] = new HashSet<Ulid>();
        }
        foreach (var value in values)
        {
            if(!TagHasValue(tag, value))
                _tagToIds[tag].Add(value);
        }
    }

    public void RegisterTags(List<(URN tag, List<Ulid> values)> tagsToPaths)
    {
        foreach (var (tag, values) in tagsToPaths)
        {
            RegisterTag(tag, values);
        }
    }

    public void RemoveTag(URN tag)
    {
        _tagToIds.Remove(tag);
    }

    public bool TryGetValue(URN path, out Ulid value)
    {
        return _pathToValue.TryGetValue(path, out value);
    }

    public IEnumerable<URN> GetAllTags()
    {
        return _tagToIds.Keys;
    }

    public bool HasTag(URN tag)
    {
        return _tagToIds.ContainsKey(tag);
    }

    public bool TagHasValue(URN tag, Ulid value)
    {
        return _tagToIds.TryGetValue(tag, out var paths) && paths.Contains(value);
    }

    public bool HasPath(URN path)
    {
        return _pathToValue.ContainsKey(path);
    }

    public SerializationInfo GetObjectData()
    {
        var info = new SerializationInfo(this.GetType());
        info.AddValue(nameof(_pathToValue), _pathToValue);
        info.AddValue(nameof(_tagToIds), _tagToIds);
        info.AddValue(nameof(_idToTag), _idToTag);
        return info;
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        return _pathToValue.Values.ToList();
    }

    public void SetObjectData(DeserializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "DeserializationInfo cannot be null.");
        }

        info.TryGetValue(nameof(_pathToValue), out Dictionary<URN, Ulid> pathToValue, new());
        info.TryGetValue(nameof(_tagToIds), out Dictionary<URN, HashSet<Ulid>> tagToIds, new());
        info.TryGetValue(nameof(_idToTag), out Dictionary<Ulid, URN> idToTag, new());

        _pathToValue = pathToValue;
        _tagToIds = tagToIds;
        _idToTag = idToTag;
    }
}