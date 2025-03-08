using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using RPGCreator.core.controllers;
using RPGCreator.core.types;
using RPGCreator.core.types.Math.Transform;
using Serilog;
using System;

namespace RPGCreator.core.UI.components
{
    /// <summary>
    /// This is the base class for all UI Related class. This define some base properties and events.
    /// </summary>
    class BaseUI : GameObject
    {

        public enum ALIGNEMENT_H
        {
            LEFT,
            CENTER,
            RIGHT
        }
        public enum ALIGNEMENT_V
        {
            TOP,
            MIDDLE,
            BOTTOM
        }

        public event EventHandler OnFocus;
        public event EventHandler OnLostFocus;

        public override BaseUI Init(params object[] args)
        {
            return this;
        }

        public BaseUI()
        {
            CallPreInit();
            Init(null);
            CallPostInit();
        }

        public override void Update(GameTime gameTime)
        {

        }

        public virtual float GetPositionByAlign(ALIGNEMENT_H H)
        {
            switch (H)
            {
                case ALIGNEMENT_H.LEFT:
                    {
                        return GetRelativePosition().X;
                    }
                case ALIGNEMENT_H.CENTER:
                    {
                        return (GetRelativePosition() + (GetScale() / 2)).X;
                    }
                case ALIGNEMENT_H.RIGHT:
                    {
                        return (GetRelativePosition() + GetScale()).X;
                    }
                default:
                    return 0;

            }
        }

        public virtual float GetPositionByAlign(Position position, ALIGNEMENT_H H)
        {
            switch (H)
            {
                case ALIGNEMENT_H.LEFT:
                    {
                        return position.X;
                    }
                case ALIGNEMENT_H.CENTER:
                    {
                        return (position + (GetScale() / 2)).X;
                    }
                case ALIGNEMENT_H.RIGHT:
                    {
                        return (position + GetScale()).X;
                    }
                default:
                    return 0;

            }
        }
        public virtual float GetPositionByAlign(ALIGNEMENT_V V)
        {
            switch (V)
            {
                case ALIGNEMENT_V.TOP:
                    {
                        return GetRelativePosition().Y;
                    }
                case ALIGNEMENT_V.MIDDLE:
                    {
                        return (GetRelativePosition() + (GetScale() / 2)).Y;
                    }
                case ALIGNEMENT_V.BOTTOM:
                    {
                        return (GetRelativePosition() + GetScale()).Y;
                    }
                default:
                    return 0;

            }
        }
        public virtual float GetPositionByAlign(Position position, ALIGNEMENT_V V)
        {
            switch (V)
            {
                case ALIGNEMENT_V.TOP:
                    {
                        return position.Y;
                    }
                case ALIGNEMENT_V.MIDDLE:
                    {
                        return (position + (GetScale() / 2)).Y;
                    }
                case ALIGNEMENT_V.BOTTOM:
                    {
                        return (position + GetScale()).Y;
                    }
                default:
                    return 0;

            }
        }
        public virtual Position GetPositionByAlign(ALIGNEMENT_H H, ALIGNEMENT_V V)
        {
            return new(GetPositionByAlign(H), GetPositionByAlign(V));
        }
        public virtual Position GetPositionByAlign(Position position, ALIGNEMENT_H H, ALIGNEMENT_V V)
        {
            return new(GetPositionByAlign(position, H), GetPositionByAlign(position, V));
        }
    }
}
