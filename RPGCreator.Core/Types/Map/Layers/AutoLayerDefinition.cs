using RPGCreator.Core.Managers.AssetsManager;
using RPGCreator.Core.Types.Internal;
using RPGCreator.Core.Types.Map.AutoLayer;

namespace RPGCreator.Core.Types.Map;

public class AutoLayerDefinition : BaseLayerDef
{
    public IntGridLayerDefinition SourceIntGrid { get; set; }
    public TileLayerDefinition InternalTileLayer { get; private set; } = new();
    
    public List<AutoLayerRule> Rules { get; set; } = new();

    public void BakeRegion(Point center, int radius = 1)
    {
        for (int x = center.X - radius; x <= center.X + radius; x++)
        {
            for (int y = center.Y - radius; y <= center.Y + radius; y++)
            {
                var position = new Point(x, y);
                var newTile = AutoTileSolver.Resolve(position, SourceIntGrid, Rules, EngineCore.Instance.Managers.Assets);
                
                if (newTile != null)
                {
                    InternalTileLayer.AddElement(newTile, position);
                }
                else
                {
                    InternalTileLayer.TryRemoveElement(position, out var _);
                }
            }
        }
    }
}