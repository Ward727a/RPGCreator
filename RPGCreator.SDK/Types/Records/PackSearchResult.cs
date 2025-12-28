namespace RPGCreator.SDK.Types.Records;

public record PackSearchResult(Ulid AssetId, Ulid PackId, string TypeName, string Path)
{
    /// <summary>
    /// The unique identifier of the asset.
    /// </summary>
    public Ulid AssetId = AssetId;
    /// <summary>
    /// The type name of the asset.
    /// </summary>
    public string TypeName = TypeName;
    /// <summary>
    /// The relative path of the asset within the pack.
    /// </summary>
    public string Path = Path;
    /// <summary>
    /// The unique identifier of the pack containing the asset.
    /// </summary>
    public Ulid PackId = PackId;
}