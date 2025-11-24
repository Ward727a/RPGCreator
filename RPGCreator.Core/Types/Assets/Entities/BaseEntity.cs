using RPGCreator.Core.ModuleSDK.Definition;
using RPGCreator.Core.ModuleSDK.ECS;
using RPGCreator.Core.Types.Internal;

namespace RPGCreator.Core.Types.Assets.Actors;

public abstract class BaseEntity : IHasUniqueId
{
    public Ulid Unique { get; } = Ulid.NewUlid();
    public URN Urn => new("entity", Unique.ToString());
    
    public CustomData Properties { get; } = new CustomData();
    public List<BaseEntityFeature> Features { get; } = new List<BaseEntityFeature>();
    
}