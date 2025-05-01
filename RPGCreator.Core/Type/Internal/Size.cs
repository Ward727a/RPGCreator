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

namespace RPGCreator.Core.Type.Internal
{
    public struct Size
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }
        public override string ToString()
        {
            return $"Width: {Width}, Height: {Height}";
        }

        public readonly MonoGame.Extended.Size ToMGExtendedSize()
        {
            return new MonoGame.Extended.Size(Width, Height);
        }

        public static implicit operator MonoGame.Extended.Size(Size size)
        {
            return new MonoGame.Extended.Size(size.Width, size.Height);
        }

        public static implicit operator Size(MonoGame.Extended.Size size)
        {
            return new Size(size.Width, size.Height);
        }

        public readonly Avalonia.Size ToAvaloniaSize()
        {
            return new Avalonia.Size(Width, Height);
        }
        public static implicit operator Avalonia.Size(Size size)
        {
            return new Avalonia.Size(size.Width, size.Height);
        }
        public static implicit operator Size(Avalonia.Size size)
        {
            return new Size((int)size.Width, (int)size.Height);
        }
    }
}
