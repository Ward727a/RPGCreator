using RPGCreator.Core.ECS.Components.Display.Animation;
using RPGCreator.Core.Runtimes.ECS.Components.Actor;
using RPGCreator.SDK.Assets.Definitions.Animations;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.ECS.Systems;
using Serilog;

namespace RPGCreator.Core.ECS.Systems;

public class CharacterAnimationSystem : ISystem
{
    private readonly ILogger _logger = Log.ForContext<CharacterAnimationSystem>();
    
    private readonly ComponentManager _componentManager;
    public override int Priority => 100;
    public override bool IsDrawingSystem => false;
    
    public CharacterAnimationSystem(ComponentManager componentManager)
    {
        _componentManager = componentManager;
    }
    
    public override void Initialize(IECSWorld iecsWorld)
    {
        _logger.Information("AnimationStateSystem initialized.");
    }

    public override void Update(TimeSpan deltaTime)
    {
        foreach (int entityId in _componentManager.Query(typeof(CharStateComponent), typeof(AnimationComponent), typeof(CharDataComponent)))
        {
            ref var stateComp = ref _componentManager.GetComponent<CharStateComponent>(entityId);
            
            if(!stateComp.HasChanged) 
                continue;
            
            ref var animationComp = ref _componentManager.GetComponent<AnimationComponent>(entityId);
            ref var charDataComp = ref _componentManager.GetComponent<CharDataComponent>(entityId);
            
            var charData = charDataComp.CharacterData;
            
            Ulid newAnimationId = Ulid.Empty;

            if (charData.AnimationsMapping.TryGetValue(stateComp.CurrentState, out var directionMap))
            {
                newAnimationId = directionMap.GetAnimation(stateComp.CurrentDirection);
                if (newAnimationId == Ulid.Empty)
                {
                    // Fallback to default direction
                    newAnimationId = directionMap.GetAnimation(EDirection.None);
                }
            }

            if (newAnimationId == Ulid.Empty)
            {
                if(charData.AnimationsMapping.TryGetValue("idle", out var idleDirectionMap))
                {
                    newAnimationId = idleDirectionMap.GetAnimation(stateComp.CurrentDirection);
                    if (newAnimationId == Ulid.Empty)
                    {
                        // Fallback to default direction
                        newAnimationId = idleDirectionMap.GetAnimation(EDirection.None);
                    }
                }
            }
            
            if (newAnimationId != Ulid.Empty && newAnimationId != stateComp.LastResolvedAnimationId)
            {
                if(animationComp.Instance != null)
                {
                    EngineCore.Instance.Managers.GameFactory.ReleaseInstance(animationComp.Instance);
                }

                if (EngineCore.Instance.Managers.Assets.TryResolveAsset<AnimationDef>(newAnimationId, out var animDef))
                {
                    animationComp.Instance = EngineCore.Instance.Managers.GameFactory.CreateInstance<AnimationInstance>(animDef);

                    animationComp.CurrentFrame = 0;
                    animationComp.ElapsedTime = 0;
                    animationComp.IsPlaying = true;
                    
                    stateComp.LastResolvedAnimationId = newAnimationId;
                    
                    _logger.Information("Entity {entityId} changed animation to {currentAnimation} due to state {currentState} and direction {currentDirection}.",
                        entityId, newAnimationId, stateComp.CurrentState, stateComp.CurrentDirection);
                }
            }
            
        }
    }
}