#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
// 
// This file is part of RPG Creator and is distributed under the MIT License.
// You are free to use, modify, and distribute this file under the terms of the MIT License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence MIT.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence MIT.
// Voir LICENSE pour plus de détails.
// 
// Contact:
// => Mail: Ward727a@gmail.com
//    Please use this object: "RPG Creator [YourObject]"
// => Discord: ward727
// 
// For urgent inquiries, sending both an email and a message on Discord is highly recommended for a quicker response.
// 
// 
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using RPGCreator.Core.Types.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.Core.Inputs.Mouse
{
    public class EngineMouseState
    {

        protected readonly MouseStateExtended _mouseState;
        public readonly EngineMouseStateEvents Events;

        protected List<BaseDrawable> HoveredElements = [];

        #region MouseStateExtended Part
        //
        // Summary:
        //     Gets the current x-coordinate position of the mouse cursor relative to the game
        //     window.
        public int X => _mouseState.X;

        //
        // Summary:
        //     Gets the current y-coordinate position of the mouse cursor relative to the game
        //     window.
        public int Y => _mouseState.Y;

        //
        // Summary:
        //     Gets the current xy-coordinate position of the mouse cursor relative to the game
        //     window.
        public Point Position => _mouseState.Position;

        //
        // Summary:
        //     Gets a value that indicates whether the position of the mouse cursor changes
        //     between the previous and current states.
        public bool PositionChanged => _mouseState.PositionChanged;

        //
        // Summary:
        //     Gets the difference in the x-coordinate position change of the mouse between
        //     the previous and current state.
        public int DeltaX => _mouseState.DeltaX;

        //
        // Summary:
        //     Gets the difference in the y-coordinate position change of the mouse between
        //     the previous and current state.
        public int DeltaY => _mouseState.DeltaY;

        //
        // Summary:
        //     Gets the difference in the xy-coordinate position change of the mouse between
        //     the previous and curren state.
        public Point DeltaPosition => _mouseState.DeltaPosition;

        //
        // Summary:
        //     Gets the current value of the mouse scroll wheel.
        public int ScrollWheelValue => _mouseState.ScrollWheelValue;

        //
        // Summary:
        //     Gets the difference in the mouse scroll wheel value between the previous and
        //     current state.
        public int DeltaScrollWheelValue => _mouseState.DeltaScrollWheelValue;

        //
        // Summary:
        //     Gets the current state of the mouse left button.
        public ButtonState LeftButton => _mouseState.LeftButton;

        //
        // Summary:
        //     Gets the current state of the mouse middle button.
        public ButtonState MiddleButton => _mouseState.MiddleButton;

        //
        // Summary:
        //     Gets the current state of the mouse right button.
        public ButtonState RightButton => _mouseState.RightButton;

        //
        // Summary:
        //     Gets the current state of the first mouse extra button.
        public ButtonState XButton1 => _mouseState.XButton1;

        //
        // Summary:
        //     Gets the current state of the second mouse extra button.
        public ButtonState XButton2 => _mouseState.XButton2;

        //
        // Summary:
        //     Returns a value that indicates whether the specified mouse button is down during
        //     the current state.
        //
        // Parameters:
        //   button:
        //     The mouse button to check.
        //
        // Returns:
        //     true if the mouse button is down during the current state; otherwise, false.
        public bool IsButtonDown(MouseButton button)
        {
            return _mouseState.IsButtonDown(button);
        }

        //
        // Summary:
        //     Returns a value that indicates whether the specified mouse button is up during
        //     the current state.
        //
        // Parameters:
        //   button:
        //     The mouse button to check.
        //
        // Returns:
        //     true if the mouse button is up during the current state; otherwise, false.
        public bool IsButtonUp(MouseButton button)
        {
            return _mouseState.IsButtonUp(button);
        }

        //
        // Summary:
        //     Returns whether the specified mouse button was up during the previous, but is
        //     now down.
        //
        // Parameters:
        //   button:
        //     The mouse button to check.
        //
        // Returns:
        //     true if the mouse button was up pressed this state-change; otherwise, false.
        public bool WasButtonPressed(MouseButton button)
        {
            return _mouseState.WasButtonPressed(button);
        }

        //
        // Summary:
        //     Returns whether the specified mouse button was down during the previous state,
        //     but is now up.
        //
        // Parameters:
        //   button:
        //     The mouse button to check.
        //
        // Returns:
        //     true if the mouse button was released this state-change; otherwise, false.
        public bool WasButtonReleased(MouseButton button)
        {
            return _mouseState.WasButtonReleased(button);
        }
        #endregion

        internal EngineMouseState(MouseStateExtended mouseState)
        {
            _mouseState = mouseState;
            Events = new EngineMouseStateEvents();
        }

        public EngineMouseState(EngineMouseState mouseState)
        {
            _mouseState = mouseState._mouseState;
            Events = mouseState.Events;
        }

        protected bool _IsInside(BaseDrawable _object)
        {
            if (_object == null)
                return false;

            if (!_object.IsVisible)
                return false;

            if(_object.Bounds.Contains(_mouseState.Position))
                return true;

            return false;
        }

        public bool IsInside(BaseDrawable _object)
        {
            if(_IsInside(_object))
            {
                HoveredElements.Add(_object);
                return true;
            } else
            {
                if (HoveredElements.Contains(_object))
                    HoveredElements.Remove(_object);
                return false;
            }
        }

        public bool IsButtonClicked(MouseButton button)
        {
            if (IsButtonDown(button) && WasButtonPressed(button))
                return true;
            return false;
        }
    }
}
