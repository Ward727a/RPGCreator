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
using RPGCreator.SDK.Common.Attributes;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.ECS.Components;

[Flags]
public enum SpriteEffects
{
    None = 0,
    FlipHorizontally = 1,
    FlipVertically = 2,
    FlipBoth = FlipHorizontally | FlipVertically
}

public enum ESizeMode : byte
{
    [Description("Stretch",
        "Stretches the sprite to fill the bounds rectangle, which may distort the image if the aspect ratio of the source and destination rectangles are different.")]
    Stretch,

    [Description("Fit", "Fits the sprite inside the bounds rectangle, cropping the image if necessary.")]
    Fit,

    [Description("Center", "Centers the sprite inside the bounds rectangle, without cropping.")]
    Center,
    
    [Description("Keep aspect ratio", "Keeps the aspect ratio of the sprite inside the bounds rectangle, but reduce the size of the sprite if necessary.")]
    KeepAspectRatio,

    // [Description("Tile",
    //     "Tiles the sprite inside the bounds rectangle, repeating it as necessary on both direction.")]
    // Tile,
    //
    // [Description("Tile X", "Tiles the sprite inside the bounds rectangle, repeating it on the X axis only.")]
    // TileX,
    //
    // [Description("Tile Y", "Tiles the sprite inside the bounds rectangle, repeating it on the Y axis only.")]
    // TileY,
}

public struct SpriteComponent : IComponent
{
    public Ulid SpritesheetId;
    public int CurrentFrameIndex;
    public Color Color;
    public SpriteEffects SpriteEffect;
    public float LayerDepth;
    public ESizeMode SizeMode;
    public Vector2 ScaledSize; // Set from the system
    public Vector2 Offset; // Set from the system
}