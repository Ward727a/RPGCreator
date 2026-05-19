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
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RPGCreator.SDK;
using RPGCreator.SDK.GameUI.Controls;
using RPGCreator.UI.Common;

namespace RPGCreator.UI.Content.GameUiEditor.Components.Explorer;

public partial class DialogUiBrowserVm : ViewModelBase
{
    public ObservableCollection<BaseControl> ControlLists { get; set; } = new();

    [ObservableProperty] private BaseControl? _selectedControl;
    
    private readonly Action _closeWindow;
    private readonly Action<BaseControl> _selectControl;
    
    public DialogUiBrowserVm(Action closeWindow, Action<BaseControl> selectControl)
    {
        RegistryServices.GuiControl.Controls.ToList().ForEach(control => ControlLists.Add(control));
        _closeWindow = closeWindow;
        _selectControl = selectControl;
    }

    [RelayCommand]
    private void Cancel()
    {
        _closeWindow();
    }
    
    [RelayCommand]
    private void SelectControl()
    {
        if(SelectedControl != null)
            _selectControl(SelectedControl);
    }
}