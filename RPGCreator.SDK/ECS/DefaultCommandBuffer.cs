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

using RPGCreator.SDK.Logging;

namespace RPGCreator.SDK.ECS;

public interface ICommand
{
    
    int EntityId { get; }
    
    /// <summary>
    /// The execution of the command.<br/>
    /// The <paramref name="finalId"/> is the final entity ID that should be used when executing the command.<br/>
    /// This is useful for commands that were created with a temporary entity ID (like <see cref="CreateEntityCommand"/>).
    /// </summary>
    /// <param name="world">The ECS world where to execute the command.</param>
    /// <param name="finalId">The final entity ID to use.</param>
    public void Execute(IEcsWorld world, int finalId);
}

/// <summary>
/// This is a command to create an entity.
/// </summary>
public sealed class CreateEntityCommand(int entityId, Action<int>? onCreated) : ICommand
{
    public int EntityId { get; } = entityId;
    private readonly Action<int>? _onCreated = onCreated;

    public void Execute(IEcsWorld world, int finalId)
    {
        
    }
    
    public void ExecuteCreation(IEcsWorld world, Dictionary<int, int> idMap)
    {
        int realId = world.CreateDirectEntity();
        idMap[EntityId] = realId;
        _onCreated?.Invoke(realId);
    }
}

public sealed class DestroyEntityCommand(int entityToDestroy) : ICommand
{
    /// <summary>
    /// Entity to destroy.<br/>
    /// This is either a temporary entity ID or a final entity ID depending on the context.
    /// </summary>
    public int EntityId { get; } = entityToDestroy;

    public void Execute(IEcsWorld world, int finalId)
    {
        world.DestroyEntity(finalId);
    }
}

public sealed class AddComponentCommand<T>(int entityId, T component) : ICommand where T : struct, IComponent
{

    /// <summary>
    /// Entity to add the component to.<br/>
    /// Beware: This IS a temporary entity ID that will be replaced by the final entity ID when executing the command buffer.
    /// </summary>
    public int EntityId { get; } = entityId;

    /// <summary>
    /// The component to add.
    /// </summary>
    private readonly T _component = component;
    
    public void Execute(IEcsWorld world, int finalId)
    {
        world.ComponentManager.AddComponent(finalId, _component);
    }
}

public sealed class RemoveComponentCommand<T>(int entityId) : ICommand where T : struct, IComponent
{
    /// <summary>
    /// Entity to remove the component from.<br/>
    /// Beware: This IS a temporary entity ID that will be replaced by the final entity ID when executing the command buffer.
    /// </summary>
    public int EntityId { get; } = entityId;

    public void Execute(IEcsWorld world, int finalId)
    {
        world.ComponentManager.RemoveComponent<T>(finalId);
    }
}

public sealed class ExecuteOnceCreatedCommand(int entityId, Action<int>? onCreated) : ICommand
{
    public int EntityId { get; } = entityId;
    public Action<int>? OnCreated { get; } = onCreated;
    public void Execute(IEcsWorld world, int finalId)
    {
        OnCreated?.Invoke(finalId);
    }
}


public sealed class DefaultCommandBuffer(IEcsWorld world) : IEcsCommandBuffer
{
    private readonly List<ICommand> _commands = new();
    private readonly Dictionary<int, int> _idMap = new();
    public IEcsWorld AssociatedWorld { get; } = world;
    private int _tempEntityId = -1;
    
    private readonly Dictionary<int, List<IComponent>> _tempComponents = new();
    
    public BufferedEntity CreateEntity(Action<int>? onCreated = null)
    {
        var id = _tempEntityId--;
        _commands.Add(new CreateEntityCommand(id, onCreated));
        return new BufferedEntity(id, this);
    }

    public void DestroyEntity(int entityId)
    {
        _commands.Add(new DestroyEntityCommand(entityId));
    }

    public void AddComponent<T>(int entityId, T component) where T : struct, IComponent
    {
        if(!_tempComponents.ContainsKey(entityId))
        {
            _tempComponents[entityId] = new List<IComponent>();
        }
        _tempComponents[entityId].Add(component);
        _commands.Add(new AddComponentCommand<T>(entityId, component));
    }

    public void RemoveComponent<T>(int entityId) where T : struct, IComponent
    {
        _commands.Add(new RemoveComponentCommand<T>(entityId));
    }

    public void ExecuteOnceCreated(int entityId, Action<int> onCreated)
    {
        _commands.Add(new ExecuteOnceCreatedCommand(entityId, onCreated));
    }

    public void Execute(IEcsWorld? world = null)
    {
        if (_commands.Count == 0) return;
        
        var targetWorld = world ?? AssociatedWorld;
        _idMap.Clear();

        var destroyedThisFrame = new HashSet<int>();
        
        for (int i = 0; i < _commands.Count; i++)
        {
            var command = _commands[i];
            
            int finalId;
            
            if (command.EntityId < 0)
            {
                if (!_idMap.TryGetValue(command.EntityId, out finalId))
                {
                    finalId = command.EntityId;
                }
            }
            else
            {
                finalId = command.EntityId;
            }

            if(command is CreateEntityCommand createCommand)
            {
                createCommand.ExecuteCreation(targetWorld, _idMap);
                continue;
            }


            if (finalId >= 0)
            {
                if (destroyedThisFrame.Contains(finalId))
                    continue;
                command.Execute(targetWorld, finalId);
                if(command is DestroyEntityCommand)
                {
                    destroyedThisFrame.Add(finalId);
                }
            }
            else
                Logger.Error("Failed to execute command: Entity ID mapping not found (for: {entityId}, finalId: {finalId}.", command.EntityId, finalId);
        }

        _commands.Clear();
        _tempEntityId = -1;
    }
}