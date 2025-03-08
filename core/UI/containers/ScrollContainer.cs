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

        public int scrollStep = 5;

        public bool allowXScroll = false;
        public bool allowYScroll = false;

        protected Scale ScrollBounds = new();

        public int TotalScrollX = 0;
        public int TotalScrollY = 0;

        public int scrollValueX = 0;
        public int scrollValueY = 0;
        public ScrollContainer(Scale scale) : base(scale)
        {
        }

        public override ScrollContainer Init()
        {
            base.Init();

            OnAddChildren += OnAddedChildren;
            OnRemoveChildren += OnRemovedChildren;

            return this;
        }

        public override void Update(GameTime gameTime)
        {
            if(IsMouseInside())
            {
                ScrollEvent();
            }
            base.Update(gameTime);
        }

        public virtual void OnAddedChildren(object sender, BaseUI child)
        {
            ScrollBounds += child.GetScale(); // This allow to block the scroll if no element go out of bounds from the scroll container box.
        }

        public virtual void OnRemovedChildren(object sender, BaseUI child)
        {
            ScrollBounds -= child.GetScale();
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

                if(KeyboardExtended.GetState().IsShiftDown() && allowXScroll)
                {
                    if (ScrollBounds.X < GetScale().X)
                    {
                        Log.Logger.Verbose("Can't scroll due to not out of bounds elements.");
                        return;
                    }

                    int tempTotalX = (TotalScrollX + scrollStep * scrollValue);
                    if (tempTotalX > scrollStep) return;
                    if (tempTotalX > (ScrollBounds.X - GetScale().Y)*-1)
                    {
                        return;
                    }
                    scrollValueX += scrollStep * scrollValue;
                    TotalScrollX += scrollValue;
                    return;
                }
                if (ScrollBounds.Y < GetScale().Y)
                {
                    Log.Logger.Verbose("Can't scroll due to not out of bounds elements.");
                    return;
                }
                int tempTotalY = (TotalScrollY + scrollStep * scrollValue);
                if (tempTotalY + scrollStep * scrollValue > scrollStep) return;
                if (tempTotalY < (ScrollBounds.Y-GetScale().Y)*-1) {
                    return;
                }
                if (allowYScroll) scrollValueY += scrollStep * scrollValue;
                TotalScrollY += scrollValueY;
            }
        }
    }
}
