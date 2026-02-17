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
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.GameUI.Interfaces;

/// <summary>
/// The UI renderer context provides necessary methods and properties for rendering UI visual elements on the screen.<br/>
/// It serves as an abstraction layer between the UI system and the underlying rendering engine.<br/>
/// For example, in monogame we will an implementations of the 'DrawTexture' method that uses the monogame SpriteBatch to draw textures.
/// </summary>
/// <remarks>
/// This is a very basic interface that only includes a few methods for drawing simple shapes and managing clipping and transformations.<br/>
/// A more complete implementation of this interface can be found in the RPGCreator.RTP assembly, which includes additional methods for drawing textures, text, nine-slice sprites, and other common UI elements.<br/>
/// As the SDK is made to be as lightweight as possible, the core assembly only includes the most essential methods for rendering UI elements.<br/>
/// More advanced features are implemented in the RTP assembly to keep the core assembly free of unnecessary dependencies and to allow for more flexibility in how the rendering is implemented by different game runners.
/// </remarks>
public interface IUiRendererContext
{

    /// <summary>
    /// Execute the rendering commands that have been issued to this context.<br/>
    /// This method should be called after all drawing operations for the current frame have been issued, and it will ensure that all drawing operations are processed and rendered to the screen correctly.<br/>
    /// The exact implementation of this method will depend on the underlying rendering engine, but it typically involves flushing any pending drawing commands and presenting the rendered frame to the viewport.
    /// </summary>
    public void Execute();
    
    /// <summary>
    /// Push a clipping rectangle to the rendering context.<br/>
    /// This will restrict all subsequent drawing operations to the specified rectangle until the corresponding PopClip is called.
    /// </summary>
    /// <param name="clippingRect">The rectangle to which drawing operations should be clipped, in screen coordinates.</param>
    /// <returns>
    /// The index of the pushed clipping rectangle, which should be used to pop it later.
    /// </returns>
    public int PushClip(Rect clippingRect);
    
    /// <summary>
    /// Pop a previously pushed clipping rectangle from the rendering context, identified by its index.<br/>
    /// This will restore the previous clipping state, allowing drawing operations to affect areas outside the popped rectangle again.<br/>
    /// Be sure to pop clipping rectangles in the correct order (last pushed, first popped) or else the engine will throw an exception.<br/>
    /// For example, if you push two clipping rectangles, you must pop the second one before popping the first one.
    /// </summary>
    /// <param name="clipId">The index of the clipping rectangle to pop, as returned by the corresponding PushClip call.</param>
    public void PopClip(int clipId);
    
    /// <summary>
    /// Push a transformation matrix to the rendering context.<br/>
    /// This will apply the specified transformation to all subsequent drawing operations until the corresponding PopTransform is called.
    /// </summary>
    /// <param name="transform">The transformation matrix to apply to drawing operations. This can include translation, rotation, scaling, or any combination of these transformations.</param>
    /// <returns>
    /// The index of the pushed transformation, which should be used to pop it later.
    /// </returns>
    public int PushTransform(Matrix3x2 transform);
    
    /// <summary>
    /// Pop a previously pushed transformation from the rendering context, identified by its index.<br/>
    /// This will restore the previous transformation state, allowing drawing operations to be rendered without the popped transformation again.<br/>
    /// Be sure to pop transformations in the correct order (last pushed, first popped) or else the engine will throw an exception.<br/>
    /// For example, if you push two transformations, you must pop the second one before popping the first one.
    /// </summary>
    /// <param name="transformId">The index of the transformation to pop, as returned by the corresponding PushTransform call.</param>
    public void PopTransform(int transformId);
    
    /// <summary>
    /// Draw a rectangle outline at the specified position and size, using the given color and thickness.
    /// </summary>
    /// <param name="position">The top-left corner of the rectangle, in screen coordinates.</param>
    /// <param name="size">The width and height of the rectangle.</param>
    /// <param name="color">The color to use for the rectangle.</param>
    /// <param name="thickness">The thickness of the rectangle outline, in pixels. Default is 1 pixel. This parameter is ignored if 'filled' is true.</param>
    /// <param name="filled">Whether to draw a filled rectangle (true) or just an outline (false). Default is false (outline).</param>
    public void DrawRectangle(Vector2 position, Vector2 size, Color color, float thickness = 1f, bool filled = false);
    
    /// <summary>
    /// Draw a line between the specified start and end points, using the given color and thickness.
    /// </summary>
    /// <param name="start">The starting point of the line, in screen coordinates.</param>
    /// <param name="end">The ending point of the line, in screen coordinates.</param>
    /// <param name="color">The color to use for the line.</param>
    /// <param name="thickness">The thickness of the line, in pixels. Default is 1 pixel.</param>
    public void DrawLine(Vector2 start, Vector2 end, Color color, float thickness = 1f);
    
    /// <summary>
    /// Draw a circle outline (or filled circle if 'filled' is true) at the specified center and radius, using the given color and thickness.
    /// </summary>
    /// <param name="center">The center point of the circle, in screen coordinates.</param>
    /// <param name="radius">The radius of the circle, in pixels.</param>
    /// <param name="color">The color to use for the circle outline or fill.</param>
    /// <param name="thickness">The thickness of the circle outline, in pixels. Default is 1 pixel. This parameter is ignored if 'filled' is true.</param>
    /// <param name="segments">The number of segments to use when approximating the circle outline. Higher values will result in a smoother circle but may impact performance. Default is 16 segments.</param>
    /// <param name="filled">Whether to draw a filled circle (true) or just an outline (false). Default is false (outline).</param>
    public void DrawCircle(Vector2 center, float radius, Color color, float thickness = 1f, int segments = 16, bool filled = false);
}