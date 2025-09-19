using RPGCreator.Core.Type.Assets.Characters.Stats;
using RPGCreator.Core.Type.Internal;
using Serilog;

namespace RPGCreator.Core.Managers.AssetsManager.Registries;

public sealed class StatsRegistry : IAssetRegistry<IStatDef>
{
    
    private readonly Dictionary<Ulid, IStatDef> _byId = new();
    private readonly Dictionary<URN, Ulid> _byUrn = new();

    public event EventHandler<IStatDef>? AssetRegistered;
    public event EventHandler<IStatDef>? AssetUnregistered;

    public void Register(IStatDef asset, bool overwrite = false)
    {
        if (_byId.ContainsKey(asset.Unique))
        {
            if (overwrite)
            {
                Unregister(_byId[asset.Unique]);
            }
            else
            {
                Log.Error("Asset with unique ID {assetUnique} already exists in the registry.", asset.Unique);
                return;
            }
        }

        if (_byUrn.ContainsKey(asset.Urn))
        {
            Log.Error("Asset with URN {assetUrn} already exists in the registry.", asset.Urn);
            return;
        }

        _byId[asset.Unique] = asset;
        _byUrn[asset.Urn] = asset.Unique;
        AssetRegistered?.Invoke(this, asset);
        Log.Information("Registered asset with unique ID {assetUnique} and URN {assetUrn}.", asset.Unique, asset.Urn);
        Log.Debug("Current registry state: {count} assets registered.", _byId.Count);
        Log.Debug("Current registry state: {count} URNs registered.", _byUrn.Count);
    }

    public void Unregister(IStatDef asset)
    {
        if (_byId.Remove(asset.Unique))
        {
            _byUrn.Remove(asset.Urn);
            AssetUnregistered?.Invoke(this, asset);
            Log.Information("Unregistered asset with unique ID {assetUnique} and URN {assetUrn}.", asset.Unique, asset.Urn);
            Log.Debug("Current registry state: {count} assets registered.", _byId.Count);
            Log.Debug("Current registry state: {count} URNs registered.", _byUrn.Count);
        }
        else
        {
            Log.Error("Asset with unique ID {assetUnique} not found in the registry.", asset.Unique);
        }
    }

    public IStatDef? Get(Ulid unique)
    {
        if (_byId.TryGetValue(unique, out var asset))
        {
            return asset;
        }

        return null;
    }

    public IStatDef? GetUrn(URN urn)
    {
        if (_byUrn.TryGetValue(urn, out var unique) && _byId.TryGetValue(unique, out var asset))
        {
            return asset;
        }

        return null;
    }

    public bool TryGet(Ulid unique, out IStatDef? asset)
    {
        if (_byId.TryGetValue(unique, out asset))
        {
            return true;
        }

        asset = null;
        return false;
    }

    public bool TryGetUrn(URN urn, out IStatDef? asset)
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

    public IEnumerable<IStatDef> All()
    {
        return _byId.Values;
    }
}