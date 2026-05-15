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

namespace RPGCreator.SDK.Types;

public readonly struct FilePath(string path) : IEquatable<FilePath>
{
    public readonly string PureText = path;
    public readonly string FullPath = FormatPath(path);

    private static string FormatPath(string path)
    {
        if(string.IsNullOrEmpty(path) || !path.Contains('#')) return path;
        if(GlobalStates.ProjectState.CurrentProject == null) return path;

        return path.Replace("#project#", GlobalStates.ProjectState.CurrentProject.MetaData.Directory)
            .Replace("#appdata#", RpgEnv.Path.ApplicationDataFolder)
            .Replace("#exe#", RpgEnv.Path.ExecutableFolder)
            .Replace("#temp#", RpgEnv.Path.TempFolder);
    }
    
    public static implicit operator string(FilePath filepath) => filepath.FullPath;
    public static implicit operator FilePath(string filepath) => new(filepath);

    public bool Equals(FilePath other)
    {
        return PureText == other.PureText;
    }

    public override bool Equals(object? obj)
    {
        return obj is FilePath other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(PureText, FullPath);
    }
}