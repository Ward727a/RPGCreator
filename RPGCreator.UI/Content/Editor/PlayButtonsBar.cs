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
using Avalonia.Controls;
using Avalonia.Layout;
using Projektanker.Icons.Avalonia;
using RPGCreator.SDK;
using RPGCreator.SDK.Exceptions;
using RPGCreator.SDK.Types.EngineClass;
using Ursa.Controls;

namespace RPGCreator.UI.Content.Editor;

/// <summary>
/// Play buttons bar, containing the "Play" "Play this", "Play as Debug" buttons, and the "Stop" button.
/// </summary>
public class PlayButtonsBar : UserControl
{

    public event Action? OnPlay;
    public event Action? OnPlayThis;
    public event Action? OnPlayDebug;
    public event Action? OnStop;
    
    private StackPanel _buttonsPanel = null!;
    private Divider _buttonsDivider = null!;
    private Button _playButton = null!;
    private Button _playThisButton = null!;
    private Button _playDebugButton = null!;
    private Button _stopButton = null!;
    
    public PlayButtonsBar()
    {
        RegistryServices.Classes.Instantiate<TestClass>().OnSuccess((obj) =>
        {
            obj.SecondProp = 10;
            obj.Test = "Hello World!";
            obj.Serialize();
        });
        CreateComponents();
        RegisterEvents();
        LinkToExtension();
        Content = _buttonsPanel;
    }

    private void CreateComponents()
    {
        _buttonsPanel = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4
        };

        _buttonsDivider = new Divider()
        {
            Orientation = Orientation.Vertical
        };
        _buttonsPanel.Children.Add(_buttonsDivider);
        
        _playButton = new Button()
        {
            MaxHeight = 32,
            MinHeight = 32,
            MinWidth = 32,
            MaxWidth = 32,
            FontSize = 24,
            Content = new Icon()
            {
                Value = "mdi-play",
                Width = 32,
                Height = 32,
            }
        };
        ToolTip.SetTip(_playButton, "Play (from the start of the game)");
        ToolTip.SetShowOnDisabled(_playButton, true);
        _buttonsPanel.Children.Add(_playButton);
        
        _playThisButton = new Button()
        {
            MaxHeight = 32,
            MinHeight = 32,
            MinWidth = 32,
            MaxWidth = 32,
            FontSize = 24,
            Content = new Icon()
            {
                Value = "mdi-play-circle",
                Width = 32,
                Height = 32,
            }
        };
        ToolTip.SetTip(_playThisButton, "Play this (from the current scene)");
        ToolTip.SetShowOnDisabled(_playThisButton, true);
        _buttonsPanel.Children.Add(_playThisButton);
        
        _playDebugButton = new Button()
        {
            MaxHeight = 32,
            MinHeight = 32,
            MinWidth = 32,
            MaxWidth = 32,
            FontSize = 24,
            Content = new Icon()
            {
                Value = "mdi-bug-play",
                Width = 32,
                Height = 32,
            }
        };
        ToolTip.SetTip(_playDebugButton, "Play as Debug (from the start of the game, with debug mode enabled)");
        ToolTip.SetShowOnDisabled(_playDebugButton, true);
        _buttonsPanel.Children.Add(_playDebugButton);
        
        _stopButton = new Button()
        {
            MaxHeight = 32,
            MinHeight = 32,
            MinWidth = 32,
            MaxWidth = 32,
            FontSize = 24,
            Content = new Icon()
            {
                Value = "mdi-stop",
                Width = 32,
                Height = 32,
            },
            IsEnabled = false
        };
        ToolTip.SetTip(_stopButton, "Stop the game");
        ToolTip.SetShowOnDisabled(_stopButton, true);
        _buttonsPanel.Children.Add(_stopButton);
    }

    private void RegisterEvents()
    {
        _playButton.Click += (_,_) => OnPlay?.Invoke();
        _playThisButton.Click += (_,_) => OnPlayThis?.Invoke();
        _playDebugButton.Click += (_,_) => OnPlayDebug?.Invoke();
        _stopButton.Click += (_,_) => OnStop?.Invoke();
        
        
        OnPlayDebug += PlayDebug;
    }

    private void PlayDebug()
    {
        // This is also a temporary way to set the current map id for the project.
        // We will need to implement a way for the user to set the starting map for the project in the project settings, or from the map tab.
        GlobalStates.ProjectState.CurrentProject?.MainMapId = GlobalStates.MapState.CurrentMapId;
        // This is a temporary way to give the module hash to the project player.
        // We will need to implement a way to get the module hashes from the project, and not hardcode them like this.
        if(!GlobalStates.ProjectState.CurrentProject!.Modules.Contains("51DEC8E658C4C507504F38CA06F6794DBF90EB13E16C50D47142A4EF5C4FEBBB"))
            GlobalStates.ProjectState.CurrentProject?.Modules.Add("51DEC8E658C4C507504F38CA06F6794DBF90EB13E16C50D47142A4EF5C4FEBBB");
        if (GlobalStates.ProjectState.CurrentProject == null)
        {
            throw new CriticalEngineException("Project is null!", GlobalStates.ProjectState);
        }
        EngineServices.ProjectsManager.SaveProject(GlobalStates.ProjectState.CurrentProject);
        EngineServices.GamePlayerService.StartDebugGame();
    }

    private void LinkToExtension()
    {
    }

}