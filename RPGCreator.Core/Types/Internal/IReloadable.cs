using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.Core.Types.Internal;

/// <summary>
/// Define a type that can be reloaded via the factory.
/// If a type doesn't implement this interface, it will not be reloaded when the factory has a new definition for it.
/// </summary>
public interface IReloadable<in TDef> where TDef : IHasUniqueId
{
    void Reload(TDef newDefinition);
}