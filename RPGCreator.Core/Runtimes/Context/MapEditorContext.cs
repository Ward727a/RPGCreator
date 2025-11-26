using RPGCreator.Core.Managers.RTP.BrushManagers.Brushs;
using RPGCreator.Core.Types.Assets.Tilesets;
using RPGCreator.Core.Types.Internal;
using RPGCreator.Core.Types.Map;

namespace RPGCreator.Core.Runtimes.Context;

public class MapEditorContext
{
    public MapDefinition Map { get; }
    public TileLayerDefinition? ActiveLayer { get; set; }
    
    public bool IsDrawing { get; set; } = false;
    public Point LastDrawAt { get; set; } = new(-1, -1);
    public IBrush? ActiveBrush { get; set; }
    public ITileDef? SelectedTile { get; set; }
}