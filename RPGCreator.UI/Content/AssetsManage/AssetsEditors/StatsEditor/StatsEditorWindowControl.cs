using Avalonia.Controls;
using RPGCreator.Core.Type.Assets.Characters.Stats;
using RPGCreator.UI.Content.AssetsManage.AssetsEditors.StatsEditor.Tabs;

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

    private TabControl _body;
    
    #endregion
    
    #region Constructors
    public StatsEditorWindowControl(IStatDef? statDef)
    {
        statDef ??= new StatDefinition();
        StatDef = statDef;
        CreateComponents();
        Content = _body;
    }
    #endregion
    
    #region Methods
    private void CreateComponents()
    {
        _body = new TabControl
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            Margin = App.style.Margin
        };

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
    }
    #endregion

    #region Events Handlers
    #endregion
    
}