using System.Numerics;
using RPGCreator.Core.Types.Map;
using RPGCreator.SDK.Assets.Definitions.Maps.AutoLayer;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Assets.Definitions.Maps.Layers.AutoLayer;

public static class AutoTileSolver
{
    public static IAutoTileSolver? Service { get; set; }
    
    public static Result<ITileDef> Resolve(Vector2 position, IntGridLayerDefinition intLayer, List<AutoLayerRule> rules, int gridSize = 32)
    {
        return Service?.Resolve(position, intLayer, rules, gridSize) ?? Result.Fail("No solver available.");
    }
}