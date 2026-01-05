using RPGCreator.Core.ModuleSDK.Definition;
using RPGCreator.Core.Types.Internal;
using RPGCreator.SDK.ECS.Features;
using RPGCreator.SDK.Modules.Definition;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Interfaces;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.ECS;

/// <summary>
/// Represents the data for a base entity in the ECS system.
/// This should not contain any logic, only data.
///
/// This should be used to create an IEntity instance via an EntityFactory.
/// </summary>
public interface IEntityDefinition : IHasUniqueId
{
    
    public string SpritePath { get; set; }
    
    public Ulid Unique { get; protected set; }
    public URN Urn => new("entity", Unique.ToString());
    
    public CustomData Properties { get; }
    public List<BaseEntityFeature> Features { get; }
    public List<string> Tags { get; }
    
}