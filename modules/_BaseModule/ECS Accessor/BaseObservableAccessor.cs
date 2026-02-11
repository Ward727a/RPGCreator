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

using _BaseModule.Features.Entity;
using CommunityToolkit.Mvvm.ComponentModel;
using RPGCreator.SDK.ECS;

namespace _BaseModule.ECS_Accessor;

public abstract class BaseObservableAccessor : ObservableObject, IAccessor
{
    public bool IsBinded { get; protected set; } = false;
    public IEcsWorld BindedToWorld { get; protected set; }
    public abstract int EntityId { get; set; }
    public abstract void Update(IEcsWorld world);

    public abstract void BindToEntity(IEcsWorld world);
}