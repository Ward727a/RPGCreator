namespace RPGCreator.SDK.ECS;

public static class ComponentTypeIdRegistry
{
    private static readonly Dictionary<System.Type, int> _componentTypeToId = new();
    private static int _nextId = 0;
    
    /// <summary>
    /// This method returns a unique integer ID for each component type T.
    /// The ID is generated the first time a component type is requested and is stored in a dictionary for future reference.
    /// This allows for efficient mapping of component types to IDs, which can be useful for various ECS operations, such as bitmasking and quick lookups.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static int GetComponentTypeId<T>() where T : IComponent
    {
        var type = typeof(T);
        if (!_componentTypeToId.TryGetValue(type, out var id))
        {
            var tempId = _nextId++;
            _componentTypeToId[type] = tempId;
            id = tempId;
        }
        return id;
    }
    
    public static int GetComponentTypeId(System.Type type)
    {
        if (!typeof(IComponent).IsAssignableFrom(type))
            throw new ArgumentException($"Type {type.FullName} does not implement IComponent interface.");

        if (!_componentTypeToId.TryGetValue(type, out var id))
        {
            var tempId = _nextId++;
            _componentTypeToId[type] = tempId;
            id = tempId;
        }
        return id;
    }
    
    /// <summary>
    /// Return the Type associated with a given component type ID.
    /// This method iterates through the dictionary to find the type that corresponds to the provided ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static System.Type? GetType(int id)
    {
        return (from kvp in _componentTypeToId where kvp.Value == id select kvp.Key).FirstOrDefault();
    }

    /// <summary>
    /// This method is an alias for GetComponentTypeId and is used to get the bit position for a component type T.
    /// The bit position can be used in a BitArray to represent the presence or absence of the component in an entity.
    /// This is useful for quickly checking if an entity has a specific component using bitwise operations.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static int GetBit<T>() where T : IComponent => GetComponentTypeId<T>();
    public static int GetBit(System.Type type) => GetComponentTypeId(type);
}