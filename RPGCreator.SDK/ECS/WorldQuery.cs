using RPGCreator.Core.Types.Internal;
using RPGCreator.SDK.Types.Collections;

namespace RPGCreator.SDK.ECS;

/// <summary>
/// WARNING: This is an experimental API, and maybe change or removed in future versions.<br/>
/// A query to retrieve components from a sparse set with a predicate.
/// </summary>
/// <param name="sparseSet">The sparse set to query.</param>
/// <typeparam name="T">The type of component to query.</typeparam>
public class WorldQuery<T>(ECSSparseSet<T> sparseSet)
    where T : struct, IComponent
{
    public IEnumerable<(int entityId, T component)> With(Func<T, bool> predicate)
    {
        foreach (var (entityId, component) in sparseSet.ActiveElements())
        {
            if (predicate(component))
            {
                yield return (entityId, component);
            }
        }
    }
}