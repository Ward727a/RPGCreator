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
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.ECS.Systems;

public class CameraSystem : ISystem
{
    public override int Priority => 50;
    public override bool IsDrawingSystem => false;
    
    private ICameraService _cameraService;
    private ComponentManager _componentManager;
    private EcsEventBus _eventBus;
    
    public override void Initialize(IEcsWorld ecsWorld)
    {
        _cameraService = RuntimeServices.CameraService;
        _componentManager = ecsWorld.ComponentManager;
        _eventBus = ecsWorld.EventBus;
        new BaseSubscriber(new URN("rpgc", "events", "on_camera_follow"), 0, OnCameraFollowEvent).Subscribe();
    }

    private void OnCameraFollowEvent(IEcsEvent obj)
    {
        var cameraEntityId = _cameraService.CameraEntityId! ?? -1;
        if(cameraEntityId == -1) return;
        
        if(!_componentManager.HasComponent<CameraComponent, TransformComponent>(cameraEntityId))
            return;

        ref var cameraData = ref _componentManager.GetComponent<CameraComponent>(cameraEntityId);
        cameraData.FollowedEntity = obj.Data.GetAsOrDefault("target", -1);
    }

    public override void Update(TimeSpan deltaTime)
    {
        _cameraService.Update(deltaTime);
        var cameraEntityId = _cameraService.CameraEntityId! ?? -1;
        
        if(cameraEntityId == -1) return;
        
        if(_componentManager.HasComponent<CameraComponent, TransformComponent>(cameraEntityId))
            return;
        
        ref var cameraData = ref _componentManager.GetComponent<CameraComponent>(cameraEntityId);
        ref var transformData = ref _componentManager.GetComponent<TransformComponent>(cameraEntityId);

        if (!cameraData.IsFollowingEntity || cameraData.FollowedEntity == -1) return;
        
        var cameraTargetId = cameraData.FollowedEntity;

        if (_componentManager.HasComponent<TransformComponent>(cameraTargetId))
        {
            ref var transformTargetComponent = ref _componentManager.GetComponent<TransformComponent>(cameraTargetId);
            var targetPosition = transformTargetComponent.Position;
            var targetWithOffset = targetPosition + cameraData.Offset;
            transformData.Position = targetWithOffset;
        }
    }

    public readonly record struct CameraFollowEvent(int Target);
}