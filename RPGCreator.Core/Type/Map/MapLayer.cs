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
        public List<Tile> Tiles { get; set; } = [];
        public List<Tile> SelectedTiles { get; set; } = [];
        private UIV_MapLayer _visual = new();
        public UIV_MapLayer Visual => _visual;

        public MapLayer(string name, int zIndex, bool visible)
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
            foreach (var tile in Tiles)
            {
                tile.Draw(sb);
            }
        }

        public virtual void AddTile(Tile tile)
        {

            if ((int)tile.Type != (int)LayerType)
                return;

            ArgumentNullException.ThrowIfNull(tile);
            Tiles.Add(tile);
            tile.Parent = this;
            TileAdded?.Invoke(this, tile);
        }
        public virtual void RemoveTile(Tile tile)
        {
            ArgumentNullException.ThrowIfNull(tile);
            Tiles.Remove(tile);
            tile.Parent = null;
            TileRemoved?.Invoke(this, tile);
        }

        protected override void _Update(GameTime gameTime)
        {
            // Updating the layer here

            // Tiles update logic to check for mouse events, etc.
            Tiles.ForEach(tile =>
            {
                tile.Update(gameTime);
            });
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
            var tile = Tiles.FirstOrDefault(t => t.Position.X <= x && t.Position.X + t.UV.Width >= x &&
                                             t.Position.Y <= y && t.Position.Y + t.UV.Height >= y);
            return tile;
        }

        public bool TryGetTileAt(int x, int y, out Tile? tile)
        {
            tile = Tiles.FirstOrDefault(t => t.Position.X <= x && t.Position.X + t.UV.Width >= x &&
                                             t.Position.Y <= y && t.Position.Y + t.UV.Height >= y);
            return tile != null;
        }
    }
}
