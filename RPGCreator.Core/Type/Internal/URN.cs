using System.Diagnostics;
using System.Runtime.CompilerServices;
using Serilog;

namespace RPGCreator.Core.Type.Internal;

[DebuggerDisplay("{ToString()}")]
public readonly record struct URN
{
    
    public string Namespace { get; }
    public string Module { get; }
    public string Name { get; }
    
    public string FullName => $"{Namespace}://{Module}/{Name}".ToLowerInvariant().Trim();

    public static readonly URN Empty = default;
    
    private string Normalize(string value)
    {
        return value.ToLowerInvariant().Trim();
    }
    
    /// <summary>
    /// Create a new URN with the specified namespace, module, and name.<br/>
    /// The namespace, module, and name are normalized to lowercase and trimmed of whitespace.<br/>
    /// If any of the parts are empty or null, an error is logged and the URN will not be created.<br/>
    /// The URN format is: "namespace://module/name".<br/>
    /// Example: "rpgc://characters/hero".
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
            Log.Error("URN cannot have empty namespace, module, or name.");
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
            Log.Error("URN cannot have empty namespace, module, or name.");
            return;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => $"{Namespace}://{Module}/{Name}";

    
    public bool IsEmpty => string.IsNullOrEmpty(Namespace);
    
    public void Deconstruct(out string @namespace, out string module, out string name)
    { @namespace = Namespace; module = Module; name = Name; }
    
    public static bool TryParse(string urn, out URN? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(urn))
        {
            Log.Error("URN cannot be null or empty.");
            return false;
        }

        var parts = urn.Split(new[] { "://", "/" }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 3)
        {
            result = new URN(parts[0], parts[1], parts[2]);
            return true;
        }
        
        Log.Error("URN must be in the format 'namespace://module/name'.");
        return false;

    }

    public static URN Parse(string urn)
    {
        if (TryParse(urn, out var result)) return result.Value;
        
        Log.Error("URN parsing failed: expected \"namespace://module/name\" got {URN} ", urn);
        return Empty;
    }
}