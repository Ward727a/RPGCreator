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

using Avalonia.Controls;
using RPGCreator.SDK;
using RPGCreator.SDK.EditorUI.Extensions;
using RPGCreator.SDK.Modules.UIModule;

namespace _BaseModule.UI.StatsFeature;

public class StatsManagement : UserControl
{
    private Grid _statsGrid = null!;
    
    public StatsManagement()
    {
        CreateComponents();
        RegisterEvents();

        var config = new StatsUiContext.Config()
        {
            GetStatsGrid = () => _statsGrid,
        };
        
        UiServices.ExtensionManager.ApplyExtensions(new UIRegion("BaseModule.StatsManagement"), this, new StatsUiContext(config));
    }

    private void CreateComponents()
    {

        _statsGrid = new Grid();

    }

    private void RegisterEvents()
    {
    }
}