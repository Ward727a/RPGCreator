using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using RPGCreator.Core.Type.Blueprint;
using RPGCreator.UI.Common.Blueprint.LeftBar;

namespace RPGCreator.UI.Common.Blueprint;

public class GraphViewLeftBar : UserControl
{
    
    #region Properties

    private GraphDocument _doc;
    private GraphLeftBarNodesList _nodesList;
    #endregion
    
    public GraphViewLeftBar(GraphDocument doc)
    {
        _doc = doc;
        // _nodesList = new GraphLeftBarNodesList(addNode);
        CreateComponents();
        RegisterEvents();
    }
    public class GraphVariableViewModel
    {
        public string Name { get; set; }
        public string VariableType { get; set; }
        public object DefaultValue { get; set; }
    }
    
    private void CreateComponents()
    {
        Canvas.SetLeft(this, 0);
        Canvas.SetTop(this, 0);

        Width = 400;
        MaxWidth = 400;
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Stretch;
        Background = // Set a test background color
            new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(30, 30, 30));
        
        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top
        };
        Content = stackPanel;

        var graphVariablesAccordion = new Expander()
        {
            Header = "Variables",
            IsExpanded = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(5)
        };
        
        stackPanel.Children.Add(graphVariablesAccordion);
        var graphVariablesPanel = new StackPanel()
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
        };
        graphVariablesAccordion.Content = graphVariablesPanel;
        var graphVariablesHint = new TextBlock()
        {
            Text = "Variables defined here can be used in the graph, and only in this graph.",
            FontSize = 10,
            Foreground = Brushes.Gray,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(5)
        };
        graphVariablesPanel.Children.Add(graphVariablesHint);
        var listVariablesScroll = new ScrollViewer()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Height = 120,
            Margin = new Thickness(5)
        };
        graphVariablesPanel.Children.Add(listVariablesScroll);
        var listVariables = new ListBox()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            SelectionMode = SelectionMode.Single,
            Margin = new Thickness(5)
        };

        void AddVariable(KeyValuePair<string, (Type, object)> kvp)
        {
            var itemPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(2)
            };
            var nameText = new TextBlock
            { Text = kvp.Key, Width = 120, Foreground = Brushes.White };
            var typeText = new TextBlock
            { Text = kvp.Value.Item1.Name, Width = 100, Foreground = Brushes.LightGray };
            var defaultValueText = new TextBlock
            { Text = kvp.Value.Item2?.ToString() ?? "null", Width = 100, Foreground = Brushes.LightGray };
            itemPanel.Children.Add(nameText);
            itemPanel.Children.Add(typeText);
            itemPanel.Children.Add(defaultValueText);
            listVariables.Items.Add(itemPanel);
        }
        
        _doc.GraphVariables.ToList().ForEach(kvp =>
        {
            AddVariable(kvp);
        });
        
        listVariablesScroll.Content = (listVariables);
        
        var variablePropertiesExpander = new Expander()
        {
            Header = "Variable Properties",
            IsExpanded = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(5)
        };
        stackPanel.Children.Add(variablePropertiesExpander);
        var variablePropertiesPanel = new StackPanel()
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
        };
        variablePropertiesExpander.Content = variablePropertiesPanel;
        var variablePropertiesHint = new TextBlock()
        {
            Text = "Properties of the selected variable.",
            FontSize = 10,
            Foreground = Brushes.Gray,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(5)
        };
        variablePropertiesPanel.Children.Add(variablePropertiesHint);
        
        var varNamePanel = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5)
        };
        var varNameLabel = new TextBlock()
        { Text = "Name:", Width = 80, Foreground = Brushes.White };
        var varNameTextBox = new TextBox()
        { Width = 200, HorizontalAlignment = HorizontalAlignment.Left };
        varNamePanel.Children.Add(varNameLabel);
        varNamePanel.Children.Add(varNameTextBox);
        variablePropertiesPanel.Children.Add(varNamePanel);
        
        var varTypePanel = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5)
        };
        var varTypeLabel = new TextBlock()
        { Text = "Type:", Width = 80, Foreground = Brushes.White };
        var varTypeTextBox = new TextBox()
        { Width = 200, HorizontalAlignment = HorizontalAlignment.Left, IsReadOnly = true };
        varTypePanel.Children.Add(varTypeLabel);
        varTypePanel.Children.Add(varTypeTextBox);
        variablePropertiesPanel.Children.Add(varTypePanel);
        
        var varDefaultValuePanel = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5)
        };
        var varDefaultValueLabel = new TextBlock()
        { Text = "Value:", Width = 80, Foreground = Brushes.White };
        var varDefaultValueTextBox = new TextBox()
        { Width = 200, HorizontalAlignment = HorizontalAlignment.Left };
        varDefaultValuePanel.Children.Add(varDefaultValueLabel);
        varDefaultValuePanel.Children.Add(varDefaultValueTextBox);
        variablePropertiesPanel.Children.Add(varDefaultValuePanel);
        
        void selectVariable(KeyValuePair<string, (Type, object)> kvp)
        {
            varNameTextBox.Text = kvp.Key;
            varTypeTextBox.Text = kvp.Value.Item1.Name;
            varDefaultValueTextBox.Text = kvp.Value.Item2?.ToString() ?? "null";
        }
        listVariables.SelectionChanged += (sender, args) =>
        {
            if (listVariables.SelectedItem != null)
            {
                var index = listVariables.SelectedIndex;
                if (index >= 0 && index < _doc.GraphVariables.Count)
                {
                    var kvp = _doc.GraphVariables.ElementAt(index);
                    selectVariable(kvp);
                }
            }
        };

        // stackPanel.Children.Add(_nodesList);

    }

    private void RegisterEvents()
    {
    }
}