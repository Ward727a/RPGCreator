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

using System.Threading.Tasks;
using Fluxor;
using RPGCreator.SDK.Scenes;
using RPGCreator.Shared.Types;

namespace RPGCreator.Editor.Tools;

public class ToolSceneEffects(ToolManager toolManager)
{
    [EffectMethod]
    public Task OnMapLoadCompleted(SceneStateActions.MapLoadCompleted action, IDispatcher dispatcher)
    {
        if (action.FirstLayerType != URN.Empty)
        {
            toolManager.SelectToolLogic(action.FirstLayerType);
        }
        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task OnActiveLayerChanged(SceneStateActions.ActiveLayerChanged action, IDispatcher dispatcher)
    {
        if (action.NewLayerType != URN.Empty)
        {
            toolManager.SelectToolLogic(action.NewLayerType);
        }
        return Task.CompletedTask;
    }
}