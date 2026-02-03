
using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Stats;
using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Graph.Nodes;
using RPGCreator.SDK.Logging;
using RPGCreator.UI.Common.Blueprint;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.StatsEditor.Tabs;

/// <summary>
/// This will manage what happens for different "events" related to the stat in the game (e.g. When the stat come to the minimum value, when the stat is modified, etc.).
/// </summary>
public class StatEventTab : UserControl
{
    #region Constants
    #endregion
    
    #region Events
    #endregion
    
    #region Properties
    public IStatDef StatDef { get; private set; }
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
    public StatEventTab(IStatDef statDef)
    {
        StatDef = statDef;
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
                _doc.Save("test_save_graph.xml");

                var compiledDocument = EngineServices.GraphService.Compile(_doc);
                StatDef.AddEvent("test", compiledDocument);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error while saving the graph {ex.Message}");
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
                if (File.Exists("test_save_graph.xml"))
                {
                    if(EngineServices.GraphService.TryLoadDocument("test_save_graph.xml", out var loadedDoc))
                    {
                        _graph.SetDocument(loadedDoc);
                        _doc = loadedDoc; // Update the current document reference
                        Logger.Info("Graph loaded successfully.");
                        return;
                    }
                    Logger.Error("Loaded object is not a GraphDocument.");
                }
                else
                {
                    Logger.Error("Graph file 'test_save_graph.xml' does not exist.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error while loading the graph {ex.Message} ");
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
        
        var start = GraphNodeRegistry.GetNode("@hide|Start");
        _doc.AddNode(start);
        
        var end = GraphNodeRegistry.GetNode("@hide|End");
        _doc.AddNode(end);
        _doc.MoveNode(end.Id, 200, 0);

        _graph.SetDocument(_doc);
    }
    #endregion

    #region Events Handlers
    #endregion
}