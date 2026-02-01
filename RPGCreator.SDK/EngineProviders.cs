using System.ComponentModel;
using RPGCreator.SDK.Inputs;

namespace RPGCreator.SDK;


public interface IGameProvider : INotifyPropertyChanged, INotifyPropertyChanging
{
    /// <summary>
    /// The current game instance.<br/>
    /// Normally this is the MonoGame Game class instance.<br/>
    /// But to keep the SDK decoupled from MonoGame, this is typed as object.
    /// </summary>
    object GameInstance { get; }
}

public interface IMouseProvider
{
    /// <summary>
    /// Update the mouse data.
    /// </summary>
    /// <param name="data"></param>
    void Update(RawMouseData data);
    
    /// <summary>
    /// Specialized update for viewport mouse data.
    /// </summary>
    /// <param name="data"></param>
    void UpdateViewport(RawMouseData data);
}

public class DefaultMouseProvider : IMouseProvider
{
    /// <summary>
    /// Update the mouse data.
    /// </summary>
    /// <param name="data"></param>
    public void Update(RawMouseData data)
    {
        EngineStates.MouseState.Update(data);
    }
    
    /// <summary>
    /// Specialized update for viewport mouse data.<br/>
    /// This will update the <b>PENDING</b> viewport mouse state.<br/>
    /// Then the engine core will swap it at the end of the frame to avoid input lag.<br/>
    /// For the 'vanilla' engine version, it should be swapped at the start of the Update loop of the game.
    /// </summary>
    /// <param name="data"></param>
    public void UpdateViewport(RawMouseData data)
    {
        EngineStates.ViewportMouseState.Update(data);
    }
}

public interface IKeyboardProvider
{
    /// <summary>
    /// Update the keyboard data.
    /// </summary>
    /// <param name="data"></param>
    void Update(RawKeyboardData data);
    /// <summary>
    /// Specialized update for viewport keyboard data.
    /// </summary>
    /// <param name="data"></param>
    void UpdateViewport(RawKeyboardData data);
}

public class DefaultKeyboardProvider : IKeyboardProvider
{
    public void Update(RawKeyboardData data)
    {
        EngineStates.KeyboardState.Update(data);
    }
    
    public void UpdateViewport(RawKeyboardData data)
    {
        EngineStates.ViewportKeyboardState.Update(data);
    }
}

/// <summary>
/// Provides access to various engine-level service providers.<br/>
/// This allow to make the 'bridge' between the SDK and various other engine parts.<br/>
/// Like but not Limited to:
/// <ul>
///     <li>
///         <b>MouseProvider</b> => Provides access to mouse handling across different engine parts.
///     </li>
///     <li>
///         <b>KeyboardProvider</b> => Provides access to keyboard handling across different engine parts.
///     </li>
/// </ul>
/// </summary>
public static class EngineProviders
{
    private static IMouseProvider _mouseProvider = new DefaultMouseProvider();

    public static IMouseProvider MouseProvider
    {
        get => _mouseProvider;
        set
        {
            if (_mouseProvider == value) return;
            
            _mouseProvider = value;
            PropertyChanged?.Invoke(nameof(MouseProvider));
        }
    }
    private static IKeyboardProvider _keyboardProvider = new DefaultKeyboardProvider();

    public static IKeyboardProvider KeyboardProvider
    {
        get => _keyboardProvider;
        set
        {
            if (_keyboardProvider == value) return;
            
            _keyboardProvider = value;
            PropertyChanged?.Invoke(nameof(KeyboardProvider));
        }
    }

    public static event Action<string>? PropertyChanged;
}