using RPGCreator.Core.Types.Internal;

namespace RPGCreator.SDK.ECS;

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