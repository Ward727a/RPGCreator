using RPGCreator.Core.Types.Internal;

namespace RPGCreator.Core.Types.Assets;

public interface IAssetDef : IHasUniqueId
{
    bool IsDirty { get; set; }
    bool IsTransient { get; set; }
}