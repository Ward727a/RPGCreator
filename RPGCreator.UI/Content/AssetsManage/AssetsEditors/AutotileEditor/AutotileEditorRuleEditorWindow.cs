using Avalonia.Controls;
using RPGCreator.Core.Types.Assets;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.AutotileEditor;

public class AutotileEditorRuleEditorWindow : Window
{

    public AutotileEditorRuleEditorWindow(AutoTilesetDef tilesetDef)
    {
        Width = 1200;
        Height = 900;
        Title = "Autotile Rule Editor";
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        this.Content = new AutotileEditorRuleEditorWindowControl(tilesetDef);

    }
    
}