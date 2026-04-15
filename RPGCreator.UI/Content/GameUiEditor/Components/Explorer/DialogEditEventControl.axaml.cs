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
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using RPGCreator.SDK;
using RPGCreator.SDK.GameUI;
using RPGCreator.SDK.GameUI.Events.Actions;
using RPGCreator.SDK.GameUI.Interfaces;
using RPGCreator.SDK.Logging;

namespace RPGCreator.UI.Content.GameUiEditor.Components.Explorer;

public partial class DialogEditEventControlVm : ObservableObject
{

    [ObservableProperty] private ControlEventDescriptor _eventDescriptor;
    
    public string WindowTitle => $"Edit event {_eventDescriptor.Name}";
    public string EventName => _eventDescriptor.Name;
    public string EventDescription => _eventDescriptor.Description;
    
    public bool HasAction => _eventDescriptor.Action != null;

    public ObservableCollection<IGuiAction> AvailableActions { get; set; } = new();
    public ObservableCollection<ControlPropertyDescriptor> ActionProperties { get; set; } = new();

    [ObservableProperty] string _selectedActionName;
    
    [ObservableProperty] int _selectedActionIndex;
    
    public DialogEditEventControlVm(ControlEventDescriptor eventDescriptor)
    {
        _eventDescriptor = eventDescriptor;

        foreach (var action in RegistryServices.GuiAction.GetActionByContext(_eventDescriptor.GuiContextType) ?? [])
        {
            AvailableActions.Add(action);
        }
        
        SelectedActionName = _eventDescriptor.Action?.Name ?? "No action currently selected.";

        if (_eventDescriptor.Action != null)
            SelectedActionIndex = AvailableActions.IndexOf(_eventDescriptor.Action);
        else
            SelectedActionIndex = -1;
        RefreshProperties();
    }


    partial void OnSelectedActionIndexChanged(int value)
    {
        if(AvailableActions.Count == 0) return;
        if (value < 0 || value >= AvailableActions.Count) return;
        _eventDescriptor.Action = AvailableActions[value];
        SelectedActionName = _eventDescriptor.Action.Name;
        RefreshProperties();
    }

    private void RefreshProperties()
    {
        ActionProperties.Clear();
        if(EventDescriptor.Action == null) return;

        if (EventDescriptor.Action is IHasControlProperties hasControlProperties)
        {
            ActionProperties.AddRange(hasControlProperties.GetAllProperties());
        }
    }
}

public partial class DialogEditEventControl : Window
{
    
    public DialogEditEventControl(ControlEventDescriptor eventDescriptor)
    {
        DataContext = new DialogEditEventControlVm(eventDescriptor);
        InitializeComponent();
    }
}