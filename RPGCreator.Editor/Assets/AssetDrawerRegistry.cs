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
using Microsoft.Extensions.DependencyInjection;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Enums;

namespace RPGCreator.Editor.Assets;

[EngineService(typeof(IAssetDrawerRegistry), ServiceLifetime.Singleton, EngineModeFlags.Editor)]
public class AssetDrawerRegistry : IAssetDrawerRegistry
{
    private readonly Dictionary<Type, Action<IAssetDto>> _drawActions = new();
    public void Register<TAsset>(Action<TAsset> drawAction) where TAsset : class, IAssetDto
    {
        _drawActions[typeof(TAsset)] = parameter => drawAction((TAsset)parameter);
    }

    public void Unregister<TAsset>() where TAsset : class, IAssetDto
    {
        _drawActions.Remove(typeof(TAsset));
    }

    public void Draw(IAssetDto param)
    {
        var type = param.GetType();
        if (_drawActions.TryGetValue(type, out var drawAction))
        {
            drawAction(param);
        }
        else
        {
            ImGui.TextDisabled($"[Missing UI Renderer for {type.Name}]");
        }
    }
}