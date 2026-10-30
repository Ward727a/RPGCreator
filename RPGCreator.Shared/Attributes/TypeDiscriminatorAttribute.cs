using RPGCreator.Shared.Types;

namespace RPGCreator.Shared.Attributes;

/// <summary>
/// An attribute used to define a type discriminator for the targeted class or struct. This attribute
/// provides a mechanism for uniquely identifying a type using a hierarchical namespace, module, and name format.
/// </summary>
/// <remarks>
/// The <see cref="TypeDiscriminatorAttribute"/> is employed to associate types with a unique
/// Uniform Resource Name (URN) that can be processed for runtime type identification or registry purposes.
/// </remarks>
/// <param name="namespace">
/// The namespace segment of the URN that identifies the high-level grouping of the type.
/// </param>
/// <param name="module">
/// The module segment of the URN that groups related types within the namespace.
/// </param>
/// <param name="names">
/// A collection of name segments that define the specific type identity within the namespace and module.
/// </param>
/// <example>
/// The attribute is typically utilized in systems that require type registration or dynamic identification.
/// </example>
/// <seealso cref="URN"/>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
[EngineAttr(order:-100)]
public class TypeDiscriminatorAttribute(string @namespace, string module, params string[] names) : Attribute
{
    public URN Urn = new URN(@namespace, module, string.Join('/', names));
}