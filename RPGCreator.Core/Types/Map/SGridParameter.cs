using Microsoft.Xna.Framework;

namespace RPGCreator.Core.Types.Map;

public struct SGridParameter : ISerializable, IDeserializable
{
    public int CellWidth;
    public int CellHeight;

    public Color CellBorderColor;


    public SerializationInfo GetObjectData()
    {
        
        return new SerializationInfo(typeof(SGridParameter))
            .AddValue(nameof(CellWidth), CellWidth)
            .AddValue(nameof(CellHeight), CellHeight)
            .AddValue(nameof(CellBorderColor), CellBorderColor);
        
    }

    public void SetObjectData(Serializer.DeserializationInfo info)
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