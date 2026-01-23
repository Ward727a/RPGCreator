#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
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
        
        protected RawMouseData MouseState;
        protected RawMouseData PreviousMouseState;

        public event Action<MouseButton>? ButtonDown;
        public event Action<MouseButton>? ButtonUp;
        public event Action<int, int>? Moved;
        public event Action<int>? WheelScrolled;
        public event Action<int>? HorizontalWheelScrolled;
        public event Action<object?>? HoveredObjectChanged;

        public int X { get; private set; }
        public int Y { get; private set; }
        public bool LeftButtonPressed { get; private set; }
        public bool RightButtonPressed { get; private set; }
        public bool MiddleButtonPressed { get; private set; }
        public Vector2 Position { get; private set; } = Vector2.Zero;
        public Vector2 DeltaPosition { get; private set; } = Vector2.Zero;
        public int WheelDelta { get; private set; }
        public int HorizontalWheelDelta { get; private set; }
        public bool IsInsideWindow { get; private set; }
        public object? InObject { get; private set; }

        public virtual void Update(RawMouseData rawMouseData)
        {
            PreviousMouseState = PreviousMouseState == default ? rawMouseData : MouseState;
            MouseState = rawMouseData;
            RefreshLogic();
        }

        public RawMouseData GetCurrentRawData()
        {
            return MouseState;
        }

        protected void RefreshLogic()
        {
            SetInsideWindow();
            SetPosition();
            SetButtons();
            SetWheel();
            SetCurrentObject();
        }

        private void SetInsideWindow()
        {
            IsInsideWindow = MouseState.IsInsideWindow;
        }

        private void SetPosition()
        {
            X = MouseState.X;
            Y = MouseState.Y;
            Position = new Vector2(X, Y);
            DeltaPosition = new Vector2(
                X - PreviousMouseState.X,
                Y - PreviousMouseState.Y
            );
            
            if (DeltaPosition.X != 0 || DeltaPosition.Y != 0)
            {
                Moved?.Invoke((int)DeltaPosition.X, (int)DeltaPosition.Y);
            }
        }
        private static readonly MouseButton[] AllButtons = (MouseButton[])Enum.GetValues(typeof(MouseButton));

        private void SetButtons()
        {
            var currentButtons = MouseState.Buttons;
            var previousButtons = PreviousMouseState.Buttons;

            LeftButtonPressed = currentButtons.HasFlag(MouseButton.Left);
            RightButtonPressed = currentButtons.HasFlag(MouseButton.Right);
            MiddleButtonPressed = currentButtons.HasFlag(MouseButton.Middle);
    
            for (int i = 0; i < AllButtons.Length; i++)
            {
                var button = AllButtons[i];
                if (button == MouseButton.None) continue;

                bool isDown = currentButtons.HasFlag(button);
                bool wasDown = previousButtons.HasFlag(button);

                if (isDown && !wasDown)
                {
                    ButtonDown?.Invoke(button);
                }
                else if (!isDown && wasDown)
                {
                    ButtonUp?.Invoke(button);
                }
            }
        }

        private void SetWheel()
        {
            WheelDelta = MouseState.Scroll - PreviousMouseState.Scroll;
            HorizontalWheelDelta = MouseState.HScroll - PreviousMouseState.HScroll;
            
            if (WheelDelta != 0)
            {
                WheelScrolled?.Invoke(WheelDelta);
            }
            
            if (HorizontalWheelDelta != 0)
            {
                HorizontalWheelScrolled?.Invoke(HorizontalWheelDelta);
            }
        }

        private void SetCurrentObject()
        {
            InObject = MouseState.InObject;
            
            if(PreviousMouseState.InObject != InObject)
                HoveredObjectChanged?.Invoke(InObject);
        }

        public bool IsButtonPressed(MouseButton buttonIndex)
        {
            return MouseState.Buttons.HasFlag(buttonIndex);
        }

        public bool IsButtonReleased(MouseButton buttonIndex)
        {
            return !MouseState.Buttons.HasFlag(buttonIndex);
        }

        public bool WasButtonPressed(MouseButton buttonIndex)
        {
            return PreviousMouseState.Buttons.HasFlag(buttonIndex);
        }

        public bool WasButtonReleased(MouseButton buttonIndex)
        {
            return !PreviousMouseState.Buttons.HasFlag(buttonIndex);
        }

        public bool WasButtonJustPressed(MouseButton buttonIndex)
        {
            return IsButtonPressed(buttonIndex) && WasButtonReleased(buttonIndex);
        }

        public bool WasButtonJustReleased(MouseButton buttonIndex)
        {
            return IsButtonReleased(buttonIndex) && WasButtonPressed(buttonIndex);
        }

        public void ResetDeltas()
        {
            DeltaPosition = Vector2.Zero;
            WheelDelta = 0;
            HorizontalWheelDelta = 0;
        }
    }
}
