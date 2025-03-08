using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Input;
using RPGCreator.core.helpers;
using RPGCreator.core.types.Math.Transform;
using RPGCreator.core.UI.components;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.UI.containers
{
    class ScrollContainer : SimpleContainer
    {

        public bool allowXScroll = false;
        public bool allowYScroll = false;

        public int scrollValueX = 0;
        public int scrollValueY = 0;
        public ScrollContainer(Scale scale) : base(scale)
        {
        }

        public override void Update(GameTime gameTime)
        {
            if(IsMouseInside())
            {
                ScrollEvent();
            }
            base.Update(gameTime);
        }

        public override void DrawContents(SpriteBatchExtended _sb)
        {
            _sb.Begin();

            foreach (BaseUI child in Childs)
            {
                Position position = child.GetRelativePosition();
                position.Y += scrollValueY;
                position.X += scrollValueX;
                child.SetPosition(position);
                child._Draw(_sb);
            }

            scrollValueY = 0;
            scrollValueX = 0;
            _sb.End();
        }

        public virtual void ScrollEvent()
        {
            if(MouseExtended.GetState().DeltaScrollWheelValue != 0)
            {
                int scrollValue = Math.Clamp(MouseExtended.GetState().DeltaScrollWheelValue / 120, -1, 1);
                Log.Logger.Verbose($"Scrolled {scrollValue}");
                if(KeyboardExtended.GetState().IsShiftDown() && allowXScroll)
                {
                    scrollValueX += 5 * scrollValue;
                    return;
                }

                if (allowYScroll) scrollValueY += 5 * scrollValue;
            }
        }
    }
}
