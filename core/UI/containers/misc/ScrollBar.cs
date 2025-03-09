using Microsoft.Xna.Framework;
using MonoGame.Extended;
using RPGCreator.core.helpers;
using RPGCreator.core.UI.components;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.UI.containers.misc
{
    class ScrollBar : BaseUI
    {
        public enum SCROLLBAR_DIRECTION
        {
            VERTICAL,
            HORIZONTAL
        }

        public enum SCROLLBAR_POSITION
        {
            INNER,
            OUTER
        }

        public SCROLLBAR_POSITION ScrollBarPosition = SCROLLBAR_POSITION.INNER;
        public SCROLLBAR_DIRECTION Direction = SCROLLBAR_DIRECTION.VERTICAL;

        public float BodyScale { get; private set; } = 5;
        public float ThumbScale = 3;

        /// <summary>
        /// Define the position:<br/>
        /// - If Direction.Vertical then 0 => full top<br/>
        /// - If Direction.Horizontal then 0 => full left
        /// </summary>
        public float ThumbPosition = 0;

        public float TotalScale;
        public float DisplayScale;

        public override void Draw(SpriteBatchExtended _sb)
        {
            if(parent == null)
            {
                Log.Logger.Error($"Can't draw scrollbar {ObjectName} because it dont have any parent.");
            }
            if (parent is not ScrollContainer)
            {
                Log.Logger.Error($"Can't draw scrollbar {ObjectName} because it's parent isn't a ScrollContainer.");
                return;
            }

            float positionX;
            float positionY;
            if(Direction == SCROLLBAR_DIRECTION.VERTICAL)
            {
                positionX = (parent.GetScale().X) - BodyScale;
                if (ScrollBarPosition == SCROLLBAR_POSITION.INNER)
                {
                    positionY = 0;
                }
                else
                {
                    positionY = BodyScale;
                }
                TotalScale = ((ScrollContainer)parent).ScrollBounds.Y;
                ThumbPosition = (((ScrollContainer)parent).TotalScrollY * DisplayScale) / (TotalScale);
                DisplayScale = parent.GetScale().Y;
                if (_sb.IsBegin)
                {
                    _sb.FillRectangle(new Vector2(positionX, positionY), new SizeF(BodyScale, DisplayScale), Color.Purple);
                    float SizeRatio = (Math.Min(DisplayScale / TotalScale, 1));
                    _sb.FillRectangle(new Vector2(positionX + (BodyScale - ThumbScale)/2, ((ThumbPosition * -1) + (BodyScale - ThumbScale) / 2)), new SizeF(ThumbScale, (DisplayScale * SizeRatio) - (BodyScale - ThumbScale)), Color.RosyBrown);
                }
            } else
            {
                if (ScrollBarPosition == SCROLLBAR_POSITION.INNER)
                {
                    positionX = 0;
                }
                else
                {
                    positionX = BodyScale;
                }
                positionY = (parent.GetScale().Y) - BodyScale;
                TotalScale = ((ScrollContainer)parent).ScrollBounds.X;
                ThumbPosition = (((ScrollContainer)parent).TotalScrollX * DisplayScale) / (TotalScale);
                DisplayScale = parent.GetScale().X;
                if(_sb.IsBegin)
                {
                    _sb.FillRectangle(new Vector2(positionX, positionY), new SizeF(DisplayScale, BodyScale), Color.GreenYellow);
                    float SizeRatio = (Math.Min(DisplayScale / TotalScale, 1));
                    _sb.FillRectangle(new Vector2(((ThumbPosition*-1) + (BodyScale - ThumbScale) / 2), positionY + (BodyScale - ThumbScale)/2), new SizeF((DisplayScale * SizeRatio) - (BodyScale - ThumbScale), ThumbScale), Color.Blue);
                }
            }

        }
    }
}
