using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using SDLWindow = System.IntPtr;
using SDL_GLContext = System.IntPtr;

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
        public enum SDL_WindowFlags
        {
            SDL_WINDOW_FULLSCREEN = 0x00000001,         /**< fullscreen window */
            SDL_WINDOW_OPENGL = 0x00000002,             /**< window usable with OpenGL context */
            SDL_WINDOW_SHOWN = 0x00000004,              /**< window is visible */
            SDL_WINDOW_HIDDEN = 0x00000008,             /**< window is not visible */
            SDL_WINDOW_BORDERLESS = 0x00000010,         /**< no window decoration */
            SDL_WINDOW_RESIZABLE = 0x00000020,          /**< window can be resized */
            SDL_WINDOW_MINIMIZED = 0x00000040,          /**< window is minimized */
            SDL_WINDOW_MAXIMIZED = 0x00000080,          /**< window is maximized */
            SDL_WINDOW_MOUSE_GRABBED = 0x00000100,      /**< window has grabbed mouse input */
            SDL_WINDOW_INPUT_FOCUS = 0x00000200,        /**< window has input focus */
            SDL_WINDOW_MOUSE_FOCUS = 0x00000400,        /**< window has mouse focus */
            SDL_WINDOW_FULLSCREEN_DESKTOP = (SDL_WINDOW_FULLSCREEN | 0x00001000),
            SDL_WINDOW_FOREIGN = 0x00000800,            /**< window not created by SDL */
            SDL_WINDOW_ALLOW_HIGHDPI = 0x00002000,      /**< window should be created in high-DPI mode if supported.
                                                     On macOS NSHighResolutionCapable must be set true in the
                                                     application's Info.plist for this to have any effect. */
            SDL_WINDOW_MOUSE_CAPTURE = 0x00004000,   /**< window has mouse captured (unrelated to MOUSE_GRABBED) */
            SDL_WINDOW_ALWAYS_ON_TOP = 0x00008000,   /**< window should always be above others */
            SDL_WINDOW_SKIP_TASKBAR = 0x00010000,   /**< window should not be added to the taskbar */
            SDL_WINDOW_UTILITY = 0x00020000,   /**< window should be treated as a utility window */
            SDL_WINDOW_TOOLTIP = 0x00040000,   /**< window should be treated as a tooltip */
            SDL_WINDOW_POPUP_MENU = 0x00080000,   /**< window should be treated as a popup menu */
            SDL_WINDOW_KEYBOARD_GRABBED = 0x00100000,   /**< window has grabbed keyboard input */
            SDL_WINDOW_VULKAN = 0x10000000,   /**< window usable for Vulkan surface */
            SDL_WINDOW_METAL = 0x20000000,   /**< window usable for Metal view */

            SDL_WINDOW_INPUT_GRABBED = SDL_WINDOW_MOUSE_GRABBED /**< equivalent to SDL_WINDOW_MOUSE_GRABBED for compatibility */
        }

        /// <summary>
        /// Allow to create a window directly from SDL.<br/>
        /// </summary>
        /// <param name="title"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        [LibraryImport("SDL2.dll", EntryPoint = "SDL_CreateWindow", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SDLWindow SDL_CreateWindow(string title, int x, int y, int w, int h, SDL_WindowFlags flags);

        [LibraryImport("SDL2.dll", EntryPoint = "SDL_GL_CreateContext", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SDL_GLContext SDL_GL_CreateContext(SDLWindow window);

        [LibraryImport("SDL2.dll", EntryPoint = "SDL_GL_MakeCurrent", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial int SDL_GL_MakeCurrent(IntPtr window, IntPtr context);

    }
}
