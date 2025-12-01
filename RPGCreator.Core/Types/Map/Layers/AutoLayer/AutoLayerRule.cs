using Avalonia;
using RPGCreator.Core.Serializer;
using RPGCreator.Core.Types.Assets.Tilesets;

namespace RPGCreator.Core.Types.Map.AutoLayer;

public enum PatternCondition
{
    DontCare,
    MustBe,
    MustNotBe
}

public struct PatternConstraint()
{
    public PatternCondition Condition { get; set; }
    
    public int TargetValue { get; set; }

    /// <summary>
    /// If true, the TargetValue is relative to the center tile; otherwise, it's an absolute value.
    /// </summary>
    public bool IsRelative { get; set; } = true;
    
    public static PatternConstraint Ignore => new(){ Condition = PatternCondition.DontCare };
    public static PatternConstraint MustBe(int value) => new(){Condition =  PatternCondition.MustBe, TargetValue = value, IsRelative = false};
    public static PatternConstraint MustBeSame() => new(){Condition =  PatternCondition.MustBe, TargetValue =  0, IsRelative = true};
    public static PatternConstraint MustNotBe(int value) => new(){Condition =  PatternCondition.MustNotBe, TargetValue = value, IsRelative = false};
    public static PatternConstraint MustNotBeSame() => new(){Condition =  PatternCondition.MustNotBe, TargetValue =  0, IsRelative = true};
}

public record struct TileData()
{
    public Ulid UniqueId = Ulid.NewUlid();
    public Point TileSize;
    public Point TilePosition;
    public Ulid TilesetId;
    
    public static TileData FromTileDef(ITileDef tileDef)
    {
        return new TileData
        {
            TileSize = new Point(tileDef.SizeInTileset.X, tileDef.SizeInTileset.Y),
            TilePosition = new Point(tileDef.PositionInTileset.X, tileDef.PositionInTileset.Y),
            TilesetId = tileDef.TilesetDef.Unique,
        };
    }
}

public class AutoLayerRule : IDeserializable, ISerializable
{
    public int TargetIntGridValue { get; set; }
    public PatternConstraint[] Pattern { get; set; } = new PatternConstraint[9];
    
    public List<TileData> OutputTiles { get; set; } = new List<TileData>();
    
    public float Chance { get; set; } = 1.0f;
    
    public bool FlipX { get; set; } = false;
    public bool FlipY { get; set; } = false;
    
    public bool RandomOffset { get; set; } = false;

    public float MinXOffset { get; set; } = 0.0f;
    public float MinYOffset { get; set; } = 0.0f;
    public float MaxXOffset { get; set; } = 0.0f;
    public float MaxYOffset { get; set; } = 0.0f;
    
    public void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue(nameof(TargetIntGridValue), out var targetIntGridValue, 0);
        info.TryGetValue(nameof(Pattern), out var pattern, new PatternConstraint[9]);
        info.TryGetValue(nameof(OutputTiles), out var outputTtileIds, new List<TileData>());
        info.TryGetValue(nameof(Chance), out var chance, 1.0f);
        info.TryGetValue(nameof(FlipX), out var flipX, false);
        info.TryGetValue(nameof(FlipY), out var flipY, false);
        info.TryGetValue(nameof(RandomOffset), out var randomOffset, false);
        info.TryGetValue(nameof(MinXOffset), out var minXOffset, 0.0f);
        info.TryGetValue(nameof(MinYOffset), out var minYOffset, 0.0f);
        info.TryGetValue(nameof(MaxXOffset), out var maxXOffset, 0.0f);
        info.TryGetValue(nameof(MaxYOffset), out var maxYOffset, 0.0f);
        
        TargetIntGridValue = targetIntGridValue;
        Pattern = pattern;
        OutputTiles = outputTtileIds;
        Chance = chance;
        FlipX = flipX;
        FlipY = flipY;
        RandomOffset = randomOffset;
        MinXOffset = minXOffset;
        MinYOffset = minYOffset;
        MaxXOffset = maxXOffset;
        MaxYOffset = maxYOffset;
    }

    public SerializationInfo GetObjectData()
    {
        return new SerializationInfo(typeof(AutoLayerRule))
            .AddValue(nameof(TargetIntGridValue), TargetIntGridValue)
            .AddValue(nameof(Pattern), Pattern)
            .AddValue(nameof(OutputTiles), OutputTiles)
            .AddValue(nameof(Chance), Chance)
            .AddValue(nameof(FlipX), FlipX)
            .AddValue(nameof(FlipY), FlipY)
            .AddValue(nameof(RandomOffset), RandomOffset)
            .AddValue(nameof(MinXOffset), MinXOffset)
            .AddValue(nameof(MinYOffset), MinYOffset)
            .AddValue(nameof(MaxXOffset), MaxXOffset)
            .AddValue(nameof(MaxYOffset), MaxYOffset);
    }
}