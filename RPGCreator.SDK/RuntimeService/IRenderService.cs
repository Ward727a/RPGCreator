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

using System.Drawing;
using System.Numerics;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers.EntityLayer;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using Size = RPGCreator.SDK.Types.Size;

namespace RPGCreator.SDK.RuntimeService;

public interface IRenderService : IService
{
    /// <summary>
    /// Draws a tile at the specified world position.<br/>
    /// This method uses the provided tile definition to render the tile correctly in the game world.
    /// </summary>
    /// <param name="tileDef">The tile definition to be drawn.</param>
    /// <param name="tilePositionInChunk">The world position where the tile should be drawn.</param>
    void DrawTile(ITileDef tileDef, Vector2 tilePositionInChunk);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entityDef"></param>
    /// <param name="position"></param>
    public void DrawEntitySpawner(EntitySpawner entityDef, Vector2 position);
    

    /// <summary>
    /// Draws a tile instance at its designated world position.<br/>
    /// This method takes into account the tile instance's properties such as position, rotation, scale, and any other relevant attributes defined in the ITileInstance interface.
    /// </summary>
    /// <param name="tileInstance">The tile instance to be drawn.</param>
    void DrawTileInstance(ITileInstance tileInstance);

    /// <summary>
    /// Draws a debug rectangle at the specified world position with the given size and color.<br/>
    /// This is useful for visualizing chunks, collision boxes, and other debug information during development.
    /// </summary>
    /// <param name="worldPos">World position where the rectangle should be drawn.</param>
    /// <param name="size">Size of the rectangle to be drawn.</param>
    /// <param name="color">Color of the rectangle outline.</param>
    /// <param name="thickness">Thickness of the rectangle outline.</param>
    void DrawDebugRect(Vector2 worldPos, Size size, Color? color = null, float thickness = 2f);
    
    /// <summary>
    /// Draws a debug line between two points with specified thickness and color.<br/>
    /// This is useful for visualizing paths, directions, and other debug information during development.
    /// </summary>
    /// <param name="startPos">The starting position of the line.</param>
    /// <param name="endPos">The ending position of the line.</param>
    /// <param name="thickness">The thickness of the line.</param>
    /// <param name="color">The color of the line.</param>
    void DrawDebugLine(Vector2 startPos, Vector2 endPos, float thickness = 1f, Color? color = null);
    
    /// <summary>
    /// Draws a debug point at the specified position with given size and color.<br/>
    /// This is useful for visualizing specific points of interest during development.
    /// </summary>
    /// <param name="position">The position of the point.</param>
    /// <param name="color">The color of the point.</param>
    /// <param name="size">The size of the point.</param>
    /// <param name="thickness">The thickness of the point outline.</param>
    void DrawDebugPoint(Vector2 position, Color? color = null, float size = 4f, float thickness = 2f);
}