using System.Numerics;
using RPGCreator.SDK.Assets.Definitions.Maps.IntGrid;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Assets.Definitions.Maps.Layers.AutoLayer;

public interface IAutoTileSolver
{
    public Result<ITileDef> Resolve(
        Vector2 position,
        IntGridLayerDefinition intLayer,
        List<AutoLayerRule> rules,
        int gridSize = 32);
}