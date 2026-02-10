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
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Types;

namespace RPGCreator.Core;

public class EngineSignalRegistry : ISignalRegistry
{
    
    private readonly Dictionary<URN, int> _signalToIndex = new();
    
    public void RegisterSignal(URN signal)
    {
        if (_signalToIndex.ContainsKey(signal))
        {
            throw new InvalidOperationException($"Signal '{signal}' is already registered.");
        }
        
        if (_signalToIndex.Count >= 256)
        {
            throw new InvalidOperationException("Cannot register more than 256 signals.");
        }
        
        var index = _signalToIndex.Count;
        _signalToIndex[signal] = index;
    }

    public int GetSignalMask(URN signal)
    {
        if (_signalToIndex.TryGetValue(signal, out var mask))
        {
            return mask;
        }
        throw new KeyNotFoundException($"Signal '{signal}' is not registered.");
    }

    public bool TryGetSignalMask(URN signal, out int mask)
    {
        return _signalToIndex.TryGetValue(signal, out mask);
    }
}