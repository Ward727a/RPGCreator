using Microsoft.Xna.Framework;
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.Core.Type.Internal;
using RPGCreator.Core.Type.Map;
using Point = Microsoft.Xna.Framework.Point;

namespace RPGCreator.Core.Type.Assets.Tilesets;

public interface ITileInstance : ICleanable, IResettable<ITileDef>
{
    public Vector2 Position { get; set; } // Position in the map, not in the tileset
    public ITileDef Definition { get; }
    /// <summary>
    /// This method returns a drawable tile based on the current tileable object.<br/>
    /// It's mainly useful for autotiles or tiles that need to be drawn differently based on the context.<br/>
    /// If the tileable object is a simple tile, it will just return a copy of itself.
    /// </summary>
    /// <param name="layer">The map layer where this tile will be drawn</param>
    /// <param name="position">The position where this tile will be drawn</param>
    /// <returns></returns>
    public ITileInstance? GetDrawableTile(TileLayerDefinition? layer = null, Point? position = null);
    public ITileInstance GetCopy(); // Returns a copy of the tileable object.
    /// <summary>
    /// This should be inherited from BaseDrawable.
    /// </summary>
    /// <param name="sb"></param>
    public void Draw(SpriteBatchExtend sb);

    /// <summary>
    /// This should be inherited from BaseDrawable.
    /// </summary>
    /// <param name="gameTime"></param>
    public void Update(GameTime gameTime);
}