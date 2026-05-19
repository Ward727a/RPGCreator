using System.Collections.Generic;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers.AutoLayer;
using RPGCreator.SDK.Assets.Definitions.Tilesets.IntGrid;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.AutoLayerEditor.Components;

public class IntRefContext
{
    public HashSet<IntGridValueRef> IntRefs { get; } = new();
    
    public Dictionary<int, List<AutoLayerRule>> RulesByIntRefValue { get; } = new();
}