using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using RPGCreator.Core;
using RPGCreator.Core.Type.Assets.Skills;
using RPGCreator.Core.Type.Blueprint;
using RPGCreator.Core.Type.Blueprint.Nodes;
using RPGCreator.Core.Type.Blueprint.Nodes.Debug;
using RPGCreator.Core.Type.Blueprint.Nodes.Math;
using RPGCreator.UI.Common.Blueprint;
using Serilog;

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
                EngineSerializer.Instance.Serialize(_doc, out var data, false);
                Log.Debug("Graph data: {Data}", data);
                // Save to a test file for now.
                File.WriteAllText("test_save_graph.xml", data);
                Log.Information("Graph saved successfully.");
                
                _doc.SavePath = "test_save_graph.xml";
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
                    EngineSerializer.Instance.Deserialize(data, out var obj, out var type);
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
        
        _doc.AddNode(new NodeEnd());

        _doc.AddNode(new NodeMathMultiply());
        _doc.AddNode(new NodeStart());
        // _doc.AddNode(new GetPlayerName());
        // _doc.AddNode(new NodePrint());
        // _doc.AddNode(new GetPlayerName());
        
        _doc.AddNode(new NodePrint());

        _graph.SetDocument(_doc);
    }
    #endregion

    #region Events Handlers
    #endregion
}