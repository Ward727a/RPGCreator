
using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using RPGCreator.Core;
using RPGCreator.Core.Types.Assets.Characters.Stats;
using RPGCreator.Core.Types.Blueprint;
using RPGCreator.Core.Types.Blueprint.Nodes;
using RPGCreator.Core.Types.Blueprint.Nodes.Debug;
using RPGCreator.Core.Types.Blueprint.Nodes.Gets;
using RPGCreator.Core.Types.Blueprint.Nodes.Math;
using RPGCreator.UI.Common.Blueprint;
using Serilog;

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
                Log.Information("Compiling the graph...");
                _doc.Compile();
                Log.Information("Graph compiled & tested successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while compiling the graph");
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
                Log.Information("Saving the graph...");
                EngineSerializer.Instance.Serialize(_doc, out var data);
                Log.Debug("Graph data: {Data}", data);
                // Save to a test file for now.
                File.WriteAllText("test_save_graph.xml", data);
                Log.Information("Graph saved successfully.");
                
                _doc.SavePath = "test_save_graph.xml";
                
                StatDef.AddEvent("test", GraphDocumentCompiler.Compile(_doc));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while saving the graph");
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
                Log.Information("Loading the graph...");
                if (File.Exists("test_save_graph.xml"))
                {
                    var data = File.ReadAllText("test_save_graph.xml");
                    EngineSerializer.Instance.Deserialize<GraphDocument>(data, out var obj, out var type);
                    if (obj is GraphDocument doc)
                    {
                        _graph.SetDocument(doc);
                        _doc = doc; // Update the current document reference
                        Log.Information("Graph loaded successfully.");
                    }
                    else
                    {
                        Log.Error("Loaded object is not a GraphDocument.");
                    }
                }
                else
                {
                    Log.Error("Graph file 'test_save_graph.xml' does not exist.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while loading the graph");
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
                Log.Information("Compiling the graph...");
                // _doc.Compile();
                _doc.Save("test.json");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while compiling the graph");
            }
        };
        grid.Children.Add(testbutton);
        Grid.SetRow(testbutton, 0);
        
        _doc.AddNode(new NodeStart());
        _doc.AddNode(new NodeEnd(){X = 200, Y = 0});

        _graph.SetDocument(_doc);
    }
    #endregion

    #region Events Handlers
    #endregion
}