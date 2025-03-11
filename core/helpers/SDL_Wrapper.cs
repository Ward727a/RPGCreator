using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.helpers
{
    /// <summary>
    /// This class contains all the binding with SDL to fix some MonoGame limits.
    /// </summary>
    static partial class SDL_Wrapper
    {
        /// <summary>
        /// Allow to set a minimum size to the window.
        /// </summary>
        /// <param name="window">Window handle getted from MonoGame.</param>
        /// <param name="min_w">Minimum width</param>
        /// <param name="min_h">Minimum height</param>
        /// <example>
        /// Example code
        /// <code>
        ///     IntPtr WindowHandle = Window.Handle; 
        ///     int MinimumWidth = 1366; 
        ///     int MinimumHeight = 768;
        ///     SDL_Wrapper.SetWindowMinSize(Window.Handle, MinimumWidth, MinimumHeight);
        /// </code>
        /// </example>
        [LibraryImport("SDL2.dll", EntryPoint = "SDL_SetWindowMinimumSize")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial void SetWindowMinSize(IntPtr window, int min_w, int min_h);
    }
}
