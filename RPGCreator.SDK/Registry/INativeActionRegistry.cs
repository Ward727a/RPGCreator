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

using System.Diagnostics.CodeAnalysis;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.Modules.NativeAction;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Registry;

public interface INativeActionRegistry : IService
{
    public static UrnSingleModule NativeActionModule => "simple_events_actions".ToUrnSingleModule();
    
    public bool RegisterNativeAction(BaseNativeAction action, bool overwriteIfExists = false);
    public bool UnregisterNativeAction(URN urn);
    
    public bool TryGetNativeAction(URN urn, [NotNullWhen(true)] out BaseNativeAction? action);
    
    public IEnumerable<BaseNativeAction> GetNativeActions();
    
    public ReadOnlySpan<URN> SearchNativeActionsBySignal(Bitmask256 signalMask);
    
    public int NativeActionCount { get; }
    
    public void ClearRegistry();
}