using RPGCreator.Core.Runtimes.ECS.Components.Actor;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.ECS.Systems;
using Serilog;
using CharStateComponent = RPGCreator.Core.ECS.Components.Display.Animation.CharStateComponent;
using Vector2 = System.Numerics.Vector2;

namespace RPGCreator.Core.ECS.Systems;

public class MovementSystem : ISystem
{
    private readonly ILogger _logger = Log.ForContext<MovementSystem>();
    
    private readonly ComponentManager _componentManager;
    public override int Priority { get; } = 100;
    public override bool IsDrawingSystem { get; } = true;

    public MovementSystem(ComponentManager componentManager)
    {
        _componentManager = componentManager;
    }
    
    public override void Initialize(IEcsWorld ecsWorld)
    {
        _logger.Information("MovementSystem initialized.");
    }

    public override void Update(TimeSpan deltaTime)
    {

        foreach (var entityId in _componentManager.Query<MovementComponent, TransformComponent>())
        {
            ref var movement = ref _componentManager.GetComponent<MovementComponent>(entityId);
            ref var transform = ref _componentManager.GetComponent<TransformComponent>(entityId);

            if (!movement.IsMoving || movement.TargetDirection == Vector2.Zero)
            {
                if (_componentManager.HasComponent<CharStateComponent>(entityId))
                {
                    ref var charState = ref _componentManager.GetComponent<CharStateComponent>(entityId);
                    charState.CurrentState = "idle";
                }
                continue;
            }

            switch (movement.Mode)
            {
                case MovementMode.Grid8:
                    HandleGridMovement(entityId, ref movement, ref transform, allowDiagonals: true);
                    break;
                case MovementMode.Grid4:
                    HandleGridMovement(entityId, ref movement, ref transform, allowDiagonals: false);
                    break;
                case MovementMode.Free:
        
                    if (_componentManager.HasComponent<CharStateComponent>(entityId))
                    {
                        ref var animState = ref _componentManager.GetComponent<CharStateComponent>(entityId);
            
                        animState.CurrentState = "walk";
            
                        var newDir = GetDirectionFromVector(movement.TargetDirection);
            
                        if (newDir != EntityDirection.Center)
                        {
                            animState.CurrentDirection = newDir;
                        }
                    }
                    transform.Position += movement.TargetDirection * (float)deltaTime.TotalSeconds * movement.Speed;
                    break;
                default:
                    _logger.Warning("Entity {entityId} has unknown movement mode {mode}.", entityId, movement.Mode);
                    break;
            }
        }
        
    }
    
    private void HandleGridMovement(int entityId, ref MovementComponent movement, ref TransformComponent transform, bool allowDiagonals)
    {
        var dir = movement.TargetDirection;

        // clamp for 4 directions
        if (!allowDiagonals)
        {
            if (Math.Abs(dir.X) > Math.Abs(dir.Y))
                dir = new Vector2(MathF.Sign(dir.X), 0);
            else
                dir = new Vector2(0, MathF.Sign(dir.Y));
        }
        else
        {
            dir = Vector2.Normalize(dir);
        }
        
        if (_componentManager.HasComponent<CharStateComponent>(entityId))
        {
            ref var animState = ref _componentManager.GetComponent<CharStateComponent>(entityId);
            
            animState.CurrentState = "walk";
            
            var newDir = GetDirectionFromVector(dir);
            
            if (newDir != EntityDirection.Center)
            {
                animState.CurrentDirection = newDir;
            }
        }

        transform.Position += dir * movement.Speed;
        _logger.Debug("Entity {entityId} moved to position {position} using grid movement.", entityId, transform.Position);
    }

    private EntityDirection GetDirectionFromVector(Vector2 dir)
    {
        if(dir == Vector2.Zero)
            return EntityDirection.Center;
        
        if(Math.Abs(dir.X) > Math.Abs(dir.Y))
        {
            return dir.X > 0 ? EntityDirection.Right : EntityDirection.Left;
        }
        else
        {
            return dir.Y > 0 ? EntityDirection.Down : EntityDirection.Up;
        }
    }
}