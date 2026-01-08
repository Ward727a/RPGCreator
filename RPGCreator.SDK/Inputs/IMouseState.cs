using System.Numerics;

namespace RPGCreator.SDK.Inputs;

public readonly record struct RawMouseData(int X, int Y, int Scroll, int HScroll, MouseButton Buttons, bool IsInside);

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

public interface IMouseState
{
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
    /// <summary>
    /// Indicates whether the right mouse button is currently pressed.
    /// </summary>
    bool RightButtonPressed { get; }
    /// <summary>
    /// Indicates whether the middle mouse button is currently pressed.
    /// </summary>
    bool MiddleButtonPressed { get; }
    
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
    /// Updates the mouse state with the provided raw mouse data.<br/>
    /// This should be automatically called by the <see cref="RPGCreator.Core"/> engine part at each <see cref="IMouseProvider"/> update.
    /// </summary>
    /// <param name="rawMouseData"></param>
    void Update(RawMouseData rawMouseData);
    
    /// <summary>
    /// Indicates whether the specified mouse button is currently pressed.
    /// </summary>
    /// <param name="buttonIndex"></param>
    /// <returns></returns>
    bool IsButtonPressed(MouseButton buttonIndex);
    /// <summary>
    /// Indicates whether the specified mouse button is currently released.
    /// </summary>
    /// <param name="buttonIndex"></param>
    /// <returns></returns>
    bool IsButtonReleased(MouseButton buttonIndex);
    /// <summary>
    /// Indicates whether the specified mouse button was pressed at the previous frame.
    /// </summary>
    /// <param name="buttonIndex"></param>
    /// <returns></returns>
    bool WasButtonPressed(MouseButton buttonIndex);
    /// <summary>
    /// Indicates whether the specified mouse button was released at the previous frame.
    /// </summary>
    /// <param name="buttonIndex"></param>
    /// <returns></returns>
    bool WasButtonReleased(MouseButton buttonIndex);
    /// <summary>
    /// Indicates whether the specified mouse button was just pressed this frame.
    /// </summary>
    /// <param name="buttonIndex"></param>
    /// <returns></returns>
    bool WasButtonJustPressed(MouseButton buttonIndex);
    /// <summary>
    /// Indicates whether the specified mouse button was just released this frame.
    /// </summary>
    /// <param name="buttonIndex"></param>
    /// <returns></returns>
    bool WasButtonJustReleased(MouseButton buttonIndex);
}