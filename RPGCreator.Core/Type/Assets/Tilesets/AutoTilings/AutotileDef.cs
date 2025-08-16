using Microsoft.Xna.Framework;
using RPGCreator.Core.Type.Map;
using Point = RPGCreator.Core.Type.Internal.Point;

namespace RPGCreator.Core.Type.Assets.Tilesets;

public class AutotileDef : ITileDef, ISerializable, IDeserializable
{
    public Vector2 DefaultPosition { get; set; }
    public Point SizeInTileset { get; set; }
    public Point PositionInTileset { get; set; }
    public Rectangle UV => new (new(PositionInTileset.X * TilesetDef.TileWidth, PositionInTileset.Y * TilesetDef.TileHeight), new(TilesetDef.TileWidth));
    public ITilesetDef TilesetDef { get; }
    public AutotileGroupDef? GroupDef { get; set; }
    public void UpdateTileset(ITilesetDef newTilesetDefinition)
    {
        throw new NotImplementedException();
    }

    public bool IsEqualTo(ITileDef other)
    {
        throw new NotImplementedException();
    }

    public bool RespectRules(TileLayerDefinition? layer, Point position)
    {
        
        if (layer == null)
            return false;
        if(Rules.Count == 0)
            return true; // If there are no rules, the autotile is always valid

        foreach (var rule in Rules)
        {
            var rulePosition = rule.Side switch
            {
                ERulePos.TOP_LEFT => new Point(position.X - TilesetDef.TileWidth, position.Y - TilesetDef.TileHeight),
                ERulePos.TOP => new Point(position.X, position.Y - TilesetDef.TileHeight),
                ERulePos.TOP_RIGHT => new Point(position.X + TilesetDef.TileWidth, position.Y - TilesetDef.TileHeight),
                ERulePos.LEFT => new Point(position.X - TilesetDef.TileWidth, position.Y),
                ERulePos.RIGHT => new Point(position.X + TilesetDef.TileWidth, position.Y),
                ERulePos.BOTTOM_LEFT => new Point(position.X - TilesetDef.TileWidth, position.Y + TilesetDef.TileHeight),
                ERulePos.BOTTOM => new Point(position.X, position.Y + TilesetDef.TileHeight),
                ERulePos.BOTTOM_RIGHT => new Point(position.X + TilesetDef.TileWidth, position.Y + TilesetDef.TileHeight),
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
    public AutotileDef(Ulid tilesetUnique, Point sizeInTileset, Point positionInTileset)
    {
        _tilesetUnique = tilesetUnique;
        SizeInTileset = sizeInTileset;
        PositionInTileset = positionInTileset;
        Rules = new List<AutotileRule>();
        Tags = new List<string>();
    }

    public AutotileDef(Ulid tilesetUnique, Point sizeInTileset, Point positionInTileset, AutotileGroupDef groupDef)
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

        info.TryGetValue(nameof(SizeInTileset), out var sizeInTileset, Point.Empty, $"{nameof(AutotileDef)}.{nameof(SizeInTileset)} not found, using default value Point.Empty.");
        SizeInTileset = sizeInTileset;
        info.TryGetValue(nameof(PositionInTileset), out var positionInTileset, Point.Empty, $"{nameof(AutotileDef)}.{nameof(PositionInTileset)} not found, using default value Point.Empty.");
        PositionInTileset = positionInTileset;
        info.TryGetValue(nameof(TilesetUnique), out var tilesetUnique, Ulid.Empty, $"{nameof(AutotileDef)}.{nameof(TilesetUnique)} not found, using default value Ulid.Empty.");
        _tilesetUnique = tilesetUnique;
        info.TryGetValue(nameof(Rules), out var rules, new List<AutotileRule>(), $"{nameof(AutotileDef)}.{nameof(Rules)} not found, using default value empty list.");
        Rules.Clear();
        Rules.AddRange(rules);
        info.TryGetValue(nameof(Tags), out var tags, new List<string>(), $"{nameof(AutotileDef)}.{nameof(Tags)} not found, using default value empty list.");
        Tags.Clear();
        Tags.AddRange(tags);
    }
}