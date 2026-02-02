using System.Numerics;
using RPGCreator.Core.Types.Map;
using RPGCreator.SDK.Assets.Definitions.Tilesets;

namespace RPGCreator.SDK.Assets.Definitions.Maps.AutoLayer;

public interface IAutoTileSolver
{
    public ITileDef? Resolve(
        Vector2 position,
        IntGridLayerDefinition intLayer,
        List<AutoLayerRule> rules,
        int gridSize = 32);
}