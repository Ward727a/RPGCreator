using RPGCreator.Core.Type.Internal;

namespace RPGCreator.Core.Type.Assets;

public interface IAssetDef : IHasUniqueId
{
    bool IsDirty { get; set; }
    bool IsTransient { get; set; }
}