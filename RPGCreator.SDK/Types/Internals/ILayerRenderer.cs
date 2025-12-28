using RPGCreator.SDK.Assets.Definitions.Maps;

namespace RPGCreator.SDK.Types.Internals;

public interface IRenderContext
{
}

/// <summary>
/// This interface is used for classes that are responsible for rendering layers in the game.<br/>
/// Example: If you have a layer that needs to be rendered inside Avalonia.
/// </summary>
public interface ILayerRenderer<TElementDef, TElementInstance>
{
    /// <summary>
    /// Drawing system for the tile layer.
    /// </summary>
    /// <param name="tileLayer">The layer that will be used.</param>
    void Draw(IRenderContext context, IMapLayerInstance<TElementDef, TElementInstance> tileLayer);
}

public interface IDrawer<T>
{
    void Draw(IRenderContext context, T drawable);
}