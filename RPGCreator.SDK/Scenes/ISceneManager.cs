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

using RPGCreator.SDK.Assets.Map;
using RPGCreator.SDK.Services.Map;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Scenes;

public interface ISceneManager
{
    IReadOnlyList<IMapLayer> ActiveLayers { get; }
    ISceneSimulation Simulation { get; }
    ISceneRender Render { get; }
    
    ValueTask LoadSceneAsync(Ulid mapId);
    
    void UnloadScene();
    ValueTask ReloadSceneAsync();
    
    void SaveSceneAsync();
    ValueTask SaveSceneAsync(string newMapPath);
    
    IMapLayer? GetLayer(int index);
    
    TLayer? EnsureActiveLayer<TLayer>(URN awaitedLayerType, int activeIndex, out int newActiveIndex) where TLayer : class, IMapLayer;
    IMapLayer? EnsurePhantomLayer(URN awaitedPhantomLayer);

    IMapLayer? AddNewLayer(MapLayerDefinition definition, bool phantom = false);

    void DrawScene(double deltaTime);
}