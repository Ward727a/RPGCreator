using Microsoft.Xna.Framework;
using RPGCreator.Core.Managers.AssetsManager;
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.Core.Types.Internal;
using RPGCreator.Core.Types.Map;
using Serilog;
using Point = RPGCreator.Core.Types.Internal.Point;

namespace RPGCreator.Core.Types.Assets.Tilesets;

public class AutotileInstance : BaseDrawable, ITileInstance
{

    private AssetsManager assets => EngineCore.Instance.Managers.Assets;
    
    public List<AutotileRule> Rules = new(); // List of rules for this autotile
    public List<string> Tags = []; // Tags for this autotile, can be used for filtering or categorization

    private readonly AutotileDef _def;
    public ITilesetDef TilesetDef { get; }

    private ITilesetInstance _tilesetInstance;
    public ITilesetInstance TilesetInstance
    {
        get => _tilesetInstance;
        private set
        {
            _tilesetInstance = value;
            _tilesetUnique = value.Definition.Unique; // Store the unique ID of the tileset for serialization
        }
    }

    private Ulid _tilesetUnique;
    public AutotileGroupInstance AutotileGroupInstance { get; internal set; }

    public AutotileInstance()
    {
    }

    public AutotileInstance(AutotileDef tileDef)
    {
        Definition = tileDef;
        TilesetInstance = EngineCore.Instance.Managers.GameFactory.CreateInstance<AutoTilesetInstance>(tileDef);
    }
    
    public AutotileInstance(AutotileDef def, AutotileGroupInstance groupInstance)
    {
        Definition = def;
        if(EngineCore.Instance.Managers.Assets.TryResolveAsset<TilesetDef>(groupInstance.Definition.TilesetUnique, out var tilesetDef))
            TilesetInstance = EngineCore.Instance.Managers.GameFactory.CreateInstance<TilesetInstance>(tilesetDef);
        AutotileGroupInstance = groupInstance;
    }

    public void UpdateTileset(ITilesetDef newTilesetDefinition)
    {
        throw new NotImplementedException();
    }

    public bool IsEqualTo(ITileDef other)
    {
        return other.TilesetDef.Unique == TilesetInstance.Definition.Unique &&
               other.PositionInTileset.IsEqualTo(Definition.PositionInTileset) &&
               other.SizeInTileset.IsEqualTo(Definition.SizeInTileset);
    }
    
    public void UpdateTileset(ITilesetInstance newTilesetInstance)
    {
        if(newTilesetInstance is TilesetInstance tileset)
            TilesetInstance = tileset;
#if DEBUG
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: Attempted to update Tileset with a non-Tileset type: {newTilesetInstance.GetType().Name}");
            Console.ResetColor();
        }
#endif
    }

    public ITileDef Definition { get; }

    public ITileInstance? GetDrawableTile(TileLayerDefinition? layer = null, Microsoft.Xna.Framework.Point? position = null)
    {
        throw new NotImplementedException();
    }

    public ITileInstance GetCopy()
    {
        return assets.TileFactory.Create(_def);
    }

    public bool AddRule(AutotileRule rule)
    {
        if (Rules.Contains(rule))
            return false;
        
        Rules.Add(rule);
        return true;
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

    protected override void _Draw(SpriteBatchExtend? sb)
    {
        sb.Draw(TilesetInstance.Definition.GetTexture(sb.GraphicsDevice), Position, _def.UV, Color.White);
    }

    protected override void _Update(GameTime gameTime)
    {
        
    }

    public void Clean()
    {
        throw new NotImplementedException();
    }

    public void ResetFrom(ITileDef def, params object[] parameters)
    {
        throw new NotImplementedException();
    }
}