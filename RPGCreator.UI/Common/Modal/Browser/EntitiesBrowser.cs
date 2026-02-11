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
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using RPGCreator.SDK;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Extensions;
using RPGCreator.SDK.Modules.UIModule;
using RPGCreator.UI.Contexts;

namespace RPGCreator.UI.Common.Modal.Browser;

public class EntitiesBrowser : UserControl
{
    public event Action? ShowAll;
    public event Action<string>? SearchFor;
    
    private readonly List<string> _items = new();
    private Grid BodyGrid = null!;
    private AutoCompleteBox SearchBox = null!;
    private ScrollViewer BodyScroll = null!;
    public WrapPanel Body = null!;
    
    public EntitiesBrowser()
    {
        CreateComponents();
        RegisterEvents();

        EditorUiServices.ExtensionManager.ApplyExtensions(UIRegion.EntitiesBrowser, this);
    }

    private void CreateComponents()
    {
        BodyGrid = new Grid()
        {
            RowDefinitions = new RowDefinitions("Auto, *"),
            ColumnDefinitions = new ColumnDefinitions("*"),
            MinWidth = (128+50)*3,
            MaxHeight =  (128+50)*3,
            RowSpacing = 8
        };

        SearchBox = new AutoCompleteBox()
        {
            InnerLeftContent = "Search",
            Watermark = "Search for name...",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        BodyGrid.Children.Add(SearchBox);

        BodyScroll = new ScrollViewer();
        BodyGrid.Children.Add(BodyScroll);
        Grid.SetRow(BodyScroll, 1);

        Body = new WrapPanel()
        {
            ItemHeight = 64 * 2 + 24,
            ItemWidth = 64 * 2,
            ItemSpacing = 12,
            LineSpacing = 12,
            MaxWidth = (64 * 2 + 12) * 3,
        };
        
        BodyScroll.Content = Body;

        Content = BodyGrid;
    }

    private void RegisterEvents()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SearchBox.TextChanged += OnSearching;
    }

    private void OnSearching(object? sender, TextChangedEventArgs e)
    {
        var searchTextContent = SearchBox.Text;

        if (string.IsNullOrEmpty(searchTextContent))
        {
            ShowAll?.Invoke();
        }
        else
        {
            SearchFor?.Invoke(searchTextContent);
        }
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        Body.Children.Clear();
    }

    EntityBrowserItem? _selectedItem = null;
    public IEntityDefinition? SelectedEntityDefinition => _selectedItem?._entityDefinition;
    
    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        EngineServices.OnceServiceReady((IAssetsManager assetsManager) =>
        {
            var entities = assetsManager.GetAssets<IEntityDefinition>();

            foreach (var entityDef in entities)
            {
                _items.Add(entityDef.Name);
                var item = new EntityBrowserItem(entityDef, this);
                Body.Children.Add(item);
                
                item.PointerPressed += (s, ev) =>
                {
                    if (_selectedItem != null)
                    {
                        _selectedItem.IsSelected = false;
                        _selectedItem.ItemBorder.BorderBrush = Brushes.Transparent;
                        _selectedItem.ItemBorder.BorderThickness = new Thickness(0);
                        _selectedItem.ItemBorder.Margin = new Thickness(4);
                    }

                    item.IsSelected = true;
                    item.ItemBorder.BorderBrush = Brushes.DodgerBlue;
                    item.ItemBorder.BorderThickness = new Thickness(2);
                    item.ItemBorder.Margin = new Thickness(2);
                    _selectedItem = item;
                };
            }
            SearchBox.ItemsSource = _items;
        });
    }
}

public class EntityBrowserItem : UserControl
{

    public bool IsSelected { get; set; } = false;
    [ExposePropToPlugin("EntitiesBrowser.Item", false)]
    public  IEntityDefinition _entityDefinition { get; private set; }

    private EntitiesBrowser _browser;
    
    public Border ItemBorder { get; private set; } = null!;
    [ExposePropToPlugin("EntitiesBrowser.Item", false)]
    public Grid ItemBody { get; private set; } = null!;
    [ExposePropToPlugin("EntitiesBrowser.Item", false)]
    public Image ItemImage { get; private set; } = null!;
    [ExposePropToPlugin("EntitiesBrowser.Item", false)]
    public TextBlock ItemName { get; private set; } = null!;
    
    public EntityBrowserItem(IEntityDefinition entityDef, EntitiesBrowser browser)
    {
        _entityDefinition = entityDef;
        _browser = browser;
        CreateComponents();
        RegisterEvents();

        var config = new EntitiesBrowserItemContext.Config()
        {
            Get_entityDefinition = () => _entityDefinition,
            GetItemBody =  () => ItemBody,
            GetItemImage =  () => ItemImage,
            GetItemName = () => ItemName,
        };
        
        EditorUiServices.ExtensionManager.ApplyExtensions(UIRegion.EntitiesBrowserItem, this, new EntitiesBrowserItemContext(config));
    }

    private void CreateComponents()
    {
        ItemBorder = new Border()
        {
            Padding = new Thickness(4),
            Background = Brushes.Transparent,
            CornerRadius = new CornerRadius(4),
            Margin = new Thickness(4)
        };
        Content = ItemBorder;
        
        ItemBody = new Grid()
        {
            RowDefinitions = new RowDefinitions("*, 24"),
            ColumnDefinitions = new ColumnDefinitions("*"),
        };
        ItemBorder.Child = ItemBody;

        ItemImage = new Image()
        {
            Source = EngineServices.ResourcesService.Load<Bitmap>(_entityDefinition.SpritePath),
        };
        Grid.SetRow(ItemImage, 0);
        ItemBody.Children.Add(ItemImage);

        ItemName = new TextBlock()
        {
            Text = _entityDefinition.Name,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetRow(ItemName, 1);
        ItemBody.Children.Add(ItemName);
    }

    private void RegisterEvents()
    {
        
        _browser.ShowAll += () =>
        {
            this.IsVisible = true;
        };

        _browser.SearchFor += (searchForName) =>
        {
            if (!_entityDefinition.Name.ToLowerInvariant().StartsWith(searchForName.ToLowerInvariant()))
            {
                this.IsVisible = false;
            }
            else
            {
                this.IsVisible = true;
            }
        };
        
        PointerEntered += OnPointerEntered;
        PointerExited += OnPointerExited;
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        ItemBorder.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.LightGray, 0.3);

        Cursor = new Cursor(StandardCursorType.Hand);
    }
    
    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        ItemBorder.Background = Brushes.Transparent;
        Cursor = new Cursor(StandardCursorType.Arrow);
    }
}