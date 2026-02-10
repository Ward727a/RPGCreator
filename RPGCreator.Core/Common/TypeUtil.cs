using System.Reflection;
using RPGCreator.Core.Module;
using RPGCreator.SDK.Attributes;

namespace RPGCreator.Core.Common;

public static class TypeUtil
{
    
    private static readonly Dictionary<Type, List<string>> _inheritanceCache = new();

    public static List<string> GetInheritance(Type type)
    {
        if(_inheritanceCache.TryGetValue(type, out var cached))
            return cached;
        var names = new List<string>();
    
        var typesToScan = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetLoadableTypes())
            .Where(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);

        foreach (var t in typesToScan)
        {
            var attr = t.GetCustomAttribute<SerializingTypeAttribute>();
            if (attr != null)
            {
                names.Add(attr.TypeId);
            }
        
            names.Add(t.FullName ?? t.Name);
        }

        _inheritanceCache[type] = names.Distinct().ToList();
        return _inheritanceCache[type];
    }
    
    public static List<string> GetChildrenType<T>()
    {
        // Get all types that inherit from T
        var type = typeof(T);
        var names = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.FullName.StartsWith("System") && !a.FullName.StartsWith("Microsoft"))
            .SelectMany(s => s.GetLoadableTypes())
            .Where(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract)
            .Select(t => t.FullName ?? t.Name)
            .ToList();
        return names;
    }
    
}