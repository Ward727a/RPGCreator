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

using RPGCreator.SDK;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Services.EngineService;
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib.Services.StorageService;

public class JsonFileStorageService : IFileStorageService
{
    private readonly ISerializerService _serializer;
    
    public JsonFileStorageService(ISerializerService serializer)
    {
        _serializer = serializer; 
    }
    
    public string StorageType => "JSON File";
    public Result<T> InternalLoad<T>(object path)
    {
        if (path is not string pathStr)
            return Result.Fail("FileStorageService - Loading await a string path");
        
        _serializer.DeserializeFrom<T>(pathStr, out var obj);
        
        if(obj != null)
            return obj;
        
        return Result.Fail("FileStorageService - Loading failed");
    }

    public Result InternalSave<T>(object path, T data)
    {
        if (path is not string pathStr)
            return Result.Fail("FileStorageService - Saving await a string path");
        
        _serializer.SerializeTo(data, pathStr);
        return Result.Success();
    }

    public Result InternalDelete(object path)
    {
        if(path is not string pathStr)
            return Result.Fail("FileStorageService - Deleting await a string path");
        if(!File.Exists(pathStr))
            return Result.Fail("FileStorageService - File does not exist");
        
        File.Delete(pathStr);
        return Result.Success();
    }

    public Result<string> FormatFilename(string filename)
    {
        return $"{filename}.json";
    }

    public Result<string> MakeValidPath(params string[] parts)
    {
        return string.Join(Path.DirectorySeparatorChar.ToString(), parts);
    }
}