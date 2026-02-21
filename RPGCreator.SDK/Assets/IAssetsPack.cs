using LiteDB;
using RPGCreator.SDK.Modules;

namespace RPGCreator.SDK.Assets;

public interface IAssetsPack
{
    public Ulid Id { get; }
    public string Name { get; }
    public string? Description { get; }


    public List<Ulid> GetWhoPointsToAsset(Ulid assetId);
    public object LoadAssetDirect(string relativePath);
    public object LoadAsset(Ulid assetId);
    public IEnumerable<IAssetIndexRecord> SearchIndex(Func<IAssetIndexRecord, bool> predicate);
    public IEnumerable<IAssetIndexRecord> SearchIndexByType(Type type);
    public IEnumerable<T> LoadAssetsByType<T>();
    public void AddOrUpdateAsset(object asset, string relativeFolderPath = "");
    public void RemoveAsset(Ulid assetId);
    public void Dispose();
    public void Save();
}