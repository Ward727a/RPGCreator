#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
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
// 
// 
#endregion
using Avalonia.Controls;
using RPGCreator.UI.Content.Editor;
using RPGCreator.UI.Content.ProjectCreator;
using System;
using RPGCreator.SDK;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Projects;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;
using RPGCreator.UI.Content.Preferences;

namespace RPGCreator.UI.Content.Launcher
{
    public class LauncherWindowControl : UserControl
    {
        
        private Window _Host => (Window)TopLevel.GetTopLevel(this)!;

        private IBaseProject? _selectedProject;
        private bool _IsProjectSelected => _selectedProject != null;

        #region components

        private StackPanel _ProjectStackPanel;

        private Button _OpenButton;
        private Button _DeleteButton;

        private TextBlock _ProjectName;
        private TextBlock _ProjectDescription;
        private TextBlock _ProjectLastModified;
        private Grid _mainGrid;
        private Grid _contentGrid;
        private TextBlock _header;
        private Button _openPreferences;
        private TextBlock _footer;
        private Grid _projectListGrid;
        private Grid _projectListOptionsGrid;
        private TextBox _searchBox;
        private Button _newProjectButton;
        private StackPanel _projectDetailsPanel;
        private Grid _detailButtons;

        #endregion

        public LauncherWindowControl() : base()
        {
            _mainGrid = new Grid
            {
                ShowGridLines = true,
                RowDefinitions = new RowDefinitions("Auto, *, Auto")
            };


            _contentGrid = new Grid
            {
                ShowGridLines = true,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                ColumnDefinitions = new ColumnDefinitions("*, Auto")
            };
            _mainGrid.Children.Add(_contentGrid);
            Grid.SetRow(_contentGrid, 1);

            _header = new TextBlock
            {
                Text = "RPG Creator Launcher",
                FontSize = App.style.TitleFontSize,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                Margin = App.style.Margin
            };
            _mainGrid.Children.Add(_header);
            Grid.SetRow(_header, 0);

            _openPreferences = new Button()
            {
                Content = "Preferences",
                FontSize = App.style.TextFontSize,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = new(0, 0, 10, 0)
            };
            _mainGrid.Children.Add(_openPreferences);
            Grid.SetRow(_openPreferences, 0);

            _openPreferences.Click += (sender, e) =>
            {
                var preferencesWindow = new PreferencesWindow();
                preferencesWindow.ShowDialog(_Host);
            };

            _footer = new TextBlock
            {
                Text = "RPG Creator - Open-source RPG Engine. (c) 2025 Ward",
                FontSize = App.style.SmallTextFontSize,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom,
                Margin = App.style.Margin
            };
            _mainGrid.Children.Add(_footer);
            Grid.SetRow(_footer, 2);

            _projectListGrid = new Grid
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                RowDefinitions = new RowDefinitions("Auto, *"),
            };
            _contentGrid.Children.Add(_projectListGrid);
            Grid.SetColumn(_projectListGrid, 0);

            _projectListOptionsGrid = new Grid
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                Margin = App.style.Margin,
                ColumnDefinitions = new ColumnDefinitions("*, Auto")
            };
            _projectListGrid.Children.Add(_projectListOptionsGrid);
            Grid.SetRow(_projectListOptionsGrid, 0);

            _searchBox = new TextBox
            {
                Watermark = "Search projects...",
                FontSize = App.style.TextFontSize,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = new(0, 0, 10, 0)
            };
            _projectListOptionsGrid.Children.Add(_searchBox);
            
            _newProjectButton = new Button
            {
                Content = "New Project",
                FontSize = App.style.TextFontSize,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };
            _projectListOptionsGrid.Children.Add(_newProjectButton);
            _newProjectButton.Click += OnNewProjectButtonClick;
            Grid.SetColumn(_newProjectButton, 1);

            _ProjectStackPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                Margin = App.style.Margin,
                Spacing = 0
            };
            _projectListGrid.Children.Add(_ProjectStackPanel);
            Grid.SetRow(_ProjectStackPanel, 1);

            _projectDetailsPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                Margin = App.style.Margin
            };

            _contentGrid.Children.Add(_projectDetailsPanel);
            Grid.SetColumn(_projectDetailsPanel, 1);
            
            _ProjectName = new TextBlock
            {
                Text = "Project Name: None",
                FontSize = App.style.TextFontSize,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top
            };
            _projectDetailsPanel.Children.Add(_ProjectName);

            _ProjectDescription = new TextBlock
            {
                Text = "Description: None.",
                FontSize = App.style.MediumTextFontSize,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top
            };
            _projectDetailsPanel.Children.Add(_ProjectDescription);

            _detailButtons = new Grid
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = App.style.Margin,
                ColumnDefinitions = new ColumnDefinitions("Auto, 4, Auto")
            };
            _projectDetailsPanel.Children.Add(_detailButtons);

            _OpenButton = new Button
            {
                Content = "Open Project",
                FontSize = App.style.TextFontSize,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                IsEnabled = _IsProjectSelected // Initially disabled, enable when a project is selected
            };
            _detailButtons.Children.Add(_OpenButton);
            _OpenButton.Click += OnOpenButtonClick;

            _DeleteButton = new Button
            {
                Content = "Delete Project",
                FontSize = App.style.TextFontSize,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                IsEnabled = _IsProjectSelected // Initially disabled, enable when a project is selected
            };
            _detailButtons.Children.Add(_DeleteButton);
            _DeleteButton.Click += OnDeleteButtonClick;
            Grid.SetColumn(_DeleteButton, 2);

            RefreshProjectList();

            Content = _mainGrid;
        }

        private void RefreshProjectDetails()
        {
            Logger.Info("Refreshing project details.");
            // Update the project details panel with the selected project's information
            if (_selectedProject != null)
            {
                _ProjectName.Text = $"Project Name: {_selectedProject.MetaData.Name}";
                _ProjectDescription.Text = $"Description: {_selectedProject.MetaData.Description}";
            }
            else
            {
                _ProjectName.Text = new StringName("No project selected.");
                _ProjectDescription.Text = string.Empty;
            }
        }

        private void RefreshButtonsState()
        {
            // Enable or disable buttons based on whether a project is selected
            Logger.Info($"Refreshing buttons state. Is project selected: {_IsProjectSelected}");
            _DeleteButton.IsEnabled = _IsProjectSelected;
            _OpenButton.IsEnabled = _IsProjectSelected;
        }

        private void RefreshProjectList()
        {
            // Logic to refresh the project list, e.g., reloading from disk or updating UI
            Logger.Info("Refreshing project list.");
            // This could involve clearing the existing items and re-adding them
            // Reset the project details and buttons state

            _selectedProject = null;

            RefreshProjectDetails();
            RefreshButtonsState();

            // Clear the existing project items in the stack panel
            _ProjectStackPanel.Children.Clear();

            // Add a list of projects to the projectStackPanel
            var projectLinks = EngineServices.ProjectsManager.GetAllProjects();

            foreach (var projectLink in projectLinks)
            {
                if (!EngineServices.ProjectsManager.TryGetProject(projectLink.ProjectLastKnownPath, out var project))
                    continue;
                
                var projectItem = new LauncherProjectItem(project);
                _ProjectStackPanel.Children.Add(projectItem);
                projectItem.ProjectSelected += OnSelectProject;
            }
        }

        #region EventHandler
        private void OnNewProjectButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Logic to create a new project
            Logger.Info("New Project button clicked.");
            // This should open a dialog to create a new project
            var newProjectDialog = new ProjectCreatorWindow();
            newProjectDialog.Control.ProjectCreated += Control_ProjectCreated;
            newProjectDialog.ShowDialog(_Host);
        }

        private void Control_ProjectCreated(object? sender, EventArgs e)
        {
            RefreshProjectList();
        }

        private void OnOpenButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // We check if a project is selected before proceeding
            if (!_IsProjectSelected)
            {
                Logger.Info("No project selected to open.");
                RefreshButtonsState();
                RefreshProjectDetails();
                return;
            }
            if( _selectedProject == null)
            {
                Logger.Error("Selected project is null despite being marked as selected.");
                return;
            }
            EngineServices.ProjectsManager.OpenProject(_selectedProject);
            Logger.Info("Open Project button clicked.");
            //this._Host.Close(); // Close the launcher window
            var ew = EditorWindow.Instance; 
        }

        private void OnDeleteButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // We check if a project is selected before proceeding
            if (!_IsProjectSelected)
            {
                Logger.Info("No project selected to delete.");
                RefreshButtonsState();
                RefreshProjectDetails();
                return;
            }

            // Logic to delete the project
            Logger.Info("Delete Project button clicked.");
            // This should prompt the user for confirmation and then delete the project

        }

        private void OnSelectProject(object? sender, IBaseProject project)
        {
            // Logic to handle project selection
            // Enable buttons and update details panel with the selected project's information
            Logger.Info($"Project selected: {project.MetaData.Name}");
            _selectedProject = project;

            RefreshButtonsState();
            RefreshProjectDetails();
        }
        #endregion
    }
}
