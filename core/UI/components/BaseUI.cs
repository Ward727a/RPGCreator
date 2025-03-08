using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using RPGCreator.core.controllers;
using RPGCreator.core.types;
using Serilog;
using System;

namespace RPGCreator.core.UI.components
{
    /// <summary>
    /// This is the base class for all UI Related class. This define some base properties and events.
    /// </summary>
    class BaseUI : GameObject
    {

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
    }
}
