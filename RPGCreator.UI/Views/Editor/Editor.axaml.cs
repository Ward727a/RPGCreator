#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
// 
// This file is part of RPG Creator and is distributed under the MIT License.
// You are free to use, modify, and distribute this file under the terms of the MIT License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence MIT.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence MIT.
// Voir LICENSE pour plus de détails.
// 
// Contact:
// => Mail: Ward727a@gmail.com
//    Please use this object: "RPG Creator [YourObject]"
// => Discord: ward727
// 
// For urgent inquiries, sending both an email and a message on Discord is highly recommended for a quicker response.
// 
// 
#endregion
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.OpenGL.Controls;
using Microsoft.Extensions.DependencyInjection;
using RPGCreator.Core;
using RPGCreator.Core.Internal;
using RPGCreator.Core.Services;
using RPGCreator.Core.Type.Project;
using RPGCreator.UI.ViewModels._Editor;

namespace RPGCreator.UI.Views.Editor;

public partial class Editor : Window
{
    public BaseProject Project { get; set; }
    public Editor()
    {
        InitializeComponent();
        //App.Services.GetRequiredService<EditorService>().CurrentProject = Project;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
    }
}