namespace RPGCreator.Core.Types.Assets.Tilesets;

public enum ERulePos
{
    TOP_LEFT,
    TOP,
    TOP_RIGHT,
    LEFT,
    RIGHT,
    BOTTOM_LEFT,
    BOTTOM,
    BOTTOM_RIGHT,
}

public enum ERuleType
{
    WHITELIST,
    BLACKLIST,
}
public class AutotileRule : ISerializable, IDeserializable
{
    public Ulid Unique { get; private set; } = Ulid.NewUlid();
    public string Name = "";
    public string Description = "";
    public ERuleType Type = ERuleType.WHITELIST;
    public ERulePos Side = ERulePos.TOP_LEFT; // Default position, can be changed later
    public List<string> Tags = [];
    
    public AutotileRule()
    {
    }

    public SerializationInfo GetObjectData()
    {
        SerializationInfo info = new SerializationInfo(typeof(AutotileRule));
        info.AddValue("ID", Unique);
        info.AddValue("Name", Name);
        info.AddValue("Description", Description);
        info.AddValue("Type", Type);
        info.AddValue("Side", Side);
        info.AddValue("Tags", Tags);
        return info;
    }

    public void SetObjectData(Serializer.DeserializationInfo info)
    {
        info.TryGetValue("ID", out Ulid unique);
        info.TryGetValue("Name", out Name);
        info.TryGetValue("Description", out Description);
        info.TryGetValue("Type", out Type);
        info.TryGetValue("Side", out Side);
        info.TryGetList("Tags", out Tags);
            
        this.Unique = unique;
    }
}