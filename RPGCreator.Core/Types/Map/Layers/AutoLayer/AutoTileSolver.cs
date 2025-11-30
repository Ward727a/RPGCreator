using RPGCreator.Core.Managers.AssetsManager;
using RPGCreator.Core.Types.Assets.Tilesets;
using RPGCreator.Core.Types.Internal;

namespace RPGCreator.Core.Types.Map.AutoLayer;

public class AutoTileSolver
{
    public static ITileDef? Resolve(
        Point position,
        IntGridLayerDefinition intLayer,
        List<AutoLayerRule> rules,
        AssetsManager assets)
    {
        int centerValue = intLayer.GetValue(position);
        
        if(centerValue == int.MinValue) // No tile present
            return null;

        foreach (var rule in rules)
        {
            if (rule.TargetIntGridValue != centerValue) continue;

            if (MatchesPattern(position, intLayer, rule.Pattern, centerValue))
            {
                return PickTile(rule, position, assets);
            }
        }
        
        return null;
    }

    private static bool MatchesPattern(Point position, IntGridLayerDefinition layer, PatternConstraint[] pattern,
        int centerValue)
    {
        int idx = 0; // Pattern index
        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                if (x == 0 && y == 0)
                {
                    idx++;
                    continue; // Skip center
                }

                var constraint = pattern[idx];
                idx++;

                if (constraint.Condition == PatternCondition.DontCare)
                    continue;
                
                var targetValue = constraint.IsRelative ? centerValue : constraint.TargetValue;
                var neighborValue = layer.GetValue(new Point(position.X + x, position.Y + y));

                switch (constraint.Condition)
                {
                    case PatternCondition.MustBe when neighborValue != targetValue:
                    case PatternCondition.MustNotBe when neighborValue == targetValue:
                        return false;
                }
            }
        }
        return true;
    }
    
    private static ITileDef? PickTile(AutoLayerRule rule, Point position, AssetsManager assets, AssetScope? scope = null)
    {

        if (scope == null)
            scope = EngineCore.Instance.Managers.Assets.CreateAssetScope();
        
        if (rule.OutputTiles.Count == 0)
            return null;

        int seed = position.X * 73856093 ^ position.Y * 19349663;
        int index = Math.Abs(seed) % rule.OutputTiles.Count;
        
        var tileData = rule.OutputTiles[index];

        var tileset = scope.Load<ITilesetDef>(tileData.TilesetId);
        return tileset.GetTileAt((int)(tileData.TilePosition.X / tileset.TileWidth), (int)(tileData.TilePosition.Y / tileset.TileHeight));
    }
}