using Microsoft.Xna.Framework;
using RPGCreator.Core.Runtimes.ECS.Components.Actor;
using RPGCreator.Core.Runtimes.ECS.Components.Display;
using Serilog;
using Vector2 = System.Numerics.Vector2;

namespace RPGCreator.Core.Runtimes.ECS.Systems;

public class MovementSystem : ISystem
{
    private readonly ILogger _logger = Log.ForContext<MovementSystem>();
    
    private readonly ComponentManager _componentManager;
    public override int Priority { get; } = 0;
    public override bool IsDrawingSystem { get; } = true;

    public MovementSystem(ComponentManager componentManager)
    {
        _componentManager = componentManager;
    }
    
    public override void Initialize(IECSWorld iecsWorld)
    {
        _logger.Information("MovementSystem initialized.");
    }

    public override void Update(GameTime deltaTime)
    {

        foreach (var entityId in _componentManager.Query<MovementComponent, TransformComponent>())
        {
            ref var movement = ref _componentManager.GetComponent<MovementComponent>(entityId);
            ref var transform = ref _componentManager.GetComponent<TransformComponent>(entityId);

            if (movement.IsMoving == false)
                continue;

            switch (movement.Mode)
            {
                case MovementMode.Grid8:
                    HandleGridMovement(entityId, ref movement, ref transform, allowDiagonals: true);
                    break;
                case MovementMode.Grid4:
                    HandleGridMovement(entityId, ref movement, ref transform, allowDiagonals: false);
                    break;
                case MovementMode.Free:
                    transform.Position += movement.TargetDirection * (float)deltaTime.ElapsedGameTime.TotalSeconds * movement.Speed;
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

        transform.Position += dir * movement.Speed;
        _logger.Debug("Entity {entityId} moved to position {position} using grid movement.", entityId, transform.Position);
    }
}