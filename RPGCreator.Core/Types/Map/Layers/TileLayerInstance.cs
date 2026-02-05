using System.Numerics;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Types.Internals;
using Serilog;

namespace RPGCreator.Core.Types.Map.Layers;

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
        
        // TODO: Code to refactor due to changes with LayerChunk system.
        /*
        foreach (var element in definition.Chunks)
        {
            var tileInstance = EngineCore.Instance.Managers.Assets.TileFactory.Create(element.Value);
            tileInstance.Position = element.Key;
            InstancedElements.Add(element.Key, tileInstance);
        }
        */
        
        _def.ElementAdded += OnElementAdded;
        _def.ElementRemoved += OnElementRemoved;
    }
    private void OnElementAdded(LayerWithElements<ITileDef>.LayerElementEventArgs e)
    {
        if(InstancedElements.TryAdd
            (
                e.Location,
                EngineCore.Instance.Managers.Assets.TileFactory.Create(e.Element)
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
                EngineCore.Instance.Managers.Assets.TileFactory.Release(tile.Value);
            }
        }
        
        if (!InstancedElements.Remove(e.Location, out var removedTile))
            return;

        EngineCore.Instance.Managers.Assets.TileFactory.Release(removedTile);
    }
    
    public void Draw()
    {
        if (!IsVisible)
            return;

        if (Renderer != null)
        {
            Renderer.Draw(null, this);
            return;
        }
        
        foreach (var tile in InstancedElements.Values)
        {
            tile.Draw(null, null/*NEED TO PASS DRAWER*/); //TODO: pass drawer
        }
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
        foreach (var tile in InstancedElements.Values)
        {
            EngineCore.Instance.Managers.Assets.TileFactory.Release(tile);
        }
        
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

        // TODO: Code to refactor due to changes with LayerChunk system.
        /*
        foreach (var element in def.Chunks)
        {
            var tileInstance = EngineCore.Instance.Managers.Assets.TileFactory.Create(element.Value);
            InstancedElements.Add(element.Key, tileInstance);
        }
        */
        _def.ElementAdded += OnElementAdded;
        _def.ElementRemoved += OnElementRemoved;
        
    }
}