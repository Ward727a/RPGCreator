namespace RPGCreator.SDK.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public class SerializingTypeAttribute(string typeId) : Attribute
{
    public override string TypeId { get; } = typeId;
}