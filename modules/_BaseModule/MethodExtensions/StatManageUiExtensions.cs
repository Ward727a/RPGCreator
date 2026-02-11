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
using RPGCreator.SDK;
using RPGCreator.SDK.Modules.UIModule;
using RPGCreator.SDK.EditorUiService;

namespace RPGCreator.UI.Extensions;

using AssetManagerScope = UiExtensionExtensions_Generated.AssetsManagerScope;

#region Context
public class StatsManagerContext
{
    private readonly Config _config;
    public Grid StatsGrid => _config.GetStatsGrid();
    
    /// <summary>Configuration object to initialize the context.</summary>
    public class Config
    {
        public Func<Grid> GetStatsGrid { get; init; } = null!;
        public Func<AutoCompleteBox> GetSearchBar { get; init; } = null!;
        public Func<ScrollViewer> GetGridScroller { get; init; } = null!;
        public Func<ListBox> GetListBox { get; init; } = null!;
        public Func<StackPanel> GetButtonsPanel { get; init; } = null!;
        public Func<Button> GetAddButton { get; init; } = null!;
        public Func<Button> GetEditButton { get; init; } = null!;
        public Func<Button> GetRemoveButton { get; init; } = null!;
        public Action ApplyFilters { get; init; } = null!;
    }
    
    public StatsManagerContext(Config config)
    {
        _config = config;
    }
}

public class StatEditorContext
{
    private readonly Config _config;
    
    public Grid EditorGrid => _config.GetEditorGrid();

    public class Config
    {
        public Func<Grid> GetEditorGrid { get; init; } = null!;
    }
    
    public StatEditorContext(Config config)
    {
        _config = config;
    }
}
#endregion

public static class StatManageUiExtensions
{
    private static IUiExtensionManager Manager => EditorUiServices.ExtensionManager;
    public static AssetManagerScope StatsManager(
        this AssetManagerScope context, Action<StatsManagement, StatsManagerContext> callback)
    {
        Manager.RegisterExtension(new UIRegion("BaseModule.StatsManagement"), (target, ctx) =>
        {
            if (ctx is StatsManagerContext typedContext && target is StatsManagement statsManagement)
                callback(statsManagement, typedContext);
        });
        return context;
    }

    public static AssetManagerScope StatEditor(
        this AssetManagerScope context, Action<StatEditor, StatEditorContext> callback)
    {
        Manager.RegisterExtension(new UIRegion("BaseModule.StatEditor"), (target, ctx) =>
        {
            if (ctx is StatEditorContext typedContext && target is StatEditor statEditor)
                callback(statEditor, typedContext);
        });
        return context;
    }
}