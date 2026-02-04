using System.Drawing;
using System.Numerics;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.Assets.Definitions.Tilesets;

public interface ITileInstance : ICleanable, IResettable<ITileDef>
{
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
    /// Drawing system for the tile instance.
    /// </summary>
    /// <param name="renderContext"></param>
    /// <param name="renderer"></param>
    public void Draw(IRenderContext renderContext, IDrawer<ITileInstance> renderer)
    {
        renderer.Draw(renderContext, this);
    }

    /// <summary>
    /// This should be inherited from BaseDrawable.
    /// </summary>
    /// <param name="gameTime"></param>
    public void Update(TimeSpan deltaTime);
}