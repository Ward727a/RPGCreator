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
using Newtonsoft.Json.Serialization;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets;

namespace RPGCreator.Core.Serializer.Binder;

public class AssetTypeBinder : ISerializationBinder
{
    public Type BindToType(string? assemblyName, string typeName)
    {var assetType = RegistryServices.AssetTypeRegistry.GetType(typeName);
    
        if (assetType != null && assetType != typeof(GenericBaseAssetStub))
        {
            return assetType;
        }

        var systemType = Type.GetType(typeName);
        if (systemType != null)
        {
            RegistryServices.AssetTypeRegistry.RegisterMapping(typeName, systemType);
            return systemType;
        }

        return typeof(GenericBaseAssetStub);
    }

    public void BindToName(Type serializedType, [UnscopedRef] out string? assemblyName, [UnscopedRef] out string? typeName)
    {
        assemblyName = null;
        typeName = RegistryServices.AssetTypeRegistry.GetKey(serializedType) ?? serializedType.FullName;
    }
}