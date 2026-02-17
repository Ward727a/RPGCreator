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
using FontStashSharp;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.SDK.GameUI.Interfaces;
using RPGCreator.SDK.Types;

namespace RPGCreator.RTP.GameUI.Interface;

public interface IMgUiRendererContext : IUiRendererContext
{
    SpriteBatch SpriteBatch { get; init; }
    
    /// <summary>
    /// Draws a texture at the specified position and size, with an optional color tint. If the color is not specified, it defaults to white (no tint).
    /// </summary>
    /// <param name="position">The position where the texture should be drawn, relative to the top-left corner of the screen.</param>
    /// <param name="size">The size of the area where the texture should be drawn. The texture will be scaled to fit this area.</param>
    /// <param name="texture">The texture to be drawn.</param>
    /// <param name="color">An optional color tint to apply to the texture. If not specified, the texture will be drawn with its original colors.</param>
    void DrawTexture(Vector2 position, Vector2 size, Texture2D texture, Color? color = null);
    
    /// <summary>
    /// Draws text at the specified position with an optional color. If the color is not specified, it defaults to white (no tint).
    /// </summary>
    /// <param name="textLayout">The RichTextLayout containing the text to be drawn, along with its formatting and styling information.</param>
    /// <param name="position">The position where the text should be drawn, relative to the top-left corner of the screen.</param>
    /// <param name="color">An optional color tint to apply to the text. If not specified, the text will be drawn with its original colors.</param>
    void DrawText(RichTextLayout textLayout, Vector2 position, Color? color = null);
    
    /// <summary>
    /// Draws text at the specified position with an optional color, font size, and font.<br/>
    /// If the color is not specified, it defaults to white (no tint).<br/>
    /// If the font is not specified, it defaults to the default font of the UI system.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="position"></param>
    /// <param name="color"></param>
    /// <param name="fontSize"></param>
    /// <param name="font"></param>
    void DrawText(string text, Vector2 position, Color? color = null, int fontSize = 16, SpriteFontBase? font = null);

    /// <summary>
    /// Draws a sprite from a texture atlas at the specified position and size, with an optional color tint.<br/>
    /// If the color is not specified, it defaults to white (no tint).<br/>
    /// The sprite to be drawn is determined by the spriteIndex, which corresponds to a specific region of the atlasTexture.<br/>
    /// For this to work, the atlasTexture must be organized in a grid of sprites of equal size, and the spriteIndex should be calculated based on the position of the desired sprite in that grid.
    /// </summary>
    /// <param name="position">The position where the sprite should be drawn, relative to the top-left corner of the screen.</param>
    /// <param name="size">
    ///     The size of the area where the sprite should be drawn.<br/>
    ///     The sprite will be scaled to fit this area.</param>
    /// <param name="spriteIndex">
    ///     The index of the sprite to be drawn from the atlasTexture.<br/>
    ///     This index is typically calculated based on the position of the sprite in a grid layout within the atlasTexture.<br/>
    ///     For example, if the atlasTexture contains sprites arranged in rows and columns, the spriteIndex can be calculated as <b>(row * numberOfColumns + column)</b>.</param>
    /// <param name="spriteSize">
    ///     The size of each individual sprite in the atlasTexture.<br/>
    ///     This is necessary to calculate the correct source rectangle for the spriteIndex when drawing from the atlasTexture.<br/>
    ///     For example, if each sprite in the atlasTexture is 32x32 pixels, then spriteSize would be (32, 32).
    /// </param>
    /// <param name="atlasTexture">
    ///     The texture atlas containing the sprites.<br/>
    ///     This texture should be organized in a grid of sprites of equal size for the spriteIndex to work correctly.</param>
    /// <param name="color">An optional color tint to apply to the sprite. If not specified, the sprite will be drawn with its original colors.</param>
    void DrawSprite(Vector2 position, Vector2 size, int spriteIndex, Vector2 spriteSize, Texture2D atlasTexture, Color? color = null);
    
    /// <summary>
    /// Draws a sprite from a texture atlas at the specified position and size, using a source rectangle to determine which part of the atlasTexture to draw, with an optional color tint.<br/>
    /// If the color is not specified, it defaults to white (no tint).<br/>
    /// This method allows for more flexibility in how the sprite is drawn, as the sourceRect can be used to specify any arbitrary region of the atlasTexture, rather than being limited to a grid of sprites of equal size.<br/>
    /// The sourceRect is defined in the coordinate space of the atlasTexture, where (0,0) is the top-left corner of the texture and (textureWidth, textureHeight) is the bottom-right corner of the texture.
    /// </summary>
    /// <param name="position">The position where the sprite should be drawn, relative to the top-left corner of the screen.</param>
    /// <param name="size">The size of the area where the sprite should be drawn. The sprite will be scaled to fit this area.</param>
    /// <param name="sourceRect">The rectangle defining the region of the atlasTexture to be drawn, in the coordinate space of the atlasTexture.</param>
    /// <param name="atlasTexture">The texture atlas containing the sprite to be drawn.</param>
    /// <param name="color">An optional color tint to apply to the sprite. If not specified, the sprite will be drawn with its original colors.</param>
    void DrawSprite(Vector2 position, Vector2 size, Rect sourceRect, Texture2D atlasTexture, Color? color = null);
    
    /// <summary>
    /// Draws a nine-slice sprite using the specified texture and nine-slice information, at the given position and size, with an optional color tint.<br/>
    /// If the color is not specified, it defaults to white (no tint).<br/>
    /// Nine-slice rendering allows for scalable UI elements by dividing the texture into a 3x3 grid, where the corners are not scaled, the edges are scaled in one direction, and the center is scaled in both directions.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="size"></param>
    /// <param name="texture"></param>
    /// <param name="info"></param>
    /// <param name="color"></param>
    void DrawNineSlice(Vector2 position, Vector2 size, Texture2D texture, NineSliceInfo info, Color? color = null);
    
    /// <summary>
    /// Draws a three-slice sprite using the specified texture and three-slice information, at the given position and size, with an optional color tint.<br/>
    /// If the color is not specified, it defaults to white (no tint).<br/>
    /// Three-slice rendering is similar to nine-slice rendering, but it divides the texture into a 1x3 grid (for horizontal slicing) or a 3x1 grid (for vertical slicing).
    /// </summary>
    /// <param name="position">The position where the three-slice sprite should be drawn, relative to the top-left corner of the screen.</param>
    /// <param name="size">The size of the area where the three-slice sprite should be drawn. The sprite will be scaled to fit this area.</param>
    /// <param name="texture">The texture containing the three-slice sprite. This texture should be organized in a grid of three sections, either horizontally or vertically, depending on the value of isHorizontal.</param>
    /// <param name="info">The ThreeSliceInfo struct containing the information about how to divide the texture into three sections for rendering.</param>
    /// <param name="isHorizontal">A boolean value indicating whether the three-slice sprite is organized horizontally (true) or vertically (false).<br/>
    /// If true, the texture is divided into three vertical sections (left, center, right) for horizontal slicing.<br/>
    /// If false, the texture is divided into three horizontal sections (top, middle, bottom) for vertical slicing.</param>
    /// <param name="color">An optional color tint to apply to the three-slice sprite. If not specified, the sprite will be drawn with its original colors.</param>
    void DrawThreeSlice(Vector2 position, Vector2 size, Texture2D texture, ThreeSliceInfo info, bool isHorizontal, Color? color = null);
}

public struct NineSliceInfo
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;

    public NineSliceInfo(int left, int top, int right, int bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }
    
    public static NineSliceInfo Uniform(int size) => new(size, size, size, size);
    public static NineSliceInfo FromCorners(int topLeft, int topRight, int bottomLeft, int bottomRight) => new(topLeft, topRight, bottomLeft, bottomRight);
    public static NineSliceInfo FromPadding(int left, int top, int right, int bottom) => new(left, top, right, bottom);
}

public struct ThreeSliceInfo
{
    public int Start;
    public int End;

    public ThreeSliceInfo(int start, int end)
    {
        Start = start;
        End = end;
    }
    
    public static ThreeSliceInfo Uniform(int size) => new(size, size);
    public static ThreeSliceInfo FromCorners(int start, int end) => new(start, end);
    public static ThreeSliceInfo FromWidth(int leftWidth, int endWidth) => new(leftWidth, endWidth);
    public static ThreeSliceInfo FromPadding(int left, int right) => new(left, right);
    public static ThreeSliceInfo FromHeight(int top, int bottom) => new(top, bottom);
}