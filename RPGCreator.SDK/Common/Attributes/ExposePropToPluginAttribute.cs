namespace RPGCreator.SDK.Attributes;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class ExposePropToPluginAttribute : Attribute
{
    /// <summary>
    /// The region ID where this property should be exposed in the plugin UI.
    /// </summary>
    public string RegionId { get; }
    
    /// <summary>
    /// Indicates whether the property can be set from the plugin UI.
    /// </summary>
    public bool CanSet { get; }

    public ExposePropToPluginAttribute(string regionId, bool canSet = false)
    {
        RegionId = regionId;
        CanSet = canSet;
    }
}