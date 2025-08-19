namespace RPGCreator.Core.Parser.Graph;

public sealed class GraphEvalEnvironment
{
    public const int MaxRegisters = 256;
    public object?[] Registers { get; } = new object?[MaxRegisters];
    public static Dictionary<string, Object?> GlobalsVariables { get; } = new(); // This should be static to hold global variables across all instances of the environment.
    /// <summary>
    /// Dictionary to hold labels and their corresponding block index.
    /// </summary>
    public Dictionary<string, int> Labels { get; } = new();
    public int CurrentBlock { get; set; } = 0;
    public int CurrentInstruction { get; set; } = 0;

    static public void AddVM(string path, object? value)
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

    static public void AddVMs(params (string path, object? value)[] values)
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