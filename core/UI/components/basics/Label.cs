using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using RPGCreator.core.helpers;
using RPGCreator.core.types.Math.Transform;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace RPGCreator.core.UI.components.basics
{
    class Label : BaseUI
    {
        public ALIGNEMENT_H AlignH = ALIGNEMENT_H.LEFT;
        public ALIGNEMENT_V AlignV = ALIGNEMENT_V.MIDDLE;

        BitmapFont _font = BaseContent.GetBasicFont();
        string _content;
        Color _color = Color.Black;
        Scale _textScale;
        float _textDrawScale = 1;

        public Label(string Content)
        {
            SetContent(Content);
        }

        private void MeasureTextScale()
        {
            SizeF size = _font.MeasureString(_content);
            _textScale = new Scale(size.Width * _textDrawScale, size.Height * _textDrawScale);
        }

        public void Rescale(float pixel)
        {
            _textDrawScale = pixel / _font.MeasureString(_content).Height;
            MeasureTextScale();
        }

        public override Label Init(params object[] args)
        {
            MeasureTextScale();

            return this;
        }

        public virtual Scale GetTextScale()
        {
            return _textScale;
        }

        public virtual void SetContent(string newContent)
        {
            _content = newContent;
            MeasureTextScale();
        }

        public virtual string GetContent()
        {
            return _content;
        }

        public virtual void SetColor(Color newColor)
        {
            _color = newColor;
        }
        
        public virtual Color GetColor()
        {
            return _color;
        }

        public virtual void SetFont(BitmapFont font)
        {
            _font = font;
            MeasureTextScale();
        }

        public virtual BitmapFont GetFont()
        {
            return _font;
        }

        public override float GetPositionByAlign(ALIGNEMENT_H H)
        {
            return GetPositionByAlign(GetAbsolutePosition(), H);
        }

        public override float GetPositionByAlign(ALIGNEMENT_V V)
        {
            return GetPositionByAlign(GetAbsolutePosition(), V);
        }

        public override Position GetPositionByAlign(ALIGNEMENT_H H, ALIGNEMENT_V V)
        {
            return GetPositionByAlign(GetAbsolutePosition(), H, V);
        }

        public override float GetPositionByAlign(Position position, ALIGNEMENT_H H)
        {
            if (parent == null) return 0;
            float newPositionX = (parent.GetRelativePosition()).X;

            switch (H)
            {
                case ALIGNEMENT_H.LEFT:
                    {
                        return newPositionX;
                    }
                case ALIGNEMENT_H.CENTER:
                    {
                        return newPositionX + (parent.GetScale() / 2).X + ((GetTextScale().X / 2 * -1));
                    }
                case ALIGNEMENT_H.RIGHT:
                    return newPositionX + (parent.GetScale()).X + ((GetTextScale().X * -1));
                default:
                    return newPositionX;
            }
        }
        public override float GetPositionByAlign(Position position, ALIGNEMENT_V V)
        {
            if (parent == null) return 0;
            float newPositionY = (parent.GetRelativePosition()).Y;

            switch (V)
            {
                case ALIGNEMENT_V.TOP:
                    {
                        return newPositionY;
                    }
                case ALIGNEMENT_V.MIDDLE:
                    {
                        return newPositionY + (parent.GetScale() / 2).Y + ((GetTextScale().Y / 2 * -1));
                    }
                case ALIGNEMENT_V.BOTTOM:
                    return newPositionY + (parent.GetScale()).Y + ((GetTextScale().Y * -1));
                default:
                    return newPositionY;
            }
        }

        public override Position GetPositionByAlign(Position position, ALIGNEMENT_H H, ALIGNEMENT_V V)
        {
            return new(GetPositionByAlign(position, H), GetPositionByAlign(position, V));
        }

        public override void Draw(SpriteBatchExtended _sb)
        {
            if (_sb.IsBegin)
            {
                _sb.DrawString(_font, _content, GetPositionByAlign(AlignH, AlignV).ToVector2(),
                    _color,
                    0,
                    new Vector2(),
                    _textDrawScale,
                    SpriteEffects.None,
                    0);
                return;
            }

            _sb.Begin();
            Draw(_sb);
            _sb.End();
        }
        public virtual void Draw(SpriteBatchExtended _sb, Color color)
        {
            if (_sb.IsBegin)
            {
                _sb.DrawString(_font, _content, GetPositionByAlign(AlignH, AlignV).ToVector2(),
                    color,
                    0,
                    new Vector2(),
                    _textDrawScale,
                    SpriteEffects.None,
                    0);
                return;
            }

            _sb.Begin();
            Draw(_sb, color);
            _sb.End();
        }
    }
}
