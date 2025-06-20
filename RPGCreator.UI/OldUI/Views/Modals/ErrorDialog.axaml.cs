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
using Avalonia.Markup.Xaml;
using RPGCreator.UI.OLD.ViewModels.Modals;
using RPGCreator.UI.OLD.Views.Abstract;
using System;
using System.Collections.Generic;

namespace RPGCreator.Modals;

public partial class ErrorDialog : ModalBase<ErrorDialog>
{
    public ErrorDialog(string title, string content)
    {
        InitializeComponent();

        DataContext = new ErrorDialogViewModel(title, content);
    }

    public static ErrorDialog Create(Dictionary<string, object> data)
    {
        return new((string)data["title"], (string)data["content"]);
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close("Ok Clicked!");
    }
}