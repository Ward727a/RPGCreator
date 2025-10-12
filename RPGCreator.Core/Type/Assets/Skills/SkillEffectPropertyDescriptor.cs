using RPGCreator.Core.Type.Internal;

namespace RPGCreator.Core.Type.Assets.Skills;

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
            EffectPropertyType.Vector2 => typeof(Point),
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
            EffectPropertyType.Vector2 => new Point(0, 0),
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

public record SkillEffectPropertyDescriptor
{
    public string Name {get; init; }
    public EffectPropertyType Type {get; init; }
    public object DefaultValue {get; init; }
}