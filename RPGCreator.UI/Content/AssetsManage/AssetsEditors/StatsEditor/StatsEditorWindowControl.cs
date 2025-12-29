using System;
using System.IO;
using Avalonia.Controls;
using RPGCreator.Core;
using RPGCreator.Core.Managers.AssetsManager;
using RPGCreator.Core.Types.Assets.BaseAssetsPack;
using RPGCreator.Core.Types.Assets.Entities.Characters.Stats;
using RPGCreator.SDK.Assets.Definitions.Characters.Stats;
using RPGCreator.UI.Content.AssetsManage.AssetsEditors.StatsEditor.Tabs;
using Serilog;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.StatsEditor;

public class StatsEditorWindowControl : UserControl
{

    #region Constants
    #endregion
    
    #region Events
    #endregion
    
    #region Properties
    public IStatDef StatDef { get; private set; }
    #endregion
    
    #region Components

    private Grid _bodyGrid;
    private TabControl _body;
    
    #endregion
    
    #region Constructors
    public StatsEditorWindowControl(IStatDef? statDef)
    {
        statDef ??= new StatDefinition();
        StatDef = statDef;
        CreateComponents();
        Content = _bodyGrid;
    }
    #endregion
    
    #region Methods
    private void CreateComponents()
    {
        _bodyGrid = new Grid
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            RowDefinitions = new RowDefinitions("*, Auto")
        };
        
        _body = new TabControl
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            Margin = App.style.Margin
        };
        _bodyGrid.Children.Add(_body);

        _body.Items.Add(
            new TabItem()
            {
                Header = "Stat",
                Content = new StatEditorTab(StatDef)
            });
        _body.Items.Add(
            new TabItem()
            {
                Header = "Events",
                Content = new StatEventTab(StatDef)
            });
        
        var buttonsPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = App.style.Margin
        };
        _bodyGrid.Children.Add(buttonsPanel);
        Grid.SetRow(buttonsPanel, 1);
        
        var saveButton = new Button
        {
            Content = "Save",
            Margin = new Avalonia.Thickness(5, 0, 0, 0),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };
        saveButton.Click += (s, e) =>
        {
            Log.Information("Saving Stat Definition...");
            // AssetsManager.AssetMapping[typeof(IStatDef)].Invoke(StatDef);
            
            EngineSerializer.Instance.Serialize(StatDef, out string data);
            
            // Default test path
            string defaultTestPAth = System.IO.Path.Combine(EngineCore.Instance.Data.EditedProject.Path, "Assets", "Stats");
            
            // Create directory if not exists
            if (!System.IO.Directory.Exists(defaultTestPAth))
            {
                System.IO.Directory.CreateDirectory(defaultTestPAth);
            }
            
            System.IO.File.WriteAllText($"{defaultTestPAth}/test.xml", data);
            
            // Add the stat definition to the selected asset pack in the statdef
            if(StatDef.PackId.HasValue && StatDef.PackId != Ulid.Empty)
            {
                // var hasPack = EngineCore.Instance.Managers.Assets.TryGetAssetsPack(StatDef.PackId.Value, out var assetsPack);
                // if (hasPack != null)
                // {
                //     assetsPack.AddAsset(StatDef);
                //     assetsPack.Save();
                //     File.WriteAllText(StatDef.SavePath, data);
                //     Log.Information("Stat Definition added to the selected Assets Pack.");
                // }
                // else
                // {
                //     Log.Warning("Assets Pack with ID {PackId} not found. Stat Definition not added to any pack.", StatDef.PackId);
                // }
            }
            else
            {
                Log.Warning("No Assets Pack selected for this Stat Definition. It won't be part of any pack.");
            }
            
            Log.Debug("Stat Definition saved at {Path} with data: {Data}", defaultTestPAth, data);
            Log.Information("Stat Definition saved.");
        };
        buttonsPanel.Children.Add(saveButton);
    }
    #endregion

    #region Events Handlers
    #endregion
    
}