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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.Models;
using RPGCreator.SDK;

namespace RPGCreator.UI.Common.IconsProvider;

public class GameIconProvider : IIconProvider
{
    
    private static readonly Dictionary<string, IconModel> _icons = new();
    
    public IconModel GetIcon(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException("Icon value cannot be null or empty", nameof(value));
        }
        
        if(_icons.TryGetValue(value, out var icon))
            return icon;
        
        icon = GetIconFromFile(value);
        _icons.Add(value, icon);
        return icon;
    }

    private static IconModel GetIconFromFile(string value)
    {
        using Stream? stream = GetIconResourceStream(value);
        if (stream == null)
            throw new KeyNotFoundException($"GameIconNet icon not found: {value}");
        using (TextReader textReader = new StreamReader(stream))
        {
            var svg = textReader.ReadToEnd();
            var viewBoxMath = _viewBoxRegex.Match(svg);
            string viewBox = "";
            if (viewBoxMath.Groups.Count == 0)
            {
                // Create a default viewBox
                viewBox = "0 0 24 24";
            }
            else
            {
                viewBox = viewBoxMath.Groups[1].Value;
            }
            var pathMath = _pathRegex.Match(svg);
            var path = pathMath.Groups[1].Value;
            return new IconModel(
                ViewBoxModel.Parse(viewBox), new PathModel(path));
        }
    }

    private static Stream? GetIconResourceStream(string value)
    {
        return TryGetIconResourceStream(value, out var stream) ? stream : throw new KeyNotFoundException($"GameIconNet icon not found: {value}");
    }

    private static bool TryGetIconResourceStream(string value, out Stream? stream)
    {
        stream = default;

        if (value.Length <= _Prefix.Length + 1)
        {
            return false;
        }
        
        var withoutPrefix = value.Substring(_Prefix.Length + 1);
        var resourceName = $"{withoutPrefix}.svg";
        if (Directory.Exists(IconDir))
        {
            var iconPath = Path.Combine(IconDir, resourceName);
            if (File.Exists(iconPath))
            {
                stream = File.OpenRead(iconPath);
                return true;
            }
        }

        return false;
    }
    
    private static readonly Regex _viewBoxRegex = new("viewBox=\"([0-9 -]+)\"");
    private static readonly Regex _pathRegex = new("<path fill=\"#000\" d=\"(.+)\"");
    private static string IconDir => Path.Combine(RpgEnv.ExeAssetsFolder, "Icons", "GameIconsNet");
    private const string _Prefix = "gameIcon";
    public string Prefix => _Prefix;
}