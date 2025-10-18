using RPGCreator.Core.Type.Assets.Characters;
using RPGCreator.Core.Type.Internal;

namespace RPGCreator.Core.Managers.AssetsManager.Registries;

public class CharacterRegistry : IAssetRegistry<CharacterData>
{
    private readonly Dictionary<Ulid, CharacterData> _byId = new();
    private readonly Dictionary<URN, Ulid> _byUrn = new();
    
    public event EventHandler<CharacterData>? AssetRegistered;
    public event EventHandler<CharacterData>? AssetUnregistered;
    public void Register(CharacterData asset, bool overwrite = false)
    {
        if (_byId.ContainsKey(asset.Unique))
        {
            throw new IdAlreadyInRegistry("Character", asset.Unique.ToString());
        }

        if (_byUrn.ContainsKey(asset.Urn))
        {
            throw new UrnAlreadyInRegistry("Character", asset.Urn.ToString());
        }

        _byId[asset.Unique] = asset;
        _byUrn[asset.Urn] = asset.Unique;
        AssetRegistered?.Invoke(this, asset);
    }

    public void Unregister(CharacterData asset)
    {
        if (!_byId.Remove(asset.Unique))
        {
            throw new IdNotFoundInRegistry("Character", asset.Unique.ToString());
        }

        _byUrn.Remove(asset.Urn);
        AssetUnregistered?.Invoke(this, asset);
    }

    public CharacterData? Get(Ulid unique)
    {
        if (_byId.TryGetValue(unique, out var asset))
        {
            return asset;
        }

        return null;
    }

    public CharacterData? GetUrn(URN urn)
    {
        if (_byUrn.TryGetValue(urn, out var unique) && _byId.TryGetValue(unique, out var asset))
        {
            return asset;
        }

        return null;
    }

    public bool TryGet(Ulid unique, out CharacterData? asset)
    {
        if (_byId.TryGetValue(unique, out asset))
        {
            return true;
        }

        asset = null;
        return false;
    }

    public bool TryGetUrn(URN urn, out CharacterData? asset)
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

    public IEnumerable<CharacterData> All()
    {
        return _byId.Values;
    }
}