using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using RPGCreator.SDK.Logging;

namespace RPGCreator.SDK.Types;

[DebuggerDisplay("{ToString()}")]
public readonly record struct URN
{
    
    public string Namespace { get; }
    public string Module { get; }
    public string Name { get; }
    
    public string FullName => $"{Namespace}://{Module}/{Name}".ToLowerInvariant().Trim();

    public static URN Empty => new ("", "", "");
    
    private string Normalize(string value)
    {
        return value.ToLowerInvariant().Trim();
    }
    
    /// <summary>
    /// Create a new URN with the specified namespace, module, and name.<br/>
    /// The namespace, module, and name are normalized to lowercase and trimmed of whitespace.<br/>
    /// If any of the parts are empty or null, an error is logged and the URN will not be created.<br/>
    /// The URN format is: "namespace://module/name".<br/>
    /// Example: "rpgc://characters/hero".<br/>
    /// <br/>
    /// You should use snake_case! Like: "rpgc://my_module/my_name" and not "RPGC://MyModule/MyName".
    /// </summary>
    /// <param name="namespace">The namespace part of the URN.</param>
    /// <param name="module">The module part of the URN.</param>
    /// <param name="name">The name part of the URN.</param>
    public URN(string @namespace, string module, string name)
    {
        Namespace = Normalize(@namespace);
        Module = Normalize(module);
        Name = Normalize(name);
        
        if (string.IsNullOrWhiteSpace(Namespace) || string.IsNullOrWhiteSpace(Module) || string.IsNullOrWhiteSpace(Name))
        {
            Logger.Error("URN cannot have empty namespace, module, or name.");
            return;
        }
    }

    /// <summary>
    /// Create a new URN with the specified module and name, using a default namespace.<br/>
    /// The namespace is set to "rpgc" by default.
    /// </summary>
    /// <param name="module">The module part of the URN.</param>
    /// <param name="name">The name part of the URN.</param>
    public URN(string module, string name)
    {
        string @namespace = "rpgc";
        Namespace = Normalize(@namespace);
        Module = Normalize(module);
        Name = Normalize(name);
        if (string.IsNullOrWhiteSpace(Namespace) || string.IsNullOrWhiteSpace(Module) || string.IsNullOrWhiteSpace(Name))
        {
            Logger.Error("URN cannot have empty namespace, module, or name.");
            return;
        }
    }
    
    public URN(string fullUrn)
    {
        if (!TryParse(fullUrn, out var result))
        {
            Namespace = "";
            Module = "";
            Name = "";
            Logger.Error("Failed to parse URN from string: {FullUrn}", fullUrn);
            return;
        }

        Namespace = result.Value.Namespace;
        Module = result.Value.Module;
        Name = result.Value.Name;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => $"{Namespace}://{Module}/{Name}";

    
    public bool IsEmpty => string.IsNullOrEmpty(Namespace) || string.IsNullOrEmpty(Module) || string.IsNullOrEmpty(Name);
    
    public void Deconstruct(out string @namespace, out string module, out string name)
    { @namespace = Namespace; module = Module; name = Name; }
    
    public static bool TryParse(string urn, [NotNullWhen(true)]out URN? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(urn))
        {
            Logger.Error("URN cannot be null or empty.");
            return false;
        }

        var parts = urn.Split(new[] { "://", "/" }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length >= 3)
        {
            var leftover = string.Join("/", parts, 2, parts.Length - 2);
            result = new URN(parts[0], parts[1], leftover);
            return true;
        }
        
        Logger.Error("URN must be in the format 'namespace://module/name'.");
        return false;

    }

    public static URN Parse(string urn)
    {
        if (TryParse(urn, out var result)) return result.Value;
        
        Logger.Error("URN parsing failed: expected \"namespace://module/name\" got {URN} ", urn);
        return Empty;
    }
    
    public static implicit operator string(URN urn) => urn.ToString();
    public static implicit operator URN(string urn) => Parse(urn);
}