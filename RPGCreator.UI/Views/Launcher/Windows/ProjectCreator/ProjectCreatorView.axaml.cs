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
using Avalonia.Platform.Storage;
using RPGCreator.UI.ViewModels.Launcher.Windows;
using System.Threading.Tasks;

namespace RPGCreator.UI.Views.Launcher;

public partial class ProjectCreatorView : UserControl
{
    public ProjectCreatorView()
    {
        InitializeComponent();
        DataContext = new ProjectCreatorViewModel(this);
        CopyrightChoice.SelectedIndex = 0;
    }

    private async Task Button_Clicka(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {

        var topLevel = TopLevel.GetTopLevel(this);

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Choose a folder where your project will be created",
            AllowMultiple = false
        });

        if(folders.Count >= 1)
        {
            string folderPath = folders[0].TryGetLocalPath();

            if (!string.IsNullOrEmpty(folderPath) && DataContext is ProjectCreatorViewModel)
            {
                ((ProjectCreatorViewModel)DataContext).Path = folderPath;
            }
        }

    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Button_Clicka(sender, e);
    }

    private void Button_Click_2(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ((ProjectCreatorViewModel)DataContext).RemoveAuthorCommand.Execute(((Button)e.Source).Tag);
    }

    private void ComboBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        ((ProjectCreatorViewModel)DataContext).CopyrightIndexChosen = ((ComboBox)sender).SelectedIndex;
    }
}