using Microsoft.Xna.Framework;
using RPGCreator.Core.Types.Internal;
using RPGCreator.Core.Types.Map;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;
using Internal_Point = RPGCreator.Core.Types.Internal.Point;
using Point = RPGCreator.Core.Types.Internal.Point;

namespace RPGCreator.Core.Types.Assets.Tilesets;

public class AutotileDef : ITileDef, ISerializable, IDeserializable
{
    public Point Position { get; set; }
    public Vector2 DefaultPosition { get; set; }
    public Internal_Point SizeInTileset { get; set; }
    public Internal_Point PositionInTileset { get; set; }
    public Rectangle UV => new (new(PositionInTileset.X * TilesetDef.TileWidth, PositionInTileset.Y * TilesetDef.TileHeight), new(TilesetDef.TileWidth));
    public TileFlip Flip { get; set; } = TileFlip.None;
    public ITilesetDef TilesetDef { get; }
    public AutotileGroupDef? GroupDef { get; set; }
    public void UpdateTileset(ITilesetDef newTilesetDefinition)
    {
        throw new NotImplementedException();
    }

    public bool IsEqualTo(ITileDef other)
    {
        if (other == null) return false;


        return TilesetUnique == other.TilesetDef.Unique &&
               SizeInTileset == other.SizeInTileset &&
               PositionInTileset == other.PositionInTileset;
    }

    public bool RespectRules(TileLayerDefinition? layer, Internal_Point position)
    {
        
        if (layer == null)
            return false;
        if(Rules.Count == 0)
            return true; // If there are no rules, the autotile is always valid

        foreach (var rule in Rules)
        {
            var rulePosition = rule.Side switch
            {
                ERulePos.TOP_LEFT => new Internal_Point(position.X - TilesetDef.TileWidth, position.Y - TilesetDef.TileHeight),
                ERulePos.TOP => new Internal_Point(position.X, position.Y - TilesetDef.TileHeight),
                ERulePos.TOP_RIGHT => new Internal_Point(position.X + TilesetDef.TileWidth, position.Y - TilesetDef.TileHeight),
                ERulePos.LEFT => new Internal_Point(position.X - TilesetDef.TileWidth, position.Y),
                ERulePos.RIGHT => new Internal_Point(position.X + TilesetDef.TileWidth, position.Y),
                ERulePos.BOTTOM_LEFT => new Internal_Point(position.X - TilesetDef.TileWidth, position.Y + TilesetDef.TileHeight),
                ERulePos.BOTTOM => new Internal_Point(position.X, position.Y + TilesetDef.TileHeight),
                ERulePos.BOTTOM_RIGHT => new Internal_Point(position.X + TilesetDef.TileWidth, position.Y + TilesetDef.TileHeight),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            var ruleType = rule.Type;

            if (!layer.TryGetElement(new(rulePosition.X, rulePosition.Y), out var tileAtRulePosition))
            {
                if (ruleType == ERuleType.WHITELIST)
                {
                    return false; // If the rule is a whitelist and the tile at the rule position is null, the autotile is invalid
                }
                continue;
            }
            
            if(tileAtRulePosition is TileInstance)
            {
                // If the rule is a whitelist and the tile at the rule position is not in the autotile's tags, the autotile is invalid
                if (ruleType == ERuleType.WHITELIST)
                {
                    return false; // The autotile does not match the rule
                }

                continue;
            }
                
            if (tileAtRulePosition is AutotileInstance autotile)
            {
                if(autotile.Tags.Count == 0)
                {
                    if (ruleType == ERuleType.WHITELIST)
                    {
                        return false; // If the autotile has no tags, it doesn't match the rule
                    }
                    continue;
                }

                switch (ruleType)
                {
                    case ERuleType.WHITELIST:
                    {
                        // If the rule has a tag that is not in the auto-tile's tags, the autotile is invalid
                        if (rule.Tags.Any(t => !autotile.Tags.Contains(t)))
                        {
                            return false; // If the auto-tile has any tag that does not match the rule, it is invalid
                        }

                        continue;
                    }
                    case ERuleType.BLACKLIST:
                    {
                        if (autotile.Tags.Any(t => rule.Tags.Contains(t)))
                        {
                            return false; // If the auto-tile has any tag that matches the rule, it is invalid
                        }

                        continue;
                    }
                    default:
                        return false; // If the rule type is not recognized, we consider the auto-tile invalid
                }
            }
        }

        return true;
    }
    
    public void AddRule(AutotileRule rule)
    {
        if (rule == null)
        {
            throw new ArgumentNullException(nameof(rule), "Rule cannot be null.");
        }
        
        if (!Rules.Contains(rule))
        {
            Rules.Add(rule);
        }
    }

    private Ulid _tilesetUnique;
    public Ulid TilesetUnique => _tilesetUnique;
    public List<AutotileRule> Rules { get; }
    public List<string> Tags { get; }
    
    public AutotileDef()
    {
    }
    public AutotileDef(Ulid tilesetUnique, Internal_Point sizeInTileset, Internal_Point positionInTileset)
    {
        _tilesetUnique = tilesetUnique;
        SizeInTileset = sizeInTileset;
        PositionInTileset = positionInTileset;
        Rules = new List<AutotileRule>();
        Tags = new List<string>();
    }

    public AutotileDef(Ulid tilesetUnique, Internal_Point sizeInTileset, Internal_Point positionInTileset, AutotileGroupDef groupDef)
    {
        _tilesetUnique = tilesetUnique;
        SizeInTileset = sizeInTileset;
        PositionInTileset = positionInTileset;
        Rules = new List<AutotileRule>();
        Tags = new List<string>();
        GroupDef = groupDef;
    }
    
    public SerializationInfo GetObjectData()
    {
        return new SerializationInfo(typeof(AutotileDef))
            .AddValue(nameof(SizeInTileset), SizeInTileset)
            .AddValue(nameof(PositionInTileset), PositionInTileset)
            .AddValue(nameof(TilesetUnique), _tilesetUnique)
            .AddValue(nameof(Rules), Rules)
            .AddValue(nameof(Tags), Tags);
    }

    public void SetObjectData(DeserializationInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);

        info.TryGetValue(nameof(SizeInTileset), out var sizeInTileset, Internal_Point.Empty);
        SizeInTileset = sizeInTileset;
        info.TryGetValue(nameof(PositionInTileset), out var positionInTileset, Internal_Point.Empty);
        PositionInTileset = positionInTileset;
        info.TryGetValue(nameof(TilesetUnique), out var tilesetUnique, Ulid.Empty);
        _tilesetUnique = tilesetUnique;
        info.TryGetValue(nameof(Rules), out var rules, new List<AutotileRule>());
        Rules.Clear();
        Rules.AddRange(rules);
        info.TryGetValue(nameof(Tags), out var tags, new List<string>());
        Tags.Clear();
        Tags.AddRange(tags);
    }

    public Ulid Unique { get; }
    public URN Urn { get; }
    public bool IsDirty { get; set; }
    public bool IsTransient { get; set; } = false;
}