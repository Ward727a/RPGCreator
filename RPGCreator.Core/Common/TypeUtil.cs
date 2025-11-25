namespace RPGCreator.Core.Common;

public static class TypeUtil
{
    
    private static readonly Dictionary<Type, List<string>> _inheritanceCache = new();

    public static List<string> GetInheritance(Type type)
    {
        if(_inheritanceCache.TryGetValue(type, out var cached))
            return cached;

        if (!type.IsInterface && !type.IsAbstract)
        {
            return new List<string> { type.FullName ?? type.Name };
        }
        
        var names = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.FullName.StartsWith("System") && !a.FullName.StartsWith("Microsoft"))
            .SelectMany(s => s.GetTypes())
            .Where(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract)
            .Select(t => t.FullName ?? t.Name)
            .ToList();
        
        _inheritanceCache[type] = names;
        return names;
    }
    
}