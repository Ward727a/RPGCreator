using RPGCreator.SDK.Assets.Definitions;
using RPGCreator.SDK.Assets.Definitions.Characters;
using RPGCreator.SDK.Modules.Definition;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.ECS;

/// <summary>
/// Represents the data for a base entity in the ECS system.
/// This should not contain any logic, only data.
///
/// This should be used to create an IEntity instance via an EntityFactory.
/// </summary>
public interface IEntityDefinition : IAssetDef
{
    public string Name { get; set; }
    
    public string SpritePath { get; }
    
    public Ulid Unique { get; protected set; }
    public URN Urn => new("entity", Unique.ToString());
    
    public CustomData Properties { get; }
    public List<EntityFeatureData> Features { get; }
    public Dictionary<string, DirectionalAnimationSet> AnimationsMapping { get; }
    public List<string> Tags { get; }
    
}