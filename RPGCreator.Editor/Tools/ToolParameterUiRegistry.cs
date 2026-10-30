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
using RPGCreator.SDK.Tools;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Enums;

namespace RPGCreator.Editor.Tools;

[EngineService(typeof(IToolParameterUiRegistry), ServiceLifetime.Singleton, EngineModeFlags.Editor)]
public class ToolParameterUiRegistry : IToolParameterUiRegistry
{
    
    private readonly Dictionary<Type, Action<IToolParameter>> _drawActions = new();
    
    public void Register<TParam>(Action<TParam> drawAction) where TParam : class, IToolParameter
    {
         _drawActions[typeof(TParam)] = parameter => drawAction((TParam)parameter);
    }

    public void Register<TParam>(Type drawerType, Action<TParam> drawAction) where TParam : class, IToolParameter
    {
        _drawActions[drawerType] = parameter => drawAction((TParam)parameter);
    }

    public void Unregister<TParam>() where TParam : class, IToolParameter
    {
        _drawActions.Remove(typeof(TParam));
    }

    public void Draw<TParam>(TParam param) where TParam : class, IToolParameter
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