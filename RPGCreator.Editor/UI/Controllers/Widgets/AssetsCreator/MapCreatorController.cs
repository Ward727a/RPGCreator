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
using RPGCreator.Editor.UI.Models.Widgets.AssetsCreator;
using RPGCreator.SDK.Assets.Map;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;
using SDL3;

namespace RPGCreator.Editor.UI.Controllers.Widgets.AssetsCreator;

[EditorUiController]
public class MapCreatorController
{
    public readonly MapCreatorModel State;
    private readonly IAssetManager _assetManager;

    public MapCreatorController(MapCreatorModel state, IAssetManager assetManager)
    {
        State = state;
        _assetManager = assetManager;
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(State.MapName) && State.MapSize is { X: > 0, Y: > 0 };
    }

    public bool Create()
    {
        if (!IsValid())
        {
            SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Error",
                "Invalid map name or size!\n" +
                "Please report this issue on the GitHub repository as you should NOT see this box!", IntPtr.Zero);
            return false;
        }
        
        var map = new MapDefinition
        {
            AssetId = Ulid.NewUlid(),
            Name = State.MapName,
            Size = State.MapSize,
            BackgroundColor = State.MapBackgroundColor
        };

        return _assetManager.SaveAsset(map).OnFailure(err=>Logger.Error(err)).IsSuccess;
    }
}