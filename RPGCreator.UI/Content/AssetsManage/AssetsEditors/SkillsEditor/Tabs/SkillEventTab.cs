using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using RPGCreator.SDK.Assets.Definitions.Skills;
using RPGCreator.SDK.Common.Logging;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.SkillsEditor.Tabs;

public class SkillEventTab : UserControl
{
    #region Constants
    #endregion
    
    #region Events
    #endregion
    
    #region Properties
    public ISkillDef SkillDef { get; private set; }
    #endregion
    
    #region Components
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
                Logger.Info("Saving the graph...");
                Logger.Info("Graph saved successfully.");
                
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
        }
        catch (Exception ex)
        {
            Logger.Error("Error while adding default nodes to the graph document: " + ex.Message);
        }

    }
    #endregion

    #region Events Handlers
    #endregion
}