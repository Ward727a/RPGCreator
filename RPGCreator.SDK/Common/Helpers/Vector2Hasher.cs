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

using System.Numerics;

namespace RPGCreator.SDK.Common.Helpers;

public static class Vector2Hasher
{
    public static long ToKey(this Vector2 pos) 
        => ToKey((int)pos.X, (int)pos.Y);

    public static long ToKey(int x, int y)
    {
        return ((long)x << 32) | (uint)y;
    }

    public static Vector2 FromKey(this long key)
    {
        int x = (int)(key >> 32);
        int y = (int)(key & 0xFFFFFFFF);
        return new Vector2(x, y);
    }
}