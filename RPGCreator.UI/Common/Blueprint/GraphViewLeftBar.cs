using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using RPGCreator.Core.Types;
using RPGCreator.SDK.Graph;

namespace RPGCreator.UI.Common.Blueprint;

public class GraphViewLeftBar : UserControl
{

    public class GraphVariableItem : UserControl
    {
        public string VariableName { get; set; }
        public Type VariableType { get; set; }
        public object DefaultValue { get; set; }
        
        private Grid ItemPanel;
        private TextBlock NameText;
        private TextBlock TypeText;
        private TextBlock DefaultValueText;
        
        public GraphVariableItem(string name, Type type, object defaultValue)
        {
            VariableName = name;
            VariableType = type;
            DefaultValue = defaultValue;
            CreateComponents();
            Content = ItemPanel;
        }
        
        private void CreateComponents()
        {
            ItemPanel = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*, *, *"),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(2)
            };
            
            // Remove the path from the name if it exists, e.g., 'path.to.Name' becomes just 'Name'
            var clearName = VariableName.Contains('.')
                ? VariableName.Split('.').Last()
                : VariableName;
            
            NameText = new TextBlock
                { Text = clearName, Width = 100, Foreground = Brushes.White, TextTrimming = TextTrimming.CharacterEllipsis };
            ItemPanel.Children.Add(NameText);
            ToolTip.SetTip(NameText, VariableName);
            Grid.SetColumn(NameText, 0);
            
            TypeText = new TextBlock
                { Text = VariableType.Name, Width = 100, Foreground = Brushes.LightGray, TextTrimming = TextTrimming.CharacterEllipsis};
            ItemPanel.Children.Add(TypeText);
            ToolTip.SetTip(TypeText, VariableType.Name);
            Grid.SetColumn(TypeText, 1);
            
            DefaultValueText = new TextBlock
                { Text = DefaultValue?.ToString() ?? "null", Width = 100, Foreground = Brushes.LightGray, TextTrimming = TextTrimming.CharacterEllipsis };
            ItemPanel.Children.Add(DefaultValueText);
            ToolTip.SetTip(DefaultValueText, DefaultValue?.ToString() ?? "null");
            Grid.SetColumn(DefaultValueText, 2);
        }
    }
    
    #region Properties

    private GraphDocument _doc;
    #endregion
    
    #region Components

    private StackPanel Body;
    
    private Expander GraphVariablesExpander;
    private StackPanel GraphVariablesPanel;
    private TextBlock GraphVariablesHint;
    private ScrollBox ListVariablesScroll;
    private ListBox GraphVariablesList;
    
    #endregion
    
    public GraphViewLeftBar(GraphDocument doc)
    {
        _doc = doc;

        Canvas.SetLeft(this, 0);
        Canvas.SetTop(this, 0);
        
        Width = 400;
        MaxWidth = 400;
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Stretch;
        Background = // Set a test background color
            new SolidColorBrush(Avalonia.Media.Color.FromRgb(30, 30, 30));
        
        CreateComponents();
        RegisterEvents();
        
        Content = Body;
    }
    
    private void CreateComponents()
    {
        
        Body = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top
        };
        
        CreateVariablesPart();
        
        //
        // var variablePropertiesExpander = new Expander()
        // {
        //     Header = "Variable Properties",
        //     IsExpanded = true,
        //     HorizontalAlignment = HorizontalAlignment.Stretch,
        //     VerticalAlignment = VerticalAlignment.Top,
        //     Margin = new Thickness(5)
        // };
        // Body.Children.Add(variablePropertiesExpander);
        // var variablePropertiesPanel = new StackPanel()
        // {
        //     Orientation = Orientation.Vertical,
        //     HorizontalAlignment = HorizontalAlignment.Stretch,
        //     VerticalAlignment = VerticalAlignment.Top,
        // };
        // variablePropertiesExpander.Content = variablePropertiesPanel;
        // var variablePropertiesHint = new TextBlock()
        // {
        //     Text = "Properties of the selected variable.",
        //     FontSize = 10,
        //     Foreground = Brushes.Gray,
        //     TextAlignment = TextAlignment.Center,
        //     Margin = new Thickness(5)
        // };
        // variablePropertiesPanel.Children.Add(variablePropertiesHint);
        //
        // var varNamePanel = new StackPanel()
        // {
        //     Orientation = Orientation.Horizontal,
        //     HorizontalAlignment = HorizontalAlignment.Stretch,
        //     VerticalAlignment = VerticalAlignment.Center,
        //     Margin = new Thickness(5)
        // };
        // var varNameLabel = new TextBlock()
        // { Text = "Name:", Width = 80, Foreground = Brushes.White };
        // var varNameTextBox = new TextBox()
        // { Width = 200, HorizontalAlignment = HorizontalAlignment.Left };
        // varNamePanel.Children.Add(varNameLabel);
        // varNamePanel.Children.Add(varNameTextBox);
        // variablePropertiesPanel.Children.Add(varNamePanel);
        //
        // var varTypePanel = new StackPanel()
        // {
        //     Orientation = Orientation.Horizontal,
        //     HorizontalAlignment = HorizontalAlignment.Stretch,
        //     VerticalAlignment = VerticalAlignment.Center,
        //     Margin = new Thickness(5)
        // };
        // var varTypeLabel = new TextBlock()
        // { Text = "Type:", Width = 80, Foreground = Brushes.White };
        // var varTypeTextBox = new TextBox()
        // { Width = 200, HorizontalAlignment = HorizontalAlignment.Left, IsReadOnly = true };
        // varTypePanel.Children.Add(varTypeLabel);
        // varTypePanel.Children.Add(varTypeTextBox);
        // variablePropertiesPanel.Children.Add(varTypePanel);
        //
        // var varDefaultValuePanel = new StackPanel()
        // {
        //     Orientation = Orientation.Horizontal,
        //     HorizontalAlignment = HorizontalAlignment.Stretch,
        //     VerticalAlignment = VerticalAlignment.Center,
        //     Margin = new Thickness(5)
        // };
        // var varDefaultValueLabel = new TextBlock()
        // { Text = "Value:", Width = 80, Foreground = Brushes.White };
        // var varDefaultValueTextBox = new TextBox()
        // { Width = 200, HorizontalAlignment = HorizontalAlignment.Left };
        // varDefaultValuePanel.Children.Add(varDefaultValueLabel);
        // varDefaultValuePanel.Children.Add(varDefaultValueTextBox);
        // variablePropertiesPanel.Children.Add(varDefaultValuePanel);
        //
        // void selectVariable(KeyValuePair<string, (Type, object)> kvp)
        // {
        //     varNameTextBox.Text = kvp.Key;
        //     varTypeTextBox.Text = kvp.Value.Item1.Name;
        //     varDefaultValueTextBox.Text = kvp.Value.Item2?.ToString() ?? "null";
        // }
        // GraphVariablesList.SelectionChanged += (sender, args) =>
        // {
        //     if (GraphVariablesList.SelectedItem != null)
        //     {
        //         var index = GraphVariablesList.SelectedIndex;
        //         if (index >= 0 && index < _doc.GraphVariables.Count)
        //         {
        //             var kvp = _doc.GraphVariables.ElementAt(index);
        //             selectVariable(kvp);
        //         }
        //     }
        // };

    }

    public void AddVariable(string name, (Type, object) args)
    {
        var itemPanel = new GraphVariableItem(name, args.Item1, args.Item2);
        GraphVariablesList.Items.Add(itemPanel);
    }
    
    public void AddVariable(KeyValuePair<string, (Type, object)> kvp)
    {
        AddVariable(kvp.Key, kvp.Value);
    }

    public void RemoveVariable(string name)
    {
        var itemToRemove = GraphVariablesList.Items
            .OfType<GraphVariableItem>()
            .FirstOrDefault(item => item.VariableName == name);
        if (itemToRemove != null)
        {
            GraphVariablesList.Items.Remove(itemToRemove);
        }
    }
    
    public void RefreshGraphVariables()
    {
        GraphVariablesList.Items.Clear();
        foreach (var kvp in _doc.GraphVariables)
        {
            AddVariable(kvp);
        }
    }
    
    private void CreateVariablesPart()
    {
        
        GraphVariablesExpander = new Expander()
        {
            Header = "Variables",
            IsExpanded = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(5)
        };
        Body.Children.Add(GraphVariablesExpander);
        
        GraphVariablesPanel = new StackPanel()
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
        };
        GraphVariablesExpander.Content = GraphVariablesPanel;
        
        GraphVariablesHint = new TextBlock()
        {
            Text = "Variables defined here can be used in the graph, and only in this graph.",
            FontSize = 10,
            Foreground = Brushes.Gray,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(5)
        };
        GraphVariablesPanel.Children.Add(GraphVariablesHint);
        
        ListVariablesScroll = new ScrollBox()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Height = 120,
            Margin = new Thickness(5)
        };
        GraphVariablesPanel.Children.Add(ListVariablesScroll);
        
        GraphVariablesList = new ListBox()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            SelectionMode = SelectionMode.Single,
            Margin = new Thickness(5)
        };
        ListVariablesScroll.Content = (GraphVariablesList);
    }
    
    private void RegisterEvents()
    {
        _doc.GraphVariableAdded += (name, args) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                AddVariable(name, args);
            });
        };
        _doc.GraphVariableRemoved += (name) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                RemoveVariable(name);
            });
        };
    }
}