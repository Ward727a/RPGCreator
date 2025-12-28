namespace RPGCreator.SDK.Types.Interfaces;

public interface IAssetsPack
{
    public Ulid Id { get; }
    public string Name { get; }
    public string? Description { get; }

    public object LoadAsset(Ulid assetId);

    public IEnumerable<IAssetIndexRecord> SearchIndex(Func<IAssetIndexRecord, bool> predicate);
    public IEnumerable<IAssetIndexRecord> SearchIndexByType(Type type);
    public void AddOrUpdateAsset(object asset, string relativeFolderPath = "");
    public void RemoveAsset(Ulid assetId);
    public void Dispose();
    public void Save();
}