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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using RPGCreator.SDK;
using RPGCreator.SDK.Modules.Features.Entity;
using RPGCreator.SDK.Types;

namespace RPGCreator.UI.Common.Modal.Browser;

public class FeatureLibraryExplorerFilters
{
    public event Action? OnFiltersChanged;

    public string? SearchTerm
    {
        get;
        set
        {
            field = value;
            Dispatcher.UIThread.Post(() => OnFiltersChanged?.Invoke(), DispatcherPriority.Normal);
        }
    } = null;

    public bool ShowAdvanced
    {
        get;
        set
        {
            field = value;
            Dispatcher.UIThread.Post(() => OnFiltersChanged?.Invoke(), DispatcherPriority.Normal);
        }
    } = false;

    public ObservableCollection<string> PossibleItemName = new ObservableCollection<string>();
    
    public void AddPossibleItemName(string name)
    {
        if (!PossibleItemName.Contains(name))
        {
            PossibleItemName.Add(name);
        }
    }
    
    public void RemovePossibleItemName(string name)
    {
        if (PossibleItemName.Contains(name))
        {
            PossibleItemName.Remove(name);
        }
    }
}

public class FeatureLibraryExplorerDialog : UserControl
{
    
    private IEntityFeature? SelectedFeature { get; set; }

    public bool HasFeature => SelectedFeature != null;
    
    private StackPanel Body { get; set; } = null!;
    private Grid HeaderGrid { get; set; } = null!;
    public AutoCompleteBox SearchBox { get; set; } = null!;
    private ToggleSwitch ShowAdvancedToggle { get; set; } = null!;
    private ListBox FeaturesListBox { get; set; } = null!;

    private TextBlock _noResultsMessage = null!;

    private FeatureLibraryExplorerFilters _filters = new FeatureLibraryExplorerFilters();
    
    public ObservableCollection<IEntityFeature> DisplayedFeatures { get; } = new();
    private List<IEntityFeature> _allFeatures = new();
    public FeatureLibraryExplorerDialog()
    {
        CreateComponents();
        RegisterEvents();
    }
    
    private void CreateComponents()
    {
        Body = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
        };
        
        HeaderGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*, auto"),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 0, 0, 10)
        };
        SearchBox = new AutoCompleteBox
        {
            Watermark = "Search features...",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            ItemsSource = _filters.PossibleItemName,
            FilterMode = AutoCompleteFilterMode.Contains
        };
        HeaderGrid.Children.Add(SearchBox);
        Grid.SetColumn(SearchBox, 0);
        
        Body.Children.Add(HeaderGrid);
        
        ShowAdvancedToggle = new ToggleSwitch
        {
            Content = "Show Advanced",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(10, 0, 0, 0),
            OffContent = "Simple",
            OnContent = "Advanced"
        };
        HeaderGrid.Children.Add(ShowAdvancedToggle);
        Grid.SetColumn(ShowAdvancedToggle, 1);
        
        FeaturesListBox = new ListBox
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            Height = 300,
            ItemsSource = DisplayedFeatures,
            ItemTemplate = new FuncDataTemplate<IEntityFeature>(
                (data, _) =>
                {
                    if (data == null) return null;
                    return new FeatureListItem(data, _filters);
                }
                // On crée le visuel ici
            )
        };
        
        Body.Children.Add(FeaturesListBox);
        
        _noResultsMessage = new TextBlock
        {
            Text = "No features match your search.",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 20, 0, 0),
            FontStyle = Avalonia.Media.FontStyle.Italic,
            Foreground = Avalonia.Media.Brushes.Gray,
            IsVisible = false
        };

        Body.Children.Add(_noResultsMessage);
        Content = Body;
    }

    private void RegisterEvents()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        FeaturesListBox.SelectionChanged += OnFeatureSelected;
        
        SearchBox.TextChanged += (s, e) =>
        {
            _filters.SearchTerm = SearchBox.Text;
        };
        ShowAdvancedToggle.IsCheckedChanged += (s, e) =>
        {
            _filters.ShowAdvanced = ShowAdvancedToggle.IsChecked ?? false;
        };
        
        _filters.OnFiltersChanged += ApplyFilters;
    }
    
    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        LoadFeatures();
    }
    private void OnUnloaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _allFeatures.Clear();
    }
    
    private void OnFeatureSelected(object? sender, SelectionChangedEventArgs e)
    {
        if(e.AddedItems.Count > 0 && e.AddedItems[0] is IEntityFeature item)
        {
            SelectedFeature = item;
        }
    }

    private void LoadFeatures()
    {
        var features = EngineServices.FeaturesManager.GetAllEntityFeatures();

        if (features.Count == 0)
        {
            FeaturesListBox.Items.Add(new TextBlock
            {
                Text = "No features available.",
                Foreground = Avalonia.Media.Brushes.Gray,
                Margin = new Avalonia.Thickness(5)
            });
            return;
        }
        
        _allFeatures = features;
        ApplyFilters();
        
    }
    
    /// <summary>
    /// Returns the selected feature.<br/>
    /// It will do a clone of the feature to avoid modifying the template.<br/>
    /// Throws an exception if no feature is selected.
    /// </summary>
    /// <returns>The selected feature clone.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no feature is selected.</exception>
    public IEntityFeature GetSelectedFeature()
    {
        if (SelectedFeature == null)
            throw new InvalidOperationException("No feature selected.");
        return SelectedFeature.Clone();
    }
    private void ApplyFilters()
    {
        SearchBox.ItemsSource = null;
        DisplayedFeatures.Clear();
        _filters.PossibleItemName.Clear(); 

        var filtered = _allFeatures.Where(f => 
        {
            if (!_filters.ShowAdvanced && f is not BaseMacroEntityFeature) 
                return false;

            if (!string.IsNullOrWhiteSpace(_filters.SearchTerm))
            {
                var search = _filters.SearchTerm.ToLower();
                bool matches = f.FeatureName.ToLower().Contains(search) || 
                               f.FeatureUrn.ToString().ToLower().Contains(search);
                if (!matches) return false;
            }

            return true;
        });

        foreach (var feature in filtered)
        {
            DisplayedFeatures.Add(feature);
            _filters.PossibleItemName.Add(feature.FeatureName);
        }
        
        SearchBox.ItemsSource = _filters.PossibleItemName;
        
        _noResultsMessage.IsVisible = DisplayedFeatures.Count == 0;
    
        FeaturesListBox.IsVisible = DisplayedFeatures.Count > 0;
    }
    
    private class FeatureListItem : UserControl
    {
        public IEntityFeature Feature { get; set; }
        public URN Urn { get; set; }
        
        private readonly FeatureLibraryExplorerFilters _filters;
        
        public FeatureListItem(IEntityFeature feature, FeatureLibraryExplorerFilters filters)
        {
            _filters = filters;
            Feature = feature;
            Urn = feature.FeatureUrn;
            CreateComponents();
            RegisterEvents();
        }
        
        private void CreateComponents()
        {
            var body = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = new Avalonia.Thickness(5)
            };

            if (File.Exists(Feature.FeatureIcon))
            {
                var icon = new Image
                {
                    Source = EngineServices.ResourcesService.Load<Bitmap>(Feature.FeatureIcon),
                    Width = 32,
                    Height = 32,
                    Margin = new Avalonia.Thickness(0, 0, 10, 0)
                };
                body.Children.Add(icon);
            }
            else
            {
                var noIconText = new TextBlock
                {
                    Text = "[No Icon]",
                    Foreground = Avalonia.Media.Brushes.Gray,
                    Width = 32,
                    Height = 32,
                    Margin = new Avalonia.Thickness(0, 0, 10, 0),
                    TextAlignment = Avalonia.Media.TextAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };
                body.Children.Add(noIconText);
            }
            
            var nameLabel = new TextBlock
            {
                Text = Feature.FeatureName,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                Margin = new Avalonia.Thickness(0, 0, 10, 0)
            };
            
            var urnLabel = new TextBlock
            {
                Text = Urn.ToString(),
                FontStyle = Avalonia.Media.FontStyle.Italic,
                Foreground = Avalonia.Media.Brushes.Gray
            };
            
            body.Children.Add(nameLabel);
            body.Children.Add(urnLabel);
            
            Content = body;
        }

        private void RegisterEvents()
        {
        }
    }
    
}