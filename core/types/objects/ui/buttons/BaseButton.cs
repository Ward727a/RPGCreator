using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Shapes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.types.objects.ui.buttons
{
    class BaseButton : BaseUI
    {

        public string Text = "Button";

        private bool b_IsPressed = false;
        private bool b_IsFocused = false;
        private bool b_IsDisabled = false;

        public Texture2D? BaseTexture;
        public Texture2D? PressedTexture = null;
        public Texture2D? DisabledTexture = null;
        public Texture2D? FocusTexture = null;

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

            return this;
        }

        protected virtual void BasePressed(object sender, System.EventArgs e)
        {
            if (b_IsDisabled || b_IsPressed) return; 
            OnPressed?.Invoke(this, null);

            b_IsPressed = true;
        }

        protected virtual void BaseReleased(object sender, System.EventArgs e)
        {
            if (b_IsDisabled || !b_IsPressed) return;
            OnReleased?.Invoke(this, null);

            b_IsPressed = false;
        }

        protected virtual void BaseHover(object sender, System.EventArgs e)
        {
            if (b_IsDisabled) return;
            OnHover?.Invoke(this, null);
        }

        public virtual void SetText(string text)
        {
            Text = text;
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

        public override void Draw(SpriteBatch _sb)
        {
            if(!b_IsDisabled)
            {
                if(!b_IsPressed)
                {
                    if(!b_IsFocused)
                    {
                        DrawBaseTexture(_sb);
                    } else
                    {
                        DrawFocusedTexture(_sb);
                    }
                } else
                {
                    DrawPressedTexture(_sb);
                }
            } else
            {
                DrawDisabledTexture(_sb);
            }
        }

        public virtual void DrawBaseTexture(SpriteBatch _sb)
        {
            if(BaseTexture == null)
            {
                _sb.DrawRectangle(new RectangleF(GetPosition().X, GetPosition().Y, GetScale().X, GetScale().Y), Microsoft.Xna.Framework.Color.Gray, thickness: 10f);
                return;
            }

            _sb.Draw(BaseTexture, new Vector2(GetPosition().X, GetPosition().Y), Microsoft.Xna.Framework.Color.White);
        }

        public virtual void DrawPressedTexture(SpriteBatch _sb)
        {
            if(PressedTexture == null)
            {
                _sb.DrawRectangle(new RectangleF(GetPosition().X, GetPosition().Y, GetScale().X, GetScale().Y), Microsoft.Xna.Framework.Color.DimGray, thickness: 10f);
                return;
            }

            _sb.Draw(PressedTexture, new Vector2(GetPosition().X, GetPosition().Y), Microsoft.Xna.Framework.Color.White);
        }

        public virtual void DrawDisabledTexture(SpriteBatch _sb)
        {
            if (DisabledTexture == null)
            {
                _sb.DrawRectangle(new RectangleF(GetPosition().X, GetPosition().Y, GetScale().X, GetScale().Y), Microsoft.Xna.Framework.Color.DarkGray, thickness: 10f);
                return;
            }

            _sb.Draw(DisabledTexture, new Vector2(GetPosition().X, GetPosition().Y), Microsoft.Xna.Framework.Color.White);
        }

        public virtual void DrawFocusedTexture(SpriteBatch _sb)
        {
            if(FocusTexture != null)
            {
                _sb.Draw(FocusTexture, new Vector2(GetPosition().X, GetPosition().Y), Microsoft.Xna.Framework.Color.White);
            }
        }
    }
}
