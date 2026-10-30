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
using RPGCreator.Editor.UI.Models.Widgets;
using RPGCreator.SDK.Assets.Tileset;
using RPGCreator.SDK.Services.Assets;

namespace RPGCreator.Editor.UI.Controllers.Widgets;

public class TilesetSelectorController(TilesetSelectorModel state, IAssetManager assetManager)
{
    public readonly TilesetSelectorModel State = state;
    
    public IEnumerable<Ulid> GetTilesets()
    {
        return assetManager.ListAssets<TilesetDefinition>();
    }

    public TilesetDefinition? GetTileset(Ulid tilesetId)
    {
        return assetManager.GetAsset<TilesetDefinition>(tilesetId).Match(
            tileset => tileset,
             _ => null
        );
    }
    
    public void Select(Ulid tilesetId)
    {
        State.SelectedTilesetId = tilesetId;
    }

    public bool IsSelected(Ulid tilesetId) => State.SelectedTilesetId == tilesetId;
}