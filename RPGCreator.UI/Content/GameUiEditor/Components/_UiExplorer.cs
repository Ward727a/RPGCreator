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

using System.Numerics;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using RPGCreator.SDK.GameUI.Controls;
using Ursa.Controls;
using Color = RPGCreator.Shared.Types.Color;

namespace RPGCreator.UI.Content.GameUiEditor.Components;

public class _UiExplorer : UserControl
{

    private class UiTreeExplorer : UserControl
    {
        public readonly _UiExplorer Explorer;

        private readonly StackPanel _panel = new StackPanel();
        private readonly Divider _divider = new Divider();
        private readonly TreeView _treeView = new TreeView();
        
        public UiTreeExplorer(_UiExplorer explorer)
        {
            this.Explorer = explorer;
            Name="Tree Explorer";
            CreateComponents();
            RegisterEvents();

            Explorer._parent.EditorReady += () =>
            {
                _treeView.ItemsSource = Explorer._parent.UiViewport.UiManager.GetRootControls();
            };
        }

        private void CreateComponents()
        {
            _panel.Spacing = 8;
            
            _divider.Content = new TextBlock { Text = "UI Hierarchy" };
            _divider.HorizontalContentAlignment = HorizontalAlignment.Center;
            _panel.Children.Add(_divider);

            _treeView.DataTemplates.Add(new TreeDataTemplate()
            {
                DataType = typeof(BaseControl),
                ItemsSource = new Binding("Children"),
                
            });
            FuncDataTemplate<BaseControl> template = null;
            
            // need to convert all of this because **AvAlOnIa**
            template = new FuncDataTemplate<BaseControl>((data, _) =>
            {
                return new TextBlock()
                {
                    Text = data.Name,
                };
            });
            
            _treeView.ItemTemplate = template;
            _treeView.ItemContainerTheme = new ControlTheme()
            {
                Setters = { new Setter(BackgroundProperty, Brushes.Transparent) }
            };
            _panel.Children.Add(_treeView);

            var testButton = new Button()
            {
                Content = "Test Button"
            };
            testButton.Click += (_, _) =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    _treeView.ItemsSource = null;
                    _treeView.ItemsSource = Explorer._parent.UiViewport.UiManager.GetRootControls();
                });
            };
            _panel.Children.Add(testButton);
            Content = _panel;
            
            FuncDataTemplate<BaseControl> GetTemplate() => template;
        }

        private void RegisterEvents()
        {
        }
    }
    
    private readonly UiEditorWindowControl _parent;
    private Grid ExplorerGrid { get; set; } = null!;

    private int _index = 0;
    
    public _UiExplorer(UiEditorWindowControl parent)
    {
        _parent = parent;
        MinWidth = 300;
        CreateComponents();
        RegisterEvents();
        _parent.EditorReady += () =>
        {
            var rootControl = new TestControl();
            rootControl.Visual.CustomTransform = Matrix3x2.CreateScale(new Vector2(.8f, 1f));
            _parent.UiViewport.UiManager.AddRootControl(rootControl);
            _testButton.Click += (_, _) =>
            {
                var addedControl = new TestControl(Color.Beige, Color.Blue);
                addedControl.Visual.X = (int)startPosition.X;
                addedControl.Visual.Y = (int)startPosition.Y;
                addedControl.Name = $"Test Control {_index++}";
                rootControl.AddChild(addedControl);
            
                startPosition += IncreaseAmount;
            };
        };
    }
    
    private Vector2 startPosition = Vector2.Zero;
    private readonly Vector2 IncreaseAmount = new Vector2(100, 100);
    private Button _testButton;
    
    private void CreateComponents()
    {
        ExplorerGrid = new Grid();
        Content = ExplorerGrid;
        
        ExplorerGrid.Children.Add(new UiTreeExplorer(this));
        
        _testButton = new Button()
        {
            Content = "Test Button"
        };
        ExplorerGrid.Children.Add(_testButton);
    }

    private void RegisterEvents()
    {
    }
    
}