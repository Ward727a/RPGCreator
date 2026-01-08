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
using RPGCreator.SDK.Inputs;
using MouseButton = RPGCreator.SDK.Inputs.MouseButton;
using Vector2 = System.Numerics.Vector2;

namespace RPGCreator.Core.Inputs.Mouse
{
    public class EngineMouseState : IMouseState
    {
        
        private RawMouseData _mouseState;
        private RawMouseData _previousMouseState;
        
        public int X { get; private set; }
        public int Y { get; private set; }
        public bool LeftButtonPressed { get; private set; }
        public bool RightButtonPressed { get; private set; }
        public bool MiddleButtonPressed { get; private set; }
        public Vector2 Position { get; private set; }
        public Vector2 DeltaPosition { get; private set; }
        public int WheelDelta { get; private set; }
        public int HorizontalWheelDelta { get; private set; }
        public bool IsInsideWindow { get; private set; }

        public void Update(RawMouseData rawMouseData)
        {
            _previousMouseState = _previousMouseState == default ? rawMouseData : _mouseState;
            _mouseState = rawMouseData;
            SetInsideWindow();
            SetPosition();
            SetButtons();
            SetWheel();
        }
        
        private void SetInsideWindow()
        {
            IsInsideWindow = _mouseState.IsInside;
        }

        private void SetPosition()
        {
            X = _mouseState.X;
            Y = _mouseState.Y;
            Position = new Vector2(X, Y);
            DeltaPosition = new Vector2(
                X - _previousMouseState.X,
                Y - _previousMouseState.Y
            );
        }

        private void SetButtons()
        {
            var buttons = _mouseState.Buttons;
            LeftButtonPressed = buttons.HasFlag(MouseButton.Left);
            RightButtonPressed = buttons.HasFlag(MouseButton.Right);
            MiddleButtonPressed = buttons.HasFlag(MouseButton.Middle);
        }

        private void SetWheel()
        {
            WheelDelta = _mouseState.Scroll - _previousMouseState.Scroll;
            HorizontalWheelDelta = _mouseState.HScroll - _previousMouseState.HScroll;
        }

        public bool IsButtonPressed(MouseButton buttonIndex)
        {
            return _mouseState.Buttons.HasFlag(buttonIndex);
        }

        public bool IsButtonReleased(MouseButton buttonIndex)
        {
            return !_mouseState.Buttons.HasFlag(buttonIndex);
        }

        public bool WasButtonPressed(MouseButton buttonIndex)
        {
            return _previousMouseState.Buttons.HasFlag(buttonIndex);
        }

        public bool WasButtonReleased(MouseButton buttonIndex)
        {
            return !_previousMouseState.Buttons.HasFlag(buttonIndex);
        }

        public bool WasButtonJustPressed(MouseButton buttonIndex)
        {
            return IsButtonPressed(buttonIndex) && WasButtonReleased(buttonIndex);
        }

        public bool WasButtonJustReleased(MouseButton buttonIndex)
        {
            return IsButtonReleased(buttonIndex) && WasButtonPressed(buttonIndex);
        }
    }
}
