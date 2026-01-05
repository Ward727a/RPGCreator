namespace RPGCreator.SDK.Modules.Attributes;

[AttributeUsage(AttributeTargets.Event, Inherited = false, AllowMultiple = false)]
public class ExposeEventToPluginAttribute : Attribute
{
    public string RegionId { get; }
    public ExposeEventToPluginAttribute(string regionId)
    {
        RegionId = regionId;
    }
}