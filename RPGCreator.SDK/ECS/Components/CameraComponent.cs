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

using Arch.Core;
using Microsoft.Xna.Framework;
using Vector2 = System.Numerics.Vector2;

namespace RPGCreator.SDK.ECS.Components;

public struct CameraComponent
{
    public bool IsActive = false;
    
    public float Rotation = 0.0f;
    public float Zoom = 1.0f;
    public Vector2 ViewportSize;
    
    public Vector2 Offset = Vector2.Zero; 

    public bool IsFollowingEntity = false;
    
    public Entity FollowedEntity = Entity.Null; 

    public Matrix ViewMatrix = Matrix.Identity;
    public Rectangle BoundingBox = Rectangle.Empty;

    public CameraComponent(Vector2 viewportSize)
    {
        ViewportSize = viewportSize;
    }
    
    public Vector2 Origin => new(ViewportSize.X * 0.5f, ViewportSize.Y * 0.5f);
    
    public static Matrix GetViewMatrix(in CameraComponent camera, in TransformComponent transform)
    {
        var origin = new Vector3(camera.Origin.X, camera.Origin.Y, 0f);
        return 
            // 1. Déplace le monde à l'inverse de la position de la caméra
            Matrix.CreateTranslation(new Vector3(-transform.Position.X, -transform.Position.Y, 0f)) *
            
            // 2. Décale par rapport à l'origine (centre de l'écran)
            Matrix.CreateTranslation(new Vector3(-origin.X, -origin.Y, 0f)) *
            
            // 3. Applique la rotation
            Matrix.CreateRotationZ(camera.Rotation) *
            
            // 4. Applique le zoom (échelle)
            Matrix.CreateScale(camera.Zoom, camera.Zoom, 1f) * // ou Matrix.CreateScale(camera.Zoom, camera.Zoom, 1f)
            
            // 5. Replace le tout par rapport à l'origine
            Matrix.CreateTranslation(origin);
    }
}