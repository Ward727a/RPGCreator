using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Core.Managers.AssetsManager;
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.Core.Types.Assets.Actors;
using RPGCreator.SDK;
using RPGCreator.SDK.Serializer;
using Serilog;
using Internal_Point = RPGCreator.Core.Types.Internal.Point;
using Point = RPGCreator.Core.Types.Internal.Point;

namespace RPGCreator.Core.Types.Assets.Characters;

/// <summary>
/// This class represents a character actor in the game.<br/>
/// This is a runtime representation of a character asset.
/// </summary>
public class CharacterActor : ISerializable, IDeserializable, IMoveableActor, IInteractableActor
{
    #region Events
    
    public event EventHandler<Ulid>? CharacterDataIdChanged;
    
    public event EventHandler<Internal_Point>? PositionChanged;
    public event Action? Interacted;
    
    #endregion
    
    #region Properties
    
    public Ulid Unique { get; private set; }

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
    private Internal_Point _position = new(0, 0);

    public Internal_Point Position
    {
        get => _position;
        set
        {
            if (_position.IsEqualTo(value)) return;
            _position = value;
            PositionChanged?.Invoke(this, _position);
        }
    }

    #endregion
    
    #region Constructors
    
    public CharacterActor(Ulid characterDataId, Internal_Point position)
    {
        Unique = Ulid.NewUlid();
        CharacterDataId = characterDataId;
        Position = position;
    }

    internal CharacterActor()
    {
        Unique = Ulid.NewUlid();
    }

    #endregion
    
    #region Methods
    public void GoTo(Internal_Point position)
    {
        if(position.IsEqualTo(Position)) return;
        Position = position;
        Log.Debug("Character Actor {actor} moved to position {position}.", Unique, Position);
    }

    public void GoTo(int x, int y)
    {
        GoTo(new Internal_Point(x, y));
    }
    
    public void Interact(IActor FromActor)
    {
        Interacted?.Invoke();
        Log.Debug("Character Actor {actor} interacted with {fromActor}.", Unique, FromActor.Unique);
    }

    public void InteractWith(IInteractableActor ToActor)
    {
        ToActor.Interact(this);
    }
    
    public void Draw(SpriteBatchExtend? sb)
    {
        if (sb is null)
            return;
        
        // Create a fake red texture for demonstration purposes
        
        var texture = new Texture2D(sb.GraphicsDevice, 32, 32);
        Color[] data = new Color[32 * 32];
        for (int i = 0; i < data.Length; ++i) data[i] = Color.Red;
        texture.SetData(data);
        
        sb.Draw(texture, Position, Color.White);
    }

    public void Update(GameTime gameTime)
    {
        // Nothing to update for now
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