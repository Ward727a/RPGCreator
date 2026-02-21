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
using RPGCreator.Core.Types;
using RPGCreator.SDK;
using RPGCreator.SDK.GlobalState;
using RPGCreator.UI.Content.Editor.LayersListComponents;

namespace RPGCreator.UI.Content.Editor.Tabs
{
    public class LayerEditor : UserControl
    {

        
        public ScrollViewer BodyScroller { get; private set; }
        public Grid Body { get; private set; }
        public StackPanel BodyContent { get; private set; }

        // Sections

        #region LayersSection
        public Accordion LayersBox { get; private set; }
        public StackPanel LayersBody { get; private set; }
        public TextBlock SelectedLayerText { get; private set; }
        public Button AddLayerButton { get; private set; }
        public ListBox LayersList { get; private set; }
        #endregion

        // Constructor

        public LayerEditor()
        {
            BodyScroller = new ScrollViewer
            {
            };

            Body = new Grid
            {
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            };
            BodyScroller.Content = Body;

            BodyContent = new StackPanel
            {
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            };
            Body.Children.Add(BodyContent);

            Content = BodyScroller;

            // Initialize the map editor UI components here
            InitializeComponents();
        }

        // Functions

        protected void InitializeComponents()
        {
            // Add UI components for the map editor, such as buttons, grids, etc.
            // This is just a placeholder; you can add actual UI elements as needed.

            #region LayersSection

            BodyContent.Children.Add(new LayersListComponent());

            #endregion
        }

        public static TabItem CreateTab(Window host)
        {
            // Need to do this to avoid create "multiple" instances...
            // In fact, even if we don't create multiple instances, it still crashes the application due to creating "multiple" instances of the same control.
            // Weird issues, but well, this works for now.
            var tab = new TabItem
            {
                Header = "Map Editor",
                Content = new LayerEditor()
            };

            return tab;
        }

        
    }
}
