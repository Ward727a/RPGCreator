using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.Types.Objects.UI
{
    class BaseUI : GameObject
    {
        public Texture2D Texture { get; set; }
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
            return null;
        }

        public BaseUI()
        {
            CallPreInit();
            Init(null);
            CallPostInit();
        }

        protected bool IsMouseInside()
        {
            Point mousePosition = Game1.MouseState.Position;
            if (mousePosition.X < GetPosition().X + GetScale().X &&
                mousePosition.X > GetPosition().X && 
                mousePosition.Y < GetPosition().Y + GetScale().Y &&
                mousePosition.Y > GetPosition().Y)
            {
                b_MouseInside = true;
            } else
            {
            }
            return b_MouseInside;
        }

        public override void Update(GameTime gameTime)
        {
            if (IsMouseInside())
            {
                if (b_MouseInside)
                {
                    OnMouseMove(this, null);
                }
                else
                {
                    b_MouseInside = true;
                    OnMouseEnter(this, null);
                }

                if(Game1.MouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                {
                    OnLeftMousePressed(this, null);
                    LastMousebuttonPressed = MouseButton.Left;
                    if (LastPressTime > 0)
                    {
                        LastPressTime = gameTime.ElapsedGameTime.Milliseconds;
                    }
                    else
                    {
                        LastPressTime += gameTime.ElapsedGameTime.Milliseconds;
                    }
                }
                else
                {
                    if (LastPressTime <= 200 && LastMousebuttonPressed == MouseButton.Left)
                    {
                        OnLeftMouseClick(this, null);
                    }
                    OnLeftMouseReleased(this, null);
                }
                if (Game1.MouseState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                {
                    OnRightMousePressed(this, null);
                    LastMousebuttonPressed = MouseButton.Right;
                    if (LastPressTime > 0)
                    {
                        LastPressTime = gameTime.ElapsedGameTime.Milliseconds;
                    }
                    else
                    {
                        LastPressTime += gameTime.ElapsedGameTime.Milliseconds;
                    }
                }
                else
                {
                    if (LastPressTime <= 200 && LastMousebuttonPressed == MouseButton.Right)
                    {
                        OnRightMouseClick(this, null);
                    }
                    OnRightMouseReleased(this, null);
                }
                if(Game1.MouseState.MiddleButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                {
                    OnMiddleMousePressed(this, null);
                    LastMousebuttonPressed = MouseButton.Middle;
                    if (LastPressTime > 0)
                    {
                        LastPressTime = gameTime.ElapsedGameTime.Milliseconds;
                    }
                    else
                    {
                        LastPressTime += gameTime.ElapsedGameTime.Milliseconds;
                    }
                }
                else
                {
                    if (LastPressTime <= 200 && LastMousebuttonPressed == MouseButton.Middle)
                    {
                        OnMiddleMouseClick(this, null);
                    }
                    OnMiddleMouseReleased(this, null);
                }
            }

        }
    }
}
