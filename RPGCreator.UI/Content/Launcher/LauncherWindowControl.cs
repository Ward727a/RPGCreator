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
using Avalonia.VisualTree;
using RPGCreator.UI.Content.Editor;
using RPGCreator.UI.Content.ProjectCreator;
using System;
using RPGCreator.SDK;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Projects;
using RPGCreator.SDK.Types.Collections;
using RPGCreator.SDK.Types.Interfaces;

namespace RPGCreator.UI.Content.Launcher
{
    public class LauncherWindowControl : UserControl
    {

        public class TestSlab : ISlabItem
        {
            public int? SlabPointerIndex { get; set; }
        }
        
        private readonly Slabs<TestSlab> _testSlabs = new Slabs<TestSlab>(4);
        
        private int _slabPointerCounter = 0;
        
        private Window _Host => (Window)this.GetVisualRoot()!;

        private IBaseProject? _selectedProject;
        private bool _IsProjectSelected => _selectedProject != null;

        #region components

        private StackPanel _ProjectStackPanel;

        private Button _OpenButton;
        private Button _DeleteButton;

        private TextBlock _ProjectName;
        private TextBlock _ProjectDescription;
        private TextBlock _ProjectLastModified;

        #endregion

        public LauncherWindowControl() : base()
        {
            var MainGrid = new Grid
            {
                ShowGridLines = true,
                RowDefinitions = new RowDefinitions("Auto, *, Auto")
            };

            var ContentGrid = new Grid
            {
                ShowGridLines = true,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                ColumnDefinitions = new ColumnDefinitions("*, Auto")
            };
            MainGrid.Children.Add(ContentGrid);
            Grid.SetRow(ContentGrid, 1);

            var header = new TextBlock
            {
                Text = "RPG Creator Launcher",
                FontSize = App.style.TitleFontSize,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                Margin = App.style.Margin
            };
            MainGrid.Children.Add(header);
            Grid.SetRow(header, 0);

            var footer = new TextBlock
            {
                Text = "RPG Creator - Open-source RPG Engine. (c) 2025 Ward",
                FontSize = App.style.SmallTextFontSize,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom,
                Margin = App.style.Margin
            };
            MainGrid.Children.Add(footer);
            Grid.SetRow(footer, 2);

            var projectListGrid = new Grid
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                RowDefinitions = new RowDefinitions("Auto, *"),
            };
            ContentGrid.Children.Add(projectListGrid);
            Grid.SetColumn(projectListGrid, 0);

            var projectListOptionsGrid = new Grid
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                Margin = App.style.Margin,
                ColumnDefinitions = new ColumnDefinitions("*, Auto")
            };
            projectListGrid.Children.Add(projectListOptionsGrid);
            Grid.SetRow(projectListOptionsGrid, 0);

            var searchBox = new TextBox
            {
                Watermark = "Search projects...",
                FontSize = App.style.TextFontSize,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = new(0, 0, 10, 0)
            };
            projectListOptionsGrid.Children.Add(searchBox);
            
            var newProjectButton = new Button
            {
                Content = "New Project",
                FontSize = App.style.TextFontSize,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };
            projectListOptionsGrid.Children.Add(newProjectButton);
            newProjectButton.Click += OnNewProjectButtonClick;
            Grid.SetColumn(newProjectButton, 1);

            _ProjectStackPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                Margin = App.style.Margin,
                Spacing = 0
            };
            projectListGrid.Children.Add(_ProjectStackPanel);
            Grid.SetRow(_ProjectStackPanel, 1);

            var projectDetailsPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                Margin = App.style.Margin
            };

            ContentGrid.Children.Add(projectDetailsPanel);
            Grid.SetColumn(projectDetailsPanel, 1);
            
            var TestAddSlabButton = new Button
            {
                Content = "Test Add Slab",
                FontSize = App.style.TextFontSize,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };
            
            var TestAddTo0Button = new Button
            {
                Content = "Test Add To 1",
                FontSize = App.style.TextFontSize,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };
            projectDetailsPanel.Children.Add(TestAddTo0Button);
            TestAddTo0Button.Click += (sender, args) =>
            {
                var newSlab = new TestSlab();
                _testSlabs.AddItem(1, newSlab);
                Logger.Info($"Added new slab to index 0. Pointer index: {newSlab.SlabPointerIndex}");
            };
            
            projectDetailsPanel.Children.Add(TestAddSlabButton);
            TestAddSlabButton.Click += (sender, args) =>
            {
                var newSlab = new TestSlab();
                _testSlabs.Allocate(newSlab);
                _slabPointerCounter++;
                Logger.Info($"Added new slab. Pointer index: {newSlab.SlabPointerIndex}");
            };
            
            var TestDebugSlabsButton = new Button
            {
                Content = "Test Debug Slabs",
                FontSize = App.style.TextFontSize,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };
            projectDetailsPanel.Children.Add(TestDebugSlabsButton);
            TestDebugSlabsButton.Click += (sender, args) =>
            {
                Logger.Info("Debugging slabs:");
                _testSlabs.DEBUG_PRINT_SCHEMA_BLOCKS();
            };
            
            var TestDebugRemoveRandomSlabButton = new Button
            {
                Content = "Test Remove 0",
                FontSize = App.style.TextFontSize,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };
            projectDetailsPanel.Children.Add(TestDebugRemoveRandomSlabButton);
            TestDebugRemoveRandomSlabButton.Click += (sender, args) =>
            {
                if (_slabPointerCounter == 0)                
                {
                    Logger.Warning("No slabs to remove.");
                    return;
                }
                var random = new Random();
                int randomIndex = random.Next(0, _slabPointerCounter);
                _testSlabs.DeallocateBlock(0);
                Logger.Info($"Removed slab at pointer index: {randomIndex}");
            };

            _ProjectName = new TextBlock
            {
                Text = "Project Name: None",
                FontSize = App.style.TextFontSize,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top
            };
            projectDetailsPanel.Children.Add(_ProjectName);

            _ProjectDescription = new TextBlock
            {
                Text = "Description: None.",
                FontSize = App.style.MediumTextFontSize,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top
            };
            projectDetailsPanel.Children.Add(_ProjectDescription);

            _ProjectLastModified = new TextBlock
            {
                Text = "Last Modified (WIP): " + DateTime.Now.ToString("g"),
                FontSize = App.style.MediumTextFontSize,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top
            };
            projectDetailsPanel.Children.Add(_ProjectLastModified);

            var detail_buttons = new Grid
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = App.style.Margin,
                ColumnDefinitions = new ColumnDefinitions("Auto, 4, Auto")
            };
            projectDetailsPanel.Children.Add(detail_buttons);

            _OpenButton = new Button
            {
                Content = "Open Project",
                FontSize = App.style.TextFontSize,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                IsEnabled = _IsProjectSelected // Initially disabled, enable when a project is selected
            };
            detail_buttons.Children.Add(_OpenButton);
            _OpenButton.Click += OnOpenButtonClick;

            _DeleteButton = new Button
            {
                Content = "Delete Project",
                FontSize = App.style.TextFontSize,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                IsEnabled = _IsProjectSelected // Initially disabled, enable when a project is selected
            };
            detail_buttons.Children.Add(_DeleteButton);
            _DeleteButton.Click += OnDeleteButtonClick;
            Grid.SetColumn(_DeleteButton, 2);

            RefreshProjectList();

            Content = MainGrid;
        }

        private void RefreshProjectDetails()
        {
            Logger.Info("Refreshing project details.");
            // Update the project details panel with the selected project's information
            if (_selectedProject != null)
            {
                _ProjectName.Text = $"Project Name: {_selectedProject.Name}";
                _ProjectDescription.Text = $"Description: {_selectedProject.Description}";
                _ProjectLastModified.Text = $"Last Modified: {DateTime.Now.ToString("g")}"; // Placeholder for last modified date
            }
            else
            {
                _ProjectName.Text = "No project selected.";
                _ProjectDescription.Text = string.Empty;
                _ProjectLastModified.Text = string.Empty;
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
                if (EngineServices.ProjectsManager.TryGetProject(projectLink.ProjectConfigPath, out var project))
                {
                    var projectItem = new LauncherProjectItem(project);
                    _ProjectStackPanel.Children.Add(projectItem);
                    projectItem.ProjectSelected += OnSelectProject;
                    Logger.Info($"Found project: {project.Name} at path {project.Path}");
                }
                else
                {
                    Logger.Error("Project link with Project ID {projectId}({path}) could not be resolved to a project.", projectLink.ProjectID, projectLink.ProjectConfigPath);
                }
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
            Logger.Info($"Project selected: {project.Name}");
            _selectedProject = project;

            RefreshButtonsState();
            RefreshProjectDetails();
        }
        #endregion
    }
}
