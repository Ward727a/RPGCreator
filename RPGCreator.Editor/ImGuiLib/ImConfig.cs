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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using RPGCreator.SDK.Services.EngineService;

namespace RPGCreator.Editor.ImGuiLib;

/// <summary>
/// A static class that contains access to mostly all services of the Engine, and other configuration of the UI.
/// </summary>
public static class ImConfig
{
    private static IServiceProvider _provider;
    public static void Initialize(IServiceProvider serviceProvider, ImGuiRenderer renderer, Game game)
    {
        _provider = serviceProvider;
        ImGuiRenderer = renderer;
        Game = game;
    }
    
    public static ImGuiRenderer ImGuiRenderer { get; private set; } = null!;
    public static Game Game { get; private set; } = null!;
    public static IEngineConfig EngineConfig => _provider.GetRequiredService<IEngineConfig>();
    
    /// <summary>
    /// This allows access to all other service, but it should not be used too much.
    /// </summary>
    /// <typeparam name="TService">The service type to get</typeparam>
    /// <returns>
    /// The service instance of the specified type.
    /// </returns>
    public static TService GetEngineService<TService>() where TService : notnull => _provider.GetRequiredService<TService>();
}