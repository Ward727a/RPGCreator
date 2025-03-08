using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using RPGCreator.core.controllers;
using RPGCreator.core.types.Math.Transform;
using RPGCreator.core.UI.components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.types
{
    /// <summary>
    /// Base class for all game object.<br/>
    /// By game object, all the software object (ex: UI) are included in.<br/>
    /// </summary>
    class GameObject  : BaseObject
    {

        public GameObject parent = null;

        protected bool b_MouseInside = false;
        public bool IsVisible = true;
        protected MouseButton LastMousebuttonPressed = MouseButton.None;
        protected int LastPressTime = 0;

        #region MouseEvents
        public event EventHandler OnMouseEnter;
        public event EventHandler OnMouseMove;
        public event EventHandler OnMouseLeave;

        #region DragEvents
        public event EventHandler OnDragStart;
        public event EventHandler OnDragStop;
        #endregion DragEvents

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


        protected bool multithread = false;
        protected bool thread_running = false;
        public string ObjectName = "";

        /// <summary>
        /// This structure is reserved for the <see cref="PreSetScale">PreSetScale</see> event.
        /// </summary>
        public readonly struct PreSetScaleArgs(Scale current, Scale _new)
        {
            /// <summary>
            /// Current scale of the object.
            /// </summary>
            public readonly Scale Current = current;
            /// <summary>
            /// New scale that will be applied to the object.
            /// </summary>
            public readonly Scale New = _new;
        }
        /// <summary>
        /// This structure is reserved for the <see cref="PostSetScale">PostSetScale</see> event.
        /// </summary>
        public readonly struct PostSetScaleArgs(PreSetScaleArgs pre)
        {
            /// <summary>
            /// Old scale of the object.
            /// </summary>
            public readonly Scale Old = pre.Current;
            /// <summary>
            /// New scale of the object.
            /// </summary>
            public readonly Scale New = pre.New;
        }

        /// <summary>
        /// This structure is reserved for the <see cref="PreSetPosition">PreSetPosition</see> event.
        /// </summary>
        public readonly struct PreSetPositionArgs(Position current, Position _new)
        {
            /// <summary>
            /// Current position of the object.
            /// </summary>
            public readonly Position Current = current;
            /// <summary>
            /// New position that will be applied to the object.
            /// </summary>
            public readonly Position New = _new;
        }
        /// <summary>
        /// This structure is reserved for the <see cref="PostSetPosition">PostSetPosition</see> event.
        /// </summary>
        public readonly struct PostSetPositionArgs(PreSetPositionArgs pre)
        {
            /// <summary>
            /// Old position of the object.
            /// </summary>
            public readonly Position Old = pre.Current;
            /// <summary>
            /// New position of the object.
            /// </summary>
            public readonly Position New = pre.New;
        }
        /// <summary>
        /// This structure is reserved for the <see cref="PreSetRotation">PreSetRotation</see> event.
        /// </summary>
        public readonly struct PreSetRotationArgs(Rotator current, Rotator _new)
        {
            /// <summary>
            /// Current rotation of the object.
            /// </summary>
            public readonly Rotator Current = current;
            /// <summary>
            /// New rotation that will be applied to the object.
            /// </summary>
            public readonly Rotator New = _new;
        }
        /// <summary>
        /// This structure is reserved for the <see cref="PostSetRotation">PostSetRotation</see> event.
        /// </summary>
        public readonly struct PostSetRotationArgs(PreSetRotationArgs pre)
        {
            /// <summary>
            /// Old rotation of the object.
            /// </summary>
            public readonly Rotator Old = pre.Current;
            /// <summary>
            /// New rotation of the object.
            /// </summary>
            public readonly Rotator New = pre.New;
        }

        #region Events

        /// <summary>
        /// Event sent before the object is fully created.
        /// </summary>
        public event EventHandler<GameObject> PreInit;
        /// <summary>
        /// Event sent after the object is fully created.
        /// </summary>
        public event EventHandler<GameObject> PostInit;

        /// <summary>
        /// Event sent before the object is destroyed
        /// </summary>
        public event EventHandler PreDestroy;
        /// <summary>
        /// Event sent after the object is destroyed
        /// </summary>
        public event EventHandler PostDestroy;

        /// <summary>
        /// Event sent before <see cref="Update(GameTime)"/> is called.
        /// </summary>
        public event EventHandler<GameTime> PreUpdate;
        /// <summary>
        /// Event sent after <see cref="Update(GameTime)"/> is called.
        /// </summary>
        public event EventHandler PostUpdate;

        /// <summary>
        /// Event sent before <see cref="Draw"/> is called.
        /// </summary>
        public event EventHandler PreDraw;
        /// <summary>
        /// Event sent after <see cref="Draw"/> is called.
        /// </summary>
        public event EventHandler PostDraw;

        /// <summary>
        /// Event sent before the new scale is set.
        /// </summary>
        public event EventHandler<PreSetScaleArgs> PreSetScale;
        /// <summary>
        /// Event sent after the new scale is set.
        /// </summary>
        public event EventHandler<PostSetScaleArgs> PostSetScale;

        /// <summary>
        /// Event sent before the new position is set.
        /// </summary>
        public event EventHandler<PreSetPositionArgs> PreSetPosition;
        /// <summary>
        /// Event sent after the new position is set.
        /// </summary>
        public event EventHandler<PostSetPositionArgs> PostSetPosition;

        /// <summary>
        /// Event sent before the new rotation is set.
        /// </summary>
        public event EventHandler<PreSetRotationArgs> PreSetRotation;
        /// <summary>
        /// Event sent after the new rotation is set.
        /// </summary>
        public event EventHandler<PostSetRotationArgs> PostSetRotation;
        #endregion Events

        protected Position _Position;
        protected Rotator _Rotation;
        protected Scale _Scale;

        public Game GetGame()
        {
            return Game1.Self;
        }

        /// <summary>
        /// Get the position relative directly to the screen.
        /// </summary>
        /// <returns></returns>
        public Position GetAbsolutePosition()
        {
            if(parent != null)
            {
                return parent.GetAbsolutePosition() + _Position;
            }
            return _Position;
        }

        /// <summary>
        /// Get the position relative to the parent (the direct position).
        /// </summary>
        /// <returns></returns>
        public Position GetRelativePosition()
        {
            return _Position;
        }

        public Rotator GetRotation()
        {
            return _Rotation;
        }

        public Scale GetScale()
        {
            return _Scale;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        public void SetPosition(Position position)
        {
            PreSetPositionArgs preArg = new(GetAbsolutePosition(), position);
            PreSetPosition?.Invoke(this, preArg);

            _Position = position;
            
            PostSetPositionArgs postArg = new(preArg);
            PostSetPosition?.Invoke(this, postArg);
        }

        public void SetRotation(Rotator rotation)
        {
            PreSetRotationArgs preArg = new(GetRotation(), rotation);
            PreSetRotation?.Invoke(this, preArg);

            _Rotation = rotation;
            
            PostSetRotationArgs postArg = new(preArg);
            PostSetRotation?.Invoke(this, postArg);
        }

        public void SetScale(Scale scale)
        {
            PreSetScaleArgs preArg = new(GetScale(), scale);
            PreSetScale?.Invoke(this, preArg);

            _Scale = scale;

            PostSetScaleArgs postArg = new(preArg);
            PostSetScale?.Invoke(this, postArg);
        }

        public GameObject()
        {
            PreInit?.Invoke(this, this);
            ObjectName = $"Object-{ID}";
            Init(null);
            PostInit?.Invoke(this, this);
        }

        public override string ToString()
        {
            return $"GameObject(\"{ObjectName}\")";
        }

        /// <summary>
        /// Called each game draw frame.
        /// Throw an <see cref="NotImplementedException"/> if no overload is defined.
        /// </summary>
        /// <exception cref="NotImplementedException">Throwed if no overload is defined.</exception>
        public virtual void Draw(SpriteBatch _sb)
        {
            throw new NotImplementedException("No overload defined.");
        }

        public virtual void DrawAt(SpriteBatch _sb, Position at)
        {
            throw new NotImplementedException("No overload defined.");
        }

        /// <summary>
        /// Inner function that can't be overloaded. This allow the object to call the <see cref="PreDraw"/> event.
        /// </summary>
        public void _Draw(SpriteBatch _sb)
        {
            if (!IsVisible) return;
            if (multithread)
            {
                if (thread_running) return;
                thread_running = true;
                Parallel.Invoke(
                    () =>
                    {
                        PreDraw?.Invoke(this, null);
                        Draw(_sb);
                        PostDraw?.Invoke(this, null);
                        thread_running = false;
                    });
            }
            else
            {
                PreDraw?.Invoke(this, null);
                Draw(_sb);
                PostDraw?.Invoke(this, null);
            }
        }

        public void _DrawAt(SpriteBatch _sb, Position at)
        {
            if (!IsVisible) return;
            if (multithread)
            {
                if (thread_running) return;
                thread_running = true;
                Parallel.Invoke(
                    () =>
                    {
                        PreDraw?.Invoke(this, null);
                        DrawAt(_sb, at);
                        PostDraw?.Invoke(this, null);
                        thread_running = false;
                    });
            }
            else
            {
                PreDraw?.Invoke(this, null);
                DrawAt(_sb, at);
                PostDraw?.Invoke(this, null);
            }
        }


        /// <summary>
        /// Called each game frame.<br/>
        /// Throw an <see cref="NotImplementedException"/> if no overload is defined.
        /// </summary>
        /// <param name="gameTime">Time state of the game.</param>
        /// <exception cref="NotImplementedException">Throwed if no overload is defined.</exception>
        public virtual void Update(GameTime gameTime)
        {
            throw new NotImplementedException("No overload defined.");
        }

        protected bool IsMouseInside()
        {
            if (!IsVisible) return false;

            Point mousePosition = MouseExtended.GetState().Position;

            // Check if object has parent, and in this case, check if the mouse position is inside the parent or not.
            // This allow us to not have false positive if this object is hidden inside the parent.
            if (parent != null)
            {
                if (!((BaseUI)parent).IsMouseInside())
                {
                    return false;
                }
            }

            if (mousePosition.X < GetAbsolutePosition().X + GetScale().X &&
                mousePosition.X > GetAbsolutePosition().X &&
                mousePosition.Y < GetAbsolutePosition().Y + GetScale().Y &&
                mousePosition.Y > GetAbsolutePosition().Y)
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

        public void CheckCursorPosition(GameTime gameTime)
        {
            if (IsMouseInside())
            {
                if (!b_MouseInside)
                {
                    MouseController.InObject(this);
                }
            }
            else
            {
                if (b_MouseInside)
                {
                    if (MouseController.LastObject() != this)
                    {
                        return;
                    }
                    MouseController.OutObject(this);
                }
            }
            if (IsMouseInside())
            {
                if (b_MouseInside)
                {
                    if (MouseExtended.GetState().DeltaPosition != new Point(0, 0))
                    {

                        OnMouseMove?.Invoke(this, null);
                    }
                }
                else
                {
                    //MouseController.InObject(this);
                    b_MouseInside = true;
                    OnMouseEnter?.Invoke(this, null);
                }
                CheckMouseClickEvent(gameTime, MouseExtended.GetState().LeftButton, MouseButton.Left, OnLeftMousePressed, OnLeftMouseReleased, OnLeftMouseClick);
                CheckMouseClickEvent(gameTime, MouseExtended.GetState().RightButton, MouseButton.Right, OnRightMousePressed, OnRightMouseReleased, OnRightMouseClick);
                CheckMouseClickEvent(gameTime, MouseExtended.GetState().MiddleButton, MouseButton.Middle, OnMiddleMousePressed, OnMiddleMouseReleased, OnMiddleMouseClick);
            }
            else
            {
                if (b_MouseInside)
                {
                    b_MouseInside = false;

                    switch (LastMousebuttonPressed)
                    {
                        case MouseButton.Left:
                            {
                                OnLeftMouseReleased?.Invoke(this, null);
                            }
                            break;
                        case MouseButton.Middle:
                            {
                                OnMiddleMouseReleased?.Invoke(this, null);
                            }
                            break;
                        case MouseButton.Right:
                            {
                                OnRightMouseReleased?.Invoke(this, null);
                            }
                            break;
                    }
                    LastMousebuttonPressed = MouseButton.None;
                    LastPressTime = 0;
                }
            }
        }

        /// <summary>
        /// Inner function that can't be overloaded. This allow the object to call the <see cref="PreUpdate"/> event.
        /// </summary>
        /// <param name="gameTime">Time state of the game.</param>
        public void _Update(GameTime gameTime)
        {
            PreUpdate?.Invoke(this, gameTime);
            CheckCursorPosition(gameTime);
            Update(gameTime);
            PostUpdate?.Invoke(this, null);
        }

        /// <summary>
        /// Called when the object is created.<br/>
        /// Throw an <see cref="NotImplementedException"/> if no overload is defined.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Throwed if no overload is defined</exception>
        public virtual GameObject Init(params object[] args)
        {
            throw new NotImplementedException("No overload defined.");
        }

        /// <summary>
        /// This allow child class to emit the <see cref="PreInit"/> event.
        /// </summary>
        protected void CallPreInit()
        {
            PreInit?.Invoke(this, this);
            ObjectName = $"Object-{ID}";
        }

        /// <summary>
        /// This allow child class to emit the <see cref="PostInit"/> event.
        /// </summary>
        protected void CallPostInit()
        {
            PostInit?.Invoke(this, this);
        }

        /// <summary>
        /// Called when the object need to be destroyed.<br/>
        /// Throw an <see cref="NotImplementedException"/> if no overload is defined.
        /// </summary>
        /// <exception cref="NotImplementedException">Throwed if no overload is defined</exception>
        public virtual void Destroy()
        {
            throw new NotImplementedException("No overload defined.");
        }

        ~GameObject()
        {
            PreDestroy?.Invoke(this, null);
            Destroy();
            PostDestroy?.Invoke(this, null);
        }
    }
}
