using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.interfaces
{
    internal abstract class InterfacesComponent : InterfaceBase
    {
        protected InterfaceBase Parent;

        public InterfacesComponent(GraphicsDevice device, InterfacesMain parent) : base(device)
        {
            Parent = parent;
        }
    }
}
