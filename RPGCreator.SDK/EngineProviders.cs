using RPGCreator.SDK.Inputs;

namespace RPGCreator.SDK;


public interface IGameProvider
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
}

public class DefaultMouseProvider : IMouseProvider
{
    public void Update(RawMouseData data)
    {
        EngineStates.MouseState.Update(data);
    }
}

/// <summary>
/// Provides access to various engine-level service providers.<br/>
/// This allow to make the 'bridge' between the SDK and various other engine parts.<br/>
/// Like but not limited to:<br/>
/// - GameProvider => Provides access to the MonoGame game instance from RTP/Player to Core and UI (without the need to reference MonoGame in the SDK).<br/>
/// - InputProvider => Provides access to input handling across different engine parts.
/// </summary>
public static class EngineProviders
{
    public static IGameProvider GameProvider { get; set; } = null!;
    public static IMouseProvider MouseProvider { get; set; } = new DefaultMouseProvider();
}