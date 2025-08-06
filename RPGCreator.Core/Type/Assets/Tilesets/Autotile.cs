using Microsoft.Xna.Framework;
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.Core.Type.Map;
using Point = RPGCreator.Core.Type.Internal.Point;

namespace RPGCreator.Core.Type.Assets.Tilesets;

public class Autotile : BaseDrawable, ITileable, ISerializable, IDeserializable
{
    public List<AutotileRule> Rules = new(); // List of rules for this autotile
    public List<string> Tags = []; // Tags for this autotile, can be used for filtering or categorization

    public Point SizeInTileset { get; set; }
    public Point PositionInTileset { get; set; }
    public Rectangle UV => new Rectangle(PositionInTileset, SizeInTileset);
    public Tileset Tileset { get; private set; }
    private bool hasCheckedRule = false; // Flag to check if the rules have been checked already, to avoid unnecessary checks
    public AutotileGroup AutotileGroup { get; }

    public Autotile()
    {
    }
    
    public Autotile(Point sizeInTileset, Point positionInTileset, Tileset tileset, AutotileGroup group)
    {
        SizeInTileset = sizeInTileset;
        PositionInTileset = positionInTileset;
        Tileset = tileset;
        AutotileGroup = group;
    }
    public bool IsEqualTo(ITileable other)
    {
        return other.Tileset.Unique == Tileset.Unique &&
               other.PositionInTileset.X == PositionInTileset.X && other.PositionInTileset.Y == PositionInTileset.Y &&
               other.SizeInTileset.X == SizeInTileset.X && other.SizeInTileset.Y == SizeInTileset.Y;
    }
    
    public void UpdateTileset(ITileset newTileset)
    {
        if(newTileset is Tileset tileset)
            Tileset = tileset;
#if DEBUG
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: Attempted to update NTileset with a non-NTileset type: {newTileset.GetType().Name}");
            Console.ResetColor();
        }
#endif
    }

    public ITileable? GetDrawableTile(TileLayer? layer = null, Point? position = null)
    {
        if (layer == null || position == null)
            return null;

        if (RespectRules(layer, position.Value))
            return GetCopy();

        var tile = AutotileGroup.GetTileAt(layer, position.Value);
        return tile?.GetCopy();
    }
    
    public ITileable GetCopy()
    {
        // Create a copy of the autotile with the same UV, Position and Tileset
        return new Autotile(SizeInTileset, PositionInTileset, Tileset, AutotileGroup)
        {
            Rules = new List<AutotileRule>(Rules), // Copy the rules
            Tags = new List<string>(Tags) // Copy the tags
        };
    }

    public bool AddRule(AutotileRule rule)
    {
        if (Rules.Contains(rule))
            return false;
        
        Rules.Add(rule);
        return true;
    }

    public SerializationInfo GetObjectData()
    {
        SerializationInfo info = new SerializationInfo(typeof(Autotile));
        info.AddValue("UV", SizeInTileset);
        info.AddValue("Position", PositionInTileset);
        info.AddValue("Tileset", Tileset.Unique);
        info.AddValue("Rules", Rules);
        info.AddValue("Tags", Tags);
        return info;
    }

    public void SetObjectData(SerializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
        }

        info.TryGetValue("UV", out Point uv, new Point(0, 0), "UV not found or invalid (Set to (0, 0) by default).");
        info.TryGetValue("Position", out Point position, new Point(0, 0), "Position not found or invalid (Set to (0, 0) by default).");
        info.TryGetValue("Tileset", out Ulid tilesetUnique, Ulid.NewUlid(), "Tileset not found or invalid (Set to new Ulid by default).");
        info.TryGetList("Rules", out List<AutotileRule> rules, [], "Rules not found or invalid (Set to empty list by default).");
        info.TryGetList("Tags", out List<string> tags, [], "Tags not found or invalid (Set to empty list by default).");

        SizeInTileset = uv;
        PositionInTileset = position;
        Rules = rules ?? [];
        Tags = tags ?? [];

        void OnEditedProjectOnOnProjectLoaded()
        {
            // When the project is loaded, we need to get the tileset from the project data
            Tileset = EngineCore.Instance.Data.EditedProject.GetAssetsType<Tileset>(BaseAsset.TYPE.TILESETS)
                .FirstOrDefault(t => t.Unique == tilesetUnique) ?? throw new Exception($"Tileset with unique ID {tilesetUnique} not found in the project.");
            
            EngineCore.Instance.Data.EditedProject.OnProjectLoaded -= OnEditedProjectOnOnProjectLoaded; // Unsubscribe from the event to avoid memory leaks
        }

        EngineCore.Instance.Data.EditedProject.OnProjectLoaded += OnEditedProjectOnOnProjectLoaded;
    }

    //TODO: Need to implement some parallel processing for this method, as it can be slow if there are many rules.
    /// <summary>
    /// Checks if the autotile respects the rules defined for it based on the position in the given MapLayer.<br/>
    /// The autotile is considered valid if it matches the rules defined for its position.<br/>
    /// The rules can be either a whitelist or a blacklist.<br/>
    /// A whitelist means that the autotile must match the rules to be valid, while a blacklist means that the autotile must not match the rules to be valid.<br/>
    /// The position is relative to the autotile's position in the MapLayer.<br/>
    /// The autotile is considered valid if it matches all the rules defined for its position.<br/>
    /// If there are no rules defined for the autotile, it is considered valid by default.
    /// </summary>
    /// <remarks>
    /// This method is still in development and may not be fully optimized.<br/><br/>
    /// <b>Notable point:</b> it does not currently support parallel processing, which may lead to performance issues if there are many rules.
    /// </remarks>
    public bool RespectRules(TileLayer? layer, Point position)
    {
        if (layer == null)
            return false;
        if(Rules.Count == 0)
            return true; // If there are no rules, the autotile is always valid

        foreach (var rule in Rules)
        {
            var rulePosition = rule.Side switch
            {
                ERulePos.TOP_LEFT => new Point(position.X - Tileset.TileWidth, position.Y - Tileset.TileHeight),
                ERulePos.TOP => new Point(position.X, position.Y - Tileset.TileHeight),
                ERulePos.TOP_RIGHT => new Point(position.X + Tileset.TileWidth, position.Y - Tileset.TileHeight),
                ERulePos.LEFT => new Point(position.X - Tileset.TileWidth, position.Y),
                ERulePos.RIGHT => new Point(position.X + Tileset.TileWidth, position.Y),
                ERulePos.BOTTOM_LEFT => new Point(position.X - Tileset.TileWidth, position.Y + Tileset.TileHeight),
                ERulePos.BOTTOM => new Point(position.X, position.Y + Tileset.TileHeight),
                ERulePos.BOTTOM_RIGHT => new Point(position.X + Tileset.TileWidth, position.Y + Tileset.TileHeight),
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
            
            if(tileAtRulePosition is Tile)
            {
                // If the rule is a whitelist and the tile at the rule position is not in the autotile's tags, the autotile is invalid
                if (ruleType == ERuleType.WHITELIST)
                {
                    return false; // The autotile does not match the rule
                }

                continue;
            }
                
            if (tileAtRulePosition is Autotile autotile)
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

    protected override void _Draw(SpriteBatchExtend? sb)
    {
        sb.Draw(Tileset.GetTexture(sb.GraphicsDevice), Position, UV, Color.White);
    }

    protected override void _Update(GameTime gameTime)
    {
        
    }
}