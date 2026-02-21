using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Editor.Brushes;
using RPGCreator.SDK.Exceptions;
using RPGCreator.SDK.GlobalState;
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.Projects;

namespace RPGCreator.SDK;

public enum BrushMode
{
    Tiling,
    Entities
}

public interface IBrushState : IState
{
    IBrushInfo? CurrentBrush { get; set; }
    BrushMode CurrentMode { get; set; }
    object? CurrentObjectToPaint { get; set; }
    bool IsPlacing { get; set; }
    bool IsDrawing { get; set; }
    Vector2 LastDrawAt { get; set; }
}

public interface IEditorState : IState
{
    bool InEditorMode { get; set; }
    ITileDef? CurrentTile { get; set; }
}

public interface IProjectState : IState
{
    IBaseProject? CurrentProject { get; set; }
}

public class GlobalStatesProvider
{
    private readonly Dictionary<Type, Dictionary<string, IState>> _states = new();

    public T GetState<T>(string groupName = "") where T : class, IState
    {
        if (_states.TryGetValue(typeof(T), out var groups))
        {
            if (groups.TryGetValue(groupName, out var state))
            {
                return (T)state;
            }
            
            if (string.IsNullOrEmpty(groupName) && groups.Count == 1)
            {
                return (T)groups.Values.First();
            }
        }

        throw new CriticalEngineException($"[Global States] Critical State Missing: {typeof(T).Name} (Group: '{groupName}')", _states);
    }
    
    
    public bool TryGetState<T>([NotNullWhen(true)] out T? state, string groupName = "") where T : class, IState
    {
        if (_states.TryGetValue(typeof(T), out var groups))
        {
            if (groups.TryGetValue(groupName, out var svc))
            {
                state = (T)svc;
                return true;
            }
            
            if (string.IsNullOrEmpty(groupName) && groups.Count == 1)
            {
                state = (T)groups.Values.First();
                return true;
            }
        }

        state = null;
        return false;
    } 

    public void RegisterState<T>(T state, string groupName) where T : class, IState
    {
        if (!_states.TryGetValue(typeof(T), out var groups))
        {
            groups = new Dictionary<string, IState>();
            _states[typeof(T)] = groups;
        }

        if (groups.ContainsKey(groupName))
        {
            throw new InvalidOperationException($"[Global States] State '{typeof(T).Name}' already registered in group '{groupName}'.");
        }

        groups[groupName] = state;
    }
}

public static class GlobalStates
{
    private static readonly GlobalStatesProvider StateProvider = new();
    private static readonly Dictionary<Type, List<Action<IState>>> StateReadyCallbacks = new();
    private static readonly object StateReadyLock = new object();
    
    // ReSharper disable MemberCanBePrivate.Global
    public static void RegisterState<T>(T state, string groupName) where T : class, IState
    {
        if(string.Equals(groupName, "default", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("[Global States] 'default' is a reserved group name. Use a different name for the state group.");
        }
        
        StateProvider.RegisterState(state, groupName);
    }
    public static T GetState<T>(string groupName = "default", T? defaultInstance = null) where T : class, IState
    {
        if (StateProvider.TryGetState<T>(out var state, groupName))
        {
            return state;
        }

        #if !DEBUG
        if(defaultInstance == null)
            throw new InvalidOperationException($"[Global States] Critical State Missing: {typeof(T).Name}. Make sure it's registered during engine initialization.");

        return defaultInstance;
        #else
        throw new InvalidOperationException($"[Global States] Critical State Missing: {typeof(T).Name}. Make sure it's registered during engine initialization.");
        #endif
    }
    
    private static void RegisterState<T>(T state) where T : class, IState
    {
        StateProvider.RegisterState(state, "default");
        InvokeStateReadyCallbacks(state);
    }
    
    private static void InvokeStateReadyCallbacks<T>(T state) where T : class, IState
    {
        lock (StateReadyLock)
        {
            if (StateReadyCallbacks.TryGetValue(typeof(T), out var callbacks))
            {
                foreach (var callback in callbacks)
                {
                    callback(state);
                }

                StateReadyCallbacks.Remove(typeof(T));
            }
        }
    }
    
    /// <summary>
    /// The name of the application.
    /// </summary>
    public static string ApplicationName => "RPG Creator";
    public static TimeSpan ElapsedTime { get; set; }
    public static TimeSpan TotalTime { get; set; }
    
    /// <summary>
    /// The current version of the engine.
    /// </summary>
    public static Version ApplicationVersion => new Version(0, 1, 0);
    public static IEditorState EditorState
    {
        get => GetState<IEditorState>();
        set => RegisterState(value);
    }

    public static IProjectState ProjectState
    {
        get => GetState<IProjectState>();
        set => RegisterState(value);
    }

    public static IBrushState BrushState
    {
        get => GetState<IBrushState>();
        set => RegisterState(value);
    }

    public static IMapState MapState
    {
        get => GetState(defaultInstance: field);
        set => RegisterState(value);
    } = new MapState();

    public static IMouseState MouseState
    {
        get => GetState<IMouseState>();
        set => RegisterState(value);
    }

    public static IKeyboardState KeyboardState
    {
        get => GetState<IKeyboardState>();
        set => RegisterState(value);
    }
    
    public static IMouseState ViewportMouseState
    {
        get => GetState<IMouseState>("viewport");
        set => RegisterState(value, "viewport");
    }

    public static IKeyboardState ViewportKeyboardState
    {
        get => GetState<IKeyboardState>("viewport");
        set => RegisterState(value, "viewport");
    }

    public static IToolState ToolState
    {
        get => GetState(defaultInstance: field);
        set => RegisterState(value);
    } = new BaseToolState();
}