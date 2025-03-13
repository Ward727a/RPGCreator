using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core
{
    /// <summary>
    /// This class contains all data that are "static" and that will not change on runtime.
    /// </summary>
    static class GlobalData
    {
        /// <summary>
        /// The current version of the editor.
        /// </summary>
        public static readonly Version EditorVersion = new(1, 0, 0);

    }
}
