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
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Types;

namespace RPGCreator.Core;

public class EngineGlobalPathData : IGlobalPathData
{
    private readonly Dictionary<URN, Ulid> _pathToValue = new();
    private readonly Dictionary<URN, List<URN>> _tagToPaths = new();
    
    
    public void RegisterPath(URN pathToValue, Ulid idValue, URN? tag = null)
    {
        _pathToValue[pathToValue] = idValue;
        if (tag != null)         
            RegisterTag(tag.Value, pathToValue);
    }

    public void RegisterPaths(List<(URN pathToValue, Ulid idValue)> pathsToValues, URN? tag = null)
    {
        foreach (var (pathToValue, idValue) in pathsToValues)
        {
            RegisterPath(pathToValue, idValue, tag);
        }
    }

    public bool TryGetPaths(URN tag, [NotNullWhen(true)] out IEnumerable<URN>? path)
    {
        if (_tagToPaths.TryGetValue(tag, out var paths))
        {
            path = paths;
            return true;
        }

        path = null;
        return false;
    }

    public void RemovePath(URN pathToValue)
    {
        _pathToValue.Remove(pathToValue);
         foreach (var paths in _tagToPaths.Values)
         {
             paths.Remove(pathToValue);
         }
    }

    public void RegisterTag(URN tag)
    {
        if (!_tagToPaths.ContainsKey(tag))
        {
            _tagToPaths[tag] = new List<URN>();
        }
    }

    public void RegisterTag(URN tag, URN pathToValue)
    {
        if (!_tagToPaths.ContainsKey(tag))
        {
            _tagToPaths[tag] = new List<URN>();
        }
        if(!TagHasPath(tag, pathToValue))
            _tagToPaths[tag].Add(pathToValue);
    }

    public void RegisterTag(URN tag, List<URN> pathsToValues)
    {
        if (!_tagToPaths.ContainsKey(tag))
        {
            _tagToPaths[tag] = new List<URN>();
        }
        foreach (var path in pathsToValues)
        {
            if(!TagHasPath(tag, path))
                _tagToPaths[tag].Add(path);
        }
    }

    public void RegisterTags(List<(URN tag, List<URN> pathsToValues)> tagsToPaths)
    {
        foreach (var (tag, pathsToValues) in tagsToPaths)
        {
            RegisterTag(tag, pathsToValues);
        }
    }

    public void RemoveTag(URN tag)
    {
        _tagToPaths.Remove(tag);
    }

    public bool TryGetValue(URN path, out Ulid value)
    {
        return _pathToValue.TryGetValue(path, out value);
    }

    public IEnumerable<URN> GetAllTags()
    {
        return _tagToPaths.Keys;
    }

    public bool HasTag(URN tag)
    {
        return _tagToPaths.ContainsKey(tag);
    }

    public bool TagHasPath(URN tag, URN pathToValue)
    {
        return _tagToPaths.TryGetValue(tag, out var paths) && paths.Contains(pathToValue);
    }

    public bool HasPath(URN path)
    {
        return _pathToValue.ContainsKey(path);
    }
}