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
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Core.Rendering.Batching;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGCreator.Core.Types.Internal.LayerRenderer;

namespace RPGCreator.Core.Types.Map
{
    /// <summary>
    /// Base class for all drawable on a map (map included).
    /// </summary>
    public abstract partial class BaseDrawable : ObservableObject
    {

        public event EventHandler? Drawed;
        protected void _Drawed()
        {
            Drawed?.Invoke(this, EventArgs.Empty);
        }
        public event EventHandler? Updated;
        protected void _Updated()
        {
            Updated?.Invoke(this, EventArgs.Empty);
        }
        public event EventHandler? Moved;
        protected void _Moved()
        {
            Moved?.Invoke(this, EventArgs.Empty);
        }
        public event EventHandler? OnAddedChild;
        protected void _OnAddedChild()
        {
            OnAddedChild?.Invoke(this, EventArgs.Empty);
        }
        public event EventHandler? OnRemovedChild;
        protected void _OnRemovedChild()
        {
            OnRemovedChild?.Invoke(this, EventArgs.Empty);
        }

        [ObservableProperty]
        protected bool _IsLocked = false;

        [ObservableProperty]
        protected bool _IsVisible = true;

        [ObservableProperty]
        protected bool _IsActive = true;

        [ObservableProperty]
        protected Vector2 _Position;

        [ObservableProperty]
        protected Vector2 _Scale;

        public List<BaseDrawable> Children { get; } = [];
        public BaseDrawable? Parent { get; set; } = null;
        public bool CanHaveChildren = true;
        public bool ClipChildren = false;

        public Rectangle Bounds => new((int)Position.X, (int)Position.Y, (int)Scale.X, (int)Scale.Y);

        /// <summary>
        /// This method is used to draw the drawable on the map without a SpriteBatchExtend instance.<br/><br/>
        /// Beaware that this method will directly call the _Draw method without any SpriteBatchExtend instance.<br/>
        /// So the drawable should not rely on any SpriteBatchExtend instance to draw itself: <br/>For example, inside <see cref="TileLayer"/> with a LayerRenderer set to <see cref="AvaloniaLayerRenderer"/>).
        /// </summary>
        public void Draw()
        {
            _Draw(null);
        }
        
        public void Draw(SpriteBatchExtend sb)
        {

            bool began = sb.IsBegin;

            sb.Begin();
            _Draw(sb);

            if (!began)
            {
                sb.End();
                return;
            }

            Drawed?.Invoke(this, EventArgs.Empty);
        }
        protected abstract void _Draw(SpriteBatchExtend? sb);

        public void Update(GameTime gameTime)
        {
            _Update(gameTime);
            Updated?.Invoke(this, EventArgs.Empty);
        }

        protected abstract void _Update(GameTime gameTime);

        partial void OnIsVisibleChanged(bool value)
        {
            if (value)
            {
                OnShow();
            }
            else
            {
                OnHide();
            }
        }

        partial void OnPositionChanged(Vector2 oldValue, Vector2 newValue)
        {
            if(IsLocked)
            {
                _Position = oldValue;
            }
            else
            {
                OnMove(oldValue, newValue);
                Moved?.Invoke(this, EventArgs.Empty);
            }
        }

        partial void OnIsLockedChanged(bool oldValue, bool newValue)
        {
            OnLock(newValue);
        }

        protected virtual void OnHide() { }
        protected virtual void OnShow() { }
        protected virtual void OnMove(Vector2 oldValue, Vector2 newValue) { }
        protected virtual void OnLock(bool value) { }

        public void AddChild(BaseDrawable child)
        {
            if (child == null || !CanHaveChildren)
                return;
            child.Parent = this;
            Children.Add(child);
            OnAddedChild?.Invoke(this, EventArgs.Empty);
        }

        public void RemoveChild(BaseDrawable child)
        {
            if (child == null || !CanHaveChildren)
                return;
            child.Parent = null;
            Children.Remove(child);
            OnRemovedChild?.Invoke(this, EventArgs.Empty);
        }
    }
}
