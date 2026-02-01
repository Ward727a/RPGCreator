using Microsoft.Xna.Framework;
using System;

namespace RPGCreator.RTP.Editor.Components
{
    public abstract class ComplexControl : Control
    {

        public Control Parent { get; set; }

        protected ComplexControl()
        {
        }

        public ComplexControl(Control parent)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent), "Parent control cannot be null.");
        }

        public override void Draw(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        public override void Initialize()
        {
            throw new NotImplementedException();
        }

        public override void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }
    }
}
