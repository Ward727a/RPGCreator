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

using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using RPGCreator.SDK;
using RPGCreator.SDK.EditorUiService;
using Ursa.Controls;

namespace RPGCreator.UI.Content.Preferences.Components.Projects;

public class ProjectSettingsControl : UserControl
{

    private Grid _bodyGrid = null!;
    private Button _saveButton = null!;
    
    private ScrollViewer _formScrollViewer = null!;
    private StackPanel _formBody = null!;
    
    private Label _projectNameLabel = null!;
    private TextBox _projectNameTextBox = null!;
    
    private Label _projectDescriptionLabel = null!;
    private TextBox _projectDescriptionTextBox = null!;
    
    private Label _projectPathLabel = null!;
    private TextBlock _projectPathTextBlock = null!;
    
    private Label _projectAuthorsLabel = null!;
    private TagInput _projectAuthorsTagInput = null!;
    
    private Label _projectUsingModulesLabel = null!;
    private TextBox _projectUsingModulesTextBox = null!;
    
    private Label _projectMainMapLabel = null!;
    private ComboBox _projectMainMapComboBox = null!;
    
    public ProjectSettingsControl()
    {
        CreateComponents();
        RegisterEvents();
        LinkToExtension();
        Content = _bodyGrid;
    }

    private void CreateComponents()
    {

        _bodyGrid = new Grid()
        {
            RowDefinitions = new RowDefinitions("*, Auto"),
        };
        
        _formScrollViewer = new ScrollViewer();
        _bodyGrid.Children.Add(_formScrollViewer);
        Grid.SetRow(_formScrollViewer, 0);
        
        _formBody = new StackPanel()
        {
            Margin = new Thickness(10),
            Spacing = 4
        };
        _formScrollViewer.Content = _formBody;
        
        var currentProject = GlobalStates.ProjectState.CurrentProject;
        if (currentProject == null)
        {
            // This should, in fact, never happen, but just in case.
            _formScrollViewer.Content = new TextBlock()
            {
                Text = "No project loaded.",
            };
            return;
        }
        
        _projectNameTextBox = new TextBox()
        {
            VerticalAlignment = VerticalAlignment.Center,
            Text = currentProject.Name
        };
        MakeItem(ref _projectNameLabel, _projectNameTextBox, "Project Name");
        
        _projectDescriptionTextBox = new TextBox()
        {
            VerticalAlignment = VerticalAlignment.Center,
            Text = currentProject.Description,
            AcceptsReturn = true,
            Height = 100
        };
        MakeItem(ref _projectDescriptionLabel, _projectDescriptionTextBox, "Project Description");
        
        _projectPathTextBlock = new TextBlock()
        {
            VerticalAlignment = VerticalAlignment.Center,
            Text = currentProject.Path,
            Foreground = Brushes.Gray,
            FontStyle = FontStyle.Italic
        };
        MakeItem(ref _projectPathLabel, _projectPathTextBlock, "Project Path");
        
        _projectAuthorsTagInput = new TagInput()
        {
            VerticalAlignment = VerticalAlignment.Center,
            Watermark = "Add an author and press Enter"
        };

        foreach (var author in currentProject.Authors)
        {
            _projectAuthorsTagInput.Tags.Add(author);
        }
        
        MakeItem(ref _projectAuthorsLabel, _projectAuthorsTagInput, "Project Authors");

        _projectMainMapComboBox = new ComboBox()
        {
            VerticalAlignment = VerticalAlignment.Center,
        };
        MakeItem(ref _projectMainMapLabel, _projectMainMapComboBox, "Project Main Map");
        
        _saveButton = new Button()
        {
            Content = "Save",
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(10)
        };
        _bodyGrid.Children.Add(_saveButton);
        Grid.SetRow(_saveButton, 1);
    }

    private void RegisterEvents()
    {
        _saveButton.Click += OnSave;
    }

    private void OnSave(object? sender, RoutedEventArgs e)
    {
        var currentProject = GlobalStates.ProjectState.CurrentProject;
        if (currentProject == null)
        {
            // This should, in fact, never happen, but just in case.
            return;
        }
        
        if(currentProject.Name != _projectNameTextBox.Text)
        {
            currentProject.Name = _projectNameTextBox.Text;
        }
        if(currentProject.Description != _projectDescriptionTextBox.Text)
        {
            currentProject.Description = _projectDescriptionTextBox.Text;
        }
        var authors = _projectAuthorsTagInput.Tags.ToList();
        if(!authors.SequenceEqual(currentProject.Authors))
        {
            currentProject.Authors.Clear();
            currentProject.Authors.AddRange(authors);
        }
        
        currentProject.Save();
        
        EditorUiServices.NotificationService.Success("Project settings saved.", "", new NotificationOptions(4_000));
    }

    private void LinkToExtension()
    {
    }

    private void MakeItem(ref Label label, Control control, string LabelText)
    {
        var grid = new Grid()
        {
            RowDefinitions = new RowDefinitions("*"),
            ColumnDefinitions = new ColumnDefinitions("Auto, *"),
            ColumnSpacing = 4
        };
        label = new Label()
        {
            Content = LabelText,
            VerticalAlignment = VerticalAlignment.Center
        };
        
        label.Target = control;
        
        grid.Children.Add(label);
        Grid.SetColumn(label, 0);
        
        grid.Children.Add(control);
        Grid.SetColumn(control, 1);
        
        _formBody.Children.Add(grid);
    }
}