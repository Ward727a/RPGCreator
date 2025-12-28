using RPGCreator.Core.ModuleSDK.Definition;
using RPGCreator.Core.Types.Internal;
using RPGCreator.SDK.ECS.Features;
using RPGCreator.SDK.Modules.Definition;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Interfaces;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.ECS;

public abstract class BaseEntity : IHasUniqueId
{
    
    public string SpritePath { get; set; } = string.Empty;
    
    public Ulid Unique { get; protected set; } = Ulid.NewUlid();
    public URN Urn => new("entity", Unique.ToString());
    
    public CustomData Properties { get; private set; } = new CustomData();
    public List<BaseEntityFeature> Features { get; private set; } = new List<BaseEntityFeature>();
    public List<string> Tags { get; private set; } = new List<string>();
    
}