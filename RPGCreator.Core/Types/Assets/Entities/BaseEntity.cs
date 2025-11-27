using RPGCreator.Core.ModuleSDK.Definition;
using RPGCreator.Core.ModuleSDK.ECS;
using RPGCreator.Core.Types.Internal;

namespace RPGCreator.Core.Types.Assets.Actors;

public abstract class BaseEntity : IHasUniqueId
{
    
    public string SpritePath { get; set; } = string.Empty;
    
    public Ulid Unique { get; protected set; } = Ulid.NewUlid();
    public URN Urn => new("entity", Unique.ToString());
    
    public CustomData Properties { get; private set; } = new CustomData();
    public List<BaseEntityFeature> Features { get; private set; } = new List<BaseEntityFeature>();
    public List<string> Tags { get; private set; } = new List<string>();
    
}