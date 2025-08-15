using Microsoft.Xna.Framework;
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.Core.Type.Assets.Tilesets;
using RPGCreator.Core.Type.Internal.LayerRenderer;
using Point = RPGCreator.Core.Type.Internal.Point;

namespace RPGCreator.Core.Type.Map;

public interface IMapLayerInstance<TLayerDefElement, TLayerInstanceElement>
{
    Ulid RuntimeUnique { get; }
    IMapLayerDef<TLayerDefElement> Definition { get; }
    ILayerRenderer<TLayerDefElement, TLayerInstanceElement>? Renderer { get; }
    bool IsVisible { get; }
    bool IsSelected { get; set; }
    
    public Dictionary<Point, TLayerInstanceElement> InstancedElements { get; }
    
    public void Draw(SpriteBatchExtend sb);
    public void Update(GameTime gameTime);
}