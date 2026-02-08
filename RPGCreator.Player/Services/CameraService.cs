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

using System;
using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using RPGCreator.SDK;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.ECS.Entities;
using RPGCreator.SDK.RuntimeService;
using RPGCreator.SDK.Types;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace RPGCreator.Player.Services;

public class CameraService : ObservableObject, ICameraService
{
    private Size _cellSize = new(32f, 32f);
    public CameraService()
    {
        RuntimeServices.MapService.OnMapLoaded += (_) =>
        {
            var loadedMapData = RuntimeServices.MapService.CurrentLoadedMapData;
            _cellSize = new(loadedMapData.CellWidth, loadedMapData.CellHeight);
        };
    }

    public int? CameraEntityId
    {
        get;
        private set
        {
            OnPropertyChanging();
            field = value;
            OnPropertyChanged();
        }
    }

    public int? LinkedEntityId 
    {
        get => GetCameraComponent().FollowedEntity;
        private set
        {
            if (!value.HasValue)
                return;
            OnPropertyChanging();
            GetCameraComponent().FollowedEntity = value.Value;
            OnPropertyChanged();
        }
    }

    public bool IsLinkedToEntity
    {
        get => GetCameraComponent().IsFollowingEntity;
        set
        {
            OnPropertyChanging();
            GetCameraComponent().IsFollowingEntity = value;
            OnPropertyChanged();
        }
    }

    public float ZoomLevel
    {
        get => GetCameraComponent().Zoom;
        private set
        {
            float clampedValue = Math.Clamp(value, 0.1f, 5.0f);
        
            if (Math.Abs(GetCameraComponent().Zoom - clampedValue) > 0.0001f)
            {
                OnPropertyChanging();
                GetCameraComponent().Zoom = clampedValue;
                OnPropertyChanged();
            }
        }
    }

    public Vector2 Position
    {
        get => GetCameraComponent().Position;
        private  set
        {
            OnPropertyChanging();
            GetCameraComponent().Position = value;
            OnPropertyChanged();
        }
    }

    public Vector2 Offset
    {
        get => GetCameraComponent().Offset;
        private set
        {
            OnPropertyChanging();
            GetCameraComponent().Offset = value;
            OnPropertyChanged();
        }
    }

    public Size ViewportSize
    {
        get => GetCameraComponent().ViewportSize;
        private set
        {
            OnPropertyChanging();
            GetCameraComponent().ViewportSize = value;
            OnPropertyChanged();
        }
    }

    public float Rotation
    {
        get => GetCameraComponent().Rotation;
        private set
        {
            OnPropertyChanging();
            GetCameraComponent().Rotation = value;
            OnPropertyChanged();
        }
    }
    
    public void SetCameraEntity(int cameraEntityId, bool preserveSettings = false)
    {
        if (CameraEntityId == cameraEntityId)
            return;

        (Vector2, float, Vector2, bool, int?, Size, float) oldSettings = (Vector2.Zero, 1.0f, Vector2.Zero, false, null, new Size(800, 600), 0.0f);

        CameraEntityId = cameraEntityId;
        
        if (!preserveSettings || !CameraEntityId.HasValue) return;
        
        Position = oldSettings.Item1;
        ZoomLevel = oldSettings.Item2;
        Offset = oldSettings.Item3;
        IsLinkedToEntity = oldSettings.Item4;
        LinkedEntityId = oldSettings.Item5;
        ViewportSize = oldSettings.Item6;
        Rotation = oldSettings.Item7;
    }

    public void ResetCamera()
    {
        Position = Vector2.Zero;
        ZoomLevel = 1.0f;
        Offset = Vector2.Zero;
        IsLinkedToEntity = false;
        LinkedEntityId = null;
        ViewportSize = new Size(800, 600); // Default size, it should be set properly later (e.g., from the window size)
        Rotation = 0.0f;
    }

    public void Update(TimeSpan gameTime)
    {
    }

    public void Drag(Vector2 delta)
    {
        Position += delta / ZoomLevel;
    }

    public void MoveTo(Vector2 newPosition)
    {
        Position = newPosition;
    }

    public void SetOffset(Vector2 offset)
    {
        Offset = offset;
    }

    public void DragOffset(Vector2 delta)
    {
        Offset += delta;
    }

    public void SetZoomLevel(float zoomLevel)
    {
        ZoomLevel = zoomLevel;
    }

    public void ZoomBy(float amount)
    {
        ZoomLevel += amount;
    }

    public void LinkToEntity(int entityId)
    {
        LinkedEntityId = entityId;
        IsLinkedToEntity = true;
    }

    public void UnlinkFromEntity()
    {
        LinkedEntityId = null;
        IsLinkedToEntity = false;
    }

    public Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        if (!Matrix4x4.Invert(GetViewMatrix(), out var invertedMatrix))
            return Vector2.Zero;
        var screenPos3D = new Vector3(screenPosition, 0);
        var worldPos3D = Vector3.Transform(screenPos3D, invertedMatrix);
        return new Vector2(worldPos3D.X, worldPos3D.Y);
    }

    public Vector2 WorldToScreen(Vector2 worldPosition)
    {
        var matrix = GetViewMatrix();
        var worldPos3D = new Vector3(worldPosition, 0);
        var screenPos3D = Vector3.Transform(worldPos3D, matrix);
        return new Vector2(screenPos3D.X, screenPos3D.Y);
    }

    public Matrix4x4 GetViewMatrix()
    {
        var viewportCenter = new Vector2(ViewportSize.Width / 2f, ViewportSize.Height / 2f);

        return Matrix4x4.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
               Matrix4x4.CreateScale(new Vector3(ZoomLevel, ZoomLevel, 1)) *
               Matrix4x4.CreateTranslation(new Vector3(viewportCenter.X, viewportCenter.Y, 0));
    }
    
    public void Dispose()
    {
    }
    
    #region Helpers
    
    private ref CameraComponent GetCameraComponent()
    {

        ComponentManager components;
        if (RuntimeServices.GameSession.ActiveEcsWorld == null)
            throw new InvalidOperationException("No active ECS world found.");
        components = RuntimeServices.GameSession.ActiveEcsWorld.ComponentManager;
        
        if (!CameraEntityId.HasValue)
            throw new InvalidOperationException("Camera entity is not set.");
        
        if (!components.HasComponent<CameraComponent>(CameraEntityId.Value))
            throw new InvalidOperationException("Camera entity does not have a CameraComponent.");
        
        return ref components.GetComponent<CameraComponent>(CameraEntityId.Value);
    }
    
    #endregion

}