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
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.UI.Models.Widgets.AssetsManager;
using RPGCreator.RuntimeLib.FNA;
using RPGCreator.SDK.Assets.Actors;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.Editor.UI.Controllers.Widgets.AssetsManager;

[EditorUiController]
public class CharacterMgrController(CharacterMgrModel model, IAssetManager assetManager, GraphicsDevice graphicsDevice, ImGuiRenderer imGuiRenderer, IResourceManager resourceManager)
{
    private ushort _currentCharacterVersion = 0;
    public readonly CharacterMgrModel Model = model;

    public void LoadCharacters(bool forceRefresh = false)
    {
        var assetCharacterVersion = assetManager.GetVersion<ActorDefinition>();
        if (_currentCharacterVersion == assetCharacterVersion && !forceRefresh)
            return;
        
        _currentCharacterVersion = assetCharacterVersion;

        List<ActorDefinition> characters = [];
        ClearTextures();
        var actorIds = assetManager.ListAssets<ActorDefinition>();

        foreach (var actorId in actorIds)
        {
            assetManager.GetAsset<ActorDefinition>(actorId).OnSuccess((actor) =>
            {
                if (actor.Category != ActorCategory.Character)
                    return;
                characters.Add(actor);
            });
        }
        
        Model.Characters = characters;
    }
    
    public void DeleteCharacter(ActorDefinition character)
    {
        assetManager.DeleteAsset(character.AssetId);
    }
    
    
    private readonly Dictionary<Ulid, ImTextureRef?> _textures = [];

    public ImTextureRef? GetTexture(ActorDefinition animation)
    {
        var resourceId = animation.IconResource;
        if (_textures.TryGetValue(resourceId, out var texture))
            return texture;

        resourceManager.GetResource(resourceId)
            .OnSuccess(data =>
            {
                var texture2D = Texture2D.FromMemory(graphicsDevice, data);
                _textures[resourceId] = imGuiRenderer.BindTexture(texture2D);
            })
            .OnFailure(err =>
            {
                Logger.Error("Couldn't load texture with id {0} due to {1}", resourceId, err);
                _textures[resourceId] = null;
            }).Finally(() => { _textures.TryAdd(resourceId, null); });

        return _textures[resourceId];
    }

    public void ClearTextures()
    {
        foreach (var texture in _textures.Values)
        {
            if (!texture.HasValue)
                continue;
            
            var textureInfo = imGuiRenderer.UnbindTexture(texture.Value);
            
            if(!textureInfo?.IsManaged ?? false)
                textureInfo.Texture.Dispose();
        }

        _textures.Clear();
    }
}