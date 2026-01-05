namespace RPGCreator.SDK.Modules.Attributes;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class ExposePropToPluginAttribute : Attribute
{
    public string RegionId { get; }
    public bool CanSet { get; }

    public ExposePropToPluginAttribute(string regionId, bool canSet = false)
    {
        RegionId = regionId;
        CanSet = canSet;
    }
}