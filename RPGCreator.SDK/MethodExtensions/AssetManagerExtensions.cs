using RPGCreator.SDK.Assets.Definitions;
using RPGCreator.SDK.EngineService;

namespace RPGCreator.SDK.Extensions;

public static class AssetManagerExtensions
{
    public static IEnumerable<T> GetAssets<T>(this IAssetsManager manager) where T : class, IBaseAssetDef
    {
        foreach (var result in manager.SearchAllPacks<T>())
        {
            if (manager.TryResolveAsset<T>(result.AssetId, out var asset))
            {
                yield return asset;
            }
        }
    }
}