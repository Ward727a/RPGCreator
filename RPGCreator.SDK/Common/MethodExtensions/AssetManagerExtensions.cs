using RPGCreator.SDK.Services.EngineService;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.Common.MethodExtensions;

public static class AssetManagerExtensions
{
    public static IEnumerable<T> GetAssets<T>(this IAssetsManager manager) where T : class, IEngineClass
    {
        return manager.LoadedAssets.OfType<T>();
    }
}