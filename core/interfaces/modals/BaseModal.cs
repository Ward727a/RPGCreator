using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.interfaces.modals
{
    internal abstract class BaseModal(GraphicsDevice graphics, InterfacesMain parent) : InterfacesComponent(graphics, parent)
    {
        // --- --- --- VARS --- --- --- //

        public bool IsOpened;

        // --- --- --- EVENTS --- --- --- //

        public event EventHandler OnOpen;
        public event EventHandler OnClose;

        // --- --- --- FUNCTS --- --- --- ///
         
        public abstract void ShowModal();
        public abstract void CloseModal();

        protected void _OnOpen()
        {
            OnOpen?.Invoke(this, EventArgs.Empty);
        }

        protected void _OnClose()
        {
            OnClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
