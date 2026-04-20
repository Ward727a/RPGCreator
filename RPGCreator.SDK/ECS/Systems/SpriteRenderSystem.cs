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

using System.Numerics;
using RPGCreator.SDK.Assets.Definitions.Animations;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.RuntimeService;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.ECS.Systems;

public class SpriteRenderSystem : ISystem
{
    
    public override int Priority => 2000;
    public override bool IsDrawingSystem => true;
    
    ComponentManager _componentManager = null!;

    private int _shouldRecalculateSpriteSizeIdx;
    
    private Action<TimeSpan> _updateAction = (_) => { };
    
    private readonly Dictionary<Ulid, SpritesheetDef> _sheetCache = new();

    public SpriteRenderSystem(int shouldRecalculateSpriteSizeIdx)
    {
        _shouldRecalculateSpriteSizeIdx = shouldRecalculateSpriteSizeIdx;
    }
    
    public override void Initialize(IEcsWorld ecsWorld)
    {
        _componentManager = ecsWorld.ComponentManager;
        _updateAction = CheckingServiceReady;
        
        RuntimeServices.MapService.MapLoaded += (_) =>
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

    private enum ENeedRecalculateSpriteSize : byte
    {
        FirstTime,
        ShouldRecalculate,
        Done
    }
    
    private void ActualUpdate(TimeSpan deltaTime)
    {
        var renderer = RuntimeServices.RenderService;
        
        // All entities have a state component without exception, so we can safely get the set without checking.
        // If it crashes, then there is a problem with how the engine ECS setup is made, and not in this system.
        var stateSet = _componentManager.GetCompSet<StateComponent>(); 
        var transformSet = _componentManager.GetCompSet<TransformComponent>();
        var spriteSet = _componentManager.GetCompSet<SpriteComponent>();
        var boundsSet = _componentManager.GetCompSet<BoundsComponent>();
        
        foreach (var entityId in _componentManager.QueryDirty<TransformComponent>().WithComponent<BoundsComponent>())
        {
            ref var stateComponent = ref stateSet.Get(entityId);
            stateComponent.SetByte(_shouldRecalculateSpriteSizeIdx, (byte)ENeedRecalculateSpriteSize.ShouldRecalculate);
        }

        foreach (var entityId in _componentManager.QueryDirty<BoundsComponent>())
        {
            ref var stateComponent = ref stateSet.Get(entityId);
            stateComponent.SetByte(_shouldRecalculateSpriteSizeIdx, (byte)ENeedRecalculateSpriteSize.ShouldRecalculate);
        }
        
        foreach (var entityId in _componentManager.Query<SpriteComponent, TransformComponent, BoundsComponent>())
        {
            ref var stateComponent = ref stateSet.Get(entityId);
            ref var transformComponent = ref transformSet.Get(entityId);
            ref var spriteComponent = ref spriteSet.Get(entityId);
            ref var boundsComponent = ref boundsSet.Get(entityId);
            
            if (!_sheetCache.TryGetValue(spriteComponent.SpritesheetId, out var spritesheet))
            {
                if (EngineServices.AssetsManager.TryResolveAsset(spriteComponent.SpritesheetId, out spritesheet))
                {
                    _sheetCache[spriteComponent.SpritesheetId] = spritesheet;
                }
                else continue;
            }
            
            var frameRect = spritesheet.GetFrameRect(spriteComponent.CurrentFrameIndex);

            var shouldRecalculateSize = stateComponent.GetByte(_shouldRecalculateSpriteSizeIdx);
            if (shouldRecalculateSize != (byte)ENeedRecalculateSpriteSize.Done)
            {
                Logger.Debug("Recalculating sprite size");
                stateComponent.SetByte(_shouldRecalculateSpriteSizeIdx, (byte)ENeedRecalculateSpriteSize.Done);
                Vector2 finalScale = transformComponent.Scale;
                Vector2 offset = Vector2.Zero;
            
                float scaleX = boundsComponent.Width / frameRect.Width;
                float scaleY = boundsComponent.Height / frameRect.Height;

                switch (spriteComponent.SizeMode)
                {
                    case ESizeMode.Stretch:
                    {
                        finalScale *= new Vector2(scaleX, scaleY);
                        break;
                    }
                    case ESizeMode.KeepAspectRatio:
                    {
                        float ratio = MathF.Min(scaleX, scaleY);
                        finalScale *= new Vector2(ratio, ratio);
                    
                        offset.X = (boundsComponent.Width - (frameRect.Width * ratio * transformComponent.Scale.X)) / 2f;
                        offset.Y = (boundsComponent.Height - (frameRect.Height * ratio * transformComponent.Scale.Y)) / 2f;
                        break;
                    }
                    case ESizeMode.Center:
                    {
                        offset.X = (boundsComponent.Width - frameRect.Width * transformComponent.Scale.X) / 2f;
                        offset.Y = (boundsComponent.Height - frameRect.Height * transformComponent.Scale.Y) / 2f;
                        break;
                    }
                }
                
                spriteComponent.ScaledSize = finalScale;
                spriteComponent.Offset = offset;
                
                Logger.Debug("Sprite size recalculated, result: ScaledSize: {scaledSize}, Offset: {offset}", spriteComponent.ScaledSize, spriteComponent.Offset);
            }
            
            // Debug drawing render square

            var originDebugRect = spritesheet.FeetOrigin * spriteComponent.ScaledSize;

            if (!boundsComponent.OffsetOrigin.HasValue || boundsComponent.OffsetOrigin.Value != originDebugRect)
            {
                boundsComponent.OffsetOrigin = originDebugRect; // This is kinda dirty to do this like that... But well, it's needed.
                // Still need to think of a better way (even more with the collision editor coming).
            }
            
            // renderer.DrawDebugRect(
            //     transformComponent.Position
            //     - (originDebugRect), // Here we use the scaledsize because the Origin is from the sprite.
            //     (boundsComponent.Size), 
            //     Color.Magenta,
            //     1f
            //     );
            
            renderer.SubmitToQueue(new RenderCommand {
                TexturePath = spritesheet.ImagePath,
                Position = transformComponent.Position + spriteComponent.Offset,
                SourceRect = frameRect,
                Color = spriteComponent.Color,
                Rotation = transformComponent.Rotation,
                Origin = spritesheet.FeetOrigin,
                Scale = spriteComponent.ScaledSize,
                SortY = transformComponent.Position.Y // Foot pivot (the engine manages this, so we only need to put the position Y)
            });
        }
    }

}