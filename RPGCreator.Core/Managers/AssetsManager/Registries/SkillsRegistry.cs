using RPGCreator.Core.Type.Assets.Skills;
using RPGCreator.Core.Type.Internal;
using Serilog;

namespace RPGCreator.Core.Managers.AssetsManager.Registries;

public class SkillsRegistry : IAssetRegistry<ISkillDef>
{
    private readonly Dictionary<Ulid, ISkillDef> _byId = new();
    private readonly Dictionary<URN, Ulid> _byUrn = new();
    
    public event EventHandler<ISkillDef>? AssetRegistered;
    public event EventHandler<ISkillDef>? AssetUnregistered;
    public void Register(ISkillDef asset, bool overwrite = false)
    {
        if (_byId.TryGetValue(asset.Unique, out var value))
        {
            if (overwrite)
            {
                Unregister(value);
            }
            else
            {
                Log.Error("Skill with unique ID {assetUnique} already exists in the registry.", asset.Unique);
                return;
            }
        }

        if (_byUrn.ContainsKey(asset.Urn))
        {
            Log.Error("Skill with URN {assetUrn} already exists in the registry.", asset.Urn);
            return;
        }

        _byId[asset.Unique] = asset;
        _byUrn[asset.Urn] = asset.Unique;
        AssetRegistered?.Invoke(this, asset);
        Log.Information("Registered skill with unique ID {assetUnique} and URN {assetUrn}.", asset.Unique, asset.Urn);
        Log.Debug("Current registry state: {count} skills registered.", _byId.Count);
        Log.Debug("Current registry state: {count} skill URNs registered.", _byUrn.Count);
    }

    public void Unregister(ISkillDef asset)
    {
        if (_byId.Remove(asset.Unique))
        {
            _byUrn.Remove(asset.Urn);
            AssetUnregistered?.Invoke(this, asset);
            Log.Information("Unregistered skill with unique ID {assetUnique} and URN {assetUrn}.", asset.Unique, asset.Urn);
            Log.Debug("Current registry state: {count} skills registered.", _byId.Count);
            Log.Debug("Current registry state: {count} skill URNs registered.", _byUrn.Count);
        }
        else
        {
            Log.Error("Skill with unique ID {assetUnique} not found in the registry.", asset.Unique);
        }
    }

    public ISkillDef? Get(Ulid unique)
    {
        return _byId.GetValueOrDefault(unique);
    }

    public ISkillDef? GetUrn(URN urn)
    {
        if (_byUrn.TryGetValue(urn, out var unique) && _byId.TryGetValue(unique, out var asset))
        {
            return asset;
        }

        return null;
    }

    public bool TryGet(Ulid unique, out ISkillDef? asset)
    {
        if (_byId.TryGetValue(unique, out asset))
        {
            return true;
        }

        asset = null;
        return false;
    }

    public bool TryGetUrn(URN urn, out ISkillDef? asset)
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

    public IEnumerable<ISkillDef> All()
    {
        return _byId.Values;
    }
}