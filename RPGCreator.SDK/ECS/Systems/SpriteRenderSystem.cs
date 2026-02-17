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

using System.Drawing;
using System.Numerics;
using RPGCreator.SDK.Assets.Definitions.Animations;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.RuntimeService;
using RPGCreator.SDK.Types.Collections;

namespace RPGCreator.SDK.ECS.Systems;

public class SpriteRenderSystem : ISystem
{
    
    public override int Priority => 2000;
    public override bool IsDrawingSystem => true;
    
    ComponentManager _componentManager = null!;
    
    private Action<TimeSpan> _updateAction = (_) => { };
    
    private readonly Dictionary<Ulid, SpritesheetDef> _sheetCache = new();
    
    public override void Initialize(IEcsWorld ecsWorld)
    {
        _componentManager = ecsWorld.ComponentManager;
        _updateAction = CheckingServiceReady;
        
        RuntimeServices.MapService.OnMapLoaded += (_) =>
        {
            _sheetCache.Clear();
        };
    }

    private void CheckingServiceReady(TimeSpan deltaTime)
    {
        if(!RuntimeServices.IsServiceReady<IRenderService>())
            return;
        _updateAction = ActualUpdate;
    }
    public override void Update(TimeSpan deltaTime)
    {
        _updateAction(deltaTime);
    }
    
    private void ActualUpdate(TimeSpan deltaTime)
    {
        var renderer = RuntimeServices.RenderService;
        
        var transformSet = _componentManager.GetCompSet<TransformComponent>();
        var spriteSet = _componentManager.GetCompSet<SpriteComponent>();
        
        foreach (var entityId in _componentManager.Query<SpriteComponent, TransformComponent>())
        {
            ref var transformComponent = ref transformSet.Get(entityId);
            ref var spriteComponent = ref spriteSet.Get(entityId);

            if (!_sheetCache.TryGetValue(spriteComponent.SpritesheetId, out var spritesheet))
            {
                if (EngineServices.AssetsManager.TryResolveAsset(spriteComponent.SpritesheetId, out spritesheet))
                {
                    _sheetCache[spriteComponent.SpritesheetId] = spritesheet;
                }
                else continue;
            }
            
            var imagePath = spritesheet.ImagePath;
            var frameRect = spritesheet.GetFrameRect(spriteComponent.CurrentFrameIndex);
            
            float depthOffset = Math.Clamp(transformComponent.Position.Y / 10000f, 0, 0.9f);
            float finalDepth = spriteComponent.LayerDepth - depthOffset;
            
            
            renderer.SubmitToQueue(new RenderCommand {
                TexturePath = spritesheet.ImagePath,
                Position = transformComponent.Position,
                SourceRect = frameRect,
                Color = spriteComponent.Color,
                Rotation = transformComponent.Rotation,
                Origin = spritesheet.FeetOrigin,
                Scale = transformComponent.Scale * 2,
                SortY = transformComponent.Position.Y // foot pivot
            });
        }
    }
}