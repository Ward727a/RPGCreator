using System.Reflection;
using RPGCreator.Core.Parser.Graph.NodesMaker;
using RPGCreator.Core.Runtimes.Contents;
using RPGCreator.Core.Type.Assets.Skills;
using RPGCreator.Core.Type.Blueprint;
using RPGCreator.Core.Type.Internal;
using Serilog;

namespace RPGCreator.Core.Managers.AssetsManager.Registries;

public class SkillEffectsRegistry: IAssetRegistry<ISkillEffect>
{
    private readonly Dictionary<Ulid, ISkillEffect> _byId = new();
    private readonly Dictionary<URN, Ulid> _byUrn = new();
    public event EventHandler<ISkillEffect>? AssetRegistered;
    public event EventHandler<ISkillEffect>? AssetUnregistered;
    
    public void Register(ISkillEffect asset, bool overwrite = false)
    {
        if (_byId.TryGetValue(asset.Unique, out var value))
        {
            if (overwrite)
            {
                Unregister(value);
            }
            else
            {
                Log.Error("Skill Effect with unique ID {assetUnique} already exists in the registry.", asset.Unique);
                return;
            }
        }

        if (_byUrn.ContainsKey(asset.Urn))
        {
            Log.Error("Skill Effect with URN {assetUrn} already exists in the registry.", asset.Urn);
            return;
        }

        _byId[asset.Unique] = asset;
        _byUrn[asset.Urn] = asset.Unique;
        AssetRegistered?.Invoke(this, asset);
        Log.Information("Registered Skill Effect with unique ID {assetUnique} and URN {assetUrn}.", asset.Unique, asset.Urn);
        Log.Debug("Current registry state: {count} Skill Effects registered.", _byId.Count);
        Log.Debug("Current registry state: {count} Skill Effect URNs registered.", _byUrn.Count);
    }

    public void Unregister(ISkillEffect asset)
    {
        if (_byId.Remove(asset.Unique))
        {
            _byUrn.Remove(asset.Urn);
            AssetUnregistered?.Invoke(this, asset);
            Log.Information("Unregistered Skill Effect with unique ID {assetUnique} and URN {assetUrn}.", asset.Unique, asset.Urn);
            Log.Debug("Current registry state: {count} Skill Effects registered.", _byId.Count);
            Log.Debug("Current registry state: {count} Skill Effect URNs registered.", _byUrn.Count);
        }
        else
        {
            Log.Error("Skill Effect with unique ID {assetUnique} not found in the registry.", asset.Unique);
        }
    }

    public ISkillEffect? Get(Ulid unique)
    {
        return _byId.GetValueOrDefault(unique);
    }

    public ISkillEffect? GetUrn(URN urn)
    {
        if (_byUrn.TryGetValue(urn, out var unique) && _byId.TryGetValue(unique, out var asset))
        {
            return asset;
        }

        return null;
    }

    public bool TryGet(Ulid unique, out ISkillEffect? asset)
    {
        if (_byId.TryGetValue(unique, out asset))
        {
            return true;
        }

        asset = null;
        return false;
    }

    public bool TryGetUrn(URN urn, out ISkillEffect? asset)
    {
        if (_byUrn.TryGetValue(urn, out var unique) && _byId.TryGetValue(unique, out asset))
        {
            return true;
        }

        asset = null;
        return false;
    }

    public bool Contains(Ulid unique)
    {
        return _byId.ContainsKey(unique);
    }

    public bool ContainsUrn(URN urn)
    {
        return _byUrn.ContainsKey(urn);
    }

    public IEnumerable<ISkillEffect> All()
    {
        return _byId.Values;
    }

    public void ReloadData()
    {
        // Clear existing data
        _byId.Clear();
        _byUrn.Clear();
        Log.Information("Skill Effects registry cleared for data reload.");
        
        // We just want assemblies that are not system or common libraries
        // If the creator of the assembly use one of those names, well, too bad for them.
        // They should not do that anyway
        var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a =>
            !a.FullName.StartsWith("System") &&
            !a.FullName.StartsWith("Avalonia") &&
            !a.FullName.StartsWith("Microsoft") &&
            !a.FullName.StartsWith("Ulid") &&
            !a.FullName.StartsWith("CommunityToolkit") &&
            !a.FullName.StartsWith("Serilog") &&
            !a.FullName.StartsWith("MonoGame"));
        var skillEffectTypes = new List<System.Type>();
        
        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetTypes()
                    .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(ISkillEffect).IsAssignableFrom(t))
                    .Where(t => t.GetCustomAttribute<SkillEffectAttribute>() != null);
                
                skillEffectTypes.AddRange(types);
                Log.Information("SkillEffectsRegistry: Found {Count} skill effect candidates in assembly {AssemblyName}.",
                    types.Count(), assembly.GetName().Name);
            }
            catch (ReflectionTypeLoadException ex)
            {
                Log.Error(ex, "Error loading types from assembly {AssemblyName}", assembly.FullName);
            }
        }
        
        Log.Information("SkillEffectsRegistry: Total {Count} skill effect candidates found across all assemblies.",
            skillEffectTypes.Count);

        foreach (var type in skillEffectTypes)
        {
            try
            {
                var skillEffectObject = Activator.CreateInstance(type);
                if (skillEffectObject is not ISkillEffect skillEffect)
                {
                    Log.Error("SkillEffectsRegistry: Type {TypeName} is not a valid ISkillEffect.", type.FullName);
                    continue;
                }

                Register(skillEffect);
                Log.Information(
                    "SkillEffectsRegistry: Registered skill effect {SkillEffectName} with unique ID {SkillEffectUnique} and URN {SkillEffectUrn}.",
                    type.FullName, skillEffect.Unique, skillEffect.Urn);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error instantiating skill effect of type {TypeName}", type.FullName);
            }
        }


    }
}