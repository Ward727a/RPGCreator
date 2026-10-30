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
using System.Collections.Generic;
using Fluxor;
using Microsoft.Xna.Framework;
using RPGCreator.SDK.Services.Map;
using RPGCreator.SDK.Tools;
using RPGCreator.Shared.Types;

namespace RPGCreator.Editor.Tools.ToolLogics;

[ToolLogic("rpgc://layer_type/tile_layer")]
// ReSharper disable once UnusedType.Global
public class TileLayerToolLogic(IState<TileLayerToolState> state, IDispatcher dispatcher) : IToolLogic
{
    private ushort TileIndexStart => state.Value.StartTileIndex;
    private ushort TileIndexEnd => state.Value.EndTileIndex;
    private bool InGroup => TileIndexStart != TileIndexEnd;
    
    private int TilesByRow => state.Value.TilesByRow;
    private byte TilesetId => state.Value.SelectedTilesetIndex;

    public IReadOnlyList<Point> Apply(IMapLayer targetLayer, Point gridPos, bool forPhantom = false)
    {
        if (targetLayer is not TileLayer tileLayer) return [];

        if (TilesByRow == -1 || !InGroup) // If TilesByRow is not set, can be caused by a bug, or more simply by picking a tile.
        {
            tileLayer.Add(gridPos, new TileData() { TileId = TileIndexStart, TilesetId = TilesetId });
            return [gridPos];
        }

        int startX = state.Value.StartTileIndex.X;
        int startY = state.Value.StartTileIndex.Y;
        int endX = state.Value.EndTileIndex.X;
        int endY = state.Value.EndTileIndex.Y;

        int minX = Math.Min(startX, endX);
        int minY = Math.Min(startY, endY);
        int maxX = Math.Max(startX, endX);
        int maxY = Math.Max(startY, endY);

        List<Point> appliedPoints = new List<Point>();
        
        for (int y = 0; y <= (maxY - minY); y++)
        {
            for (int x = 0; x <= (maxX - minX); x++)
            {
                Point targetPos = new Point(gridPos.X + x, gridPos.Y + y);

                ushort tileId = TileIndex.Make(minX + x, minY + y, TilesByRow);

                tileLayer.Add(targetPos, new TileData() { TileId = tileId, TilesetId = TilesetId });
                appliedPoints.Add(targetPos);
            }
        }
        return appliedPoints;
    }

    public void Erase(IMapLayer targetLayer, Point gridPos, bool forPhantom = false)
    {
        if (targetLayer is not TileLayer tileLayer) return;

        if (TilesByRow == -1 || !forPhantom || !InGroup)
        {
            tileLayer.Remove(gridPos);
            return;
        }

        var startX = state.Value.StartTileIndex.X;
        var startY = state.Value.StartTileIndex.Y;
        var endX = state.Value.EndTileIndex.X;
        var endY = state.Value.EndTileIndex.Y;

        var minX = Math.Min(startX, endX);
        var minY = Math.Min(startY, endY);
        var maxX = Math.Max(startX, endX);
        var maxY = Math.Max(startY, endY);

        for (var y = 0; y <= (maxY - minY); y++)
        {
            for (var x = 0; x <= (maxX - minX); x++)
            {
                var targetPos = new Point(gridPos.X + x, gridPos.Y + y);

                tileLayer.Remove(targetPos);
            }
        }
    }

    public void Pick(IMapLayer targetLayer, Point gridPos)
    {
        if (targetLayer is not TileLayer tileLayer || !tileLayer.Has(gridPos)) return;
        
        var tile = tileLayer.Get(gridPos);
        dispatcher.Dispatch(new TileLayerToolActions.SetTilesetAction(tile.TilesetId, -1, TileIndex.Make(tile.TileId), TileIndex.Make(tile.TileId)));
    }

    public void UpdateState()
    {
    }
}