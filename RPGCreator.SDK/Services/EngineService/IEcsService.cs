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

using RPGCreator.SDK.ECS;

namespace RPGCreator.SDK.Services.EngineService;

public interface IEcsService : IService
{
    public IEcsWorld CreateWorld();
    /// <summary>
    /// State registry.
    /// Allows registering entity states and keeps track of their storage information.<br/>
    /// This allows each component to know where to read/write the state value in the storage arrays.<br/>
    /// State value is shared between all components on the same entity.<br/>
    /// <br/>
    /// If a state is registered multiple times, the same StateStorageInfo is returned, ensuring consistency.<br/>
    /// Here is a schema:<br/>
    /// Loop (All Components present in module) => Calling 'OnSetup' on each component => Each component register its states (with <see cref="EntityStateRegistry.Register"/>) => The registry assigns storage index for each state.<br/>
    /// <br/>
    /// This allows the engine to KNOW what size the storage arrays should be for each entity (TotalFloat, TotalInt, etc...).<br/>
    /// </summary>
    public EntityStateRegistry StateRegistry { get; }
}