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

using CommunityToolkit.Mvvm.ComponentModel;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.GlobalState;

namespace RPGCreator.SDK.RuntimeService;

public class DefaultGameSession : BaseState, IGameSession
{

    public IEcsWorld? ActiveEcsWorld
    {
        get;
        set
        {
            if(value == field) return;
            SetProperty(ref field, value);
        }
    }

    public bool IsPaused
    {
        get;
        set
        {
            if (value == field) return;
            SetProperty(ref field, value);
        }
    } = true;

    public int CurrentPlayerId
    {
        get;
        set
        {
            if (value == field) return;
            SetProperty(ref field, value);
        }
    } = -1;
    
    public override void Reset()
    {
        ActiveEcsWorld = null;
        IsPaused = true;
        CurrentPlayerId = -1;
    }
}