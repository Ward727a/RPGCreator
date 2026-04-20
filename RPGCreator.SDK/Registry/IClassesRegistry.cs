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
using RPGCreator.SDK.Types.EngineClass;

namespace RPGCreator.SDK.Registry;

public interface IClassesRegistry : IService
{
    Result Register<T>(URN urn, Func<T> constructor) where T : EBaseClass;
    Result Unregister(URN urn);
    
    Result<Func<EBaseClass>?> GetConstructor(URN urn);
    Result<EBaseClass> Instantiate(URN urn);
    Result<EBaseClass> Instantiate<T>() where T : EBaseClass;
    
    bool HasConstructor(URN urn);
    bool HasConstructor<T>() where T : EBaseClass;
}