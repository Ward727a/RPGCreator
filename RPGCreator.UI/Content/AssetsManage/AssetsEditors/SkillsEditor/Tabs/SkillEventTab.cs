using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using RPGCreator.SDK.Assets.Definitions.Skills;
using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Graph.Nodes;
using RPGCreator.SDK.Logging;
using RPGCreator.UI.Common.Blueprint;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.SkillsEditor.Tabs;

public class SkillEventTab : UserControl
{
    #region Constants
    #endregion
    
    #region Events
    #endregion
    
    #region Properties
    public ISkillDef SkillDef { get; private set; }
    private GraphDocument _doc = new();
    #endregion
    
    #region Components
    private GraphView _graph;
    private StackPanel _topMenu;
    private Button _compileAndRunButton;
    private Button _saveGraphButton;
    private Button _testLoadGraphButton;
    #endregion
    
    #region Constructors
    public SkillEventTab(ISkillDef skillDef)
    {
        ArgumentNullException.ThrowIfNull(skillDef, nameof(skillDef));
        SkillDef = skillDef;
        CreateComponents();
    }
    #endregion
    
    #region Methods

    private void CreateComponents()
    {
        var grid = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            RowDefinitions = new RowDefinitions("Auto, *"),
            ClipToBounds = true,
        };
        Content = grid;
        
        _graph = new GraphView();
        grid.Children.Add(_graph);
        Grid.SetRow(_graph, 1);
        
        
        _topMenu = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(5),
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.LightGray)
        };
        grid.Children.Add(_topMenu);
        Grid.SetRow(_topMenu, 0);
        
        _compileAndRunButton = new Button()
        {
            Content = "Compile and Run"
        };
        _compileAndRunButton.Click += (s, e) =>
        {
            try
            {
                Logger.Info("Compiling the graph...");
                _doc.Compile();
                Logger.Info("Graph compiled & tested successfully.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error while compiling the graph {ex.Message}");
            }
        };
        _topMenu.Children.Add(_compileAndRunButton);
        
        _saveGraphButton = new Button()
        {
            Content = "Save Graph"
        };
        _saveGraphButton.Click += (s, e) =>
        {
            try
            {
                Logger.Info("Saving the graph...");
                _doc.Save("test_save_graph.xml");
                Logger.Info("Graph saved successfully.");
                
                _doc.SavePath = "test_save_graph.xml";
            }
            catch (Exception ex)
            {
                Logger.Error($"Error while compiling the graph {ex.Message}");
            }
        };
        _topMenu.Children.Add(_saveGraphButton);
        
        _testLoadGraphButton = new Button()
        {
            Content = "Load Graph"
        };
        _testLoadGraphButton.Click += (s, e) =>
        {
            try
            {
                Logger.Info("Loading the graph...");
                _doc = GraphDocument.Load("test_save_graph.xml");
                _graph.SetDocument(_doc);
                Logger.Info("Graph loaded successfully.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error while compiling the graph {ex.Message}");
            }
        };
        _topMenu.Children.Add(_testLoadGraphButton);

        var testbutton = new Button()
        {
            Content = "Test compile"
        };
        testbutton.Click += (s, e) =>
        {
            try
            {
                Logger.Info("Compiling the graph...");
                // _doc.Compile();
                _doc.Save("test.json");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error while compiling the graph {ex.Message}");
            }
        };
        grid.Children.Add(testbutton);
        Grid.SetRow(testbutton, 0);

        try
        {
            _doc.AddNode(GraphNodeRegistry.GetNode("System|End").Clone());
            _doc.AddNode(GraphNodeRegistry.GetNode("System|Start").Clone());
        }
        catch (Exception ex)
        {
            Logger.Error("Error while adding default nodes to the graph document: " + ex.Message);
        }

        _graph.SetDocument(_doc);
    }
    #endregion

    #region Events Handlers
    #endregion
}