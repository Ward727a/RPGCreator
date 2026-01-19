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
using Microsoft.Xna.Framework;
using RPGCreator.Core.Types.Map.Chunks;
using RPGCreator.SDK;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.ECS.Entities;
using RPGCreator.SDK.RuntimeService;
using RPGCreator.SDK.Types;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace RPGCreator.RTP.Services;

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
    
    private IEntity? _cameraEntity;
    public IEntity? CameraEntity
    {
        get => _cameraEntity;
        private set
        {
            OnPropertyChanging();
            _cameraEntity = value;
            OnPropertyChanged();
        }
    }
    
    public IEntity? LinkedEntity 
    {
        get => GetCameraComponent().FollowedEntity;
        private set
        {
            OnPropertyChanging();
            GetCameraComponent().FollowedEntity = value;
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
            OnPropertyChanging();
            GetCameraComponent().Zoom = value;
            OnPropertyChanged();
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
    
    public void SetCameraEntity(IEntity cameraEntity, bool preserveSettings = false)
    {
        CameraEntity = cameraEntity;
    }

    public void ResetCamera()
    {
        Position = Vector2.Zero;
        ZoomLevel = 1.0f;
        Offset = Vector2.Zero;
        IsLinkedToEntity = false;
        LinkedEntity = null;
        ViewportSize = new Size(800, 600); // Default size, it should be set properly later (e.g., from the window size)
        Rotation = 0.0f;
    }

    public void Update(TimeSpan gameTime)
    {
    }

    public void Drag(Vector2 delta)
    {
        var deltaVectorSized = new Vector2(delta.X / _cellSize.Width, delta.Y / _cellSize.Height);
        Position += new Vector2(10,0) / ZoomLevel;
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

    public void LinkToEntity(IEntity entity)
    {
        LinkedEntity = entity;
        IsLinkedToEntity = true;
    }

    public void UnlinkFromEntity()
    {
        LinkedEntity = null;
        IsLinkedToEntity = false;
    }
    public (long minX, long maxX, long minY, long maxY) GetVisibleChunkBounds()
    {
        float tileSize = _cellSize.Width;

        var halfWidth = (ViewportSize.Width / 2f) / (ZoomLevel * tileSize);
        var halfHeight = (ViewportSize.Height / 2f) / (ZoomLevel * tileSize);

        var left = Position.X - halfWidth;
        var right = Position.X + halfWidth;
        var top = Position.Y - halfHeight;
        var bottom = Position.Y + halfHeight;

        return (
            minX: (long)Math.Floor(left / LayerChunk.ChunkSize),
            maxX: (long)Math.Floor(right / LayerChunk.ChunkSize),
            minY: (long)Math.Floor(top / LayerChunk.ChunkSize),
            maxY: (long)Math.Floor(bottom / LayerChunk.ChunkSize)
        );
    }
    
    public Matrix4x4 GetViewMatrix()
    {
        var viewportCenter = new Vector2(ViewportSize.Width / 2f, ViewportSize.Height / 2f);

        return Matrix4x4.CreateTranslation(new Vector3(-Position.X * _cellSize.Width, -Position.Y * _cellSize.Height, 0)) *
               Matrix4x4.CreateScale(new Vector3(ZoomLevel, ZoomLevel, 1)) *
               Matrix4x4.CreateTranslation(new Vector3(viewportCenter.X, viewportCenter.Y, 0));
    }
    
    public void Dispose()
    {
        CameraEntity = null;
    }
    
    #region Helpers
    
    private ref CameraComponent GetCameraComponent()
    {
        if (CameraEntity == null)
            throw new InvalidOperationException("Camera entity is not set.");
        
        if (!CameraEntity.HasComponent<CameraComponent>())
            throw new InvalidOperationException("Camera entity does not have a CameraComponent.");
        
        return ref CameraEntity.GetComponent<CameraComponent>();
    }
    
    #endregion

}