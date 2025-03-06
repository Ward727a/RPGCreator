using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.core.types.Math.Transform;
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

        public Position GetPosition()
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
            PreSetPositionArgs preArg = new(GetPosition(), position);
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

        public GameObject(params object[] args)
        {
            PreInit?.Invoke(this, this);
            ObjectName = $"Object-{ID}";
            Init(args);
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

        /// <summary>
        /// Inner function that can't be overloaded. This allow the object to call the <see cref="PreDraw"/> event.
        /// </summary>
        public void _Draw(SpriteBatch _sb)
        {
            if (multithread && !thread_running)
            {
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

        /// <summary>
        /// Inner function that can't be overloaded. This allow the object to call the <see cref="PreUpdate"/> event.
        /// </summary>
        /// <param name="gameTime">Time state of the game.</param>
        public void _Update(GameTime gameTime)
        {
            PreUpdate?.Invoke(this, gameTime);
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
