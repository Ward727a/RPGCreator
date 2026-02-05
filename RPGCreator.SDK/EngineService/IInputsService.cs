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

using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.RuntimeService;

namespace RPGCreator.SDK.EngineService;

/// <summary>
/// Scroll types for mouse input.
/// </summary>
public enum ScrollType
{
    /// <summary>
    /// Scroll up action.
    /// </summary>
    Up,
    /// <summary>
    /// Scroll down action.
    /// </summary>
    Down,
    /// <summary>
    /// Default value, no scroll action.
    /// </summary>
    None
}

/// <summary>
/// An action that has been binded to an input.
/// </summary>
/// <param name="Action">The action to be executed.</param>
/// <param name="IsContinuous">Indicates if the action is continuous (called while the input is held down).</param>
public record struct BindedAction(Action Action, bool IsContinuous = false, bool ShouldCtrlBeHeld = false, bool ShouldAltBeHeld = false, bool ShouldShiftBeHeld = false);

public abstract record InputTriggerEmpty;

/// <summary>
/// Defines an input trigger for an action.
/// </summary>
/// <param name="ActionName">The name of the action to be triggered.</param>
public abstract record InputTrigger(string ActionName) : InputTriggerEmpty;

/// <summary>
/// Triggered by a keyboard key.
/// </summary>
/// <param name="Key">The keyboard key that triggers the action.</param>
/// <param name="ActionName">The name of the action to be triggered.</param>
public record KeyTrigger(KeyboardKeys Key, string ActionName) : InputTrigger(ActionName);

/// <summary>
/// Triggered by a mouse button.
/// </summary>
/// <param name="Button">The mouse button that triggers the action.</param>
/// <param name="ActionName">>The name of the action to be triggered.</param>
public record MouseTrigger(MouseButton Button, string ActionName) : InputTrigger(ActionName);

/// <summary>
/// Triggered by a scroll action.
/// </summary>
/// <param name="Type">The type of scroll that triggers the action.</param>
/// <param name="ActionName">The name of the action to be triggered.</param>
public record ScrollTrigger(ScrollType Type, string ActionName) : InputTrigger(ActionName);

public record AxisTrigger(string AxisName) : InputTriggerEmpty;

public record KeyAxisTrigger(KeyboardKeys Key, string AxisName, float Scale) : AxisTrigger(AxisName);


public record struct AxisValue()
{
    public float Value { get; set; }
    public float MinValue { get; set; }
    public float MaxValue { get; set; }
}

/// <summary>
/// A service that manages input actions and their bindings to keyboard keys and mouse buttons.<br/>
/// This allows the user (player) to customize controls.<br/>
/// If you still need to detect raw input (without actions), use the <see cref="IKeyboardState"/> and <see cref="IMouseState"/> states.
/// </summary>
public interface IInputsService : IService
{
    
    /// <summary>
    /// The update method called each frame to process input states and execute registered actions.<br/>
    /// Note that this is called automatically by the engine; you should not call it manually.<br/>
    /// Also note that in-game AND in-editor, this method is called inside the main RTP update loop (<see cref="IGameRunner"/>)
    /// </summary>
    /// <param name="keyboardState">Optional keyboard state to use for this update. If null, the current keyboard state will be used.</param>
    /// <param name="mouseState">Optional mouse state to use for this update. If null, the current mouse state will be used.</param>
    /// <exception cref="NotImplementedException">
    /// Thrown if the method is not implemented by the current used service.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    void Update(IKeyboardState? keyboardState = null, IMouseState? mouseState = null);
    
    /// <summary>
    /// Get the current value of the specified axis.<br/>
    /// Axes can be configured to use keyboard keys for positive and negative input.<br/>
    /// This allows for customizable input controls for actions like movement or camera control.
    /// </summary>
    /// <param name="axisName">The unique name of the axis.</param>
    /// <returns>
    /// The current value of the axis, ranging from -1 to 1 (or the configured min/max values).
    /// </returns>
    public float GetAxis(string axisName);
    
    /// <summary>
    /// Set the keyboard keys for the specified axis.<br/>
    /// This allows for customizable input controls for actions like movement or camera control.
    /// </summary>
    /// <param name="axisName">The unique name of the axis.</param>
    /// <param name="positiveKey">The keyboard key that represents positive input for the axis.</param>
    /// <param name="negativeKey">The keyboard key that represents negative input for the axis.</param>
    /// <param name="minValue">The minimum value of the axis when the negative key is pressed. Default is -1.</param>
    /// <param name="maxValue">The maximum value of the axis when the positive key is pressed. Default is 1.</param>
    /// <returns>
    /// The current value of the axis after setting the keys.
    /// </returns>
    public float SetAxisBinding(string axisName, KeyboardKeys positiveKey, KeyboardKeys negativeKey, float minValue = -1f, float maxValue = 1f);
    
    /// <summary>
    /// Register an action to be called when input is detected.<br/>
    /// You then need to bind keys or mouse buttons to this action to make it work.
    /// </summary>
    /// <param name="actionName">The unique name of the action.</param>
    /// <param name="action">The action to be executed when the input is detected.</param>
    /// <param name="isContinuous">Indicates if the action should be called continuously while the input is held down.</param>
    /// <param name="shouldCtrlBeHeld">Indicates if the Ctrl key should be held for this action to trigger.</param>
    /// <param name="shouldAltBeHeld">Indicates if the Alt key should be held for this action to trigger.</param>
    /// <param name="shouldShiftBeHeld">Indicates if the Shift key should be held for this action to trigger.</param>
    /// <param name="overrideIfExists">Indicates if an existing action with the same name should be overridden.</param>
    /// <returns>
    /// True if the action was successfully registered; false if an action with the same name already exists and overrideIfExists is false.
    /// </returns>
    /// <exception cref="NotImplementedException">
    /// Thrown if the method is not implemented by the current used service.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    bool RegisterAction(string actionName, Action action, bool isContinuous = false, bool shouldCtrlBeHeld = false, bool shouldAltBeHeld = false, bool shouldShiftBeHeld = false,  bool overrideIfExists = false);
    
    /// <summary>
    /// Unregister an action by its name.<br/>
    /// This will NOT remove any existing bindings to this action, but the action will no longer be called when input is detected.
    /// </summary>
    /// <param name="actionName">The unique name of the action to unregister.</param>
    /// <returns>
    /// True if the action was successfully unregistered; false if no action with the specified name exists.
    /// </returns>
    /// <exception cref="NotImplementedException">
    /// Thrown if the method is not implemented by the current used service.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    bool UnregisterAction(string actionName);
    
    /// <summary>
    /// Bind a keyboard key to a registered action.<br/>
    /// If the action does not exist, the binding will fail.
    /// </summary>
    /// <param name="key">The keyboard key to bind.</param>
    /// <param name="actionName">The unique name of the action to bind the keyboard key to. Set to null to unbind.</param>
    /// <returns>
    /// True if the binding was successful; false if the action does not exist.
    /// </returns>
    /// <exception cref="NotImplementedException">
    /// Thrown if the method is not implemented by the current used service.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    bool SetBinding(KeyboardKeys key, string? actionName);
    
    /// <summary>
    /// Bind a scroll action to a registered action.<br/>
    /// If the action does not exist, the binding will fail.
    /// </summary>
    /// <param name="scrollType">The scroll type to bind (Up, Down).</param>
    /// <param name="actionName">The unique name of the action to bind the scroll action to. Set to null to unbind.</param>
    /// <returns></returns>
    bool SetBinding(ScrollType scrollType, string? actionName);
    
    /// <summary>
    /// Bind a mouse button to a registered action.<br/>
    /// If the action does not exist, the binding will fail.
    /// </summary>
    /// <param name="button">The mouse button to bind.</param>
    /// <param name="actionName">The unique name of the action to bind the mouse button to. Set to null to unbind.</param>
    /// <returns>
    /// True if the binding was successful; false if the action does not exist.
    /// </returns>
    /// <exception cref="NotImplementedException">
    /// Thrown if the method is not implemented by the current used service.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    bool SetBinding(MouseButton button, string? actionName);

    /// <summary>
    /// Remove the binding for the specified keyboard key.
    /// </summary>
    /// <param name="key">The keyboard key to unbind.</param>
    /// <param name="actionName">The unique name of the action to unbind from the key. Set to null to unbind any action.</param>
    /// <returns>
    /// True if the binding was successfully removed; false if no binding exists for the specified key.
    /// </returns>
    /// <exception cref="NotImplementedException">
    /// Thrown if the method is not implemented by the current used service.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    bool UnsetBinding(KeyboardKeys key, string? actionName = null);
    
    /// <summary>
    /// Remove the binding for the specified mouse button.
    /// </summary>
    /// <param name="button">The mouse button to unbind.</param>
    /// <param name="actionName">The unique name of the action to unbind from the button. Set to null to unbind any action.</param>
    /// <returns>
    /// True if the binding was successfully removed; false if no binding exists for the specified button.
    /// </returns>
    /// <exception cref="NotImplementedException">
    /// Thrown if the method is not implemented by the current used service.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    bool UnsetBinding(MouseButton button, string? actionName = null);
    
    /// <summary>
    /// Remove the binding for the specified scroll action.
    /// </summary>
    /// <param name="scrollType">The scroll type to unbind.</param>
    /// <param name="actionName">The unique name of the action to unbind from the scroll type. Set to null to unbind any action.</param>
    /// <returns>
    /// True if the binding was successfully removed; false if no binding exists for the specified button.
    /// </returns>
    /// <exception cref="NotImplementedException">
    /// Thrown if the method is not implemented by the current used service.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    bool UnsetBinding(ScrollType scrollType, string? actionName = null);

    /// <summary>
    /// Reset all input axes to their default state.<br/>
    /// This will set all axis values to zero and clear any key bindings associated with them.
    /// </summary>
    public void ResetInputAxis();
}