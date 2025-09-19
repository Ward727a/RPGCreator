using RPGCreator.Core.Parser.Graph;
using RPGCreator.Core.Type.Internal;

namespace RPGCreator.Core.Type.Assets.Characters.Stats;

public sealed class StatInstance : IReloadable<IStatDef>
{
    /// <summary>
    /// Define the unique identifier of the stat type.
    /// </summary>
    public readonly Ulid RuntimeId;
    /// <summary>
    /// Define the unique identifier of the stat definition in the game resources.<br/>
    /// This is the one that will be used by this stat type.
    /// </summary>
    public readonly Ulid StatDefinitionId;
    /// <summary>
    /// The base value of the stat.<br/>
    /// This value is the one that will be used to calculate the current value of the stat.<br/>
    /// It is the value that will be modified by modifiers, such as items, effects, or any other object that can modify the stat.<br/>
    /// BUT it should not be modified by any modifier, as it is the base value of the stat. (Except for resources based stats, such as health or mana, which can be modified by effects or items that restore or consume resources.)
    /// </summary>
    public float BaseValue;
    /// <summary>
    /// The current value of the stat.<br/>
    /// This value is the one that will be used by the character to perform actions, such as attacking, defending, or casting spells.<br/>
    /// It can be modified by modifiers, such as items, effects, or any other object that can modify the stat.
    /// </summary>
    public float CurrentValue { get; private set; }

    private IStatDef _statDefinition;
    public IStatDef StatDefinition => _statDefinition;

    public StatInstance()
    {
        
    }

    public StatInstance(IStatDef def)
    {
        _statDefinition = def ?? throw new ArgumentNullException(nameof(def));
        StatDefinitionId = def.Unique;
        RuntimeId = Ulid.NewUlid();
        BaseValue = def.DefaultValue;
        CurrentValue = BaseValue;
    }
    
    public bool TryRunEvent(string eventName, GraphEvalEnvironment? env = null)
    {
        if(StatDefinition.TryGetEvent(eventName, out var eventCompiled) && eventCompiled != null)
        {
            return eventCompiled.Run(env);
        }
        return false;
    }

    public void Reload(IStatDef newDefinition)
    {
        if (newDefinition.Unique != StatDefinitionId)
        {
            throw new InvalidOperationException("Cannot reload StatInstance with a different StatDefinition unique ID.");
        }
        _statDefinition = newDefinition;
        // Optionally, you might want to reset the BaseValue to the new definition's DefaultValue
        BaseValue = newDefinition.DefaultValue;
        // And reset CurrentValue to BaseValue or keep it as is, depending on your game's logic
        // CurrentValue = Math.Clamp(CurrentValue, newDefinition.StatMinValue, GetStatCapValue());
    }
}