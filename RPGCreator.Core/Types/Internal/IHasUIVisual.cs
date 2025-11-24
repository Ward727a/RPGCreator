using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.Core.Types.Internal
{
    internal interface IHasUIVisual<TUI> where TUI : UIVisual
    {
        public TUI Visual { get; }
    }

    public abstract class UIVisual
    { }
}
