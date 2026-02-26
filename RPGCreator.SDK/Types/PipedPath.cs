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

using RPGCreator.SDK.Logging;

namespace RPGCreator.SDK.Types;

public readonly struct PipedPath : IEquatable<PipedPath>
{
    private const char SeparatorChar = '|';
    private readonly string _path;
    private readonly int _hashCode;

    private PipedPath(string path)
    {
        _path = path;
        _hashCode = path?.GetHashCode(StringComparison.Ordinal) ?? 0;
    }

    public PipedPath Extend(string segment) => new($"{_path}{SeparatorChar}{segment}");
    
    public static PipedPath Empty => new(string.Empty);
    public bool IsEmpty => string.IsNullOrEmpty(_path);
    
    public ReadOnlySpan<char> AsSpan() => _path.AsSpan();
    public override string ToString() => _path;
    
    public static PipedPath Parse(string path) => new(path);
    
    public string Root 
    {
        get 
        {
            int index = _path.IndexOf(SeparatorChar);
            return index == -1 ? _path : _path.Substring(0, index);
        }
    }
    
    public string[] Segments => _path.Split(SeparatorChar);
    public string Name => _path.Split(SeparatorChar).Last();

    /// <summary>
    /// Check if <b>this</b> path is the parent of the <b>other</b> path.
    /// </summary>
    /// <param name="other">The path to check against.</param>
    /// <returns>
    /// True if <b>this</b> path is the parent of the <b>other</b> path, false otherwise.
    /// </returns>
    public bool IsParentOf(PipedPath other)
    {
        ReadOnlySpan<char> self = _path.AsSpan();
        ReadOnlySpan<char> target = other._path.AsSpan();

        // If the target path doesn't start with our path, or if the target path is shorter or equal to our path, then we can't be the parent.
        if (!target.StartsWith(self) || target.Length <= self.Length)
            return false;

        // If the target path doesn't have a separator right after our path, then we can't be the parent.
        if (target[self.Length] != SeparatorChar)
            return false;

        // If the remaining part of the target path after our path contains a separator, then we can't be the parent, because that means there is another segment after the one that would be our child.
        ReadOnlySpan<char> remaining = target.Slice(self.Length + 1);
        return remaining.IndexOf(SeparatorChar) == -1;
    }
    
    /// <summary>
    /// Check if <b>this</b> path is a brother of the <b>other</b> path.<br/>
    /// Example:<br/>
    /// "a|b|c" and "a|b|d" are brothers, while "a|b|c" and "a|b|c|d" are not brothers, because the first one is the parent of the second one, not a brother.<br/>
    /// This allows to check if two paths have the same parent, without needing to know the parent path.
    /// </summary>
    /// <param name="other">The path to check against.</param>
    /// <returns>
    /// True if <b>this</b> path is a brother of the <b>other</b> path, false otherwise.
    /// </returns>
    public bool IsBrotherOf(PipedPath other)
    {
        ReadOnlySpan<char> self = _path.AsSpan();
        ReadOnlySpan<char> target = other._path.AsSpan();

        int lastIndexSelf = self.LastIndexOf(SeparatorChar);
        int lastIndexTarget = target.LastIndexOf(SeparatorChar);

        if (lastIndexSelf == -1 || lastIndexSelf != lastIndexTarget)
            return false;

        return self.Slice(0, lastIndexSelf).SequenceEqual(target.Slice(0, lastIndexTarget));
    }
    
    /// <summary>
    /// Check if <b>this</b> path is a child of the <b>other</b> path.
    /// </summary>
    /// <param name="other">The path to check against.</param>
    /// <returns>
    /// True if <b>this</b> path is a child of the <b>other</b> path, false otherwise.
    /// </returns>
    public bool IsChildOf(PipedPath other)
    {
        return other.IsParentOf(this);
    }

    public override int GetHashCode() => _hashCode;
    
    public bool Equals(PipedPath other) => _path == other._path;
    public override bool Equals(object? obj) => obj is PipedPath other && Equals(other);

    public static bool operator ==(PipedPath left, PipedPath right) => left.Equals(right);
    public static bool operator !=(PipedPath left, PipedPath right) => !left.Equals(right);
    
    
    
    public static PipedPath OpenWindow => new("OpenWindow");
    public static PipedPath EditorAction => new("EditorAction");
}

public static class PipedPathExtensions
{
    public static PipedPath ToPipedPath(this string path) => PipedPath.Parse(path);

    public static IEnumerable<(PipedPath path, T item)> GetSortedByParentsAndBrother<T>(this Dictionary<PipedPath, T> dict)
    {
        var sorted = new List<(PipedPath, T)>(dict.Count);
        var visited = new HashSet<PipedPath>(dict.Count);

        void Visit(PipedPath path)
        {
            if (visited.Contains(path))
                return;

            string pathStr = path.ToString();
            int lastPipeIndex = pathStr.LastIndexOf('|');

            if (lastPipeIndex != -1)
            {
                var parentPath = PipedPath.Parse(pathStr.Substring(0, lastPipeIndex));
                Visit(parentPath);
            }

            if (dict.TryGetValue(path, out var value))
            {
                visited.Add(path);
                sorted.Add((path, value));
            }
            else
            {
                Logger.Warning("[PipedPathExtensions] Path '{Path}' not found in dictionary. Skipping.", path);
                visited.Add(path);
            }
        }

        foreach (var key in dict.Keys)
        {
            Visit(key);
        }

        return sorted;
    }
}