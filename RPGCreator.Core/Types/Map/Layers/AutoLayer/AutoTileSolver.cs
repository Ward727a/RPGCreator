using RPGCreator.Core.Managers.AssetsManager;
using RPGCreator.Core.Types.Assets.Tilesets;
using RPGCreator.Core.Types.Internal;

namespace RPGCreator.Core.Types.Map.AutoLayer;

public class AutoTileSolver
{
    private class PatternMatch()
    {
        public AutoLayerRule Rule { get; set; }
        public int Score { get; set; }
        public bool FlipX { get; set; }
        public bool FlipY { get; set; }
    }
    
    public static ITileDef? Resolve(
        Point position,
        IntGridLayerDefinition intLayer,
        List<AutoLayerRule> rules,
        AssetsManager assets,
        int GridSize = 32)
    {
        int centerValue = intLayer.GetValue(position);
        
        if(centerValue == int.MinValue) // No tile present
            return null;

        
        List<PatternMatch> matchedRules = new();
        
        foreach (var rule in rules)
        {
            if (rule.TargetIntGridValue != centerValue) continue;

            if (MatchesPattern(position, intLayer, rule, centerValue, out int score))
            {
                matchedRules.Add(new PatternMatch() { Rule = rule, Score = score, FlipX = false, FlipY = false });
            }
            else if (rule.FlipX && MatchesXFlippedPattern(position, intLayer, rule, centerValue))
            {
                matchedRules.Add(new PatternMatch() { Rule = rule, Score = score, FlipX = true, FlipY = false });
            }
            else if (rule.FlipY && MatchesYFlippedPattern(position, intLayer, rule, centerValue))
            {
                matchedRules.Add(new PatternMatch() { Rule = rule, Score = score, FlipX = false, FlipY = true });
            }
            
        }

        if (matchedRules.Count > 0)
        {
            // Pick the best match (highest score)
            var bestMatch = matchedRules.OrderByDescending(m => m.Score).First();
            return PickTile(bestMatch.Rule, position, assets, null, bestMatch.FlipX, bestMatch.FlipY);
        }
        
        return null;
    }

    private static bool MatchesPattern(Point position, IntGridLayerDefinition layer, AutoLayerRule rule,
        int centerValue, out int score, int GridSize = 32)
    {
        var pattern = rule.Pattern;
        score = 0;
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
                var neighborValue = layer.GetValue(new Point(position.X + (x * GridSize), position.Y + (y * GridSize)));

                switch (constraint.Condition)
                {
                    case PatternCondition.MustBe when neighborValue != targetValue:
                    case PatternCondition.MustNotBe when neighborValue == targetValue:
                        return false;
                }
                score++;
            }
        }
        return true;
    }
    
    private static bool MatchesXFlippedPattern(Point position, IntGridLayerDefinition layer, AutoLayerRule rule,
        int centerValue, int GridSize = 32)
    {
        var pattern = rule.Pattern;
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
                var neighborValue = layer.GetValue(new Point(position.X + (-x * GridSize), position.Y + (y * GridSize)));

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
    
    private static bool MatchesYFlippedPattern(Point position, IntGridLayerDefinition layer, AutoLayerRule rule,
        int centerValue, int GridSize = 32)
    {
        var pattern = rule.Pattern;
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
                var neighborValue = layer.GetValue(new Point(position.X + (x * GridSize), position.Y + (-y * GridSize)));

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
    
    private static ITileDef? PickTile(AutoLayerRule rule, Point position, AssetsManager assets, AssetScope? scope = null, bool flipX = false, bool flipY = false)
    {

        if (scope == null)
            scope = EngineCore.Instance.Managers.Assets.CreateAssetScope();
        
        if (rule.OutputTiles.Count == 0)
            return null;

        int seed = position.X * 73856093 ^ position.Y * 19349663;
        int index = Math.Abs(seed) % rule.OutputTiles.Count;
        
        var tileData = rule.OutputTiles[index];

        var tileset = scope.Load<ITilesetDef>(tileData.TilesetId);
        var tile = tileset.GetTileAt((int)(tileData.TilePosition.X / tileset.TileWidth), (int)(tileData.TilePosition.Y / tileset.TileHeight));

        if (!flipX && !flipY)
            return tile;

        if (flipX)
        {
            tile.Flip |= TileFlip.Horizontal;
        }
        if (flipY)
        {
            tile.Flip |= TileFlip.Vertical;
        }
        return tile;
    }
}