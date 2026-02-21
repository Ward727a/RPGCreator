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
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Characters;
using RPGCreator.SDK.Types.Collections;

namespace RPGCreator.UI.Common.CharacterCommonComponents;

public class CharacterExplorer : UserControl
{
    public event Action<CharacterData>? CharacterSelected;
    
    private IAssetScope _scope;
 
    private ObservableCollection<CharacterData> _characters = new();
    private ObservableCollection<CharacterData> _sortedCharacters = new();
    
    #region Components
    
    private Grid _body;
    private TextBox _autoCompleteBox;
    private ScrollViewer _scrollViewer;
    private ListBox _itemsBox;
    
    #endregion
    
    public CharacterExplorer(
        IAssetScope scope)
    {
        _scope = scope ?? throw new ArgumentNullException(nameof(scope), "Asset scope cannot be null.");
        
        Dispatcher.UIThread.Post(LoadContent);
        CreateComponents();
        RegisterEvents();
        LinkToExtension();
        Content = _body;
    }

    private void LoadContent()
    {
        _characters = new(EngineServices.AssetsManager.GetAssetsOfType<CharacterData>());
        _sortedCharacters = new(_characters.OrderBy(c => c.Name));
    }

    private void CreateComponents()
    {
        _body = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*"),
            RowSpacing = 4
        };

        _autoCompleteBox = new()
        {
            Watermark = "Search for a character...",
        };
        _body.Children.Add(_autoCompleteBox);
        
        _scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
        };
        _body.Children.Add(_scrollViewer);
        Grid.SetRow(_scrollViewer, 1);

        _itemsBox = new()
        {
            ItemsSource = _sortedCharacters,
            ItemTemplate = new FuncDataTemplate<CharacterData>(((data, scope) =>
            {
                if (data == null) return null;
                var characterIconPath = data.PortraitPath;
                var characterIcon = EngineServices.ResourcesService.Load<Bitmap>(characterIconPath);
                var image = new Image
                {
                    Source = characterIcon,
                    Width = 32,
                    Height = 32,
                    Margin = new Thickness(4),
                    VerticalAlignment = VerticalAlignment.Center
                };
                var textBlock = new TextBlock
                {
                    Text = data.Name,
                    Margin = new Thickness(4),
                    VerticalAlignment = VerticalAlignment.Center
                };
                var stackPanel = new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Children =
                    {
                        image,
                        textBlock
                    }
                };
                return stackPanel;
            }))
        };
        _scrollViewer.Content = _itemsBox;
    }

    private void RegisterEvents()
    {
        _autoCompleteBox.TextChanged += (sender, args) =>
        {
            var searchText = _autoCompleteBox.Text?.Trim().ToLower() ?? "";
            Dispatcher.UIThread.Post(() => FilterCharacters(searchText));
        };
        
        _itemsBox.SelectionChanged += (sender, args) =>
        {
            if(_itemsBox.SelectedItem is CharacterData selectedCharacter)
                CharacterSelected?.Invoke(selectedCharacter);
        };
    }

    private void LinkToExtension()
    {
    }
    
    
    private void FilterCharacters(string searchText)
    {
        _sortedCharacters.Clear();
        var filtered = _characters.Where(c => c.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)).OrderBy(c => c.Name);
        _sortedCharacters.AddRange(filtered);
    }
}