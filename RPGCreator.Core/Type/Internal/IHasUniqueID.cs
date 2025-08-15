namespace RPGCreator.Core.Type.Internal;

public interface IHasUniqueId
{
    public Ulid Unique { get; }
    public URN Urn { get; }
}