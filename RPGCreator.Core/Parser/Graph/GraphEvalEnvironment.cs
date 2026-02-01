using RPGCreator.SDK.Graph;

namespace RPGCreator.Core.Parser.Graph;

public sealed class GraphEvalEnvironment : IGraphEnv
{

    public struct RegisterValue
    {
        private enum ValueType
        {
            Null,
            Integer,
            Float,
            String,
            Boolean,
            Object
        }

        private ValueType Type;

        private int IntValue;
        private float FloatValue;
        private string? StringValue;
        private bool BoolValue;
        private object? ObjectValue;
        
        public override string ToString()
        {
            return Type switch
            {
                ValueType.Null => "null",
                ValueType.Integer => IntValue.ToString(),
                ValueType.Float => FloatValue.ToString(),
                ValueType.String => StringValue ?? string.Empty,
                ValueType.Boolean => BoolValue.ToString(),
                ValueType.Object => ObjectValue?.ToString() ?? "null",
                _ => "unknown"
            };
        }
        
        public static RegisterValue FromObject(object? value)
        {
            return value switch
            {
                int i => new RegisterValue { Type = ValueType.Integer, IntValue = i },
                float f => new RegisterValue { Type = ValueType.Float, FloatValue = f },
                bool b => new RegisterValue { Type = ValueType.Boolean, BoolValue = b },
                string s => new RegisterValue { Type = ValueType.String, StringValue = s },
                null => new RegisterValue { Type = ValueType.Null },
                _ => new RegisterValue { Type = ValueType.Object, ObjectValue = value }
            };
        }
        
        public T As<T>()
        {
            return Type switch
            {
                ValueType.Integer when typeof(T) == typeof(int) => (T)(object)IntValue,
                ValueType.Float when typeof(T) == typeof(float) => (T)(object)FloatValue,
                ValueType.Boolean when typeof(T) == typeof(bool) => (T)(object)BoolValue,
                ValueType.String when typeof(T) == typeof(string) => (T)(object)(StringValue ?? string.Empty),
                ValueType.Object when typeof(T).IsAssignableFrom(ObjectValue?.GetType()) => (T)ObjectValue,
                _ => throw new InvalidCastException($"Cannot convert register of type {Type} to {typeof(T).Name}.")
            };
        }

        public object? GetValue()
        {
            return Type switch
            {
                ValueType.Integer => IntValue,
                ValueType.Float => FloatValue,
                ValueType.Boolean => BoolValue,
                ValueType.String => StringValue,
                ValueType.Object => ObjectValue,
                ValueType.Null => null,
                _ => null
            };
        }
    }
    
    public const int MaxRegisters = 256;
    /// <summary>
    /// Registers are used to store temporary values during the evaluation of the graph.<br/>
    /// To access a register, you should use the `GetRegister` and `SetRegister` methods.<br/>
    /// NEVER access the `Registers` array directly unless you know what you're doing.<br/>
    /// The index of the register should be between 0 and `MaxRegisters - 1`.<br/>
    /// If you need to store more than `MaxRegisters` values, consider using a different data structure or breaking your graph into smaller parts.<br/>
    /// The registers are initialized to `null` by default, so you can safely use them without worrying about uninitialized values.
    /// </summary>
    internal RegisterValue?[] Registers { get; } = new RegisterValue?[MaxRegisters];
    private Dictionary<string, Object?> Variables { get; } = new(); // This holds instance-specific variables.
    public static Dictionary<string, object?> GlobalsVariables { get; } = new(); // This should be static to hold global variables across all instances of the environment.
    /// <summary>
    /// Dictionary to hold labels and their corresponding block index.
    /// </summary>
    public Dictionary<string, int> Labels { get; } = new();
    public int CurrentBlock { get; set; } = 0;
    public int CurrentInstruction { get; set; } = 0;
    
    public bool SetRegister<T>(int index, T value)
    {
        if (index < 0 || index >= MaxRegisters)
        {
            throw new ArgumentOutOfRangeException(nameof(index), $"Register index must be between 0 and {MaxRegisters - 1}.");
        }
        Registers[index] = RegisterValue.FromObject(value);
        return true;
    }

    public RegisterValue? GetRegister(int index)
    {
        if (index < 0 || index >= MaxRegisters)
        {
            throw new ArgumentOutOfRangeException(nameof(index), $"Register index must be between 0 and {MaxRegisters - 1}.");
        }
        return Registers[index];
    }
    
    public T GetRegister<T>(int index)
    {
        if (index < 0 || index >= MaxRegisters)
        {
            throw new ArgumentOutOfRangeException(nameof(index), $"Register index must be between 0 and {MaxRegisters - 1}.");
        }

        if (Registers[index].HasValue)
        {
            return Registers[index]!.Value.As<T>();
        }
        throw new InvalidCastException($"Register at index {index} cannot be cast to type {typeof(T).Name}.");
    }

    public object? GetVariable(string name)
    {
        if (Variables.TryGetValue(name, out var value))
        {
            return value;
        }
        throw new KeyNotFoundException($"Variable '{name}' not found.");
    }
    
    public void SetVariable(string name, object? value)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Variable name cannot be null or empty.", nameof(name));
        }
        Variables[name] = value;
    }
    
    public T? GetVariable<T>(string name)
    {
        if (Variables.TryGetValue(name, out var value))
        {
            if (value is T typedValue)
            {
                return typedValue;
            }
            throw new InvalidCastException($"Variable '{name}' cannot be cast to type {typeof(T).Name}.");
        }
        throw new KeyNotFoundException($"Variable '{name}' not found.");
    }

    public static void AddVM(string path, object? value)
    {
        if (IsMultiPath(path))
        {
            AddMultipathVM(path, value);
        }
        else
        {
            AddSimpleVM(path, value);
        }
    }

    public static void AddVMs(params (string path, object? value)[] values)
    {
        foreach (var value in values)
        {
            if (IsMultiPath(value.path))
            {
                AddMultipathVM(value.path, value.value);
            }
            else
            {
                AddSimpleVM(value.path, value.value);
            }
        }
    }

    static private void AddMultipathVM(string path, object? value)
    {
        
        var components = BreakPaths(path);

        var current = GlobalsVariables;
        string currentPath;
        var lastPath = components.Last();
        for (int i = 0; i <= components.Count - 1; i++)
        {
            currentPath = components[i];
            if (lastPath == currentPath)
            {
                current[currentPath] = value;
                break;
            }
                    
            if (!current.TryGetValue(currentPath, out var next))
            {
                next = new Dictionary<string, object?>();
                current[currentPath] = next;
            }
            if (next is not Dictionary<string, object?> nextDict)
            {
                throw new InvalidOperationException($"Global Variable at path '{path}' is not a valid multi-path. Expected a dictionary at '{currentPath}'.");
            }
                    
            current = nextDict;
        }
    }

    static private void AddSimpleVM(string path, object? value)
    {
        GlobalsVariables[path] = value;
    }
    
    public object? GetVM(string path)
    {
        object? value;
        if (IsMultiPath(path))
        {
            GetMutlipathVM(path, out value);
        }
        else
        {
            GetSimpleVM(path, out value);
        }
        return value;
    }

    private void GetMutlipathVM(string path, out object? value)
    {
        var components = BreakPaths(path);

        var current = GlobalsVariables;
        string currentPath;
        var lastPath = components.Last();
        for (int i = 0; i <= components.Count - 1; i++)
        {
            currentPath = components[i];
            if (lastPath == currentPath)
            {
                if (current.TryGetValue(currentPath, out value))
                {
                    return;
                }
                throw new KeyNotFoundException($"Global variable '{path}' not found.");
            }
                    
            if (current.TryGetValue(currentPath, out var next))
            {
                if(next is Dictionary<string, object?> nextDict)
                {
                    current = nextDict;
                    continue;
                }
                throw new KeyNotFoundException($"Global variable '{path}' not found.");
            }
            
            throw new KeyNotFoundException($"Global variable '{currentPath}' not found in path '{path}'.");
        }
        
        if (current.TryGetValue(lastPath, out value))
        {
            return;
        }
        
        throw new KeyNotFoundException($"Global variable '{path}' not found.");
    }

    private void GetSimpleVM(string path, out object? value)
    {
        if(GlobalsVariables.TryGetValue(path, out value))
        {
            return;
        }
        throw new KeyNotFoundException($"Global variable '{path}' not found.");
    }
    
    static private List<String> BreakPaths(string path)
    {
        // This method should break the path into its components.
        // For example, "player.name" should return ["player", "name"].
        // This is a simple implementation, you might want to enhance it to handle more complex paths.
        return path.Split('.').Select(p => p.Trim()).ToList();
    }
    
    static private bool IsMultiPath(string path)
    {
        // This method checks if the path is a multi-path (i.e., contains multiple components).
        // For example, "player.name" is a multi-path, while "player" is not.
        return path.Contains('.');
    }
}