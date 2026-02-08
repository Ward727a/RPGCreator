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

using _BaseModule.AssetDefinitions.BaseStats;
using RPGCreator.SDK.Assets;

namespace _BaseModule.Registry;

public class StatModifierRegistry : RegistryBase<StatModifierDefinition>
{
    public override string ModuleName => "stat_modifiers";

    private int _currentId = 0;
    private readonly Dictionary<Ulid, int> _statModifierMapping = new();
    private readonly Dictionary<int, Ulid> _idToUniqueMapping = new();
    
    public override void Register(StatModifierDefinition asset, bool overwrite = false)
    {
        base.Register(asset, overwrite);
        
        if (_statModifierMapping.ContainsKey(asset.Unique) && !overwrite)
            return;
        
        asset.RegistryId = AddIdMapping(asset.Unique);
    }

    public override void Unregister(StatModifierDefinition asset)
    {
        base.Unregister(asset);
        RemoveIdMapping(asset.Unique);
        asset.RegistryId = -1;
    }
    
    public int GetStatModifierId(Ulid unique)
    {
        if (_statModifierMapping.TryGetValue(unique, out var id))
            return id;
        throw new Exception($"Stat Modifier with unique {unique} not found in registry.");
    }
    
    public Ulid GetStatModifierUnique(int id)
    {
        if (_idToUniqueMapping.TryGetValue(id, out var unique))
            return unique;
        throw new Exception($"Stat Modifier with id {id} not found in registry.");
    }
    
    private int AddIdMapping(Ulid unique)
    {
        var id = _currentId++;
        _statModifierMapping[unique] = id;
        _idToUniqueMapping[id] = unique;
        return id;
    }

    private void RemoveIdMapping(Ulid unique)
    {
        if (_statModifierMapping.Remove(unique, out var id))
        {
            _idToUniqueMapping.Remove(id);
        }
    }
}