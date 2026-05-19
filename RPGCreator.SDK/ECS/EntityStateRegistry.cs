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

namespace RPGCreator.SDK.ECS;

public enum StateStorageType
{
    Float,
    Int,
    Bool,
    String,
    Vector2,
    Byte
}

public record struct StateStorageInfo(int Index, StateStorageType StorageType);

/// <summary>
/// A class that allows to register entity states and keeps track of their storage information.<br/>
/// This allows each component to know where to read/write the state value in the storage arrays.<br/>
/// State value are shared between all components on the same entity.<br/>
/// <br/>
/// If a state is registered multiple times, the same StateStorageInfo is returned, ensuring consistency.<br/>
/// Here is a schema:<br/>
/// Loop(All Components present in module) => Calling 'OnSetup' on each component => Each component register its states (with <see cref="Register"/>) => The registry assign storage index for each state.<br/>
/// <br/>
/// This allows the engine to KNOW what size the storage arrays should be for each entity (TotalFloat, TotalInt, etc...).<br/>
/// <remarks>
/// It's not a perfect system (Even if a feature is not on an entity, if it defines a float, the size will be at least 1), but it's working.
/// </remarks>
/// </summary>
public class EntityStateRegistry
{
    public static UrnSingleModule ModuleUrn => "entity_states".ToUrnSingleModule();
    
    private readonly Dictionary<URN, StateStorageInfo> _registry = new();

    private readonly Dictionary<URN, int> _actionsUrn = new();
    
    public int TotalFloat { get; private set; }
    public int TotalInt { get; private set; }
    public int TotalBool { get; private set; }
    public int TotalString { get; private set; }
    public int TotalVector2 { get; private set; }
    public int TotalByte { get; private set; }

    /// <summary>
    /// Register a state with the given URN and storage type.<br/>
    /// If the state is already registered, it returns the existing StateStorageInfo.<br/>
    /// If the state is registered with a different storage type, an exception is thrown.
    /// </summary>
    /// <param name="stateUrn">The URN of the state to register.</param>
    /// <param name="storageType">The storage type of the state.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Raised when trying to register a state with a different storage type than previously registered.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Raised when the storage type is not recognized.</exception>
    public StateStorageInfo Register(URN stateUrn, StateStorageType storageType)
    {
        if(_registry.TryGetValue(stateUrn, out var existingInfo))
        {
            if(existingInfo.StorageType != storageType)
            {
                throw new InvalidOperationException(
                    $"State '{stateUrn}' is already registered with a different storage type. " +
                    $"Existing: {existingInfo.StorageType}, New: {storageType}");
            }
            return existingInfo;
        }
        
        int index = storageType switch
        {
            StateStorageType.Float => TotalFloat++,
            StateStorageType.Int => TotalInt++,
            StateStorageType.Bool => TotalBool++,
            StateStorageType.String => TotalString++,
            StateStorageType.Vector2 => TotalVector2++,
            StateStorageType.Byte => TotalByte++,
            _ => throw new ArgumentOutOfRangeException(nameof(storageType), storageType, null)
        };
        
        var info = new StateStorageInfo(index, storageType);
        _registry[stateUrn] = info;
        return info;
    }

    public bool Unregister(URN stateUrn)
    {
        if (!_registry.Remove(stateUrn, out var info))
        {
            return false;
        }

        switch (info.StorageType)
        {
            case StateStorageType.Float:
                TotalFloat--;
                break;
            case StateStorageType.Int:
                TotalInt--;
                break;
            case StateStorageType.Bool:
                TotalBool--;
                break;
            case StateStorageType.String:
                TotalString--;
                break;
            case StateStorageType.Vector2:
                TotalVector2--;
                break;
            case StateStorageType.Byte:
                TotalByte--;
                break;
            default:
                return false;
        }

        return true;
    }

    /// <summary>
    /// Register an action with the given URN.<br/>
    /// This can be used, for example, for defining if an entity is walking or not, via a global URN path logic.<br/>
    /// So, if you need to know if an entity is walking, you can check if the entity has the Action 'rpgc://action/walking' inside his state.
    /// </summary>
    /// <param name="actionUrn">The URN of the action to register.</param>
    /// <returns>
    /// Return a new int, or the existing int if the action was already registered.
    /// </returns>
    public int RegisterAction(URN actionUrn)
    {
        if (_actionsUrn.ContainsKey(actionUrn))
            return _actionsUrn[actionUrn];
        
        int index = _actionsUrn.Count+1;
        _actionsUrn[actionUrn] = index;
        return index;
    }

    public bool UnregisterAction(URN actionUrn)
    {
        return _actionsUrn.Remove(actionUrn);
    }
    
    public int GetActionId(URN actionUrn)
    {
        if (_actionsUrn.TryGetValue(actionUrn, out var actionId))
            return actionId;

        return -1;
    }
}