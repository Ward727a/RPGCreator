using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Shapes;
using RPGCreator.core.helpers;
using RPGCreator.core.types.Math.Transform;
using RPGCreator.core.UI.components;
using RPGCreator.core.UI.components.basics;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Label = RPGCreator.core.UI.components.basics.Label;

namespace RPGCreator.core.UI.components.buttons
{
    class BaseButton : BaseUI
    {

        public Label label = new("Button");

        private bool b_IsPressed = false;
        private bool b_IsFocused = false;
        private bool b_IsDisabled = false;

        public ALIGNEMENT_H HorizontalTextAlign = ALIGNEMENT_H.CENTER;
        public ALIGNEMENT_V VerticalTextAlign = ALIGNEMENT_V.MIDDLE;

        public Color BaseTextColor = Color.Black;
        public Color PressedTextColor = Color.White;
        public Color DisabledTextColor = Color.Gray;
        public Color FocusTextColor = Color.Black;

        public BitmapFont TextFont = BaseContent.GetBasicFont();

        public Texture2D BaseTexture;
        public Texture2D PressedTexture = null;
        public Texture2D DisabledTexture = null;
        public Texture2D FocusTexture = null;

        public event EventHandler OnPressed;
        public event EventHandler OnReleased;
        public event EventHandler OnHover;
        public event EventHandler OnUnhover;

        public event EventHandler TextChanged;

        public override BaseButton Init(params object[] args)
        {

            OnLeftMousePressed += BasePressed;
            OnLeftMouseReleased += BaseReleased;
            OnMouseEnter += BaseHover;
            OnMouseMove += BaseHover;

            label.parent = this;
            label.AlignH = HorizontalTextAlign;
            label.AlignV = VerticalTextAlign;
            label.Rescale(24);

            return this;
        }

        protected virtual void BasePressed(object sender, EventArgs e)
        {
            if (b_IsDisabled || b_IsPressed) return; 
            OnPressed?.Invoke(this, null);

            b_IsPressed = true;
        }

        protected virtual void BaseReleased(object sender, EventArgs e)
        {
            if (b_IsDisabled || !b_IsPressed) return;
            OnReleased?.Invoke(this, null);

            b_IsPressed = false;
        }

        protected virtual void BaseHover(object sender, EventArgs e)
        {
            if (b_IsDisabled) return;
            OnHover?.Invoke(this, null);
        }

        public virtual void SetText(string text)
        {
            label.SetContent(text);
            TextChanged(this, null);
        }

        public virtual bool IsPressed()
        {
            return b_IsPressed;
        }

        public virtual bool IsDisabled()
        {
            return b_IsDisabled;
        }

        public virtual bool IsFocused()
        {
            return b_IsFocused;
        }

        public virtual void SetDisable(bool state)
        {
            b_IsPressed = false;
            b_IsFocused = false;
            b_IsDisabled = true;
        }

        public override void Draw(SpriteBatchExtended _sb)
        {
            if(!b_IsDisabled)
            {
                if(!b_IsPressed)
                {
                    if(!b_IsFocused)
                    {
                        DrawBaseTexture(_sb, GetRelativePosition());
                        label.Draw(_sb, BaseTextColor);
                    } else
                    {
                        DrawFocusedTexture(_sb, GetRelativePosition());
                        label.Draw(_sb, FocusTextColor);
                    }
                } else
                {
                    DrawPressedTexture(_sb, GetRelativePosition());
                    label.Draw(_sb, PressedTextColor);
                }
            } else
            {
                DrawDisabledTexture(_sb, GetRelativePosition());
                label.Draw(_sb, DisabledTextColor);
            }
        }

        public override void DrawAt(SpriteBatchExtended _sb, Position at)
        {

            if (!b_IsDisabled)
            {
                if (!b_IsPressed)
                {
                    if (!b_IsFocused)
                    {
                        DrawBaseTexture(_sb, at);

                        /*
                         * 
                        _sb.DrawString(
                        TextFont, 
                        Text, 
                        (
                            GetPositionByAlign(
                                HorizontalTextAlign, 
                                VerticalTextAlign
                            )
                            + 
                            new Scale(
                                TextFont.MeasureString(Text).Width / 2 * -1, 
                                TextFont.MeasureString(Text).Height / 2 * -1
                            )
                        ).ToVector2(),
                        BaseTextColor, 
                        0, 
                        new Vector2(), 
                        new Vector2(1, 1),
                        SpriteEffects.None,
                        0);
                         */

                        label.Draw(_sb, BaseTextColor);
                    }
                    else
                    {
                        DrawFocusedTexture(_sb, at);
                        label.Draw(_sb, FocusTextColor);
                    }
                }
                else
                {
                    DrawPressedTexture(_sb, at);
                    label.Draw(_sb, PressedTextColor);
                }
            }
            else
            {
                DrawDisabledTexture(_sb, at);
                label.Draw(_sb, DisabledTextColor);
            }
        }

        public virtual void DrawBaseTexture(SpriteBatch _sb, Position at)
        {
            if(BaseTexture == null)
            {
                _sb.DrawRectangle(new RectangleF(at.X, at.Y, GetScale().X, GetScale().Y), Color.Gray, thickness: 10f);
                return;
            }

            _sb.Draw(BaseTexture, new Vector2(at.X, at.Y), Color.White);
        }

        public virtual void DrawPressedTexture(SpriteBatch _sb, Position at)
        {
            if(PressedTexture == null)
            {
                _sb.DrawRectangle(new RectangleF(at.X, at.Y, GetScale().X, GetScale().Y), Color.DimGray, thickness: 10f);
                return;
            }

            _sb.Draw(PressedTexture, new Vector2(at.X, at.Y), Color.White);
        }

        public virtual void DrawDisabledTexture(SpriteBatch _sb, Position at)
        {
            if (DisabledTexture == null)
            {
                _sb.DrawRectangle(new RectangleF(at.X, at.Y, GetScale().X, GetScale().Y), Color.DarkGray, thickness: 10f);
                return;
            }

            _sb.Draw(DisabledTexture, new Vector2(at.X, at.Y), Color.White);
        }

        public virtual void DrawFocusedTexture(SpriteBatch _sb, Position at)
        {
            if(FocusTexture != null)
            {
                _sb.Draw(FocusTexture, new Vector2(at.X, at.Y), Color.White);
            }
        }
    }
}
