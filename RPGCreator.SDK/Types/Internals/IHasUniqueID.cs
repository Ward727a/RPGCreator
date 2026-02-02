namespace RPGCreator.SDK.Types.Internals;

public interface IHasUniqueId
{
    public Ulid Unique { get; }
    public URN Urn { get; }
    
    public void Init(Ulid id);
}