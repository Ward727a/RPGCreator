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

using System.ComponentModel;
using System.Numerics;

namespace RPGCreator.SDK.RuntimeService;

public interface IChunkService : INotifyPropertyChanged, INotifyPropertyChanging, IDisposable, IService
{
    
    event Action<long>? OnChunkLoaded;
    event Action<long>? OnChunkUnloaded;
    
    /// <summary>
    /// Read-only collection of currently active (loaded) chunk IDs.<br/>
    /// As chunks are very sensible, we just return a read-only collection to avoid any modification from outside.<br/>
    /// This collection is updated automatically based on the camera position and the loading/unloading logic defined in the service.
    /// </summary>
    public IReadOnlyCollection<long> ActiveChunks { get; }
    
    /// <summary>
    /// The number of chunks around the camera to load as buffer.
    /// </summary>
    public const int ChunkLoadDistance = 2;
    /// <summary>
    /// The distance in chunks from the camera to unload chunks.
    /// </summary>
    public const int ChunkUnloadDistance = 3;
    
    /// <summary>
    /// If true, the loading of chunks is frozen and will not update based on the camera position.<br/>
    /// This can be useful for performance optimization or when the game is paused.
    /// </summary>
    bool IsLoadFrozen { get; }
    
    /// <summary>
    /// Allow to update the loaded chunks based on the camera position.<br/>
    /// It will load all chunks visible by the camera, load 2 chunk around for buffer, and unload chunks that are at least 3 chunks away from the camera.
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// Thrown if the method is not implemented by the current used service.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    void UpdateLoadedChunk();
    
    /// <summary>
    /// This method clears all currently loaded chunks from memory.<br/>
    /// It is useful when switching maps or resetting the game state to ensure that no residual chunks remain loaded.
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// Thrown if the method is not implemented by the current used service.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    void ClearLoadedChunks();
    
    /// <summary>
    /// This method forces a reload of all chunks for the current map.<br/>
    /// It is useful when changes have been made to the map data that require all chunks to be refreshed in memory.<br/>
    /// What is doing is kinda stupid: ClearLoadedChunks THEN UpdateLoadedChunk.
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// Thrown if the method is not implemented by the current used service.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    void ReloadAllChunks();
    
    /// <summary>
    /// This method returns the world position of the given chunk ID.<br/>
    /// World position is the position in the game world with the camera at (0,0).<br/>
    /// It is useful for converting chunk IDs to actual positions in the game world for rendering or logic purposes.<br/>
    /// <br/>
    /// Be aware that even if the chunk is not loaded, this method will still return its world position based on its ID.<br/>
    /// If the chunk ID is invalid, it will return <see cref="Vector2.Zero"/>.
    /// </summary>
    /// <param name="chunkId">The ID of the chunk to get the world position for.</param>
    /// <returns>
    /// The world position of the specified chunk, or <see cref="Vector2.Zero"/> if the chunk ID is invalid.
    /// </returns>
    /// <exception cref="NotImplementedException">
    /// Thrown if the method is not implemented by the current used service.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    Vector2 GetChunkWorldPosition(long chunkId);
    
    /// <summary>
    /// This method returns the bounds of the visible chunks based on the current camera position and zoom level.<br/>
    /// The returned values are the minimum and maximum X and Y coordinates of the chunks that are currently visible on the screen.<br/>
    /// This is useful for optimizing rendering and processing by only focusing on the chunks that are within the camera's view.
    /// </summary>
    /// <param name="padding">
    /// Defines an additional padding (in chunks) to include around the visible area.<br/>
    /// This can be useful to preload chunks that are just outside the visible area to ensure smooth transitions when the camera moves.<br/>
    /// Default is 1 chunk of padding.
    /// </param>
    /// <returns>
    /// A tuple containing the minimum and maximum X and Y coordinates of the visible chunks:<br/>
    /// - minX: The minimum X coordinate of the visible chunks.<br/>
    /// - maxX: The maximum X coordinate of the visible chunks.<br/>
    /// - minY: The minimum Y coordinate of the visible chunks.<br/>
    /// - maxY: The maximum Y coordinate of the visible chunks.<br/>
    /// Every chunk that is inside those bounds should be rendered.
    /// </returns>
    /// <exception cref="NotImplementedException">
    /// Thrown if the method is not implemented by the current used service.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    (long minX, long maxX, long minY, long maxY) GetVisibleChunkBounds(int padding = 1);
    
    /// <summary>
    /// Freeze the loading of chunks, preventing any updates based on camera movement.<br/>
    /// This is useful for performance optimization or when the game is paused.<br/>
    /// To resume normal chunk loading behavior, call <see cref="UnfreezeLoading"/>.
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// Thrown if the method is not implemented by the current used service.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    void FreezeLoading();
    /// <summary>
    /// Unfreeze the loading of chunks, allowing updates based on camera movement to resume.<br/>
    /// This will re-enable the automatic loading and unloading of chunks as the camera moves.<br/>
    /// To pause chunk loading again, call <see cref="FreezeLoading"/>.
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// Thrown if the method is not implemented by the current used service.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    void UnfreezeLoading();
    
    /// <summary>
    /// This method draws debug information for all currently loaded chunks.<br/>
    /// It is useful for developers to visualize chunk boundaries.
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// Thrown if the method is not implemented by the current used service.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    void DebugDrawLoadedChunks();
    
    /// <summary>
    /// This method draws a grid representing all items <b>EMPLACEMENT</b> in the chunks.<br/>
    /// It is useful for developers to visualize where items can be placed within the chunks.
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// Thrown if the method is not implemented by the current used service.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    void DebugDrawChunkItemsGrid();
    
    /// <summary>
    /// This method draws all actual items present in the loaded chunks for debugging purposes.<br/>
    /// It helps developers visualize the place and state of items within the chunks.<br/>
    /// It is different from <see cref="DebugDrawChunkItemsGrid"/>, which shows ALL possible item placements, while this method shows only the items that are actually present.
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// Thrown if the method is not implemented by the current used service.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    void DebugDrawChunkActualItems();
}