
using System.Drawing;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.Assets.Definitions.Maps;

public interface IMapLayerInstance<TLayerDefElement, TLayerInstanceElement>
{
    Ulid RuntimeUnique { get; }
    TileLayerDefinition Definition { get; }
    ILayerRenderer<TLayerDefElement, TLayerInstanceElement>? Renderer { get; }
    bool IsVisible { get; }
    bool IsSelected { get; set; }
    
    public Dictionary<Point, TLayerInstanceElement> InstancedElements { get; }

    public void Draw(IRenderContext context, ILayerRenderer<TLayerDefElement, TLayerInstanceElement> renderer)
    {
        renderer.Draw(context, this);
    }
    public void Update(TimeSpan gameTime);
}