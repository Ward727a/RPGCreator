using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Assets.Definitions.Maps;

[SerializingType("SGridParameter")]
public struct SGridParameter : ISerializable, IDeserializable
{
    public float CellWidth;
    public float CellHeight;

    public Size CellBorderSize => new Size(CellWidth, CellHeight);
    
    public Color CellBorderColor;


    public SerializationInfo GetObjectData()
    {
        
        return new SerializationInfo(typeof(SGridParameter))
            .AddValue(nameof(CellWidth), CellWidth)
            .AddValue(nameof(CellHeight), CellHeight)
            .AddValue(nameof(CellBorderColor), CellBorderColor);
        
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        return new List<Ulid>();
    }

    public void SetObjectData(DeserializationInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);

        info.TryGetValue(nameof(CellWidth), out var cellWidth, 32);
        CellWidth = cellWidth;
        info.TryGetValue(nameof(CellHeight), out var cellHeight, 32);
        CellHeight = cellHeight;
        info.TryGetValue(nameof(CellBorderColor), out var cellBorderColor, Color.Black);
        CellBorderColor = cellBorderColor;
    }
}