#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
// 
// This file is part of RPG Creator and is distributed under the MIT License.
// You are free to use, modify, and distribute this file under the terms of the MIT License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence MIT.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence MIT.
// Voir LICENSE pour plus de détails.
// 
// Contact:
// => Mail: Ward727a@gmail.com
//    Please use this object: "RPG Creator [YourObject]"
// => Discord: ward727
// 
// For urgent inquiries, sending both an email and a message on Discord is highly recommended for a quicker response.
// 
// 
#endregion
using Microsoft.Xna.Framework;
using RPGCreator.Core.Managers.AssetsManager.EventsArgs;
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.Core.Type.Assets;
using RPGCreator.Core.Type.Internal;
using RPGCreator.Core.Type.Assets.Tilesets;
using RPGCreator.Core.Type.Interfaces.UIRelated;
using RPGCreator.Core.Type.Internal.LayerRenderer;
using Serilog;
using Color = Avalonia.Media.Color;
using Point = RPGCreator.Core.Type.Internal.Point;

namespace RPGCreator.Core.Type.Map
{

    public abstract class MapLayer<TLayerElement> : BaseDrawable, ICanBeSelected
    {
        #region Properties

        protected ILayerRenderer? _layerRenderer;
        private int _zIndex = 0;
        public int ZIndex { get => _zIndex; set { _zIndex = value; ZIndexChanged?.Invoke(value); } }
        
        /// <summary>
        /// Elements in the layer, where the key is the position of the element and the value is the element itself.
        /// </summary>
        public Dictionary<Point, TLayerElement> Elements { get; set; } = new();
        protected List<Point> _surroundingElementsToIgnore = []; // This is used to ignore the surrounding element when adding a new element, for example, when autotiling. It should be cleared after each operation.
        #endregion
        
        #region Events
        
        /// <summary>
        /// Event triggered when an element is added to the layer. For example, for a TileLayer, this would be when a tile is added.
        /// </summary>
        public event EventHandler<Point>? ElementAdded;
        /// <summary>
        /// Event triggered when an element is removed from the layer. For example, for a TileLayer, this would be when a tile is removed.
        /// </summary>
        public event EventHandler<Point>? ElementRemoved;
        /// <summary>
        /// Event triggered when an element is selected in the layer. For example, for a TileLayer, this would be when a tile is selected.
        /// </summary>
        public event EventHandler<Point>? ElementSelected;
        
        /// <summary>
        /// Event triggered when the ZIndex of the layer changes.
        /// </summary>
        public event Action<int>? ZIndexChanged;
        
        /// <summary>
        /// Event triggered when the layer has been selected.
        /// </summary>
        public event Action? HasBeenSelected;
        
        #endregion
        
        #region EventsCallers
        
        protected void _ElementAdded(Point position)
        {
            ElementAdded?.Invoke(this, position);
        }
        protected void _ElementRemoved(Point position)
        {
            ElementRemoved?.Invoke(this, position);
        }
        protected void _ElementSelected(Point position)
        {
            ElementSelected?.Invoke(this, position);
        }
        protected void _ZIndexChanged(int zIndex)
        {
            ZIndexChanged?.Invoke(zIndex);
        }
        
        protected void _HasBeenSelected()
        {
            HasBeenSelected?.Invoke();
        }
        
        #endregion
        
        public virtual void AddElement(TLayerElement element, Point position)
        {
            if (Elements.ContainsKey(position))
                // throw new InvalidOperationException("An element already exists at the specified position.");
                return;

            Elements[position] = element;
            ElementAdded?.Invoke(this, position);
        }
        
        public virtual bool TryRemoveElement(Point position, out TLayerElement? element)
        {
            if (Elements.TryGetValue(position, out element))
            {
                Elements.Remove(position);
                ElementRemoved?.Invoke(this, position);
                return true;
            }
            return false;
        }
        
        public virtual bool TryGetElement(Point position, out TLayerElement? element)
        {
            return Elements.TryGetValue(position, out element);
        }
        
        public virtual bool TryRemoveElement(TLayerElement element, out Point? position)
        {
            position = Elements.FirstOrDefault(x => x.Value.Equals(element)).Key;
            
            if (position == null || !Elements.ContainsKey(position.Value)) return false;
            
            Elements.Remove(position.Value);
            ElementRemoved?.Invoke(this, position.Value);
            return true;
        }
        
        public Dictionary<Point, TLayerElement> GetSurroundingElements(Point position, int offset = 1)
        {
            Dictionary<Point,TLayerElement> surroundingElements = new();
            // Check the 8 surrounding positions
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue; // Skip the center position
                    int offsetX = x * offset;
                    int offsetY = y * offset;
                    Point surroundingPosition = new(position.X + offsetX, position.Y + offsetY);
                    if (Elements.TryGetValue(surroundingPosition, out TLayerElement? element))
                    {
                        if(_surroundingElementsToIgnore.Contains(surroundingPosition))
                        {
                            // If the element is in the list of surrounding tiles to ignore, skip it
                            continue;
                        }
                        surroundingElements[surroundingPosition] = element;
                    }
                }
            }
            return surroundingElements;
        }

        private bool _isSelected = false;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    if (_isSelected)
                    {
                        _HasBeenSelected();
                    }
                }
            }
        }
    }

    public class UIV_TileLayer : UIVisual
    {
        public readonly Color UnknownColor = Color.FromArgb(255, 0, 0, 255); // 255, 0, 0, 255
        public readonly Color DefaultColor = Color.FromArgb(240, 248, 255, 255); // 240, 248, 255, 255
        public readonly Color CollisionColor = Color.FromArgb(255, 165, 0, 255); // 255, 165, 0, 255
        public readonly Color EntityColor = Color.FromArgb(255, 192, 203, 255); // 255, 192, 203, 255
    }

    public partial class TileLayer : MapLayer<ITileable>, IHasUIVisual<UIV_TileLayer>
    {
        #if DEBUG
        private int _watchdog = 0;
        const int MAX_WATCHDOG = 1000; // Prevent infinite loop in case of autotile issues
        #endif
        // This should be the same as the one found inside the Tile type (ETileType)
        public string Name { get; set; } = string.Empty;


        //public Dictionary<Point, Ulid> TileIndexMapping { get; set; } = new(); // Maps the tile position to its index in the Tiles list for quick access
        private UIV_TileLayer _visual = new();
        public UIV_TileLayer Visual => _visual;

        public TileLayer(string name, ILayerRenderer? renderer, int zIndex = 0, bool visible = true)
        {
            Name = name;
            ZIndex = zIndex;
            IsVisible = visible;
            if (renderer != null) //TODO Remove this once the renderer for the RTP (MonoGame) is implemented
            {
                _layerRenderer = renderer;
            }

            EngineCore.Instance.Managers.Assets.Event.UpdatedAsset += Assets_Event_UpdatedAsset;
            EngineCore.Instance.Managers.Assets.Event.RemovedAsset += Assets_Event_RemovedAsset;
            
            ElementAdded += OnElementAdded;
        }

        public override void AddElement(ITileable element, Point position)
        {
            if (Elements.ContainsKey(position))
                return;
            base.AddElement(element, position);
            element.Position = position.ToMGVector2();
        }

        private void OnElementAdded(object? sender, Point position)
        {
            var tile = Elements[position];

            if(_watchdog >= MAX_WATCHDOG)
            {
                // Prevent infinite loop in case of autotile issues
                Console.WriteLine($"TileLayer: Infinite loop detected while adding tile at {position}. Watchdog limit reached.");
                return;
            }
            
            _watchdog++;
            _surroundingElementsToIgnore.Add(position);
            if (tile is Autotile)
            {
                // In this case, we need to check if the surrounding tile still respects their rules.
                var autotile = (Autotile)tile;
                
                // Get the surrounding tiles
                var surroundingTiles = GetSurroundingElements(position, tile.Tileset.TileWidth);
                // Check if the surrounding tiles still respect their rules

                foreach (var surroundingTile in surroundingTiles)
                {
                    _watchdog++;
                    if(_watchdog >= MAX_WATCHDOG)
                    {
                        // Prevent infinite loop in case of autotile issues
                        Console.WriteLine($"TileLayer: Infinite loop detected while adding tile at {position}. Watchdog limit reached.");
                        return;
                    }
                    if (surroundingTile.Value is Autotile surroundingAutotile)
                    {
                        if (!surroundingAutotile.RespectRules(this, surroundingTile.Key))
                        {
                            TryRemoveElement(surroundingTile.Key, out _);
                            
                            // Get a new autotile that respects the rules
                            var newAutotile = autotile.AutotileGroup.GetTileAt(this, surroundingTile.Key);
                            if (newAutotile != null)
                            {
                                // Add the new autotile to the layer
                                AddElement(newAutotile.GetCopy(), surroundingTile.Key);
                            }
                        }

                        if (autotile.AutotileGroup is null)
                        {
                            Log.Fatal("TileLayer: Autotile group is null for autotile at position {Position}.",position);
                            return;
                        }
                        // In the case of base tile, we have to check if it is still valid to be a base tile or not
                        if(autotile.AutotileGroup.BaseTile is null)
                            Log.Error("TileLayer: Autotile group base tile is null for autotile {AutotileName} at position {Position}.", autotile.AutotileGroup.Name, position);

                        else
                        {
                            if (surroundingAutotile.IsEqualTo(surroundingAutotile.AutotileGroup.BaseTile))
                            {
                                var newAutotile = surroundingAutotile.AutotileGroup.GetTileAt(this, surroundingTile.Key);
                                if (newAutotile != null && newAutotile is Autotile newSurroundingAutotile)
                                {
                                    // If the new autotile is not the same as the current autotile, we need to replace it
                                    if (!newSurroundingAutotile.IsEqualTo(surroundingAutotile))
                                    {
                                        TryRemoveElement(surroundingTile.Key, out _);
                                        AddElement(newSurroundingAutotile.GetCopy(), surroundingTile.Key);
                                    }
                                }
                            }
                        }

                    }
                    _watchdog--;
                    
                }
                
            }

            _watchdog--;
            _surroundingElementsToIgnore.Remove(position);

        }

        private void Assets_Event_RemovedAsset(object? sender, AssetsManagerRemovedAssetArgs e)
        {
            if (e != null)
            {
                if (e.Type == BaseAsset.TYPE.TILESETS)
                {
                    // We need to check if the removed asset is a Tileset and if it is used in this layer
                    // If it is, we need to remove the tiles that use that Tileset
                    if (e.removedAsset is not Tileset tileset)
                        return;
                    foreach (var tile in Elements.Values)
                    {
                        if (tile.Tileset.Unique == tileset.Unique)
                        {
                            TryRemoveElement(tile, out _);
                        }
                    }
                }
            }
        }

        private void Assets_Event_UpdatedAsset(object? sender, AssetsManagerUpdatedAssetArgs? e)
        {
            // We need to check if the updated asset is a Tileset and if it is used in this layer
            // If it is, we need to update the tiles in this layer that use that Tileset
            // If the tile are not present anymore in the Tileset, we need to remove them from the layer
            if (e?.Type == BaseAsset.TYPE.TILESETS)
            {

                if(e.asset is not ITileset tileset)
                    return;

                foreach (var tile in Elements.Values)
                {
                    if (tile.Tileset.Unique == tileset.Unique)
                    {

                        // Check if the tile position is still valid in the updated Tileset
                        if (tile.PositionInTileset.X > tileset.GetBitmap().Size.Width || tile.PositionInTileset.Y > tileset.GetBitmap().Size.Height ||
                            tile.PositionInTileset.X + tile.SizeInTileset.X > tileset.GetBitmap().Size.Width ||
                            tile.PositionInTileset.Y + tile.SizeInTileset.Y > tileset.GetBitmap().Size.Height || tile.SizeInTileset.X != tileset.TileWidth || tile.SizeInTileset.Y != tileset.TileHeight)
                        {
                            // If the tile position is not valid anymore, we need to remove it from the layer
                            TryRemoveElement(tile, out _);
                            continue; // Skip to the next tile
                        }

                        // If the tile's Tileset is the one that was updated, we need to update the tile
                        tile.UpdateTileset(tileset);
                    }
                }
            }
        }

        protected override void _Draw(SpriteBatchExtend? sb)
        {
            if (!IsVisible)
                return;
            
            if(_layerRenderer != null)
            {
                _layerRenderer.Draw(this);
                return; // If a custom renderer is used, we don't need to draw the layer manually
            }
            
            // Draw the layer here
            // This is where you would implement the logic to draw the layer using the provided SpriteBatchExtend instance.
            // For example, you might loop through the tiles in the layer and draw them using sb.Draw() method.
            foreach (var tile in Elements.Values)
            {
                tile.Draw(sb);
            }
        }

        protected override void _Update(GameTime gameTime)
        {
            // Tiles update logic to check for mouse events, etc.
            foreach (var tile in Elements.Values)
            {
                tile.Update(gameTime);
            }
        }
    }
}
