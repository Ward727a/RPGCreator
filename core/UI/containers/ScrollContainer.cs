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
            if (AlignItem == ALIGN_ITEM.LEFT_TO_RIGHT)
            {
                ScrollBounds = new Scale(ScrollBounds.X + child.GetScale().X, (ScrollBounds.Y > child.GetScale().Y) ? ScrollBounds.Y : child.GetScale().Y);
            }
            else
            {
                ScrollBounds = new Scale((ScrollBounds.X > child.GetScale().X) ? ScrollBounds.X : child.GetScale().X, ScrollBounds.Y + child.GetScale().Y); // This allow to block the scroll if no element go out of bounds from the scroll container box.
            }
            child.PostSetScale += UpdateChildrenScale;
        }

        public virtual void UpdateChildrenScale(object sender, PostSetScaleArgs args)
        {
            float Max = (AlignItem == ALIGN_ITEM.LEFT_TO_RIGHT)?ScrollBounds.Y : ScrollBounds.X;
            if((AlignItem == ALIGN_ITEM.TOP_TO_BOTTOM && args.Old.X == Max && args.Old.X != args.New.X) || (AlignItem == ALIGN_ITEM.LEFT_TO_RIGHT && args.Old.Y == Max && args.Old.Y != args.New.Y)) // In this case, this means that we need to recalculate the size of X
            {
                foreach (BaseUI child in Childs)
                {
                    if ((AlignItem == ALIGN_ITEM.TOP_TO_BOTTOM && child.GetScale().X > Max) || (AlignItem == ALIGN_ITEM.LEFT_TO_RIGHT && child.GetScale().Y > Max))
                    {
                        Max = (AlignItem == ALIGN_ITEM.TOP_TO_BOTTOM)?child.GetScale().X : child.GetScale().Y;
                    }
                }
            }

            if (AlignItem == ALIGN_ITEM.TOP_TO_BOTTOM)
            {
                ScrollBounds = new Scale(Max, (ScrollBounds.Y - args.Old.Y) + args.New.Y);
            } else
            {
                ScrollBounds = new Scale((ScrollBounds.X - args.Old.X) + args.New.X, Max);
            }
        }

        public virtual void OnRemovedChildren(object sender, BaseUI child)
        {
            float Max = (AlignItem == ALIGN_ITEM.LEFT_TO_RIGHT) ? ScrollBounds.Y : ScrollBounds.X;
            if ((AlignItem == ALIGN_ITEM.TOP_TO_BOTTOM && child.GetScale().X == Max) || (AlignItem == ALIGN_ITEM.LEFT_TO_RIGHT && child.GetScale().Y == Max)) // In this case, this means that we need to recalculate the size of X
            {
                foreach (BaseUI _child in Childs)
                {
                    if ((AlignItem == ALIGN_ITEM.TOP_TO_BOTTOM && _child.GetScale().X > Max) || (AlignItem == ALIGN_ITEM.LEFT_TO_RIGHT && _child.GetScale().Y > Max))
                    {
                        Max = (AlignItem == ALIGN_ITEM.TOP_TO_BOTTOM) ? _child.GetScale().X : _child.GetScale().Y;
                    }
                }
            }

            if (AlignItem == ALIGN_ITEM.TOP_TO_BOTTOM)
            {
                ScrollBounds = new Scale(Max, (ScrollBounds.Y - child.GetScale().Y));
            }
            else
            {
                ScrollBounds = new Scale((ScrollBounds.X - child.GetScale().X), Max);
            }
        }

        public override void DrawContents(SpriteBatchExtended _sb)
        {
            _sb.Begin();

            Scale OldScale = new();
            foreach (BaseUI child in Childs)
            {
                Position position;
                if (AlignItem == ALIGN_ITEM.LEFT_TO_RIGHT)
                {
                    position = new Position(OldScale.X + TotalScrollX, TotalScrollY);
                }
                else
                {
                    position = new Position(TotalScrollX, OldScale.Y + TotalScrollY);
                }
                child.SetPosition(position);
                child._Draw(_sb);
                OldScale += child.GetScale();
            }
            _sb.End();
        }

        public virtual void ScrollEvent()
        {
            if(MouseExtended.GetState().DeltaScrollWheelValue != 0)
            {
                int scrollValue = Math.Clamp(MouseExtended.GetState().DeltaScrollWheelValue / 120, -1, 1) * -1;

                if(KeyboardExtended.GetState().IsShiftDown() && allowXScroll)
                {
                    if (ScrollBounds.X < GetScale().X)
                    {
                        Log.Logger.Verbose("Can't scroll due to not out of bounds elements.");
                        return;
                    }

                    int tempTotalX = (TotalScrollX + scrollStep * scrollValue);
                    if (tempTotalX + scrollStep * scrollValue > scrollStep) return;
                    if (tempTotalX < (ScrollBounds.X - GetScale().X)*-1)
                    {
                        Log.Logger.Verbose("Can't scroll >");
                        return;
                    }
                    TotalScrollX += scrollStep * scrollValue;
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
                if (allowYScroll) TotalScrollY += scrollStep * scrollValue;
            }
        }
    }
}
