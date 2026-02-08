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
using RPGCreator.SDK.ECS.Entities;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.RuntimeService;

/// <summary>
/// The camera service interface for managing camera-related functionalities.<br/>
/// DevNote: The camera is an independent entity, this service allow to manage it with ease.
/// </summary>
public interface ICameraService : INotifyPropertyChanged, INotifyPropertyChanging, IDisposable, IService
{
    /// <summary>
    /// Gets the camera entity.<br/>
    /// This entity represents the camera in the game world.<br/>
    /// It can be set to other camera entities if presents, for example for cutscenes.
    /// </summary>
    int? CameraEntityId { get; }
    
    /// <summary>
    /// Gets the entity that the camera is linked to.<br/>
    /// When linked, the camera will follow the entity's position in the game world.
    /// </summary>
    int? LinkedEntityId { get; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the camera is linked to an entity.<br/>
    /// If true, the camera will follow the linked entity's position.<br/>
    /// If false, the camera can be moved independently.<br/>
    /// This allows to enable/disable the camera follow behavior without unlinking it completely.
    /// </summary>
    bool IsLinkedToEntity { get; set; }
    
    /// <summary>
    /// Gets or sets the current zoom level of the camera.
    /// </summary>
    float ZoomLevel { get; }

    /// <summary>
    /// Gets the position of the camera in the game world.<br/>
    /// The position is the coordinates of the center of the camera's view (aka your screen center).
    /// </summary>
    Vector2 Position { get; }
    
    /// <summary>
    /// Gets the offset of the camera from its default position.<br/>
    /// This represents how much the camera is moved from its original position.
    /// </summary>
    Vector2 Offset { get; }
    
    /// <summary>
    /// The size of the viewport in world units.
    /// </summary>
    Size ViewportSize { get; }

    /// <summary>
    /// Sets the camera entity.<br/>
    /// This entity represents the camera in the game world.<br/>
    /// Note: If you change the camera entity, all camera settings (position, zoom, offset) will be set to the new entity's settings except if preserveSettings is true.
    /// </summary>
    /// <param name="cameraEntity">The new camera entity to set.</param>
    /// <param name="preserveSettings">
    /// If true, preserves the current camera settings when changing the entity.<br/>
    /// >>> <b>IF TRUE</b> : THIS WILL OVERRIDE THE NEW ENTITY SETTINGS!
    /// </param>
    void SetCameraEntity(int cameraEntityId, bool preserveSettings = false);
    
    /// <summary>
    /// Resets the camera to its default position and zoom level.<br/>
    /// This will also reset any offsets applied to the camera.<br/>
    /// If linked to an entity, it will reset to the entity's position.
    /// </summary>
    void ResetCamera();
    
    /// <summary>
    /// Updates the camera's state based on the elapsed game time.<br/>
    /// Note: This should not be called manually, it's handled by the camera entity system.
    /// </summary>
    /// <param name="gameTime">The elapsed game time since the last update.</param>
    void Update(TimeSpan gameTime);
    
    /// <summary>
    /// Drags the camera by the specified delta.<br/>
    /// This moves the camera's position by the given amount.
    /// </summary>
    /// <param name="delta">The amount to drag the camera by.</param>
    void Drag(Vector2 delta);
    
    /// <summary>
    /// Moves the camera to a new position in the game world.
    /// </summary>
    /// <param name="newPosition">The new position to move the camera to.</param>
    void MoveTo(Vector2 newPosition);
    
    /// <summary>
    /// Sets the offset of the camera from its default position.<br/>
    /// This represents how much the camera is moved from its original position.<br/>
    /// For example, an offset of (10, 5) moves the camera 10 units right and 5 units down from its default position.<br/>
    /// If the camera is linked to an entity, the offset will be applied relative to the entity's position.
    /// </summary>
    /// <param name="offset">The new offset to set for the camera.</param>
    void SetOffset(Vector2 offset);
    
    /// <summary>
    /// Drags the camera's offset by the specified delta.<br/>
    /// This moves the camera's offset by the given amount.
    /// </summary>
    /// <param name="delta">The amount to drag the camera's offset by.</param>
    void DragOffset(Vector2 delta);
    
    /// <summary>
    /// Sets the zoom level of the camera.<br/>
    /// A zoom level of 1.0 represents the default zoom.<br/>
    /// Values greater than 1.0 zoom in, while values less than 1.0 zoom out.
    /// </summary>
    /// <param name="zoomLevel">The new zoom level to set for the camera.</param>
    void SetZoomLevel(float zoomLevel);
    
    /// <summary>
    /// Zooms the camera in or out by the specified amount.<br/>
    /// Positive values zoom in, while negative values zoom out.
    /// </summary>
    /// <param name="amount">The amount to zoom the camera by.</param>
    void ZoomBy(float amount);
    
    /// <summary>
    /// Links the camera to the specified entity.<br/>
    /// The camera will follow the entity's position in the game world.
    /// </summary>
    /// <param name="entityId">The entity to link the camera to.</param>
    /// <exception cref="ArgumentNullException">Thrown if the provided entity is null.</exception>
    void LinkToEntity(int entityId);
    
    /// <summary>
    /// Unlinks the camera from the currently linked entity.<br/>
    /// The camera will no longer follow the entity's position and can be moved independently.
    /// </summary>
    void UnlinkFromEntity();
    
    /// <summary>
    /// Converts a screen position to a world position based on the current camera settings.<br/>
    /// This takes into account the camera's position, zoom level, and offset to accurately map screen coordinates to world coordinates.
    /// </summary>
    /// <param name="screenPosition">The screen position to convert.</param>
    /// <returns></returns>
    Vector2 ScreenToWorld(Vector2 screenPosition);
    
    /// <summary>
    /// Converts a world position to a screen position based on the current camera settings.<br/>
    /// This takes into account the camera's position, zoom level, and offset to accurately map world coordinates to screen coordinates.
    /// </summary>
    /// <param name="worldPosition">The world position to convert.</param>
    /// <returns></returns>
    Vector2 WorldToScreen(Vector2 worldPosition);

    
    Matrix4x4 GetViewMatrix();
}