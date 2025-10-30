using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Core.Runtimes.ECS.Components.Display.Animation;
using Serilog;

namespace RPGCreator.Core.Runtimes.ECS.Systems;

public class AnimationStateSystem : ISystem
{
    private readonly ILogger _logger = Log.ForContext<AnimationStateSystem>();
    
    private readonly ComponentManager _componentManager;
    public override int Priority { get; }
    public override bool IsDrawingSystem { get; }
    
    public AnimationStateSystem(ComponentManager componentManager)
    {
        _componentManager = componentManager;
    }
    
    public override void Initialize(IECSWorld iecsWorld)
    {
        iecsWorld._eventBus.Subscribe<ComponentChangedEvent<StateComponent>>(arg =>
        {
            int entityId = arg.EntityId;
            ref var stateComp = ref _componentManager.GetComponent<StateComponent>(entityId);
            ref var animationComp = ref _componentManager.GetComponent<AnimationComponent>(entityId);
            
            if (stateComp.CurrentState != stateComp.PreviousState)
            {
                animationComp.CurrentAnimation = stateComp.CurrentState;
                animationComp.CurrentFrame = 0;
                animationComp.ElapsedTime = 0;
                
                _logger.Information("Entity {entityId} changed state from {previousState} to {currentState}, switching animation to {currentAnimation}.",
                    entityId, stateComp.PreviousState, stateComp.CurrentState, animationComp.CurrentAnimation);
                
                stateComp.PreviousState = stateComp.CurrentState;
            }
        });
        
        _logger.Information("AnimationStateSystem initialized.");
    }

    public override void Update(GameTime deltaTime)
    {
        foreach (int entityId in _componentManager.QueryDirty(typeof(StateComponent)))
        {
            ref var stateComp = ref _componentManager.GetComponent<StateComponent>(entityId);
            ref var animationComp = ref _componentManager.GetComponent<AnimationComponent>(entityId);
            
            if (stateComp.CurrentState != stateComp.PreviousState)
            {
                animationComp.CurrentAnimation = stateComp.CurrentState;
                animationComp.CurrentFrame = 0;
                animationComp.ElapsedTime = 0;
                
                _logger.Information("Entity {entityId} changed state from {previousState} to {currentState}, switching animation to {currentAnimation}.",
                    entityId, stateComp.PreviousState, stateComp.CurrentState, animationComp.CurrentAnimation);
                
                stateComp.PreviousState = stateComp.CurrentState;
            }
        }
    }
}