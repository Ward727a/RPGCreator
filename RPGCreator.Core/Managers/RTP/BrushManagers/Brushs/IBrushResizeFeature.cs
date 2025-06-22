#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
// 
// This file is part of RPG Creator and is distributed under the MIT License.
// You are free to use, modify, and distribute this file under the terms of the MIT License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence MIT.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence MIT.
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.Core.Managers.RTP.BrushManagers.Brushs
{
    public interface IBrushResizeFeature
    {

        int Step { get; } // Step size for resizing the brush
        int MaxSize { get; } // Maximum size of the brush
        int MinSize { get; } // Minimum size of the brush

        /// <summary>
        /// Resize the brush to the specified size.
        /// </summary>
        /// <param name="newSize">The new size of the brush.</param>
        void ResizeBrush(int newSize);
        /// <summary>
        /// Get the current size of the brush.
        /// </summary>
        /// <returns>The current size of the brush.</returns>
        int GetBrushSize();
    }
}
