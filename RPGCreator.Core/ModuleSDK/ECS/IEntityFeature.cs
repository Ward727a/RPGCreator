using Microsoft.Xna.Framework;
using RPGCreator.Core.ModuleSDK.Definition;
using RPGCreator.Core.Runtimes;

namespace RPGCreator.Core.ModuleSDK.ECS;

public interface IEntityFeature 
{
    /// <summary>
    /// This feature's configuration.<br/>
    /// This data <b>WILL</b> be serialized and saved with the project.<br/>
    /// And as such, it only allows serializable data types that are stored as strings, and converted back when retrieved.
    /// </summary>
    CustomData Configuration { get; }

    /// <summary>
    /// When this feature is initialized (created) on an entity.<br/>
    /// </summary>
    /// <param name="entity">The entity on which this feature is being initialized.</param>
    void OnInitialize(IEntity entity);

    /// <summary>
    /// When the entity is being updated.<br/>
    /// Do not confuse with drawing - This is for logic updates only.
    /// </summary>
    /// <param name="entity">The entity being updated.</param>
    /// <param name="gameTime">The game time.</param>
    void OnUpdate(IEntity entity, GameTime gameTime)
    {
    }

    /// <summary>
    /// When the entity is being drawn.<br/>
    /// Do not confuse with updating - This is for drawing only.
    /// </summary>
    /// <param name="entity">The entity being drawn.</param>
    /// <param name="gameTime">The game time.</param>
    void OnDraw(IEntity entity, GameTime gameTime)
    {
    }

    /// <summary>
    /// When this feature is being destroyed (removed) from an entity (e.g. when the entity is deleted).<br/>
    /// This is the last chance to clean up any resources or references related to this feature on the entity.
    /// </summary>
    /// <param name="entity"></param>
    void OnDestroy(IEntity entity);
}