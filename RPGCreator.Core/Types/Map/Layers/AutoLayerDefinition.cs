using System.Numerics;
using RPGCreator.Core.Types.Internal;
using RPGCreator.Core.Types.Map.Layers.AutoLayer;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Tilesets.IntGrid;

namespace RPGCreator.Core.Types.Map;

public class AutoLayerDefinition : BaseLayerDef
{
    public IntGridLayerDefinition SourceIntGrid { get; set; } = new();
    public TileLayerDefinition InternalTileLayer { get; private set; } = new();
    
    public Ulid IntGridSetUnique { get; set; }
    public IntGridTilesetDef? IntGridSet { get; set; }

    public void BakeRegion(Vector2 center, float radius = 0)
    {
        for (var x = center.X - radius; x <= center.X + radius; x++)
        {
            for (var y = center.Y - radius; y <= center.Y + radius; y++)
            {
                var position = new Vector2(x, y);
                var newTile = AutoTileSolver.Resolve(position, SourceIntGrid, IntGridSet.Rules, EngineCore.Instance.Managers.Assets);
                
                if (newTile != null)
                {
                    InternalTileLayer.AddElement(newTile, position);
                    BakeDirtyTiles(CheckAround(position));
                }
                else
                {
                    // InternalTileLayer.TryRemoveElement(position, out var _);
                }
            }
        }
    }
    
    protected class BakeContext
    {
        private HashSet<Point> PositionsAlreadyChecked { get; } = new();
        private Queue<Point> PositionsToCheck { get; } = new();
        
        public void AddPositionsToCheck(IEnumerable<Point> positions)
        {
            foreach (var pos in positions)
            {
                if (!PositionsAlreadyChecked.Contains(pos))
                {
                    PositionsToCheck.Enqueue(pos);
                }
            }
        }
        
        public void AddPositionChecked(Point position)
        {
            PositionsAlreadyChecked.Add(position);
        }
        
        public bool HasPositionsToCheck()
        {
            return PositionsToCheck.Count > 0;
        }
        
        public Point DequeuePosition()
        {
            var pos = PositionsToCheck.Dequeue();
            PositionsAlreadyChecked.Add(pos);
            return pos;
        }
        
        public IEnumerable<Point>  Positions
        {
            get
            {
                while (HasPositionsToCheck())
                {
                    yield return DequeuePosition();
                }
            }
        }
    }
    
    protected void BakeDirtyTiles(BakeContext context)
    {
        
        foreach (var position in context.Positions)
        {
            var currentTile = InternalTileLayer.GetElement(position);
            var newTile = AutoTileSolver.Resolve(position, SourceIntGrid, IntGridSet.Rules, EngineCore.Instance.Managers.Assets);

            if (newTile != null)
            {

                if (currentTile == null)
                {
                    InternalTileLayer.AddElement(newTile, position);
                    BakeDirtyTiles(CheckAround(position, context));
                    continue;
                }
                
                if (newTile.SizeInTileset != currentTile.SizeInTileset || newTile.PositionInTileset != currentTile.PositionInTileset ||
                        newTile.TilesetDef.Unique != currentTile.TilesetDef.Unique)
                {
                    InternalTileLayer.AddElement(newTile, position);
                    BakeDirtyTiles(CheckAround(position, context));
                }
            }
            else
            {
                // InternalTileLayer.TryRemoveElement(position, out var _);
            }
        }
    }
    
    protected BakeContext CheckAround(Point center, BakeContext? context = null, int GridSize = 32)
    {
        if(context == null)
        {
            context = new BakeContext();
        }
        
        List<Point> positionsToBake = new();
        context.AddPositionChecked(center);
        
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue; // Skip center
                
                var checkPos = new Point(center.X + (x * GridSize), center.Y + (y * GridSize));
                
                if(SourceIntGrid.HasElement(checkPos))
                    positionsToBake.Add(checkPos);
            }
        }
        
        context.AddPositionsToCheck(positionsToBake);

        return context;
    }
}