
using System;
using Avalonia.Controls;
using Avalonia.Layout;
using RPGCreator.Core.Type.Assets.Characters.Stats;
using RPGCreator.Core.Type.Blueprint;
using RPGCreator.Core.Type.Blueprint.Nodes;
using RPGCreator.Core.Type.Blueprint.Nodes.Debug;
using RPGCreator.Core.Type.Blueprint.Nodes.Gets;
using RPGCreator.Core.Type.Blueprint.Nodes.Math;
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
    private readonly GraphDocument _doc = new();
    #endregion
    
    #region Components

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
            RowDefinitions = new RowDefinitions("30, *"),
            ClipToBounds = true,
        };
        Content = grid;

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
        
        var graph = new GraphView();
        grid.Children.Add(graph);
        Grid.SetRow(graph, 1);
        
        _doc.AddNode(new NodeEnd());

        _doc.AddNode(new NodeMathMultiply());
        _doc.AddNode(new NodeStart());
        // _doc.AddNode(new GetPlayerName());
        // _doc.AddNode(new NodePrint());
        // _doc.AddNode(new GetPlayerName());
        
        _doc.AddNode(new NodePrint());

        graph.SetDocument(_doc);
    }
    #endregion

    #region Events Handlers
    #endregion
}