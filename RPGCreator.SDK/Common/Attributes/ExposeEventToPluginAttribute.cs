namespace RPGCreator.SDK.Attributes;

[AttributeUsage(AttributeTargets.Event, Inherited = false, AllowMultiple = false)]
public class ExposeEventToPluginAttribute : Attribute
{
    public string RegionId { get; }
    public ExposeEventToPluginAttribute(string regionId)
    {
        RegionId = regionId;
    }
}