using RPGCreator.SDK.Logging;

namespace RPGCreator.SDK.Serializer;

/// <summary>
/// This class is used to hold the serialization information of an object.
/// </summary>
public sealed class SerializationInfo
{
    public Type ObjectType { get; private set; }
    public string AssemblyName { get;  private set; }
    public string QualifiedName { get;  private set; }

    // On stocke directement l'objet (int, string, List<T>, ou ISerializable)
    // Json.NET se débrouillera pour sérialiser ce qu'il y a dedans.
    private readonly Dictionary<string, object?> _values = new();

    public SerializationInfo(Type objectType)
    {
        ObjectType = objectType;
        AssemblyName = objectType.Assembly.FullName ?? "";
        QualifiedName = objectType.FullName ?? "";
    }

    // --- AJOUT DE VALEURS (API Simplifiée) ---

    // Cette méthode unique remplace toutes tes surcharges AddValue/SetValue.
    // Json.NET est intelligent : si 'value' est une Liste, un Dico ou un ISerializable, 
    // il saura quoi en faire grâce à notre EngineJsonConverter.
    public SerializationInfo AddValue(string name, object? value)
    {
        if (string.IsNullOrEmpty(name))
        {
            Logger.Error("Cannot add value with null or empty name.");
            return this;
        }

        // Pas besoin de wrapper dans un Entry !
        _values[name] = value; 
        return this;
    }

    // Surcharge pour supprimer une valeur (utile pour les updates)
    public SerializationInfo RemoveValue(string name)
    {
        _values.Remove(name);
        return this;
    }

    // --- RECUPERATION (Pour le Converter) ---

    public List<string> GetNames() => _values.Keys.ToList();

    public bool TryGetValue(string name, out object? value)
    {
        return _values.TryGetValue(name, out value);
    }
    
    // Garde cette méthode si tu en as besoin ailleurs, mais TryGetValue suffit souvent
    public object? GetValue(string name)
    {
        return _values.TryGetValue(name, out var val) ? val : null;
    }
}