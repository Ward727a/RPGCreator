using System.Numerics;
using RPGCreator.SDK.GlobalState;

namespace RPGCreator.SDK.Inputs;

public readonly record struct RawMouseData(int X, int Y, int Scroll, int HScroll, MouseButton Buttons, bool IsInsideWindow, object? InObject = null, bool IsNotDefault = true);

[Flags]
public enum MouseButton
{
    None = 0,
    Left = 1,
    Middle = 2,
    Right = 4,
    XButton1 = 8,
    XButton2 = 16 /* 0x10 */,
}

public interface IMouseState : IState
{
    event Action<MouseButton>? ButtonDown;
    event Action<MouseButton>? ButtonUp;
    event Action<MouseButton>? Clicked;
    event Action<MouseButton>? DoubleClicked;
    event Action<Vector2>? Moved;
    event Action<int>? WheelScrolled;
    event Action<int>? HorizontalWheelScrolled;
    event Action<object?>? HoveredObjectChanged;
    HashSet<MouseButton> PressedButtons { get; }
    
    /// <summary>
    /// The current X position of the mouse cursor.
    /// </summary>
    int X { get; }
    /// <summary>
    /// The current Y position of the mouse cursor.
    /// </summary>
    int Y { get; }
    /// <summary>
    /// Indicates whether the left mouse button is currently pressed.
    /// </summary>
    bool LeftButtonPressed { get; }
    double TimeSinceLastLeftPress { get; }
    bool LeftButtonDoublePressed { get; }
    /// <summary>
    /// Indicates whether the left mouse button was just clicked this frame.<br/>
    /// </summary>
    public bool LeftClicked { get; }
    /// <summary>
    /// Indicates whether the left mouse button was just double-clicked this frame.<br/>
    /// </summary>
    public bool LeftDoubleClicked { get; }
    /// <summary>
    /// Indicates whether the right mouse button is currently pressed.
    /// </summary>
    bool RightButtonPressed { get; }
    double TimeSinceLastRightPress { get; }
    bool RightButtonDoublePressed { get; }
    /// <summary>
    /// Indicates whether the right mouse button was just clicked this frame.<br/>
    /// </summary>
    public bool RightClicked { get; }
    /// <summary>
    /// Indicates whether the right mouse button was just double-clicked this frame.<br/>
    /// </summary>
    public bool RightDoubleClicked { get; }
    /// <summary>
    /// Indicates whether the middle mouse button is currently pressed.
    /// </summary>
    bool MiddleButtonPressed { get; }
    double TimeSinceLastMiddlePress { get; }
    bool MiddleButtonDoublePressed { get; }
    /// <summary>
    /// Indicates whether the middle mouse button was just clicked this frame.<br/>
    /// </summary>
    public bool MiddleClicked { get; }
    /// <summary>
    /// Indicates whether the middle mouse button was just double-clicked this frame.<br/>
    /// </summary>
    public bool MiddleDoubleClicked { get; }
    
    
    /// <summary>
    /// The current mouse position.
    /// </summary>
    Vector2 Position { get; }
    /// <summary>
    /// The change in mouse position since the last frame.
    /// </summary>
    Vector2 DeltaPosition { get; }
    
    /// <summary>
    /// The amount the mouse wheel has scrolled since the last frame.
    /// </summary>
    int WheelDelta { get; }
    /// <summary>
    /// The amount the horizontal mouse wheel has scrolled since the last frame.
    /// </summary>
    int HorizontalWheelDelta { get; }
    
    /// <summary>
    /// Indicates whether the mouse cursor is currently inside the application window.
    /// </summary>
    bool IsInsideWindow { get; }
    
    /// <summary>
    /// Indicates the object the mouse is currently hovering over, if any.<br/>
    /// If the mouse is not hovering over any object, this will be null.<br/>
    /// Otherwise, this will be the object instance being hovered.
    /// </summary>
    object? InObject { get; }
    
    /// <summary>
    /// Updates the mouse state with the provided raw mouse data.<br/>
    /// This should be automatically called by the <see cref="RPGCreator.Core"/> engine part at each <see cref="IMouseProvider"/> update.
    /// </summary>
    /// <param name="rawMouseData"></param>
    /// <exception cref="NotImplementedException">
    /// Thrown if the method is not implemented by the current used service.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    void Update(RawMouseData rawMouseData);
    
    /// <summary>
    /// Gets the current raw mouse data.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException">
    /// Thrown if the method is not implemented by the current used service.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    RawMouseData GetCurrentRawData();
    
    /// <summary>
    /// Indicates whether the specified mouse button is currently pressed.
    /// </summary>
    /// <param name="buttonIndex"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException">
    /// Thrown if the method is not implemented by the current used service.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    bool IsButtonPressed(MouseButton buttonIndex);
    /// <summary>
    /// Indicates whether the specified mouse button is currently released.
    /// </summary>
    /// <param name="buttonIndex"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException">
    /// Thrown if the method is not implemented by the current used service.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    bool IsButtonReleased(MouseButton buttonIndex);
    /// <summary>
    /// Indicates whether the specified mouse button was pressed at the previous frame.
    /// </summary>
    /// <param name="buttonIndex"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException">
    /// Thrown if the method is not implemented by the current used service.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    bool WasButtonPressed(MouseButton buttonIndex);
    /// <summary>
    /// Indicates whether the specified mouse button was released at the previous frame.
    /// </summary>
    /// <param name="buttonIndex"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException">
    /// Thrown if the method is not implemented by the current used service.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    bool WasButtonReleased(MouseButton buttonIndex);
    /// <summary>
    /// Indicates whether the specified mouse button was just pressed this frame.
    /// </summary>
    /// <param name="buttonIndex"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException">
    /// Thrown if the method is not implemented by the current used service.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    bool WasButtonJustPressed(MouseButton buttonIndex);
    /// <summary>
    /// Indicates whether the specified mouse button was just released this frame.
    /// </summary>
    /// <param name="buttonIndex"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException">
    /// Thrown if the method is not implemented by the current used service.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    bool WasButtonJustReleased(MouseButton buttonIndex);
    
    /// <summary>
    /// After processing the current frame, reset the delta values (position delta, wheel delta, etc.) to zero.<br/>
    /// This is typically called at the end of each frame update to prepare for the next frame.
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// Thrown if the method is not implemented by the current used service.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    void ResetDeltas();
}