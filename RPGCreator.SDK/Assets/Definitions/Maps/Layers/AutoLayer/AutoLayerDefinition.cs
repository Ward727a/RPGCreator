using System.Numerics;
using RPGCreator.Core.Types.Editor.Visual.PaintTargets;
using RPGCreator.Core.Types.Map;
using RPGCreator.SDK.Assets.Definitions.Maps.AutoLayer;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers.PaintTargets;
using RPGCreator.SDK.Assets.Definitions.Tilesets.IntGrid;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Editor;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Assets.Definitions.Maps.Layers.AutoLayer;

[SerializingType("AutoLayerDefinition")]
public class AutoLayerDefinition : BaseLayerDef
{
    public IntGridLayerDefinition SourceIntGrid { get; set; } = new();
    public TileLayerDefinition InternalTileLayer { get; private set; } = new();
    public IntGridTilesetDef? IntGridSet { get; set; }

    private IntGridLayerTarget? _paintTargetCache;

    public override IPaintTarget? GetPaintTarget()
    {
        if(GlobalStates.MapState.CurrentMapDef == null)
            return null;
        var mapDef = GlobalStates.MapState.CurrentMapDef;
        
        if(_paintTargetCache != null && _paintTargetCache.MapDef == mapDef)
            return _paintTargetCache;
        
        _paintTargetCache = new IntGridLayerTarget(this, mapDef);
        return _paintTargetCache;
    }

    public override bool CanPaintObject(object? objectToPaint)
    {
        return objectToPaint is IntGridData;
    }

    /// <summary>
    /// Bakes auto-tiles in the specified region.
    /// </summary>
    /// <param name="center">The center position of the region to bake.</param>
    /// <param name="radius">
    /// The radius around the center to bake. Default is 0 (only the center position).<br/>
    /// Note: The radius is measured in grid units.
    /// </param>
    public void BakeRegion(Vector2 center, float radius = 0)
    {

        Logger.Debug("BAKING AUTOTILES for region centered at {center} with radius {radius}", center, radius);
        
        if (!RuntimeServices.MapService.HasLoadedMap)
        {
            Logger.Error("Cannot bake autotiles: No map is currently loaded.");
            return;
        }

        if (IntGridSet == null)
        {
            Logger.Error("Cannot bake autotiles: IntGridTilesetDef is not set.");
            return;
        }
        
        radius *= RuntimeServices.MapService.CurrentLoadedMapDefinition!.GridParameter.CellHeight;
        
        for (var x = center.X - radius; x <= center.X + radius; x++)
        {
            for (var y = center.Y - radius; y <= center.Y + radius; y++)
            {
                var position = new Vector2(x, y);
                var newTile = AutoTileSolver.Resolve(position, SourceIntGrid, IntGridSet.Rules);
                
                if (newTile != null)
                {
                    InternalTileLayer.AddElement(newTile, position);
                    BakeDirtyTiles(CheckAround(position));
                }
                else if(InternalTileLayer.GetElement(position) == null && SourceIntGrid.Elements != null && SourceIntGrid.HasElement(position))
                {
                    var defaultTile = IntGridSet.IntRefs[SourceIntGrid.GetValue(position)].DefaultTileData;
                    var tile = defaultTile.ToTileDef();

                    InternalTileLayer.AddElement(tile, position);
                    BakeDirtyTiles(CheckAround(position));
                }
                else
                {
                    InternalTileLayer.TryRemoveElement(position, out _);
                    BakeDirtyTiles(CheckAround(position));
                }
            }
        }
    }
    
    protected class BakeContext
    {
        private HashSet<Vector2> PositionsAlreadyChecked { get; } = new();
        private Queue<Vector2> PositionsToCheck { get; } = new();
        
        public void AddPositionsToCheck(IEnumerable<Vector2> positions)
        {
            foreach (var pos in positions)
            {
                if (!PositionsAlreadyChecked.Contains(pos))
                {
                    PositionsToCheck.Enqueue(pos);
                }
            }
        }
        
        public void AddPositionChecked(Vector2 position)
        {
            PositionsAlreadyChecked.Add(position);
        }
        
        public bool HasPositionsToCheck()
        {
            return PositionsToCheck.Count > 0;
        }
        
        public Vector2 DequeuePosition()
        {
            var pos = PositionsToCheck.Dequeue();
            PositionsAlreadyChecked.Add(pos);
            return pos;
        }
        
        public IEnumerable<Vector2>  Positions
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
    
    /// <summary>
    /// Bakes tiles at the positions specified in the context, and recursively checks surrounding tiles for updates.<br/>
    /// This method continues until no more tiles need to be updated, and to avoid infinite loops, it keeps track of already checked positions (context).
    /// </summary>
    /// <param name="context">The bake context containing positions to check and already checked positions.</param>
    protected void BakeDirtyTiles(BakeContext context)
    {
        
        foreach (var position in context.Positions)
        {
            var currentTile = InternalTileLayer.GetElement(position);
            var newTile = AutoTileSolver.Resolve(position, SourceIntGrid, IntGridSet.Rules);

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
            else if(InternalTileLayer.GetElement(position) != null)
            {
                var defaultTile = IntGridSet.IntRefs[SourceIntGrid.GetValue(position)].DefaultTileData;
                var tile = defaultTile.ToTileDef();

                // VERY IMPORTANT HERE: Do not add a "BakeDirtyTiles" call here, or it will create an infinite loop!
                InternalTileLayer.AddElement(tile, position);
            }
        }
    }
    
    /// <summary>
    /// Checks the surrounding positions of the given center position in a grid and adds them to the bake context if they contain elements in the source int grid.
    /// </summary>
    /// <param name="center">The center position to check around.</param>
    /// <param name="context">The existing bake context to update, or null to create a new one.</param>
    /// <param name="GridSize">The size of the grid cells. Default is 32.</param>
    /// <returns></returns>
    protected BakeContext CheckAround(Vector2 center, BakeContext? context = null, int GridSize = 32)
    {
        if(context == null)
        {
            context = new BakeContext();
        }
        
        List<Vector2> positionsToBake = new();
        context.AddPositionChecked(center);
        
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue; // Skip center
                
                var checkPos = new Vector2(center.X + (x * GridSize), center.Y + (y * GridSize));
                
                if(SourceIntGrid.HasElement(checkPos))
                    positionsToBake.Add(checkPos);
            }
        }
        
        context.AddPositionsToCheck(positionsToBake);

        return context;
    }

    public override UrnSingleModule UrnModule => "autolayer".ToUrnSingleModule();
}