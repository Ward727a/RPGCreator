using Microsoft.Xna.Framework;
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.Core.Type.Map;
using Point = RPGCreator.Core.Type.Internal.Point;

namespace RPGCreator.Core.Type.Assets.Tilesets;

public interface ITileable
{
    public Vector2 Position { get; set; }
    public Point SizeInTileset { get; }
    public Point PositionInTileset { get; } // Position in the tileset grid (row by column)
    public Tileset Tileset { get; } // The tileset this tile belongs to


    public void UpdateTileset(ITileset newTileset);
    
    /// <summary>
    /// This method returns a drawable tile based on the current tileable object.<br/>
    /// It's mainly useful for autotiles or tiles that need to be drawn differently based on the context.<br/>
    /// If the tileable object is a simple tile, it will just return a copy of itself.
    /// </summary>
    /// <param name="layer">The map layer where this tile will be drawn</param>
    /// <param name="position">The position where this tile will be drawn</param>
    /// <returns></returns>
    public ITileable? GetDrawableTile(TileLayer? layer = null, Point? position = null);
    public ITileable GetCopy(); // Returns a copy of the tileable object.
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

    public bool IsEqualTo(ITileable other);
}