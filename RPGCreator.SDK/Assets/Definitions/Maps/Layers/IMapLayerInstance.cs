
using System.Numerics;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.Assets.Definitions.Maps;

public interface IMapLayerInstance<TLayerDefElement, TLayerInstanceElement> : IMapLayerInstance
{
    ILayerRenderer<TLayerDefElement, TLayerInstanceElement>? Renderer { get; }
    
    public Dictionary<Vector2, TLayerInstanceElement> InstancedElements { get; }

    public void Draw(IRenderContext context, ILayerRenderer<TLayerDefElement, TLayerInstanceElement> renderer)
    {
        renderer.Draw(context, this);
    }
    public void Update(TimeSpan gameTime);
}
public interface IMapLayerInstance
{
    Ulid RuntimeUnique { get; }
    TileLayerDefinition Definition { get; }
    bool IsVisible { get; set; }
    bool IsSelected { get; set; }
}