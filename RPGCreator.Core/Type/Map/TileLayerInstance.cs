using Microsoft.Xna.Framework;
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.Core.Type.Assets.Tilesets;
using RPGCreator.Core.Type.Internal;
using RPGCreator.Core.Type.Internal.LayerRenderer;
using Point = RPGCreator.Core.Type.Internal.Point;

namespace RPGCreator.Core.Type.Map;

public class TileLayerInstance : IMapLayerInstance<ITileDef, ITileInstance>, IResettable<TileLayerDefinition>, ICleanable
{
    public Ulid RuntimeUnique { get; }
    public IMapLayerDef<ITileDef> Definition => _def;
    public ILayerRenderer<ITileDef, ITileInstance>? Renderer { get; set; }
    private TileLayerDefinition _def;
    public bool IsVisible { get; }
    public bool IsSelected { get; set; }
    /// <summary>
    /// Instanced elements of this layer.<br/>
    /// This dictionary is not saved between sessions, it's only used for the current game session.<br/>
    /// If you need those to be saved, you should check the <see cref="TileLayerDefinition._elements"/> instead.<br/>
    /// Check <see cref="TileLayerDefinition"/> for more information on how to add or remove elements from the layer definition.
    /// </summary>
    public Dictionary<Point, ITileInstance> InstancedElements { get; } = new();
    
    public TileLayerInstance(TileLayerDefinition definition)
    {
        RuntimeUnique = Ulid.NewUlid();
        IsVisible = definition.VisibleByDefault;
        _def = definition;
        
        foreach (var element in definition.Elements)
        {
            var tileInstance = EngineCore.Instance.Managers.Assets.TileFactory.Create(element.Value);
            tileInstance.Position = element.Key;
            InstancedElements.Add(element.Key, tileInstance);
        }
        
        _def.ElementAdded += OnElementAdded;
        _def.ElementRemoved += OnElementRemoved;
    }
    private void OnElementAdded(object? sender, (Point location, ITileDef def) e)
    {
        InstancedElements.TryAdd
            (
                e.location,
                EngineCore.Instance.Managers.Assets.TileFactory.Create(e.def)
            );
    }

    private void OnElementRemoved(object? sender, (Point, ITileDef?) e)
    {
        if (e.Item1 == default && e.Item2 == null)
        {
            foreach (var tile in InstancedElements.ToList())
            {
                InstancedElements.Remove(tile.Key);
                EngineCore.Instance.Managers.Assets.TileFactory.Release(tile.Value);
            }
        }
        
        if (!InstancedElements.Remove(e.Item1, out var removedTile))
            return;

        EngineCore.Instance.Managers.Assets.TileFactory.Release(removedTile);
    }
    
    public void Draw(SpriteBatchExtend? sb)
    {
        if (!IsVisible)
            return;

        if (Renderer != null)
        {
            Renderer.Draw(this);
            return;
        }
        
        foreach (var tile in InstancedElements.Values)
        {
            tile.Draw(sb);
        }
    }

    public void Update(GameTime gameTime)
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

        foreach (var element in def.Elements)
        {
            var tileInstance = EngineCore.Instance.Managers.Assets.TileFactory.Create(element.Value);
            InstancedElements.Add(element.Key, tileInstance);
        }
        
        _def.ElementAdded += OnElementAdded;
        _def.ElementRemoved += OnElementRemoved;
    }
}