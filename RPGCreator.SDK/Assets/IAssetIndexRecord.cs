namespace RPGCreator.SDK.Assets;

public interface IAssetIndexRecord
{
    public Ulid Id { get; }
    public string RelativePath { get; }
    public string TypeName { get; }
    public DateTime LastIndexed { get; }
}