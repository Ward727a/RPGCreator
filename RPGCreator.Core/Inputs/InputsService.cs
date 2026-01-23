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

using CommunityToolkit.HighPerformance;
using RPGCreator.SDK;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.Logging;

namespace RPGCreator.Core.Inputs;

public class InputsService : IInputsService
{
    private static readonly ScopedLogger Logger = SDK.Logging.Logger.ForContext<InputsService>();
    
    private readonly List<InputTrigger> _triggerBinding = new();

    private readonly Dictionary<string, BindedAction> _bindedActions = new();

    public void Update(IKeyboardState? keyboardState = null, IMouseState? mouseState = null)
    {
        if(_triggerBinding.Count <= 0 || _bindedActions.Count <= 0)
            return;
            
        keyboardState ??= EngineStates.KeyboardState;
        mouseState ??= EngineStates.MouseState;
        
        var ctrlHeld = keyboardState.IsKeyPressed(KeyboardKeys.LeftControl) || keyboardState.IsKeyPressed(KeyboardKeys.RightControl);
        var altHeld = keyboardState.IsKeyPressed(KeyboardKeys.LeftAlt) || keyboardState.IsKeyPressed(KeyboardKeys.RightAlt);
        var shiftHeld = keyboardState.IsKeyPressed(KeyboardKeys.LeftShift) || keyboardState.IsKeyPressed(KeyboardKeys.RightShift);

        var scrollDelta = mouseState.WheelDelta;
        
        // Avoiding allocations
        // This also block any modification during the iteration
        var triggerSpan = _triggerBinding.AsSpan();
        
        foreach (var trigger in triggerSpan)
        {
            var actionName = trigger.ActionName;
            if (actionName == null)
                continue;
            
            if (!_bindedActions.TryGetValue(actionName, out var bindedAction))
                continue;

            if (bindedAction.ShouldCtrlBeHeld != ctrlHeld || bindedAction.ShouldAltBeHeld != altHeld || bindedAction.ShouldShiftBeHeld != shiftHeld)
                continue;

            var isTriggered = trigger switch
            {
                KeyTrigger keyTrigger => bindedAction.IsContinuous?
                keyboardState.IsKeyPressed(keyTrigger.Key)
                : keyboardState.WasKeyJustPressed(keyTrigger.Key)
                ,
                MouseTrigger mouseTrigger => bindedAction.IsContinuous ?
                mouseState.IsButtonPressed(mouseTrigger.Button)
                : mouseState.WasButtonJustPressed(mouseTrigger.Button),
                ScrollTrigger scrollTrigger => scrollTrigger.Type switch
                {
                    ScrollType.Up => scrollDelta > 0,
                    ScrollType.Down => scrollDelta < 0,
                    _ => false
                },
                _ => false
            };
            
            if (isTriggered)
            {
                bindedAction.Action.Invoke();
            }
        }
    }

    public bool RegisterAction(string actionName, Action action, bool isContinuous = false, bool shouldCtrlBeHeld = false, bool shouldAltBeHeld = false, bool shouldShiftBeHeld = false,  bool overrideIfExists = false)
    {
        if(_bindedActions.ContainsKey(actionName) && !overrideIfExists)
            return false;

        _bindedActions[actionName] = new BindedAction(action, isContinuous, shouldCtrlBeHeld, shouldAltBeHeld, shouldShiftBeHeld);
        return true;
    }

    public bool UnregisterAction(string actionName)
    {
        return _bindedActions.Remove(actionName);
    }

    private bool PredicateKeyTrigger(InputTrigger trigger, KeyboardKeys awaitedKey, string? awaitedActionName = null)
    {
        if (trigger is not KeyTrigger keyTrigger)
            return false;
        
        if(keyTrigger.Key != awaitedKey)
            return false;
        
        if(keyTrigger.ActionName != awaitedActionName && awaitedActionName != null)
            return false;
        return true;
    }

    public bool SetBinding(KeyboardKeys key, string? actionName)
    {
        if (_triggerBinding.Exists(t => PredicateKeyTrigger(t, key, actionName)))
        {
            return UnsetBinding(key, actionName);
        }
        
        if(actionName == null)
            return false;
        
        if(!_bindedActions.ContainsKey(actionName))
            Logger.Warning("Setting a keyboard binding to an action name that is not registered: {ActionName}", args: actionName);
        
        _triggerBinding.Add(new KeyTrigger(key, actionName));
        return true;
    }

    
    private bool PredicateScrollTrigger(InputTrigger trigger, ScrollType awaitedScrollType, string? awaitedActionName = null)
    {
        if (trigger is not ScrollTrigger scrollTrigger)
            return false;
        
        if(scrollTrigger.Type != awaitedScrollType)
            return false;
        
        if(scrollTrigger.ActionName != awaitedActionName && awaitedActionName != null)
            return false;
        return true;
    }
    public bool SetBinding(ScrollType scrollType, string? actionName)
    {
        if (_triggerBinding.Exists(t => PredicateScrollTrigger(t, scrollType, actionName)))
        {
            return UnsetBinding(scrollType, actionName);
        }
        
        if(actionName == null)
            return false;
        
        if(!_bindedActions.ContainsKey(actionName))
            Logger.Warning("Setting a scroll binding to an action name that is not registered: {ActionName}", args: actionName);
        
        _triggerBinding.Add(new ScrollTrigger(scrollType, actionName));
        return true;
    }

    private bool PredicateMouseTrigger(InputTrigger trigger, MouseButton awaitedButton, string? awaitedActionName = null)
    {
        if (trigger is not MouseTrigger mouseTrigger)
            return false;
        
        if(mouseTrigger.Button != awaitedButton)
            return false;
        
        if(mouseTrigger.ActionName != awaitedActionName && awaitedActionName != null)
            return false;
        return true;
    }
    public bool SetBinding(MouseButton button, string? actionName)
    {
        if(_triggerBinding.Exists(t => PredicateMouseTrigger(t, button, actionName)))
        {
            return UnsetBinding(button, actionName);
        }
        
        if(actionName == null)
            return false;
        
        if(!_bindedActions.ContainsKey(actionName))
            Logger.Warning("Setting a mouse binding to an action name that is not registered: {ActionName}", args: actionName);
        
        _triggerBinding.Add(new MouseTrigger(button, actionName));
        return true;
    }

    public bool UnsetBinding(KeyboardKeys key, string? actionName = null)
    {
        return _triggerBinding.RemoveAll(t => PredicateKeyTrigger(t, key, actionName)) > 0;
    }

    public bool UnsetBinding(MouseButton button, string? actionName = null)
    {
        return _triggerBinding.RemoveAll(t => PredicateMouseTrigger(t, button, actionName)) > 0;
    }

    public bool UnsetBinding(ScrollType scrollType, string? actionName = null)
    {
        return _triggerBinding.RemoveAll(t => PredicateScrollTrigger(t, scrollType, actionName)) > 0;
    }
}