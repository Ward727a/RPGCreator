using RPGCreator.Core.Types.Internal;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.Types.Interfaces;

public interface IAssetDef : IHasUniqueId
{
    bool IsDirty { get; set; }
    bool IsTransient { get; set; }
}