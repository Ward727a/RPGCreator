using System.Numerics;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Serializer;

namespace RPGCreator.SDK.Assets.Definitions.Maps.Layers.EntityLayer;

[SerializingType("EntitySpawner")]
public class EntitySpawner : ILayerElem, IDisposable, ISerializable, IDeserializable
{
    private IEntityDefinition _entityDefinition;

    /// <summary>
    /// Entity being represented visually in the editor.
    /// </summary>
    public IEntityDefinition EntityDefinition
    {
        get => _entityDefinition;
        private set
        {
            _entityDefinition = value;
            if (_entityDefinition.Unique != Ulid.Empty && _entityDefinition.Unique != EntityUnique)
            {
                EntityUnique = _entityDefinition.Unique;
            }
        }
    }

    /// <summary>
    /// Unique identifier for this specific entity instance.
    /// If null, a new unique ID will be generated when the entity is spawned in-game.
    /// </summary>
    public Ulid? EntityUnique;

    /// <summary>
    /// Position of the entity on the map editor grid.
    /// </summary>
    public Vector2 Position { get; set; } = Vector2.One;

    public string PreviewImagePath => EntityDefinition.SpritePath;

    public EntitySpawner(IEntityDefinition entityDefinition, Vector2 position)
    {
        EntityDefinition = entityDefinition;
        Position = position;
    }

    public EntitySpawner()
    {
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }

    public SerializationInfo GetObjectData()
    {
        var info = new SerializationInfo(typeof(EntitySpawner));
        info.AddValue("EntityUnique", EntityUnique?.ToString() ?? string.Empty);
        info.AddValue("PositionX", Position.X);
        info.AddValue("PositionY", Position.Y);
        return info;
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        var referencedIds = new List<Ulid>();
        if (EntityUnique.HasValue)
        {
            referencedIds.Add(EntityUnique.Value);
        }
        return referencedIds;
    }

    public void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue(nameof(EntityUnique), out EntityUnique);
        float posX = 0;
        float posY = 0;
        info.TryGetValue("PositionX", out posX);
        info.TryGetValue("PositionY", out posY);
        Position = new Vector2(posX, posY);

        EngineServices.OnceServiceReady((IAssetsManager assetManager) =>
        {
            if (!EntityUnique.HasValue)
            {
                Logger.Error("EntityUnique is null or empty during deserialization of EntitySpawner.");
                return;
            }
            
            if (assetManager.TryResolveAsset(EntityUnique.Value, out IEntityDefinition? entityDef))
            {
                EntityDefinition = entityDef;
            }
            else
            {
                Logger.Error($"Failed to resolve entity definition with Unique ID: {EntityUnique}");
            }
        });
    }
}