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

using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Registry;
using RPGCreator.SDK.Types;

namespace RPGCreator.Core.Registry;

public sealed class UrnRegistry : IUrnRegistry
{
    private HashSet<URN> registeredUrns = new HashSet<URN>();
    
    public bool RegisterUrn(ref URN urn, UrnCollisionStrategy urnCollisionStrategy = UrnCollisionStrategy.RenameWithUlidSuffix)
    {
        
        if(urn == URN.Empty)
        {
            urn = "rpgc".ToUrnNamespace().ToUrnModule("unknown_error").ToUrn($"unnamed_{Ulid.NewUlid()}");
            return false;
        }
        
        Logger.Debug("======================== BEGIN MESSAGE ========================");
        if (registeredUrns.Contains(urn))
        {
            switch (urnCollisionStrategy)
            {
                case UrnCollisionStrategy.RenameWithUlidSuffix:
                    var old = urn;
                    urn = new URN(urn.Namespace, urn.Module, $"{urn.Name}_{Ulid.NewUlid()}".AsMemory());
                    Logger.Debug("URN collision detected for {0}. Renamed to {1}", args: [old, urn]);
                    var stackTrace = new System.Diagnostics.StackTrace(2, true);
                    Logger.Debug("Collision stack trace:\n{0}", args: [stackTrace]);
                    break;
                case UrnCollisionStrategy.Ignore:
                    return true;
                case UrnCollisionStrategy.Fail:
                    return false;
            }
        }
        
        Logger.Debug($"Registering URN: {urn} (Total registered URNs: {registeredUrns.Count + 1})");
        Logger.Debug("======================== END MESSAGE ========================");
        
        registeredUrns.Add(urn);
        return true;
    }

    public bool UnregisterUrn(URN urn)
    {
        return registeredUrns.Remove(urn);
    }

    public bool IsUrnRegistered(URN urn)
    {
        return registeredUrns.Contains(urn);
    }
}