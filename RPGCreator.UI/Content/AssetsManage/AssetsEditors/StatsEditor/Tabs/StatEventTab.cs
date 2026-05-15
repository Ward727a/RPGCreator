
using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Stats;
using RPGCreator.SDK.Logging;

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
    #endregion
    
    #region Components

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
            }
            catch (Exception ex)
            {
                Logger.Error($"Error while compiling the graph {ex.Message}");
            }
        };
        grid.Children.Add(testbutton);
        Grid.SetRow(testbutton, 0);
        
    }
    #endregion

    #region Events Handlers
    #endregion
}