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

using System.Text.Json;
using Arch.Core;
using Arch.Core.Extensions;
using Microsoft.Xna.Framework;
using RPGCreator.SDK.Assets.Map;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.Scenes;

namespace RPGCreator.SDK.Services.Map;

public interface IMapLayer
{
    public bool IsPhantom { get; }
    public MapLayerDefinition LayerDefinition { get; }
    public Entity LayerEntity { get; }
    public int ZIndex { get; }
    public bool IsVisible { get; set; }
    public void Build(MapLayerDefinition layerDefinition);
    public void Remove(Point gridPos);
    public bool Has(Point gridPos);
    public void Clear();
    public void SetPhantom();

    public void CreateEntityForEcs(World world);
    public object? GetSaveData();
    public void SetVisibility(ISceneSimulation sceneSimulation, bool isVisible);
}

public abstract class MapLayer<TData> : IMapLayer where TData : unmanaged
{
    public MapLayerDefinition LayerDefinition { get; protected set; }
    public Entity LayerEntity { get; protected set; } = Entity.Null;
    
    public bool IsPhantom { get; private set; } = false;
    public int ZIndex { get; protected set; } = 0;
    public bool IsVisible { get; set; } = true;
    public int Width { get; protected set; } = 0;
    public int Height { get; protected set; } = 0;
    protected TData[] Tiles = [];

    /// <summary>
    /// Marks the current map layer as a phantom layer.
    /// A phantom layer typically does not interact with the underlying game logic or
    /// physics and is treated differently from standard layers.
    /// <br/>
    /// Warning: This is a one way operation and cannot be undone. Use with caution.
    /// </summary>
    public void SetPhantom()
    {
        IsPhantom = true;
    }
    
    public virtual void Build(MapLayerDefinition layerDefinition)
    {
        TData[]? data = null;
        if (layerDefinition.Data.ValueKind != JsonValueKind.Undefined)
        {
            data = layerDefinition.Data.Deserialize<TData[]>();
        } 

        Width = layerDefinition.Size.X;
        Height = layerDefinition.Size.Y;
        ZIndex = layerDefinition.ZIndex;

        if (data != null)
        {
            if (data.Length == GetMaxIndex())
            {
                // In this case, the data is valid and we can use it directly.
                Tiles = data;
            }
            else if (data.Length < GetMaxIndex())
            {
                // In this case, we can assume that the data is incomplete and we need to fill the missing values with default values.
                Tiles = new TData[GetMaxIndex()];
                Array.Copy(data, Tiles, data.Length);
            }
            else
            {
                // In this case, we can assume that the data has a non-savable error, and we need to restart it from zero.
                // We could also just reduce the size of the layer to match the data, but for now, we'll just reset it to default values.
                Tiles = new TData[GetMaxIndex()];
                Logger.Critical("The layer data is invalid and has been reset to default values.");
            }
        }
        else
        {
            // In this case, this is just a new layer, so we need to initialize it with default values.
            Tiles = new TData[GetMaxIndex()];
        }
        
        LayerDefinition = layerDefinition;
    }
    
    public abstract void Remove(Point gridPos);
    public abstract bool Has(Point gridPos);

    public virtual void Clear()
    {
        Tiles = new TData[GetMaxIndex()];
    }

    public abstract void CreateEntityForEcs(World world);

    public abstract void Add(Point gridPos, in TData obj);
    public abstract TData Get(Point gridPos);
    
    public abstract ReadOnlySpan<TData> GetAll();
    /// <summary>
    /// This method will return the data that will be saved to the map definition inside the <see cref="MapLayerDefinition.Data"/> property.
    /// </summary>
    /// <returns></returns>
    public virtual object? GetSaveData() => Tiles;
    
    #region Helpers
    
    protected bool IsValid(Point gridPos) => 
        gridPos.X >= 0 && gridPos.X < Width && gridPos.Y >= 0 && gridPos.Y < Height;
    protected int GetIndex(Point gridPos) => gridPos.X + (gridPos.Y * Width);
    public Point GetGridPos(int index) => new Point(index % Width, index / Width);
    protected int GetMaxIndex() => Width * Height;

    public void SetVisibility(ISceneSimulation sceneSimulation, bool isVisible)
    {
        IsVisible = isVisible;

        if (sceneSimulation.CurrentWorld == null)
            return;

        var world = sceneSimulation.CurrentWorld;

        if (!world.IsAlive(LayerEntity))
            return;

        LayerEntity.Set(new LayerComponent(ZIndex, isVisible));
    }
    
    #endregion
}