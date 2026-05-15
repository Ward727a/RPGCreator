using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public class EngineTypeAttribute(string @namespace, string module, params string[] names) : Attribute
{
    public URN Urn = new URN(@namespace, module, string.Join('/', names));
}