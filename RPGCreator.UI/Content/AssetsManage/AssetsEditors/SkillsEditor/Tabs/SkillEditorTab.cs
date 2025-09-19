using System;
using Avalonia.Controls;
using Avalonia.Layout;
using RPGCreator.Core;
using RPGCreator.Core.Type;
using RPGCreator.Core.Type.Assets.Skills;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.SkillsEditor.Tabs;

public class SkillEditorTab : UserControl
{
    #region Constants
    #endregion
    
    #region Events
    #endregion
    
    #region Properties
    public ISkillDef SkillDef { get; private set; }
    #endregion
    
    #region Components
    private ScrollBox _body;
    private StackPanel _bodyPanel;
    private ComboBox _skillPack;
    #endregion
    
    #region Constructors
    public SkillEditorTab(ISkillDef skillDef)
    {
        ArgumentNullException.ThrowIfNull(skillDef, nameof(skillDef));
        SkillDef = skillDef;
        CreateComponents();
        Content = SkillDef.Name;
    }
    #endregion
    
    #region Methods
    private void CreateComponents()
    {
        
        _body = new ScrollBox()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = App.style.Margin
        };
        _bodyPanel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = App.style.Margin
        };
        _body.Content = _bodyPanel;
        
        _skillPack = new ComboBox()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = App.style.Margin,
        };
        var inputStatPack = new InputLabel("Assets Pack", _skillPack);
        _bodyPanel.Children.Add(inputStatPack);
        ToolTip.SetTip(inputStatPack, "The assets pack this skill belongs to.");
        
        foreach (var pack in EngineCore.Instance.Managers.Assets.GetAssetsPacks())
        {
            _skillPack.Items.Add(pack.Name);
        }

        if (SkillDef.PackId.HasValue && SkillDef.PackId.Value != Ulid.Empty)
        {
            var hasPack = EngineCore.Instance.Managers.Assets.TryGetAssetsPack(SkillDef.PackId.Value, out var assetsPack);
            if (hasPack)
            {
                _skillPack.SelectedItem = assetsPack;
            }
        }
        else
        {
            _skillPack.SelectedIndex = 0;
            SkillDef.PackId = EngineCore.Instance.Managers.Assets.GetAssetsPacks()[0].Id;
        }
    }
    #endregion

    #region Events Handlers
    #endregion
}