using Microsoft.Xna.Framework;
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.Core.Types.Assets.Tilesets;
using RPGCreator.Core.Types.Internal.LayerRenderer;
using Point = RPGCreator.Core.Types.Internal.Point;

namespace RPGCreator.Core.Types.Map;

public interface IMapLayerInstance<TLayerDefElement, TLayerInstanceElement>
{
    Ulid RuntimeUnique { get; }
    TileLayerDefinition Definition { get; }
    ILayerRenderer<TLayerDefElement, TLayerInstanceElement>? Renderer { get; }
    bool IsVisible { get; }
    bool IsSelected { get; set; }
    
    public Dictionary<Point, TLayerInstanceElement> InstancedElements { get; }
    
    public void Draw(SpriteBatchExtend sb);
    public void Update(GameTime gameTime);
}