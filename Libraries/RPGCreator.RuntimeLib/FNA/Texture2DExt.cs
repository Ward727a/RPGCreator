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

using CommunityToolkit.HighPerformance;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RPGCreator.RuntimeLib.FNA;

public static class Texture2DExt
{
    private static Texture2D? _defaultTexture;
    
    extension(Texture2D)
    {
        
        public static Texture2D FromMemory(GraphicsDevice graphicsDevice, ReadOnlyMemory<byte> data)
        {
            var value = Texture2D.FromStream(graphicsDevice, data.AsStream());
            value.Name = "FromStream";
            return value;
        }

        public static Texture2D GetDefaultTexture(GraphicsDevice graphicsDevice)
        {
            if (_defaultTexture != null)
            {
                return _defaultTexture;
            }

            _defaultTexture = new Texture2D(graphicsDevice, 128, 128);
            _defaultTexture.Name = "Default texture [DO NOT DISPOSE!]";
            
            const int width = 128;
            const int height = 128;
            const int checkerSize = 16;
    
            _defaultTexture = new Texture2D(graphicsDevice, width, height);
    
            var data = new Color[width * height];
    
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var isColorA = ((x / checkerSize) % 2) == y / checkerSize % 2;
            
                    data[y * width + x] = isColorA ? Color.Magenta : Color.Black; 
                }
            }
    
            _defaultTexture.SetData(data);
    
            return _defaultTexture;
        }
    }
}