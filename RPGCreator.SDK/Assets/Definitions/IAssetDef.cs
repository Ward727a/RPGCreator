using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.Assets.Definitions;

public interface IAssetDef : IHasUniqueId
{
    bool IsDirty { get; set; }
    bool IsTransient { get; set; }
    /// <summary>
    /// Allow initialization of the asset with a specific ID.<br/>
    /// This is used for example when creating a new asset with the <see cref="IAssetsManager.CreateAsset"/> method.
    /// </summary>
    /// <param name="id">The unique identifier to assign to the asset.</param>
    void Init(Ulid id);
}