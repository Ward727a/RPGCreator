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

using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.RuntimeService;

namespace RPGCreator.SDK.GlobalState;

public interface IMapState : IState
{
    bool HasCurrentMap { get; set; }
    bool IsMapDirty { get; set; }
    Ulid CurrentMapId { get; set; }
    MapData? CurrentMapData { get; set; }
    IMapDef? CurrentMapDef { get; set; }
    
    /// <summary>
    /// Is there a selected layer?
    /// </summary>
    bool HasSelectedLayer { get; }

    /// <summary>
    /// Can a layer be selected?<br/>
    /// If false, no layer selecting actions should be allowed (in the case where no layers exist, or <see cref="IMapService.HasLoadedMap"/> is false).
    /// </summary>
    bool CanSelectLayer { get; }
    
    /// <summary>
    /// The index of the currently selected layer.
    /// </summary>
    int CurrentLayerIndex { get; set; }
    
    /// <summary>
    /// The total number of layers available. (Starts at 1 => So if there is 0 layers, this will return 0, if there is 1 layer, this will return 1, etc...)
    /// </summary>
    int LayerCount { get; }
}