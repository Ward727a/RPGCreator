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

public readonly struct PipedPath
{
    private const string Separator = "|";
    private readonly string _path;
    
    private PipedPath(string path) => _path = path;
    
    public PipedPath Extend(string segment) => new($"{_path}{Separator}{segment}");

    public string Make() => _path;
    public override string ToString() => _path;
    
    public static PipedPath OpenWindow => new("OpenWindow");
    public static PipedPath EditorAction => new("EditorAction");
    
    public static PipedPath Parse(string path) => new(path);
    
    public string Root => _path.Split(Separator)[0];
    public string[] Segments => _path.Split(Separator);
}

public static class PipedPathExtensions
{
    public static PipedPath ToPipedPath(this string path) => PipedPath.Parse(path);
}