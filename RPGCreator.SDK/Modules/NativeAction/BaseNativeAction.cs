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
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Registry;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.Modules.NativeAction;

public abstract class BaseNativeAction : IEngineObject
{
    protected UrnSingleModule NativeActionModule => "native_actions".ToUrnSingleModule();
    protected UrnSingleModule SignalItemModule => ISignalRegistry.SignalModuleUrn;

    private Bitmask256 _triggerMask;
    
    public Bitmask256 TriggerMask => _triggerMask;

    public abstract URN ExpectedSignal { get; }
    public Ulid Unique { get; set; }
    public abstract URN ClassUrn { get; set; }

    public bool BuildAction()
    {
        _triggerMask = new Bitmask256();
        
        if(RegistryServices.Signal.TryGetSignalMask(ExpectedSignal, out var signalIndex))
        {
            _triggerMask.Set(signalIndex, true);
            return true;
        }

        Logger.Error("Couldn't find signal {signalName} in the SignalRegistry.", ExpectedSignal);
        return false;
    }
    
    public void Init(Ulid id)
    {
        if (Unique != Ulid.Empty) return;
        Unique = id;
    }

    public abstract void Execute(int entityId, IEcsWorld world);
}