using System;
using Avalonia.Controls;
using Avalonia.Input;
using RPGCreator.SDK.Assets.Definitions.Stats;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.StatsEditor;

public class StatsManageItemControl : UserControl
{
    #region Constants
    #endregion
    
    #region Events
    public event EventHandler? ItemSelected;
    #endregion
    
    #region Properties
    public IStatDef StatDef { get; private set; }
    #endregion
    
    #region Components
    private Grid Body { get; set; }
    private TextBlock StatNameTextBlock { get; set; }
    #endregion
    
    #region Constructors
    
    public StatsManageItemControl(IStatDef statDef)
    {
        ArgumentNullException.ThrowIfNull(statDef, nameof(statDef));
        
        StatDef = statDef;
        
        CreateComponents();
        RegisterEvents();
        
        Content = Body;
    }
    #endregion
    
    #region Methods
    private void CreateComponents()
    {
        // Initialize UI components here
        Body = new Grid
        {
            Margin = App.style.Margin,
            // Set the background color to a custom dark transparent color
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(0x55, 0x2A, 0x2A, 0x2A)),
            RowDefinitions = new RowDefinitions("Auto"),
        };
        
        StatNameTextBlock = new TextBlock
        {
            Text = StatDef.Name,
            Margin = App.style.Margin,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
        };
        Body.Children.Add(StatNameTextBlock);
        Grid.SetRow(StatNameTextBlock, 0);
    }
    private void RegisterEvents()
    {
        Body.PointerPressed += OnItemSelected;
    }


    #endregion

    #region Events Handlers
    
    private void OnItemSelected(object? sender, PointerPressedEventArgs e)
    {
        ItemSelected?.Invoke(this, EventArgs.Empty);
    }
    #endregion
}