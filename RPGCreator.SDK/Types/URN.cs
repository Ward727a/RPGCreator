using System.Diagnostics;
using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Logging;

namespace RPGCreator.SDK.Types;

[DebuggerDisplay("{ToString()}")]
[EngineType("rpgc", "sdk", "types", "urn")]
public record struct URN
{
    public readonly ReadOnlyMemory<char> Namespace;
    public readonly ReadOnlyMemory<char> Module;
    public readonly ReadOnlyMemory<char> Name;
    
    public string FullName => $"{Namespace}://{Module}/{Name}".ToLowerInvariant().Trim();

    public static URN Empty => new ();
    
    public URN(ReadOnlyMemory<char> @namespace, ReadOnlyMemory<char> module, ReadOnlyMemory<char> name)
    {
        Namespace = @namespace;
        Module = module;
        Name = name;
    }

    public URN()
    {
        Namespace = ReadOnlyMemory<char>.Empty;
        Module = ReadOnlyMemory<char>.Empty;
        Name = ReadOnlyMemory<char>.Empty;
    }

    public URN(params string[] parts) : this(parts[0], parts[1],
        string.Join("/", parts.Skip(2)).TrimStart("/").ToString())
    {
    }

    /// <summary>
    /// Create a new URN with the specified namespace, module, and name.<br/>
    /// The namespace, module, and name are trimmed of whitespace.<br/>
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
        if (string.IsNullOrWhiteSpace(@namespace) || string.IsNullOrWhiteSpace(module) || string.IsNullOrWhiteSpace(name))
        {
            Logger.Error("URN cannot have empty parts.");
            this = Empty;
            return;
        }

        if (@namespace.Contains("://") || module.Contains('/'))
        {
            throw new ArgumentException("Namespace and/or module cannot contain '://' or '/'.");
        }

        Namespace = @namespace.AsMemory().Trim();
        Module = module.AsMemory().Trim();
        Name = name.AsMemory().Trim();
    }

    /// <summary>
    /// Create a new URN with the specified module and name, using a default namespace.<br/>
    /// The namespace is set to "rpgc" by default.
    /// </summary>
    /// <param name="module">The module part of the URN.</param>
    /// <param name="name">The name part of the URN.</param>
    public URN(string module, string name) : this("rpgc", module, name) { }

    public bool Equals(URN other)
    {
        return Namespace.Span.SequenceEqual(other.Namespace.Span) &&
               Module.Span.SequenceEqual(other.Module.Span) &&
               Name.Span.SequenceEqual(other.Name.Span);
    }
    
    public URN(string fullUrn)
    {
        if (!TryParse(fullUrn, out var result))
        {
            Namespace = ReadOnlyMemory<char>.Empty;
            Module = ReadOnlyMemory<char>.Empty;
            Name = ReadOnlyMemory<char>.Empty;
        
            Logger.Error("Failed to parse URN from string: {FullUrn}", fullUrn);
            return;
        }

        Namespace = result.Namespace;
        Module = result.Module;
        Name = result.Name;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => $"{Namespace.Span}://{Module.Span}/{Name.Span}";

    private int _hashCode = int.MinValue;
    
    public override int GetHashCode()
    {
        if (_hashCode != int.MinValue) return _hashCode;
        
        var h = new HashCode();
        h.Add(Namespace.Span);
        h.Add("://"); 
        h.Add(Module.Span);
        h.Add("/");
        h.Add(Name.Span);
        var result = h.ToHashCode();
        _hashCode = (result == int.MinValue) ? (int.MinValue + 1) : result;
        return _hashCode;
    }

    public static bool TryParse(string? input, out URN result)
    {
        result = default;
        if (string.IsNullOrEmpty(input)) return false;

        ReadOnlySpan<char> span = input.AsSpan();

        int protoIdx = span.IndexOf("://".AsSpan());
        if (protoIdx <= 0) return false;

        ReadOnlySpan<char> afterProto = span.Slice(protoIdx + 3);
        int pathIdx = afterProto.IndexOf('/');
        if (pathIdx <= 0) return false;

        result = new URN(
            input.AsMemory(0, protoIdx),
            input.AsMemory(protoIdx + 3, pathIdx),
            input.AsMemory(protoIdx + 3 + pathIdx + 1)
        );

        return true;
    }

    public static URN Parse(string urn)
    {
        if (TryParse(urn, out var result)) return result;
        
        return Empty;
    }

    [Flags]
    public enum SearchComparisionType
    {
        NamespaceOnly = 1,
        ModuleOnly = 2,
        Full = NamespaceOnly | ModuleOnly,
    }
    public bool StartsWith(string prefix, SearchComparisionType type = SearchComparisionType.Full)
    {
        if (string.IsNullOrWhiteSpace(prefix)) return false;

        ReadOnlySpan<char> prefixSpan = prefix.AsSpan();

        if (type.HasFlag(SearchComparisionType.NamespaceOnly))
        {
            if (!Namespace.Span.StartsWith(prefixSpan, StringComparison.OrdinalIgnoreCase))
            {
                if (prefixSpan.Length > Namespace.Length)
                {
                    if (!prefixSpan.StartsWith(Namespace.Span, StringComparison.OrdinalIgnoreCase)) return false;

                    prefixSpan = prefixSpan.Slice(Namespace.Length);

                    if (prefixSpan.StartsWith("://".AsSpan()))
                    {
                        prefixSpan = prefixSpan.Slice(3);
                    }
                    else return false; // Namespace is wrong (e.g. searching for "rpgca" on a "rgpc" URN)
                }
                else
                    return
                        false; // Namespace is wrong (Size are not right, too short, e.g. searching for "rpg" on a "rpgc" URN)
            }
            else
                return
                    true; // Namespace is correct (e.g. "rpgc://module/name".StartsWith("rpgc") == true - because Namespace ("rpgc") is the same as prefixSpan ("rpgc"))
        }

        if (prefixSpan.IsEmpty) return true; // If prefix is empty, then we can't have a module, and in this case we can return true.

        if (!Module.Span.StartsWith(prefixSpan, StringComparison.OrdinalIgnoreCase) && type.HasFlag(SearchComparisionType.ModuleOnly))
        {
            if (prefixSpan.Length > Module.Length)
            {
                if (!prefixSpan.StartsWith(Module.Span, StringComparison.OrdinalIgnoreCase)) return false;
            
                prefixSpan = prefixSpan.Slice(Module.Length);
            
                if (prefixSpan.StartsWith("/".AsSpan()))
                {
                    prefixSpan = prefixSpan.Slice(1);
                }
                else return false; // Module is wrong (e.g. searching for "rpgc://module_a" on a "rpgc://module/name" URN)
            }
            else return false; // Module is wrong (Size are not right, too short, e.g. searching for "rpgc://mod/" on a "rpgc://module/name" URN)
        }
        else return true; // Module is right (this need to not end with a slash for this return to work, like: "rpgc://module/name".StartsWith("rpgc://module") == true)

        if (prefixSpan.IsEmpty) return true; // If prefix is empty, then we can't have a name, and in this case we can return true.
        return Name.Span.StartsWith(prefixSpan, StringComparison.OrdinalIgnoreCase);
    }
    
    public static implicit operator string(URN urn) => urn.ToString();
    public static implicit operator URN(string urn) => Parse(urn);
}

public record UrnNamespace(ReadOnlyMemory<char> Namespace);
public record UrnModule(UrnNamespace @Ns, ReadOnlyMemory<char> Module);
public record UrnName(UrnModule Module, ReadOnlyMemory<char> Name);

public record UrnSingleModule(ReadOnlyMemory<char> Module);

public static class UrnExtensions
{
    public static UrnNamespace ToUrnNamespace(this string @namespace)
    {
        if  (string.IsNullOrWhiteSpace(@namespace))  
        {
            Logger.Error("Namespace cannot be empty.");
            return new UrnNamespace(ReadOnlyMemory<char>.Empty);
        }
        
        return new UrnNamespace(@namespace.AsMemory().Trim());
    }
    
    public static UrnSingleModule ToUrnSingleModule(this string module)
    {
        if (string.IsNullOrWhiteSpace(module))
        {
            Logger.Error("Module cannot be empty.");
            return new UrnSingleModule(ReadOnlyMemory<char>.Empty);
        }
        
        return new UrnSingleModule(module.AsMemory().Trim());
    }

    public static UrnModule ToUrnModule(this UrnSingleModule singleModule, string @namespace)
    {
        if (string.IsNullOrWhiteSpace(@namespace))
        {
            Logger.Error("Namespace cannot be empty.");
            return new UrnModule(new UrnNamespace(ReadOnlyMemory<char>.Empty), singleModule.Module);
        }
        
        return new UrnModule(new UrnNamespace(@namespace.AsMemory().Trim()), singleModule.Module);
    }

    public static UrnModule ToUrnModule(this UrnNamespace @namespace, string module)
    {
        if (string.IsNullOrWhiteSpace(module))
        {
            Logger.Error("Module cannot be empty.");
            return new UrnModule(@namespace, ReadOnlyMemory<char>.Empty);
        }
        
        return new UrnModule(@namespace, module.AsMemory().Trim());
    }

    public static UrnModule ToUrnModule(this UrnNamespace @namespace, UrnSingleModule singleModule)
    {
        if (singleModule.Module.Span.IsWhiteSpace() || singleModule.Module.IsEmpty)
        {
            Logger.Error("Module cannot be empty.");
            return new UrnModule(@namespace, ReadOnlyMemory<char>.Empty);
        }
        
        return new UrnModule(@namespace, singleModule.Module);
    }

    public static UrnName ToUrnName(this UrnModule module, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Logger.Error("Name cannot be empty.");
            return new UrnName(module, ReadOnlyMemory<char>.Empty);
        }
        
        return new UrnName(module, name.AsMemory().Trim());
    }
    public static UrnModule ToUrnModule(this UrnName name) => name.Module;
    public static UrnNamespace ToUrnNamespace(this UrnModule module) => module.Ns;
    public static UrnNamespace ToUrnNamespace(this UrnName name) => name.Module.Ns;
    public static UrnModule ToUrn(this UrnName name, out URN urn)
    {
        urn = new URN(name.Module.Ns.Namespace, name.Module.Module, name.Name);
        return name.Module;
    }
    public static UrnModule ToUrn(this UrnModule module, string name, out URN urn)
    {
        if(string.IsNullOrWhiteSpace(name))
        {
            Logger.Error("Name cannot be empty.");
            urn = URN.Empty;
            return module;
        }
        urn = new URN(module.Ns.Namespace, module.Module, name.AsMemory().Trim());
        
        return module;
    }
    
    public static URN ToUrn(this UrnModule module, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Logger.Error("Name cannot be empty.");
            return URN.Empty;
        }
        
        return new URN(module.Ns.Namespace, module.Module, name.AsMemory().Trim());
    }
    public static URN ToUrn(this UrnName name)
    {
        return new URN(name.Module.Ns.Namespace, name.Module.Module, name.Name);
    }

    public static UrnNamespace ToUrnNamespace(this URN urn)
    {
        return new UrnNamespace(urn.Namespace);
    }
    
    public static UrnModule ToUrnModule(this URN urn)
    {
        return new UrnModule(urn.ToUrnNamespace(), urn.Module);
    }

    public static UrnName ToUrnName(this URN urn)
    {
        return new UrnName(urn.ToUrnModule(), urn.Name);
    }
}