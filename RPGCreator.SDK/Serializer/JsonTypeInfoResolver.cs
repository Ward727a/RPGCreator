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

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using RPGCreator.SDK.Types.EngineClass;

namespace RPGCreator.SDK.Serializer;

public class JsonTypeInfoResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);
        
        if (typeof(EBaseClass).IsAssignableFrom(type))
        {
            var polymorphismOptions = new JsonPolymorphismOptions()
            {
                TypeDiscriminatorPropertyName = "$type",
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType
            };

            var result = RegistryServices.Classes.GetAllClasses();
            if(result.IsFailure)
                throw new Exception(result.Error);
            foreach (var registered in result.Value)
            {
                polymorphismOptions.DerivedTypes.Add(
                    new JsonDerivedType(registered.type, registered.urn.ToString())
                );
            }
            
            jsonTypeInfo.PolymorphismOptions = polymorphismOptions;
        }
        
        return jsonTypeInfo;
    }
}