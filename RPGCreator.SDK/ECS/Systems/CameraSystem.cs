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

using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.RuntimeService;

namespace RPGCreator.SDK.ECS.Systems;

public class CameraSystem : ISystem
{
    public override int Priority => 50;
    public override bool IsDrawingSystem => false;
    
    private ICameraService _cameraService;
    
    public override void Initialize(IEcsWorld ecsWorld)
    {
        _cameraService = RuntimeServices.CameraService;
    }

    public override void Update(TimeSpan deltaTime)
    {
        _cameraService.Update(deltaTime);
        var cameraEntity = _cameraService.CameraEntity;
        if (cameraEntity == null)
            return;
        
        if(!cameraEntity.HasComponent<CameraComponent>() || !cameraEntity.HasComponent<TransformComponent>())
            return;
        
        ref var cameraData = ref cameraEntity.GetComponent<CameraComponent>();
        ref var transformData = ref cameraEntity.GetComponent<TransformComponent>();

        if (cameraData is not { IsFollowingEntity: true, FollowedEntity: not null }) return;
        
        var cameraTarget = cameraData.FollowedEntity;
        
        if (cameraTarget == null || !cameraTarget.HasComponent<TransformComponent>()) return;
        
        var targetPosition = cameraTarget.GetComponent<TransformComponent>().Position;
        var targetWithOffset = targetPosition + cameraData.Offset;
        transformData.Position = targetWithOffset;
    }
}