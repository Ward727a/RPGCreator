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

public record SkillEffectPropertyDescriptor
{
    public string Name {get; init; }
    public EffectPropertyType Type {get; init; }
    public object DefaultValue {get; init; }
}