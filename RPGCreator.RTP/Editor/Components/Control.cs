using Microsoft.Xna.Framework;
using System;

namespace RPGCreator.RTP.Editor.Components
{
    public abstract class Control
    {

        #region events

        public event EventHandler<EventArgs> PositionChanged;
        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> VisibleChanged;

        #endregion

        public Control Child { get; set; }

        protected Point _position;
        public Point Position { get => _position; set { _position = value; PositionChanged?.Invoke(this, EventArgs.Empty); } }

        protected bool _enabled = true;
        public bool Enabled { get => _enabled; set { _enabled = value; EnabledChanged?.Invoke(this, EventArgs.Empty); } }

        protected bool _visible = true;
        public bool Visible { get => _visible; set { _visible = value; VisibleChanged?.Invoke(this, EventArgs.Empty); } }

        protected Control()
        { }

        public abstract void Initialize();
        
        public abstract void Update(GameTime gameTime);

        public abstract void Draw(GameTime gameTime);

    }
}
