using System.Numerics;
using RPGCreator.SDK.Assets.Definitions.Tilesets.Collision;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Collections;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.Assets.Definitions.Tilesets;

public record struct CollisionData(ECollisionGroupingType CollisionGroupingType, ECollisionFlag CollisionFlag);

public static class CollisionPhysicsExtensions
{
    private static readonly (ECollisionFlag Mask, float xOff, float yOff)[] IntersectionRules = 
    {
        (ECollisionFlag.Top | ECollisionFlag.Left, 0, 0),
        (ECollisionFlag.Top | ECollisionFlag.Right, 0.5f, 0),
        (ECollisionFlag.Bottom | ECollisionFlag.Left, 0, 0.5f),
        (ECollisionFlag.Bottom | ECollisionFlag.Right, 0.5f, 0.5f)
    };
    
    public static IEnumerable<Rect> GetCollisionShapes(this CollisionData data, Vector2 tileOffset, float tileSize = 32f)
    {
        float half = tileSize / 2f;

        if(data.CollisionFlag == ECollisionFlag.None)
            yield break;

        bool CanBeIntersect = (data.CollisionFlag & (ECollisionFlag.Top | ECollisionFlag.Bottom)) != 0 && 
                              (data.CollisionFlag & (ECollisionFlag.Left | ECollisionFlag.Right)) != 0;
        
        // If group is intersection AND there is atleast 2 flags set, then we need to check for intersection
        if (data.CollisionGroupingType == ECollisionGroupingType.Intersection && CanBeIntersect)
        {
            foreach (var (mask, xOff, yOff) in IntersectionRules)
            {
                if ((data.CollisionFlag & mask) == mask)
                    yield return new Rect(tileOffset.X + xOff * tileSize, tileOffset.Y + yOff * tileSize, half, half);
            }
            yield break;
        }

        if (data.CollisionFlag.HasFlag(ECollisionFlag.Top)) yield return new(tileOffset.X, tileOffset.Y, tileSize, half);
        if (data.CollisionFlag.HasFlag(ECollisionFlag.Bottom)) yield return new(tileOffset.X, tileOffset.Y + half, tileSize, half);
        if (data.CollisionFlag.HasFlag(ECollisionFlag.Left)) yield return new(tileOffset.X, tileOffset.Y, half, tileSize);
        if (data.CollisionFlag.HasFlag(ECollisionFlag.Right)) yield return new(tileOffset.X + half, tileOffset.Y, half, tileSize);

    }
}


public enum CollisionShapeType : byte
{
    None = 0,
    Rectangle = 1,
    Triangle = 2
}

[SerializingType("BaseTilesetDef")]
public abstract class BaseTilesetDef : BaseAssetDef, ISerializable, IDeserializable, IHasSavePath
{
    
    public event Action? ImageChanged;
    public string SavePath { get; set; } = null!;

    public string PackName { get; set; } = "";
    public virtual IAssetsPack Pack { get; set; } = null!;
    public virtual string ImagePath { get; set; } = null!;
    public virtual int ImageWidth { get; set; }
    public virtual int ImageHeight { get; set; }
    public virtual int TileWidth { get; set; }
    public virtual int TileHeight { get; set; }

    public Dictionary<Vector2, CollisionData> Collisions { get; set; } = new Dictionary<Vector2, CollisionData>();
    public Dictionary<Vector2, List<Rect>> RuntimeCollisionCache { get; private set; } = new();
    
    public void ClearRuntimeCollisionCache() => RuntimeCollisionCache.Clear();

    public void BuildRuntimeCollisionCache()
    {
        RuntimeCollisionCache.Clear();
        foreach (var (tile, data) in Collisions)
        {
            RuntimeCollisionCache[tile] = data.GetCollisionShapes(tile).ToList();
        }
    }

    public RuntimeBag Tags { get; } = new RuntimeBag();
    
    public virtual SerializationInfo GetObjectData()
    {
        var info = new SerializationInfo(GetType());
        info.AddValue("Unique", Unique);
        info.AddValue("PackName", string.IsNullOrWhiteSpace(PackName) ? Pack?.Name ?? string.Empty : PackName);
        info.AddValue("Name", Name);
        info.AddValue("ImagePath", ImagePath);
        info.AddValue("ImageWidth", ImageWidth);
        info.AddValue("ImageHeight", ImageHeight);
        info.AddValue("TileWidth", TileWidth);
        info.AddValue("TileHeight", TileHeight);
        return info;
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        var referencedIds = new List<Ulid>();
        if (Pack != null)
            referencedIds.Add(Pack.Id);
        return referencedIds;
    }

    public virtual void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue("Unique", out Ulid unique, Ulid.Empty);
        Unique = unique;
        info.TryGetValue("PackName", out string packName, string.Empty);
        PackName = packName;
        info.TryGetValue("Name", out string name, string.Empty);
        Name = name;
        info.TryGetValue("ImagePath", out string imagePath, string.Empty);
        ImagePath = imagePath;
        info.TryGetValue("ImageWidth", out int imageWidth, 0);
        ImageWidth = imageWidth;
        info.TryGetValue("ImageHeight", out int imageHeight, 0);
        ImageHeight = imageHeight;
        info.TryGetValue("TileWidth", out int tileWidth, 0);
        TileWidth = tileWidth;
        info.TryGetValue("TileHeight", out int tileHeight, 0);
        TileHeight = tileHeight;
        info.TryGetValue("Collisions", out Dictionary<Vector2, CollisionData> collisions, new Dictionary<Vector2, CollisionData>());
        Collisions = collisions;
        
        //Get the assets pack
        if (EngineServices.AssetsManager.TryGetPack(PackName, out var pack))
        {
            Pack = pack;
        }
        else
        {
            Logger.Warning("Tileset {TilesetName} ({Unique}) references missing pack {PackName}", Name, Unique, PackName);
        }

        BuildRuntimeCollisionCache();
    }
}