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


using _BaseModule.UI.StatsFeature;
using Avalonia.Controls;
using RPGCreator.SDK.Modules.UIModule;
using RPGCreator.SDK.UiService;

namespace RPGCreator.SDK.EditorUI.Extensions;

public class StatsUiContext
{
    private readonly Config _config;
    public Grid StatsGrid => _config.GetStatsGrid();
    
    /// <summary>Configuration object to initialize the context.</summary>
    public class Config
    {
        public Func<Grid> GetStatsGrid { get; init; } = null!;
    }
    
    public StatsUiContext(Config config)
    {
        _config = config;
    }
}

public static class StatsUiExtensions
{
    private static IUiExtensionManager _manager = UiServices.ExtensionManager;
    public static UiExtensionExtensions_Generated.AssetsManagerScope Stats(
        this UiExtensionExtensions_Generated.AssetsManagerScope context, Action<StatsManagement, StatsUiContext> callback)
    {
        _manager.RegisterExtension(new UIRegion("BaseModule.StatsManagement"), (target, ctx) =>
        {
            if (ctx is StatsUiContext typedContext && target is StatsManagement statsManagement)
                callback(statsManagement, typedContext);
        });
        return context;
    }
}