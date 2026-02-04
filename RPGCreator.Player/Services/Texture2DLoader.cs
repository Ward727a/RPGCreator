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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.SDK.Resources;

namespace RPGCreator.Player.Services;

public class Texture2DLoader(GraphicsDevice graphicsDevice) : IResourceLoader
{
    private static Texture2D? _fakeTexture2D = null!;
    public object Load(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return FakeTexture2D(graphicsDevice);
        }
        return Texture2D.FromFile(graphicsDevice, path);
    }
    private static Texture2D FakeTexture2D(GraphicsDevice graphicsDevice)
    {
        if (_fakeTexture2D != null)
            return _fakeTexture2D;
            
        Texture2D texture = new Texture2D(graphicsDevice, 16, 16);
        Color[] data = new Color[16 * 16];
        for (int i = 0; i < data.Length; ++i) data[i] = Color.Magenta;
        texture.SetData(data);
        _fakeTexture2D = texture;
        return texture;
    }
}