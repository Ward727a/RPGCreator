using System.Numerics;
using RPGCreator.Core.Types.Map;
using RPGCreator.SDK.Assets.Definitions.Maps.AutoLayer;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Assets.Definitions.Maps.Layers.AutoLayer;

public interface IAutoTileSolver
{
    public Result<ITileDef> Resolve(
        Vector2 position,
        IntGridLayerDefinition intLayer,
        List<AutoLayerRule> rules,
        int gridSize = 32);
}