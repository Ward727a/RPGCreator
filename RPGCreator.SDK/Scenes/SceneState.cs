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

using Fluxor;
using Microsoft.Xna.Framework;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Scenes;

[FeatureState]
public record SceneState
{
    public Ulid CurrentMapId { get; init; }
    public Point MapSize { get; init; }
    public int ActiveLayerIndex { get; init; }
    public URN ActiveLayerType { get; init; }
    public bool HasUnsavedChanges { get; init; }
    public bool IsLoading { get; init; }
    public bool IsSceneLoaded => CurrentMapId != Ulid.Empty;

    public SceneState()
    {
        CurrentMapId = Ulid.Empty;
        MapSize = new Point(10, 10);
        ActiveLayerIndex = 0;
        ActiveLayerType = URN.Empty;
        HasUnsavedChanges = false;
        IsLoading = false;
    }
}