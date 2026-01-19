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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System;
using RPGCreator.SDK;
using Ursa.Controls;

namespace RPGCreator.UI.Content.ProjectCreator
{
    public class ProjectCreatorWindowControl : UserControl
    {
        public event EventHandler? ProjectCreated;

        #region Components
        private TextBox _NameInput, _DescriptionInput;
        private PathPicker _FolderInput;
        private Button _CreateProject;
        #endregion

        private string _ProjectName = string.Empty;
        private string _ProjectDescription = string.Empty;
        private string _ProjectPath = string.Empty;

        private bool _IsProjectReady =>
            !string.IsNullOrEmpty(_ProjectName) &&
            !string.IsNullOrEmpty(_ProjectPath);

        private Window _Host => (Window)this.GetVisualRoot()!;
        public ProjectCreatorWindowControl()
        {
            // Initialize the control, this could include setting up UI elements, event handlers, etc.
            // For now, we will just set the content to a simple TextBlock as a placeholder.
            var mainPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                Margin = App.style.Margin
            };

            mainPanel.Children.Add(CreateInput("Name", out _NameInput));
            _NameInput.TextChanged += OnNameChanged;

            mainPanel.Children.Add(CreateInput("Description", out _DescriptionInput));
            _DescriptionInput.TextChanged += OnDescriptionChanged;

            mainPanel.Children.Add(CreateInput("", out _FolderInput));
            _FolderInput.PropertyChanged += OnPathChanged;
            _FolderInput.UsePickerType = UsePickerTypes.OpenFolder;
            _FolderInput.Title = "Select Project Folder";

            _CreateProject = new Button
            {
                Content = "Create Project",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                IsEnabled = _IsProjectReady
            };
            _CreateProject.Click += OnCreateProjectClicked;

            mainPanel.Children.Add(_CreateProject);

            Content = mainPanel;
        }

        private Grid CreateInput<InputType>(string labelText, out InputType inputControl) where InputType : Control
        {
            var grid = new Grid
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Margin = App.style.Margin,
                ColumnDefinitions = new Avalonia.Controls.ColumnDefinitions("Auto, *")
            };
            if (!string.IsNullOrEmpty(labelText))
            {
                var label = new TextBlock
                {
                    Text = labelText,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Margin = new Avalonia.Thickness(0, 0, 10, 0)
                };

                grid.Children.Add(label);
            }
            inputControl = Activator.CreateInstance<InputType>();
            if (inputControl is Control control)
            {
                control.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
                control.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                grid.Children.Add(control);
                Grid.SetColumn(control, 1);
            }
            return grid;
        }

        private void RefreshState()
        {
            // This method can be used to refresh the state of the control,
            // For example, to unlock the "Create Project" button when all (required) fields are filled.
            _CreateProject.IsEnabled = _IsProjectReady;
        }

        #region EventsHandlers

        private void OnNameChanged(object? sender, TextChangedEventArgs e)
        {
            _ProjectName = _NameInput.Text!.Trim();
            RefreshState();
        }

        private void OnDescriptionChanged(object? sender, TextChangedEventArgs e)
        {
            _ProjectDescription = _DescriptionInput.Text!.Trim();
        }

        private void OnPathChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.Name == nameof(PathPicker.SelectedPaths))
            {
                _ProjectPath = _FolderInput.SelectedPaths[0]?.Trim() ?? string.Empty;
                RefreshState();
            }
        }

        private void OnCreateProjectClicked(object? sender, RoutedEventArgs e)
        {
            // Handle the project creation logic here
            // This could include saving the project details to a file, initializing the project structure, etc.
            // For now, we will just show a message box with the project details as a placeholder.
            if (!_IsProjectReady)
            {
                RefreshState();
                return;
            }

            var project = EngineServices.ProjectsManager.CreateProject(_ProjectName, _ProjectPath);
            if(project == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                // Show an error message if the project creation failed
                Console.WriteLine($"Project {_ProjectName} couldn't be created.");
                Console.ResetColor();
                //App.ShowError("Project creation failed", "An error occurred while creating the project. Please check the logs for more details.");
                return;
            }
            project.Description = _ProjectDescription;

            project.Save();

            ProjectCreated?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}
