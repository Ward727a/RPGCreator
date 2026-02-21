using RPGCreator.SDK.Editor;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Assets.Definitions.Maps.Layers;

public enum RenderingMode
{
    /// <summary>
    /// Always render below entities, regardless of their Y position. This is the default rendering mode for background layers.<br/>
    /// </summary>
    StaticUnder,
    /// <summary>
    /// Always render above entities, regardless of their Y position. This is the default rendering mode for foreground layers.<br/>
    /// </summary>
    StaticOver,
    /// <summary>
    /// Y-Sorted rendering mode. Entities will be rendered between the layers based on their Y position. This is the default rendering mode for non-foreground layers.<br/>
    /// </summary>
    Dynamic
}

public abstract class BaseLayerDef : BaseAssetDef, ISerializable, IDeserializable
{
    public abstract IPaintTarget? GetPaintTarget();
    public abstract bool CanPaintObject(object? objectToPaint);
    
    /// <summary>
    /// Define whether this layer is a foreground layer or not.<br/>
    /// A foreground layer is ALWAYS rendered above the entities, and a non-foreground layer will be rendered based on the Y position of the entities.<br/>
    /// Entities will be rendered above non-foreground layers when they are below them, and below non-foreground layers when they are above them.<br/>
    /// Note: EntityLayerDefinition will NEVER be a foreground layer, and will always be rendered in Dynamic mode. This is because the entity layer is the reference point for rendering order of entities and layers.
    /// </summary>
    public bool IsForeground { get; set; } = false;
    
    /// <summary>
    /// The rendering mode of the layer. This defines how the layer is rendered in relation to the entities.<br/>
    /// This will be pre-calculated based on the <see cref="IsForeground"/> property, and will be set to <see cref="RenderingMode.StaticOver"/> if the layer is a foreground layer,<br/>
    /// If it's not, then it will either be <see cref="RenderingMode.StaticUnder"/> or <see cref="RenderingMode.Dynamic"/> based on<br/>
    /// the layer's position in the layer stack (if it's above or below the entity layer).<br/>
    /// Above entity layer AND not IsForeground => Dynamic<br/>
    /// Below entity layer AND not IsForeground => StaticUnder
    /// </summary>
    public RenderingMode RenderingMode { get; set; } = RenderingMode.StaticUnder;
    
    public int LayerIndex { get; set; } = 0;
    public int ZIndex { get; set; } = 0;
    public bool VisibleByDefault { get; set; } = true;
    public float Opacity { get; set; } = 1.0f;

    public BaseLayerDef()
    {
    }

    // Pour la sérialisation polymorphique
    public virtual SerializationInfo GetObjectData()
    {
        var info = new SerializationInfo(GetType());
        info.AddValue(nameof(Unique), Unique);
        info.AddValue(nameof(LayerIndex), LayerIndex);
        info.AddValue(nameof(Name), Name);
        info.AddValue(nameof(ZIndex), ZIndex);
        info.AddValue(nameof(VisibleByDefault), VisibleByDefault);
        info.AddValue(nameof(Opacity), Opacity);
        return info;
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        return new List<Ulid>();
    }

    public virtual void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue(nameof(Unique), out Ulid unique);
        info.TryGetValue(nameof(LayerIndex), out int layerIndex);
        info.TryGetValue(nameof(Name), out string name);
        info.TryGetValue(nameof(ZIndex), out int zIndex);
        info.TryGetValue(nameof(VisibleByDefault), out bool visible);
        info.TryGetValue(nameof(Opacity), out float opacity, 1.0f);

        Unique = unique;
        LayerIndex = layerIndex;
        Name = name ?? "Layer";
        ZIndex = zIndex;
        VisibleByDefault = visible;
        Opacity = opacity;
    }
}