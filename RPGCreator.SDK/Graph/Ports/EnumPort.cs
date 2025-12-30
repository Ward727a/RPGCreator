using RPGCreator.Core.Types.Blueprint;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Serializer;

namespace RPGCreator.SDK.Graph.Ports;

public class EnumPort : Port
{
    public System.Type EnumType { get; private set; }

    public EnumPort()
    {
    }

    public override void SetData(GraphDocument.PortData data)
    {
        base.SetData(data);
        if (data.Value is string strValue && Enum.IsDefined(EnumType, strValue))
        {
            Value = Enum.Parse(EnumType, strValue);
        } else if(data.Value is Enum enumValue && EnumType.IsInstanceOfType(enumValue))
        {
            Value = enumValue;
        }
        else if (data.Value is int intValue && Enum.IsDefined(EnumType, intValue))
        {
            Value = Enum.ToObject(EnumType, intValue);
        }
        else
        {
            Logger.Error("The value '{Value}' is not a valid value for the enum type '{EnumType}'.", data.Value, EnumType.Name);
            Value = Enum.GetValues(EnumType).GetValue(0); // Default to the first value of the enum
        }
    }

    public EnumPort(System.Type enumType)
    {
        if (!enumType.IsEnum)
            throw new ArgumentException("The provided type must be an enum type.", nameof(enumType));
        
        Kind = PortKind.Enum;
        ValueType = enumType.Name;
        EnumType = enumType;
        Value = Enum.GetValues(enumType).GetValue(0); // Default to the first value of the enum
    }

    public override SerializationInfo GetObjectData()
    {
        var info = base.GetObjectData()
            .AddValue(nameof(EnumType), EnumType);
        return info;
    }
    
    public override void SetObjectData(DeserializationInfo info)
    {
        base.SetObjectData(info);
        info.TryGetValue(nameof(EnumType), out System.Type enumType, typeof(Enum));
        if (!enumType.IsEnum)
            throw new InvalidOperationException("The deserialized type is not an enum type.");
        EnumType = enumType;
        var value_string = Value;
        if (value_string is string strValue && Enum.IsDefined(enumType, strValue))
        {
            Value = Enum.Parse(enumType, strValue);
        }
        else
        {
            Logger.Error("The value '{Value}' is not a valid value for the enum type '{EnumType}'.", value_string, enumType.Name);
            Value = Enum.GetValues(enumType).GetValue(0); // Default to the first value of the enum
        }
    }
}