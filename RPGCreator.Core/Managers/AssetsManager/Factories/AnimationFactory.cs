using RPGCreator.Core.Types.Assets.Animations;

namespace RPGCreator.Core.Managers.AssetsManager.Factories;

public class AnimationFactory : GenericPooledFactory<AnimationInstance, AnimationDef>
{
    public override AnimationInstance Create(AnimationDef def)
    {
        // First we retrieve the spritesheet associated with the animation.
        if (EngineCore.Instance.Managers.Assets.TryResolveAsset(def.SpriteSheetId, out SpritesheetDef? spritesheetDef))
        {
            AnimationInstance instance;
            
            if (Pool.Count > 0)
            {
                // Rent an instance from the pool.
                instance = Pool.Rent();
            }
            else
            {
                // Create a new instance if the pool is empty.
                instance = Constructor(def);
            }

            instance.ResetFrom(def, spritesheetDef);
            
            return instance;
        }
        throw new InvalidOperationException($"Failed to resolve spritesheet with ID {def.SpriteSheetId} for animation {def.Urn}");
    }
}