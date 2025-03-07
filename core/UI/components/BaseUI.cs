using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using RPGCreator.core.types;
using Serilog;
using System;

namespace RPGCreator.core.UI.components
{
    /// <summary>
    /// This is the base class for all UI Related class. This define some base properties and events.
    /// </summary>
    class BaseUI : GameObject
    {
        protected bool b_MouseInside = false;
        protected MouseButton LastMousebuttonPressed = MouseButton.None;
        protected int LastPressTime = 0;

        #region MouseEvents
        public event EventHandler OnMouseEnter;
        public event EventHandler OnMouseMove;
        public event EventHandler OnMouseLeave;

        #region MouseClickEvents
        public event EventHandler OnLeftMousePressed;
        public event EventHandler OnLeftMouseClick;
        public event EventHandler OnLeftMouseReleased;
        
        public event EventHandler OnRightMousePressed;
        public event EventHandler OnRightMouseClick;
        public event EventHandler OnRightMouseReleased;

        public event EventHandler OnMiddleMousePressed;
        public event EventHandler OnMiddleMouseClick;
        public event EventHandler OnMiddleMouseReleased;
        //public event EventHandler OnLeftMouseDoubleClick;
        //public event EventHandler OnRightMouseDoubleClick;
        //public event EventHandler OnMiddleMouseDoubleClick;
        #endregion MouseClickEvents
        #endregion MouseEvents

        public event EventHandler OnFocus;
        public event EventHandler OnLostFocus;

        public override BaseUI Init(params object[] args)
        {
            return this;
        }

        public BaseUI()
        {
            CallPreInit();
            Init(null);
            CallPostInit();
        }

        protected bool IsMouseInside()
        {
            Point mousePosition = MouseExtended.GetState().Position;
            if (mousePosition.X < GetPosition().X + GetScale().X &&
                mousePosition.X > GetPosition().X &&
                mousePosition.Y < GetPosition().Y + GetScale().Y &&
                mousePosition.Y > GetPosition().Y)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void CheckMouseClickEvent(GameTime gameTime, ButtonState State, MouseButton Type, EventHandler ePressed, EventHandler eReleased, EventHandler eClicked)
        {
            if (State == ButtonState.Pressed)
            {
                ePressed?.Invoke(this, null);
                if (LastMousebuttonPressed != Type)
                {
                    LastMousebuttonPressed = Type;
                    LastPressTime = gameTime.ElapsedGameTime.Milliseconds;
                }
                else
                {
                    LastMousebuttonPressed = Type;
                    LastPressTime += gameTime.ElapsedGameTime.Milliseconds;
                }
            }
            else
            {
                if (LastPressTime <= 200 && LastMousebuttonPressed == Type)
                {
                    eClicked?.Invoke(this, null);
                    LastMousebuttonPressed = MouseButton.None;
                    LastPressTime = 0;
                }
                else if (LastMousebuttonPressed == Type)
                {
                    LastMousebuttonPressed = MouseButton.None;
                    LastPressTime = 0;
                }
                eReleased?.Invoke(this, null);
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (IsMouseInside())
            {
                if (b_MouseInside)
                {
                    if(MouseExtended.GetState().DeltaPosition != new Point(0, 0))
                    {

                        OnMouseMove?.Invoke(this, null);
                    }
                }
                else
                {
                    b_MouseInside = true;
                    OnMouseEnter?.Invoke(this, null);
                }
                CheckMouseClickEvent(gameTime, MouseExtended.GetState().LeftButton, MouseButton.Left, OnLeftMousePressed, OnLeftMouseReleased, OnLeftMouseClick);
                CheckMouseClickEvent(gameTime, MouseExtended.GetState().RightButton, MouseButton.Right, OnRightMousePressed, OnRightMouseReleased, OnRightMouseClick);
                CheckMouseClickEvent(gameTime, MouseExtended.GetState().MiddleButton, MouseButton.Middle, OnMiddleMousePressed, OnMiddleMouseReleased, OnMiddleMouseClick);
            } else
            {
                if (b_MouseInside)
                {
                    Log.Logger.Verbose("Mouse out");
                    b_MouseInside = false;

                    switch (LastMousebuttonPressed)
                    {
                        case MouseButton.Left:
                            {
                                OnLeftMouseReleased?.Invoke(this, null);
                            }break;
                        case MouseButton.Middle:
                            {
                                OnMiddleMouseReleased?.Invoke(this, null);
                            }break;
                        case MouseButton.Right:
                            {
                                OnRightMouseReleased?.Invoke(this, null);
                            }break;
                    }
                    LastMousebuttonPressed = MouseButton.None;
                    LastPressTime = 0;
                }
            }

        }
    }
}
