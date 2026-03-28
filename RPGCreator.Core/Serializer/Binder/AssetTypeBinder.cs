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
    {
        var assetType = RegistryServices.AssetsType.GetType(typeName);
    
        if (assetType != null && assetType != typeof(GenericBaseAssetStub))
        {
            return assetType;
        }

        var systemType = Type.GetType(typeName);
        if (systemType != null)
        {
            RegistryServices.AssetsType.RegisterMapping(typeName, systemType);
            return systemType;
        }
        
        try 
        {
            // Si assemblyName est fourni par Newtonsoft, on tente de charger l'assembly
            if (!string.IsNullOrEmpty(assemblyName))
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.FullName == assemblyName || a.GetName().Name == assemblyName);
            
                if (assembly != null)
                {
                    var resolvedType = assembly.GetType(typeName);
                    if (resolvedType != null) return resolvedType;
                }
            }
        }
        catch { /* Ignore */ }

        // 4. Fallback de dernier recours : parcourir tous les assemblies chargés
        // Utile pour les ObservableCollection et types système
        var fallbackType = AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(typeName))
            .FirstOrDefault(t => t != null);

        if (fallbackType != null) return fallbackType;

        EditorUiServices.NotificationService.Error("Error while loading asset!",
            $"Unknown asset type: {typeName}, using GenericAssetStub as fallback.");
        return typeof(GenericBaseAssetStub);
    }

    public void BindToName(Type serializedType, [UnscopedRef] out string? assemblyName, [UnscopedRef] out string? typeName)
    {
        assemblyName = null;
        typeName = RegistryServices.AssetsType.GetKey(serializedType) ?? serializedType.FullName;
    }
}