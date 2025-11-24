namespace RPGCreator.Core.Types.Internal;

public interface IHasUniqueId
{
    public Ulid Unique { get; }
    public URN Urn { get; }
}