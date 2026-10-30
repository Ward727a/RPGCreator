// RPG Creator - Open-source RPG Engine.
// (c) 2026 Ward
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


using SDL3;
using System.Runtime.InteropServices;
using RPGCreator.SDK.Resources;

namespace RPGCreator.SDK.SDLHelper
{
    public static unsafe class SdlDialogFileFilterBuilder
    {
        /// <summary>
        /// Creates a new file filter for SDL file dialog operations.
        /// </summary>
        /// <param name="name">The display name of the file filter.</param>
        /// <param name="extensions">An array of file extensions to include in the filter, formatted without wildcards or separators (e.g., "txt", "jpg").</param>
        /// <returns>Returns an initialized instance of <see cref="SDL.SDL_DialogFileFilter">SDL.SDL_DialogFileFilter</see> containing the given name and file extension pattern.</returns>
        /// <remarks>You must call <see cref="FreeFilter"/> after the SDL file dialog is closed to release the allocated memory.</remarks>
        public static SDL.SDL_DialogFileFilter CreateFilter(string name, string[] extensions)
        {
            for (int i = 0; i < extensions.Length; i++)
            {
                // We remove the leading dot from each extension if present
                // SDL already add it by default
                var extension = extensions[i];
                extension = extension.TrimStart('.');
                extensions[i] = extension;
            }
            var pattern = string.Join(";", extensions);

            return new SDL.SDL_DialogFileFilter
            {
                name = (byte*)Marshal.StringToCoTaskMemUTF8(name),
                pattern = (byte*)Marshal.StringToCoTaskMemUTF8(pattern)
            };
        }

        /// <summary>
        /// Releases the memory allocated for the specified file filter object.
        /// </summary>
        /// <param name="filter">A reference to the <c>SDL.SDL_DialogFileFilter</c> object to free. The memory associated with its <c>name</c> and <c>pattern</c> fields will be deallocated, and the fields will be set to null.</param>
        /// <remarks>Ensure that this method is called after the file dialog operation is completed to prevent memory leaks.</remarks>
        public static void FreeFilter(ref SDL.SDL_DialogFileFilter filter)
        {
            if (filter.name != null)
            {
                Marshal.FreeCoTaskMem((IntPtr)filter.name);
                filter.name = null;
            }

            if (filter.pattern != null)
            {
                Marshal.FreeCoTaskMem((IntPtr)filter.pattern);
                filter.pattern = null;
            }
        }
        
        public static SDL.SDL_DialogFileFilter ResourceTypeToFilter(IResourceManager.ResourceType type)
        {
            string pattern = string.Join(";", IResourceManager.ResourceFormats[type]);
            return new SDL.SDL_DialogFileFilter
            {
                name = (byte*)Marshal.StringToCoTaskMemUTF8(nameof(type)),
                pattern = (byte*)Marshal.StringToCoTaskMemUTF8(pattern)
            };
        }
    }
}