using RPGCreator.SDK.Assets.Definitions;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Services.EngineService;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.Extensions;

public static class AssetManagerExtensions
{
    public static IEnumerable<T> GetAssets<T>(this IAssetsManager manager) where T : class, IEngineClass
    {
        return manager.LoadedAssets.OfType<T>();
    }
}