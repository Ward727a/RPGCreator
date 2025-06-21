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
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Xna.Framework;
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.Core.Type.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = Avalonia.Media.Color;
using Point = RPGCreator.Core.Type.Internal.Point;

namespace RPGCreator.Core.Type.Map
{

    public class UIV_MapLayer : UIVisual
    {
        public readonly Color UnknownColor = Color.FromArgb(255, 0, 0, 255); // 255, 0, 0, 255
        public readonly Color DefaultColor = Color.FromArgb(240, 248, 255, 255); // 240, 248, 255, 255
        public readonly Color CollisionColor = Color.FromArgb(255, 165, 0, 255); // 255, 165, 0, 255
        public readonly Color EntityColor = Color.FromArgb(255, 192, 203, 255); // 255, 192, 203, 255
    }

    public partial class MapLayer : BaseDrawable, IHasUIVisual<UIV_MapLayer>
    {
        
        // This should be the same as the one found inside the Tile type (ETileType)
        public enum ELayerType
        {
            UNKNOWN,
            DEFAULT,
            COLLISION,
            ENTITY
        }

        event EventHandler<Tile>? TileAdded;
        event EventHandler<Tile>? TileRemoved;
        event EventHandler<Tile>? TileSelected;

        [ObservableProperty]
        private bool _IsSelected = false;

        [ObservableProperty]
        private ELayerType _LayerType = ELayerType.DEFAULT;
        public string Name { get; set; } = string.Empty;
        public int ZIndex { get; set; } = 0;
        public bool Visible { get; set; } = true;
        public Dictionary<Point, Tile> Tiles { get; set; } = [];
        //public Dictionary<Point, Ulid> TileIndexMapping { get; set; } = new(); // Maps the tile position to its index in the Tiles list for quick access
        public List<Tile> SelectedTiles { get; set; } = [];
        private UIV_MapLayer _visual = new();
        public UIV_MapLayer Visual => _visual;

        public MapLayer(string name, int zIndex = 0, bool visible = true)
        {
            Name = name;
            ZIndex = zIndex;
            Visible = visible;
        }

        protected override void _Draw(SpriteBatchExtend sb)
        {
            if (!IsVisible)
                return;
            // Draw the layer here
            // This is where you would implement the logic to draw the layer using the provided SpriteBatchExtend instance.
            // For example, you might loop through the tiles in the layer and draw them using sb.Draw() method.
            foreach (var tile in Tiles.Values)
            {
                tile.Draw(sb);
            }
        }

        public virtual void AddTile(Tile tile)
        {

            if ((int)tile.Type != (int)LayerType)
                return;

            ArgumentNullException.ThrowIfNull(tile);
            Tiles.Add(tile.Position, tile);
            tile.Parent = this;
            TileAdded?.Invoke(this, tile);
        }
        public virtual void AddTileAt(Tile tile, Point at)
        {
            if((int)tile.Type != (int)LayerType)
                return;

            if(TryGetTileAt(at, out Tile? samePositionTile))
            {
                if (samePositionTile == null)
                    throw new InvalidOperationException("Tile at the specified position is placed, but null, this should not happen.");
                RemoveTile(samePositionTile);
            }

            ArgumentNullException.ThrowIfNull(tile);
            Tile? CopyTile = tile.Clone() as Tile;
            CopyTile.Position = at.ToMG().ToVector2();
            Tiles.Add(at, CopyTile);
            CopyTile.Parent = this;
        }
        public virtual void RemoveTile(Tile tile)
        {
            ArgumentNullException.ThrowIfNull(tile);
            Tiles.Remove(tile.Position);
            tile.Parent = null;
            TileRemoved?.Invoke(this, tile);
        }

        protected override void _Update(GameTime gameTime)
        {
            // Updating the layer here

            // Tiles update logic to check for mouse events, etc.
            foreach (var tile in Tiles.Values)
            {
                tile.Update(gameTime);
            }
        }

        public void SelectTile(Tile tile)
        {
            ArgumentNullException.ThrowIfNull(tile);

            if(SelectedTiles.Count > 0)
            {
                foreach (var selectedTile in SelectedTiles)
                {
                    selectedTile.SelectedTile = false;
                }
                SelectedTiles.Clear();
            }
            tile.SelectedTile = true;
            TileSelected?.Invoke(this, tile);
        }

        public Tile? GetTileAt(int x, int y)
        {
            var at = new Point(x, y);
            if (Tiles.TryGetValue(at, out Tile value))
            {
                return value;
            }
            return null;
        }

        public bool TryGetTileAt(int x, int y, out Tile? tile)
        {
            var at = new Point(x, y);
            tile = null;
            Tiles.TryGetValue(at, out tile);
            return tile != null;
        }
        public bool TryGetTileAt(Point at, out Tile? tile)
        {
            tile = null;
            Tiles.TryGetValue(at, out tile);
            return tile != null;
        }
    }
}
