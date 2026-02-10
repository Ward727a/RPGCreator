using System.Globalization;
using System.Numerics;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Serializer;

namespace RPGCreator.SDK.Types;

[SerializingType("Size")]
public record struct Size : ISerializable, IDeserializable
{
    public float Width { get; set; }
    public float Height { get; set; }

    public Size()
    {
        Width = 0;
        Height = 0;
    }
    
    public Size(float width, float height)
    {
        Width = width;
        Height = height;
    }
    
    public static readonly Size Zero = new(0, 0);

    public override string ToString()
    {
        return $"Width: {Width}, Height: {Height}";
    }

    public static implicit operator Vector2(Size s) => new(s.Width, s.Height);
    public static implicit operator Size(Vector2 v) => new(v.X, v.Y);

    public Vector2 ToVector2()
    {
        return new Vector2(Width, Height);
    }
    
    public static Size Parse(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) throw new ArgumentException("Input cannot be empty", nameof(s));

        var parts = s.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length < 4) throw new FormatException("Invalid Size format");

        float w = float.Parse(parts[1], CultureInfo.InvariantCulture);
        float h = float.Parse(parts[3], CultureInfo.InvariantCulture);

        return new Size(w, h);
    }
    
    public SerializationInfo GetObjectData() 
        => new SerializationInfo(typeof(Size)).AddValue("W", Width).AddValue("H", Height);

    public List<Ulid> GetReferencedAssetIds()
    {
        return [];
    }

    public void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue("W", out float w, 0f);
        info.TryGetValue("H", out float h, 0f);
        Width = w;
        Height = h;
    }
}