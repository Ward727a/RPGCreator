using RPGCreator.Shared.Types;

namespace RPGCreator.Shared.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public class EngineTypeAttribute(string @namespace, string module, params string[] names) : Attribute
{
    public URN Urn = new URN(@namespace, module, string.Join('/', names));
}