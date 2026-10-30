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
using RPGCreator.SDK.Assets.Skills;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.Editor.UI.Controllers.Widgets.AssetsManager;

[EditorUiController]
public class SkillMgrController(
    SkillMgrModel model,
    IAssetManager assetManager,
    IResourceManager resourceManager,
    GraphicsDevice graphicsDevice,
    ImGuiRenderer imGuiRenderer)
{
    public readonly SkillMgrModel Model = model;
    private ushort _currentSkillVersion = 0;
    private readonly Dictionary<Ulid, ImTextureRef?> _textures = [];

    public void LoadSkills(bool forceRefresh = false)
    {
        var assetVersion = assetManager.GetVersion<SkillDefinition>();
        if (_currentSkillVersion == assetVersion && !forceRefresh)
            return;

        ClearTextures();
        _currentSkillVersion = assetVersion;

        List<SkillDefinition> skills = [];
        foreach (var skillId in assetManager.ListAssets<SkillDefinition>())
        {
            assetManager.GetAsset<SkillDefinition>(skillId)
                .OnSuccess(skills.Add)
                .OnFailure(err => Logger.Error(err));
        }

        Model.Skills = skills;

        if (Model.SelectedSkill != null)
        {
            Model.SelectedSkill = skills.Find(s => s.AssetId == Model.SelectedSkill.AssetId);
        }
    }

    public Result DeleteSkill(SkillDefinition skill)
    {
        var res = assetManager.DeleteAsset(skill.AssetId);
        if (Model.SelectedSkill == skill)
            Model.SelectedSkill = null;
        return res;
    }

    public ImTextureRef? GetTexture(SkillDefinition skill)
    {
        var resourceId = skill.IconResource;
        if (resourceId == Ulid.Empty)
            return null;

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
                Logger.Once.Error("Couldn't load skill icon texture {0}: {1}", resourceId, err);
                _textures[resourceId] = null;
            });

        return _textures.GetValueOrDefault(resourceId);
    }

    public void ClearTextures()
    {
        foreach (var texture in _textures.Values)
        {
            if (!texture.HasValue)
                continue;

            var textureInfo = imGuiRenderer.UnbindTexture(texture.Value);
            if (textureInfo is { IsManaged: false })
                textureInfo.Texture.Dispose();
        }

        _textures.Clear();
    }
}