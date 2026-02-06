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

using System;
using RPGCreator.SDK;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Systems;
using RPGCreator.SDK.RuntimeService;

namespace RPGCreator.Player.ECS.Systems;

public class RenderBatchSystem : ISystem
{
    public override int Priority => 5000;
    public override bool IsDrawingSystem => true;
    
    private Action _drawAction = () => { };
    
    public override void Initialize(IEcsWorld ecsWorld)
    {
        RuntimeServices.OnceServiceReady((IRenderService render) =>
        {
            _drawAction = render.DrawSortedQueue;
        });
    }

    public override void Update(TimeSpan deltaTime)
    {
        _drawAction();
    }
}