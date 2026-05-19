using System.Numerics;
using RPGCreator.EngineLib.Types.Assets.Tilesets;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Types.Internals;
using Serilog;

namespace RPGCreator.EngineLib.Types.Map.Layers;

public class TileLayerInstance : IMapLayerInstance<ITileDef, ITileInstance>, IResettable<TileLayerDefinition>, ICleanable
{
    public Ulid RuntimeUnique { get; }
    public TileLayerDefinition Definition => _def;
    public ILayerRenderer<ITileDef, ITileInstance>? Renderer { get; set; }
    private TileLayerDefinition _def;
    public bool IsVisible { get; set; }
    public bool IsSelected { get; set; }
    /// <summary>
    /// Instanced elements of this layer.<br/>
    /// This dictionary is not saved between sessions, it's only used for the current game session.<br/>
    /// If you need those to be saved, you should check the <see cref="LayerWithElements{TDef}._chunks"/> instead.<br/>
    /// Check <see cref="TileLayerDefinition"/> for more information on how to add or remove elements from the layer definition.
    /// </summary>
    public Dictionary<Vector2, ITileInstance> InstancedElements { get; } = new();
    
    public TileLayerInstance(TileLayerDefinition definition)
    {
        RuntimeUnique = Ulid.NewUlid();
        IsVisible = definition.VisibleByDefault;
        _def = definition;
        
        
        _def.ElementAdded += OnElementAdded;
        _def.ElementRemoved += OnElementRemoved;
    }
    private void OnElementAdded(LayerWithElements<ITileDef>.LayerElementEventArgs e)
    {
        if(InstancedElements.TryAdd
            (
                e.Location,
                new TileInstance(e.Element)
            ))
            Log.Information("[TileLayerInstance: {LayerName}] Added tile instance at {Location}", _def.Name, e.Location);
        else
            Log.Warning("[TileLayerInstance: {LayerName}] Failed to add tile instance at {Location} - already exists", _def.Name, e.Location);
    }

    private void OnElementRemoved(LayerWithElements<ITileDef>.LayerElementEventArgs e)
    {
        if (e.Location == default && e.Element == null)
        {
            foreach (var tile in InstancedElements.ToList())
            {
                InstancedElements.Remove(tile.Key);
            }
        }

        InstancedElements.Remove(e.Location, out _);
    }

    public void Update(TimeSpan gameTime)
    {
        foreach (var tile in InstancedElements.Values)
        {
            tile.Update(gameTime);
        }
    }

    public void Clean()
    {
        InstancedElements.Clear();
        
        _def.ElementAdded -= OnElementAdded;
        _def.ElementRemoved -= OnElementRemoved;
        
        Renderer = null;
        _def = null!;
    }

    public void ResetFrom(TileLayerDefinition def, params object[] parameters)
    {
        if (def == null)
            throw new ArgumentNullException(nameof(def), "The definition cannot be null.");

        _def = def;
        InstancedElements.Clear();
        
        _def.ElementAdded += OnElementAdded;
        _def.ElementRemoved += OnElementRemoved;
        
    }
}