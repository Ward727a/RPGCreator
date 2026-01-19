#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
// 
// This file is part of RPG Creator and is distributed under the Apache 2.0 License.
// You are free to use, modify, and distribute this file under the terms of the Apache 2.0 License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence Apache 2.0.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence Apache 2.0.
// Voir LICENSE pour plus de détails.
// 
// Contact:
// => Mail: Ward727a@gmail.com
//    Please use this object: "RPG Creator [YourObject]"
// => Discord: ward727
// 
// For urgent inquiries, sending both an email and a message on Discord is highly recommended for a quicker response.
// 
// 
#endregion
using Microsoft.Xna.Framework;
using MonoGame.Extended.Input;
using RPGCreator.Core.Inputs.Mouse;
using RPGCreator.Core.Rendering.Batching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGCreator.SDK;
using RPGCreator.SDK.Inputs;

namespace RPGCreator.Core.Types.Map.Interactable
{
    // A voir si cette class est vraiment utile ou non.
    // De base cette class doit être utilisé principalement pour la map, mais bon
    // on pourrait la rendre plus générique.
    //
    // A étudier...
    public abstract class BaseInteractable : BaseDrawable
    {
        public BaseInteractableEvents Events { get; } = new();
        public IMouseState Mouse => EngineStates.MouseState;
        
        public bool Active { get; set; } = true;

        private bool WasInside = false;
        public bool CursorIn { get; private set; } = false;

        public BaseInteractable()
        {
        }


        #region MouseManagement
        protected void CheckMouse()
        {
            CursorIn = Bounds.Contains(Mouse.X, Mouse.Y);

            if(CursorIn && !WasInside)
            {
                OnMouseEnter();
            }
            else if (!CursorIn && WasInside)
            {
                OnMouseLeave();
            }
            WasInside = CursorIn;

            if (CursorIn)
            {
                if (Mouse.LeftButtonPressed)
                {
                    OnClick();
                }
            }
        }

        protected virtual void OnMouseEnter()
        {
            // Handle mouse enter event
            Console.WriteLine("Mouse entered interactable!");
            Events.OnMouseEnterEvent();
        }

        protected virtual void OnMouseLeave()
        {
            // Handle mouse leave event
            Console.WriteLine("Mouse left interactable!");
            Events.OnMouseLeaveEvent();
        }
        #endregion

        protected override void _Update(GameTime gameTime)
        {
            if (!Active) return;

            if (Bounds.Contains(Mouse.X, Mouse.Y))
            {
                if (Mouse.LeftButtonPressed)
                {
                    OnClick();
                }
            }
        }

        protected virtual void OnClick()
        {
            // Handle click event
            Console.WriteLine("Interactable clicked!");
        }
    }
}
