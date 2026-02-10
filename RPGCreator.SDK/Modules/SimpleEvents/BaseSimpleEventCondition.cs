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

public abstract class BaseSimpleEventCondition : IHasUniqueId
{
    public static UrnSingleModule Module => ISimpleEventRegistry.ConditionModule;
    
    /// <summary>
    /// The Unique ID of this class. It is used IN the engine, but it <b>NOT BE USED</b> by the users as it could be changed between different sessions.<br/>
    /// If you need to reference this class, you should use the URN instead, which is a unique and stable identifier that can be used to reference this class!
    /// </summary>
    public Ulid Unique { get; private set; }
    public abstract URN Urn { get; }

    public abstract string Name { get; }
    public abstract string Description { get; }

    /// <summary>
    /// Result the condition expect
    /// </summary>
    public abstract bool ResultExpected { get; }
    
    public void Init(Ulid id)
    {
        if (Unique != Ulid.Empty) return;
        Unique = id;
    }

    public CustomData Parameters { get; set; } = new CustomData();

    public bool EvaluateCondition(CustomData context)
    {
        return EvaluateCondition(context, Parameters);
    }
    public abstract bool EvaluateCondition(CustomData context, CustomData parameters);

    /// <summary>
    /// The condition text is what is shown in the SimpleEvent editor line when it's not in edit mode:<br/>
    /// IF [ConditionText] THEN XYZ
    /// </summary>
    public abstract string GetConditionText(CustomData parameters);
    
    public abstract Dictionary<string, string> GetConditionsTextAsPart(CustomData parameters);
    
    /// <summary>
    /// The required properties for that condition.<br/>
    /// The returned list will be used by the engine to show the correct input to the user.<br/>
    /// <br/>
    /// Note: If a conditionProperties as a key "EntityTarget" and the next action also has it, the engine will auto-fill the action properties with this condition properties.
    /// </summary>
    /// <returns>
    /// A list of property descriptor
    /// </returns>
    public abstract List<SimpleEventPropertyDescriptor> GetConditionProperties();
}