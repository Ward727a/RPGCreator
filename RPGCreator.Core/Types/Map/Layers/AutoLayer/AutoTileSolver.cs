using System.Numerics;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps.AutoLayer;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Types.Collections;

namespace RPGCreator.Core.Types.Map.Layers.AutoLayer;

public class AutoTileSolver : IAutoTileSolver
{
    private class PatternMatch()
    {
        public AutoLayerRule Rule { get; set; }
        public int Score { get; set; }
        public bool FlipX { get; set; }
        public bool FlipY { get; set; }
    }
    
    public ITileDef? Resolve(
        Vector2 position,
        IntGridLayerDefinition intLayer,
        List<AutoLayerRule> rules,
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
            int maxScore = matchedRules.Max(m => m.Score);
            var bestCandidates = matchedRules.Where(m => m.Score == maxScore).ToList();

            if (bestCandidates.Count == 1)
            {
                var match = bestCandidates[0];
                return PickTile(match.Rule, position, null, match.FlipX, match.FlipY);
            }

            var random = new Random(GetSeed(position, intLayer.ZIndex));
    
            double totalWeight = bestCandidates.Sum(m => m.Rule.Chance);
            double randomValue = random.NextDouble() * totalWeight;
    
            foreach (var match in bestCandidates)
            {
                randomValue -= match.Rule.Chance;
                if (randomValue <= 0)
                {
                    return PickTile(match.Rule, position, null, match.FlipX, match.FlipY);
                }
            }
    
            var fallback = bestCandidates.Last();
            return PickTile(fallback.Rule, position, null, fallback.FlipX, fallback.FlipY);
        }

        return null;
    }
    
    private static int GetSeed(Vector2 pos, int layerIndex = 0)
    {
        int hash = 17;
        unchecked 
        {
            hash = hash * 23 + pos.X.GetHashCode();
            hash = hash * 73856093 + pos.Y.GetHashCode();
            hash = hash * 19349663 + layerIndex.GetHashCode(); 
        }
        return hash;
    }

    private static bool MatchesPattern(Vector2 position, IntGridLayerDefinition layer, AutoLayerRule rule,
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
                var neighborValue = layer.GetValue(new Vector2(position.X + (x * GridSize), position.Y + (y * GridSize)));

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
    
    private static bool MatchesXFlippedPattern(Vector2 position, IntGridLayerDefinition layer, AutoLayerRule rule,
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
                var neighborValue = layer.GetValue(new Vector2(position.X + (-x * GridSize), position.Y + (y * GridSize)));

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
    
    private static bool MatchesYFlippedPattern(Vector2 position, IntGridLayerDefinition layer, AutoLayerRule rule,
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
                var neighborValue = layer.GetValue(new Vector2(position.X + (x * GridSize), position.Y + (-y * GridSize)));

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
    
    private static ITileDef? PickTile(AutoLayerRule rule, Vector2 position, IAssetScope? scope = null, bool flipX = false, bool flipY = false)
    {

        if (scope == null)
            scope = EngineServices.AssetsManager.CreateAssetScope();
        
        if (rule.OutputTiles.Count == 0)
            return null;

        int seed = (int)Math.Floor(position.X) * 73856093 ^ (int)Math.Floor(position.Y) * 19349663;
        int index = Math.Abs(seed) % rule.OutputTiles.Count;
        
        var tileData = rule.OutputTiles[index];

        var tilesetDef = scope.Load<BaseTilesetDef>(tileData.TilesetId);
        // Instantiate the tileset
        var tileset = EngineServices.GameFactory.CreateInstance<ITilesetInstance>(tilesetDef);
        var tile = tileset.GetTileAt((int)(tileData.TilePosition.X / tilesetDef.TileWidth), (int)(tileData.TilePosition.Y / tilesetDef.TileHeight));

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