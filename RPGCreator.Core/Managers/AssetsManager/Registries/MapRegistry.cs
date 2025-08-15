using RPGCreator.Core.Type.Internal;
using RPGCreator.Core.Type.Map;

namespace RPGCreator.Core.Managers.AssetsManager.Registries;

public class MapRegistry : IAssetRegistry<IMapDef>
{
    private readonly Dictionary<Ulid, IMapDef> _byId = new();
    private readonly Dictionary<URN, Ulid> _byUrn = new();

    public event EventHandler<IMapDef>? AssetRegistered;
    public event EventHandler<IMapDef>? AssetUnregistered;

    public void Register(IMapDef asset)
    {
        if (_byId.ContainsKey(asset.Unique))
        {
            throw new InvalidOperationException($"Asset with unique ID {asset.Unique} already exists in the registry.");
        }

        if (_byUrn.ContainsKey(asset.Urn))
        {
            throw new InvalidOperationException($"Asset with URN {asset.Urn} already exists in the registry.");
        }

        _byId[asset.Unique] = asset;
        _byUrn[asset.Urn] = asset.Unique;
        AssetRegistered?.Invoke(this, asset);
    }

    public void Unregister(IMapDef asset)
    {
        if (!_byId.Remove(asset.Unique))
        {
            throw new InvalidOperationException($"Asset with unique ID {asset.Unique} not found in the registry.");
        }

        _byUrn.Remove(asset.Urn);
        AssetUnregistered?.Invoke(this, asset);
    }

    public IMapDef? Get(Ulid unique)
    {
        if (_byId.TryGetValue(unique, out var asset))
        {
            return asset;
        }

        return null;
    }

    public IMapDef? GetUrn(URN urn)
    {
        if (_byUrn.TryGetValue(urn, out var unique) && _byId.TryGetValue(unique, out var asset))
        {
            return asset;
        }

        return null;
    }

    public bool TryGet(Ulid unique, out IMapDef? asset)
    {
        if (_byId.TryGetValue(unique, out asset))
        {
            return true;
        }

        asset = null;
        return false;
    }

    public bool TryGetUrn(URN urn, out IMapDef? asset)
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

    public IEnumerable<IMapDef> All()
    {
        return _byId.Values;
    }
}