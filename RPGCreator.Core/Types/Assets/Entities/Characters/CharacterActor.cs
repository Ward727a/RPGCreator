using Microsoft.Xna.Framework;
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.SDK.Assets.Definitions.Characters;
using RPGCreator.SDK.Serializer;
using Serilog;
using Internal_Point = RPGCreator.Core.Types.Internal.Point;

namespace RPGCreator.Core.Types.Assets.Entities.Characters;

/// <summary>
/// This class represents a character actor in the game.<br/>
/// This is a runtime representation of a character asset.
/// </summary>
public class CharacterActor : Actor, ISerializable, IDeserializable, IInteractableActor
{
    #region Events
    
    public event EventHandler<Ulid>? CharacterDataIdChanged;
    public event Action? Interacted;
    
    #endregion
    
    #region Properties
    
    private Ulid _characterDataId;
    public Ulid CharacterDataId { 
        get => _characterDataId;
        set
        {
            if (_characterDataId == value) return;
            EngineCore.Instance.Managers.Assets.TryResolveAsset<CharacterData>(value, out var result);
            
            if(result is not null)
            {
                _characterDataId = value;
                CharacterData = result;
                CharacterDataIdChanged?.Invoke(this, _characterDataId);
            }
            else
            {
                Log.Error("Character data with ID {id} not found in cache.", value);
            }
        }
    }
    public CharacterData CharacterData { get; internal set; }

    #endregion
    
    #region Constructors
    
    public CharacterActor(Ulid characterDataId, Internal_Point position)
    {
        CharacterDataId = characterDataId;
    }

    internal CharacterActor()
    {
    }

    #endregion
    
    #region Methods
    public void GoTo(Internal_Point position)
    {
        // TODO : Switch to use the TransformComponent.
        // TODO : Add pathfinding.
        Log.Debug("Character Actor {actor} moving from {from} to {to}.", Id, "UNKNOWN", position);
    }

    public void GoTo(int x, int y)
    {
        GoTo(new Internal_Point(x, y));
    }
    
    public void Interact(Actor FromActor)
    {
        Interacted?.Invoke();
        Log.Debug("Character Actor {actor} interacted with {fromActor}.", Id, FromActor.Id);
    }

    public void InteractWith(IInteractableActor ToActor)
    {
        ToActor.Interact(this);
    }
    
    public void Draw(SpriteBatchExtend? sb)
    {
        // Drawing logic for the character actor is from the SpriteComponent.
    }

    public void Update(GameTime gameTime)
    {
        // Update logic for the character actor is from various components.
    }
    #endregion
    
    #region Serialization
    public SerializationInfo GetObjectData()
    {
        throw new NotImplementedException();
    }

    public void SetObjectData(DeserializationInfo info)
    {
        throw new NotImplementedException();
    }
    #endregion
}