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

using System.Collections.Generic;
using Fluxor;
using Microsoft.Xna.Framework;
using RPGCreator.SDK.Assets.AutoTiles;
using RPGCreator.SDK.Services.Map;
using RPGCreator.SDK.Tools;

namespace RPGCreator.Editor.Tools.ToolLogics;

[ToolLogic("rpgc://layer_type/auto_tile_layer")]
public class AutoTileLayerToolLogic(IState<AutoTileLayerToolState> state, IDispatcher dispatcher) : IToolLogic
{
    private MaterialId MaterialId => state.Value.MaterialId;
    
    public IReadOnlyList<Point> Apply(IMapLayer targetLayer, Point gridPos, bool forPhantom = false)
    {
        if (targetLayer is not AutoTileLayer autoTileLayer) return [];
        
        if(MaterialId == MaterialId.Empty)
            return [];

        autoTileLayer.Add(gridPos, new IntGridData(MaterialId));
        autoTileLayer.UpdateEntity();
        return [gridPos];
    }

    public void Erase(IMapLayer targetLayer, Point gridPos, bool forPhantom = false)
    {
        if (targetLayer is not AutoTileLayer autoTileLayer) return;
        
        autoTileLayer.Remove(gridPos);
        autoTileLayer.UpdateEntity();
    }

    public void Pick(IMapLayer targetLayer, Point gridPos)
    {
        if(targetLayer is not AutoTileLayer autoTileLayer || !autoTileLayer.Has(gridPos)) return;
        
        var materialId = autoTileLayer.Get(gridPos).MaterialId;
        dispatcher.Dispatch(new AutoTileLayerToolActions.SetMaterial(materialId));
    }

    public void UpdateState()
    {
    }
}