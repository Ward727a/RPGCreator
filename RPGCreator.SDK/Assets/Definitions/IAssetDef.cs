using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.Assets.Definitions;

public interface IAssetDef : IHasUniqueId
{
    bool IsDirty { get; set; }
    bool IsTransient { get; set; }
}