using System.Numerics;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Assets.Definitions.Skills;

public enum EffectPropertyType
{
    None,
    Number,
    Text,
    Boolean,
    Vector2,
    SkillReference,
    ItemReference,
    StatReference,
    AnimationReference,
    SoundReference,
    /// For complex types that need custom editor UI<br/>
    /// If used, the static method:<br/>
    /// <c>CreateControl{PropertyName}(string propertyName, object value, ISkillEffect effect)</c><br/>
    /// must be implemented in the effect class to create the custom control
    Custom,
    
}

public static class EffectPropertyTypeExtensions
{
    public static System.Type ToSystemType(this EffectPropertyType propertyType)
    {
        return propertyType switch
        {
            EffectPropertyType.None => typeof(object),
            EffectPropertyType.Number => typeof(float),
            EffectPropertyType.Text => typeof(string),
            EffectPropertyType.Boolean => typeof(bool),
            EffectPropertyType.Vector2 => typeof(Vector2),
            EffectPropertyType.SkillReference => typeof(URN),
            EffectPropertyType.ItemReference => typeof(URN),
            EffectPropertyType.StatReference => typeof(URN),
            EffectPropertyType.AnimationReference => typeof(URN),
            EffectPropertyType.SoundReference => typeof(URN),
            EffectPropertyType.Custom => typeof(object),
            _ => throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, null)
        };
    }

    public static object GetDefaultValue(this EffectPropertyType propertyType)
    {
        return propertyType switch
        {
            EffectPropertyType.None => null!,
            EffectPropertyType.Number => 0f,
            EffectPropertyType.Text => string.Empty,
            EffectPropertyType.Boolean => false,
            EffectPropertyType.Vector2 => Vector2.Zero,
            EffectPropertyType.SkillReference => URN.Empty,
            EffectPropertyType.ItemReference => URN.Empty,
            EffectPropertyType.StatReference => URN.Empty,
            EffectPropertyType.AnimationReference => URN.Empty,
            EffectPropertyType.SoundReference => URN.Empty,
            EffectPropertyType.Custom => null!,
            _ => throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, null)
        };
    }
}

[SerializingType("SkillEffectPropertyDescriptor")]
public record SkillEffectPropertyDescriptor : ISerializable, IDeserializable
{
    public string Name {get; set; }
    public EffectPropertyType Type {get; set; }
    
    public object DefaultValue {get; set; }
    public SerializationInfo GetObjectData()
    {
        
        return new SerializationInfo(typeof(SkillEffectPropertyDescriptor))
            .AddValue(nameof(Name), Name)
            .AddValue(nameof(Type), Type)
            .AddValue(nameof(DefaultValue), DefaultValue);
        
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        // If the property is a reference type, we might want to track it as a referenced asset
        if (Type == EffectPropertyType.SkillReference ||
            Type == EffectPropertyType.ItemReference ||
            Type == EffectPropertyType.StatReference ||
            Type == EffectPropertyType.AnimationReference ||
            Type == EffectPropertyType.SoundReference)
        {
            if (DefaultValue is URN urn && urn != URN.Empty)
            {
                // Assuming the URN format is "type:subtype:id"
                var parts = urn.ToString().Split(':');
                if (parts.Length == 3 && Ulid.TryParse(parts[2], out var assetId))
                {
                    return [assetId];
                }
            }
        }
        return [];
    }

    public void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue(nameof(Name), out string name, string.Empty);
        info.TryGetValue(nameof(Type), out EffectPropertyType type, EffectPropertyType.None);
        info.TryGetValue(nameof(DefaultValue), out object defaultValue, type.GetDefaultValue());
        
        Name = name;
        Type = type;
        DefaultValue = defaultValue;
    }
}