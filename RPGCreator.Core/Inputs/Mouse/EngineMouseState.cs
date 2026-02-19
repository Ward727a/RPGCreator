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

using RPGCreator.SDK.Editor.Rendering;
using RPGCreator.SDK.GlobalState;
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.Logging;
using Serilog;
using MouseButton = RPGCreator.SDK.Inputs.MouseButton;
using Vector2 = System.Numerics.Vector2;

namespace RPGCreator.Core.Inputs.Mouse
{
    public class EngineMouseState : BaseState, IMouseState
    {
        
        protected RawMouseData MouseState;
        protected RawMouseData PreviousMouseState;
        
        private struct ButtonClickMetadata
        {
            public double LastPressTime;
            public double LastReleaseTime;
            public Vector2 LastClickPosition;
        }

        private readonly ButtonClickMetadata[] _clickTracker = new ButtonClickMetadata[AllButtons.Length];
        
        private const double DoubleClickInterval = 300.0;
        private const float MaxDistance = 3.0f;

        public event Action<MouseButton>? ButtonDown;
        public event Action<MouseButton>? ButtonUp;
        public event Action<MouseButton>? Clicked;
        public event Action<MouseButton>? DoubleClicked;
        public event Action<Vector2>? Moved;
        public event Action<int>? WheelScrolled;
        public event Action<int>? HorizontalWheelScrolled;
        public event Action<object?>? HoveredObjectChanged;

        public int X { get; private set; }
        public int Y { get; private set; }
        public bool LeftButtonPressed { get; private set; }
        public double TimeSinceLastLeftPress { get; private set; }
        public bool LeftButtonDoublePressed { get; private set; }
        public bool LeftClicked { get; private set; }
        public bool LeftDoubleClicked { get; private set; }
        public bool RightButtonPressed { get; private set; }
        public double TimeSinceLastRightPress { get; private set; }
        public bool RightButtonDoublePressed { get; private set; }
        public bool RightClicked { get; private set; }
        public bool RightDoubleClicked { get; private set; }
        public bool MiddleButtonPressed { get; private set; }
        public double TimeSinceLastMiddlePress { get; private set; }
        public bool MiddleButtonDoublePressed { get; private set; }
        public bool MiddleClicked { get; private set; }
        public bool MiddleDoubleClicked { get; private set; }
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
            ResetFlags();
            SetPosition();
            SetButtons();
            SetWheel();
            SetCurrentObject();
        }

        protected void SetInsideWindow()
        {
            IsInsideWindow = MouseState.IsInsideWindow;
        }

        protected void SetPosition()
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
                Moved?.Invoke(DeltaPosition);
            }
        }
        protected static readonly MouseButton[] AllButtons = (MouseButton[])Enum.GetValues(typeof(MouseButton));
        public HashSet<MouseButton> PressedButtons { get; protected set; } = new();

        protected void ResetFlags()
        {
            LeftButtonDoublePressed = false;
            RightButtonDoublePressed = false;
            MiddleButtonDoublePressed = false;
            ResetClick();
            ResetDoubleClick();
        }
        
        protected void SetButtons()
        {
            var currentButtons = MouseState.Buttons;
            var previousButtons = PreviousMouseState.Buttons;


            double currentTime = DateTime.UtcNow.TimeOfDay.TotalMilliseconds;

            for (int i = 0; i < AllButtons.Length; i++)
            {
                var button = AllButtons[i];
                if (button == MouseButton.None) continue;

                bool isDown = currentButtons.HasFlag(button);
                bool wasDown = previousButtons.HasFlag(button);
                ref var meta = ref _clickTracker[i];

                if (isDown && !wasDown)
                {
                    PressedButtons.Add(button);
                    ButtonDown?.Invoke(button);

                    float distance = Vector2.Distance(Position, meta.LastClickPosition);
                    bool isWithinTime = (currentTime - meta.LastPressTime) < DoubleClickInterval;

                    if (isWithinTime && distance < MaxDistance)
                    {
                        SetDoublePressedFlag(button, true);
                        meta.LastPressTime = 0;
                    }
                    else
                    {
                        meta.LastPressTime = currentTime;
                    }
                    
                    meta.LastClickPosition = Position;
                }
                else if (!isDown && wasDown)
                {
                    PressedButtons.Remove(button);
                    ButtonUp?.Invoke(button);

                    float distance = Vector2.Distance(Position, meta.LastClickPosition);
                    bool isWithinTime = (currentTime - meta.LastReleaseTime) < 5000;

                    if (isWithinTime && distance < MaxDistance)
                    {
                        OnDoubleClick(button);
                        meta.LastReleaseTime = 0;
                    }
                    else
                    {
                        OnClick(button);
                        meta.LastReleaseTime = currentTime;
                    }
                }
            }

            UpdateTimeSinceLastPress(currentTime);
            
            LeftButtonPressed = currentButtons.HasFlag(MouseButton.Left);
            RightButtonPressed = currentButtons.HasFlag(MouseButton.Right);
            MiddleButtonPressed = currentButtons.HasFlag(MouseButton.Middle);
        }
        
        private void SetDoublePressedFlag(MouseButton button, bool value)
        {
            switch (button)
            {
                case MouseButton.Left: LeftButtonDoublePressed = value; break;
                case MouseButton.Right: RightButtonDoublePressed = value; break;
                case MouseButton.Middle: MiddleButtonDoublePressed = value; break;
            }
        }

        private void UpdateTimeSinceLastPress(double currentTime)
        {
            TimeSinceLastLeftPress = LeftButtonPressed ? 0 : currentTime - _clickTracker[0].LastPressTime;
            TimeSinceLastRightPress = RightButtonPressed ? 0 : currentTime - _clickTracker[1].LastPressTime;
            TimeSinceLastMiddlePress = MiddleButtonPressed ? 0 : currentTime - _clickTracker[2].LastPressTime;
        }

        protected void SetWheel()
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

        protected void SetCurrentObject()
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
        
        public void OnClick(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    LeftClicked = true;
                    Clicked?.Invoke(button);
                    break;
                case MouseButton.Right:
                    RightClicked = true;
                    Clicked?.Invoke(button);
                    break;
                case MouseButton.Middle:
                    MiddleClicked = true;
                    Clicked?.Invoke(button);
                    break;
            }
        }
        
        public void OnDoubleClick(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    LeftDoubleClicked = true;
                    DoubleClicked?.Invoke(button);
                    break;
                case MouseButton.Right:
                    RightDoubleClicked = true;
                    DoubleClicked?.Invoke(button);
                    break;
                case MouseButton.Middle:
                    MiddleDoubleClicked = true;
                    DoubleClicked?.Invoke(button);
                    break;
            }
        }

        public void ResetClick()
        {
            LeftClicked = false;
            RightClicked = false;
            MiddleClicked = false;
        }

        public void ResetDoubleClick()
        {
            LeftDoubleClicked = false;
            RightDoubleClicked = false;
            MiddleDoubleClicked = false;
        }

        public void ResetDeltas()
        {
            DeltaPosition = Vector2.Zero;
            WheelDelta = 0;
            HorizontalWheelDelta = 0;
        }

        public override void Reset()
        {
            // Do nothing.
        }
    }
}
