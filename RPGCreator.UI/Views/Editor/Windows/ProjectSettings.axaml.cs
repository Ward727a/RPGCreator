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
using Avalonia.VisualTree;
using RPGCreator.UI.ViewModels._Editor.Windows._ProjectSettings;
using RPGCreator.UI.Views.Editor._ProjectSettings;
using System.Linq;

namespace RPGCreator.UI.Views.Editor.Windows;

public partial class ProjectSettings : Window
{
    private static ProjectSettings? _instance;
    private Information information;
    private AssetsTilesets assetsTilesets;
    private Test test;

    public ProjectSettings()
    {
        _instance = this;
        InitializeComponent();
    }

    static public ProjectSettings? GetInstance()
    {
        return _instance;
    }

    private void NavInformation_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        information ??= new Information();
        Scroller.Content = information;
    }
    private void NavTest_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        test ??= new Test();
        Scroller.Content = test;
    }
    private void NavAssets_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        test ??= new Test();
        Scroller.Content = test;
    }

    private void NavAssetTilesets_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        assetsTilesets ??= new AssetsTilesets();
        Scroller.Content = assetsTilesets;
    }

    private void Save_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ((ViewModelSettings)((UserControl)Scroller.Content!).DataContext!).Save();
    }
}