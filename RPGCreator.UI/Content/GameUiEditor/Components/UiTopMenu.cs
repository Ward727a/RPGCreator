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
using Ursa.Controls;

namespace RPGCreator.UI.Content.GameUiEditor.Components;

public class UiTopMenu : UserControl
{
    
    private ToolBar _toolBar = null!;

    private Button _fileButton = null!;
    private Button _editButton = null!;
    private Button _viewButton = null!;
    private Button _helpButton = null!;
    
    public UiTopMenu()
    {
        CreateComponents();
        RegisterEvents();
    }

    private void CreateComponents()
    {
        _toolBar = new ToolBar();
        
        _fileButton = new Button() { Content = "File" };
        _editButton = new Button() { Content = "Edit" };
        _viewButton = new Button() { Content = "View" };
        _helpButton = new Button() { Content = "Help" };
        
        _toolBar.Items.Add(_fileButton);
        _toolBar.Items.Add(_editButton);
        _toolBar.Items.Add(_viewButton);
        _toolBar.Items.Add(_helpButton);
    }

    private void RegisterEvents()
    {
    }

}