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

using RPGCreator.SDK.Assets.Definitions.SimpleEvents;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Modules.Definition;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.Modules.SimpleEvents;

public abstract class BaseSimpleEventAction : IHasUniqueId
{
    public static UrnSingleModule Module => ISimpleEventRegistry.ActionModule;
    
    /// <summary>
    /// The Unique ID of this class. It is used IN the engine, but it <b>NOT BE USED</b> by the users as it could be changed between different sessions.<br/>
    /// If you need to reference this class, you should use the URN instead, which is a unique and stable identifier that can be used to reference this class!
    /// </summary>
    public Ulid Unique { get; private set; }
    public abstract URN Urn { get; }
    
    public abstract string Name { get; }
    public abstract string Description { get; }
    
    public CustomData Parameters { get; set; } = new();
    
    public void Init(Ulid id)
    {
        if (Unique != Ulid.Empty) return;
        Unique = id;
    }

    public void Execute(CustomData context)
    {
        Execute(context, Parameters);
    }
    public abstract void Execute(CustomData context, CustomData data);
    public abstract List<SimpleEventPropertyDescriptor> GetActionProperties();
}