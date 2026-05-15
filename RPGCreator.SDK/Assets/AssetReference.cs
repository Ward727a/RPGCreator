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

using RPGCreator.SDK.Services.EngineService;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.Assets;

public class AssetReference<T> where T : class, IEngineObject
{
    private IAssetsManager AssetsManager => EngineServices.AssetsManager;
    public Type Type { get; } = typeof(T);
    public Ulid Id { get; private set; } = Ulid.Empty;
    public bool HasReference => Id != Ulid.Empty;
    
    private T? _cachedEngineObject;
    public bool HasCachedValue => _cachedEngineObject != null;

    public AssetReference()
    {
    }
    
    public AssetReference(Ulid id)
    {
        Id = id;
    }
    
    public AssetReference<T> WithId(Ulid id)
    {
        if (id == Id) return this;
        
        Id = id;
        
        _cachedEngineObject = null;
        return this;
    }

    public Result<T> Get()
    {
        if(!HasReference)
            return Result.Fail($"Asset reference for type {Type.Name} has no value");
        
        if(HasCachedValue)
            return Result<T>.Ok(_cachedEngineObject!);
        
        if (EngineServices.AssetsManager.Has(Id))
        {
            return EngineServices.AssetsManager.Load<T>(Id).OnSuccess(o =>
            {
                _cachedEngineObject = o;
            });
        }

        return Result.Fail($"Asset reference for type {Type.Name} with UID {Id} not found.");
    }
}