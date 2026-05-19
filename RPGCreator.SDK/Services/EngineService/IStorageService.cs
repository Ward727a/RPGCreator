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

using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Services.EngineService;

public interface IStorageService
{
    public string StorageType { get; }
    protected Result<T> InternalLoad<T>(object path);
    protected Result InternalSave<T>(object path, T data);
    protected Result InternalDelete(object path);
}

public interface IFileStorageService : IStorageService
{
    public Result<T> Load<T>(string path) => InternalLoad<T>(path);
    public Result Save<T>(string path, T data) => InternalSave(path, data);
    public Result Delete(string path) => InternalDelete(path);
    public Result<string> FormatFilename(string filename);
    public Result<string> MakeValidPath(params string[] parts);
}

public interface IDbStorageService<in TKey> : IStorageService
{
    public bool HasKey(string key);
    public Result<T> Get<T>(string key) => InternalLoad<T>(key);
    public Result Set<T>(string key, T value) => InternalSave(key, value);
}