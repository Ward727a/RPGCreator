namespace RPGCreator.SDK.Common.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class ExposeToPluginAttribute : Attribute
{
    public string RegionId { get; }

    public ExposeToPluginAttribute(string regionId)
    {
        RegionId = regionId;
    }
}