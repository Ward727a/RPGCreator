using System.Numerics;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.ECS.Entities;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Services.EngineService;

namespace RPGCreator.SDK.Assets.Definitions.Maps.Layers.EntityLayer;

public class EntitySpawner : ILayerElem, IDisposable, ISerializable, IDeserializable
{
    /// <summary>
    /// Entity being represented visually in the editor.
    /// </summary>
    public IEntityDefinition EntityDefinition
    {
        get;
        private set
        {
            field = value;
            if (field.Unique != Ulid.Empty && field.Unique != EntityUnique)
            {
                EntityUnique = field.Unique;
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
    public Vector2 Offset { get; set; } = Vector2.One;

    public string PreviewImagePath => EntityDefinition.SpritePath;

    public EntitySpawner(IEntityDefinition entityDefinition, Vector2 position)
    {
        EntityDefinition = entityDefinition;
        Offset = position;
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
        info.AddValue("PositionX", Offset.X);
        info.AddValue("PositionY", Offset.Y);
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
        info.TryGetValue("PositionX", out float posX);
        info.TryGetValue("PositionY", out float posY);
        Offset = new Vector2(posX, posY);

        EngineServices.OnceServiceReady((IAssetsManager assetManager) =>
        {
            if (!EntityUnique.HasValue)
            {
                Logger.Error("EntityUnique is null or empty during deserialization of EntitySpawner.");
                return;
            }

            assetManager.Load<IEntityDefinition>(EntityUnique.Value).OnSuccess((entityDef) =>
            {
                EntityDefinition = entityDef;
            }).OnFailure((s) =>
            {
                Logger.Error($"Failed to load entity definition with Unique ID: {EntityUnique} - {s}");
            });
        });
    }
}