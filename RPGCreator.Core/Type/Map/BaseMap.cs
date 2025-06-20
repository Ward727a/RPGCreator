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
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.Core.Type.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Size = RPGCreator.Core.Type.Internal.Size;

namespace RPGCreator.Core.Type.Map
{

    public struct GRID_PARAMETER
    {
        public int CellWidth;
        public int CellHeight;

        public Color CellBorderColor;
    }

    public partial class BaseMap : BaseDrawable
    {
        // The Ulid Identifier can be used to sort map by creation date
        // See more: https://github.com/ulid/spec
        public Ulid Identifier { get; private set; } = Ulid.NewUlid();
        [ObservableProperty]
        private string _Name = string.Empty;
        [ObservableProperty]
        private string _Description = string.Empty;
        public ObservableCollection<BaseMap> Levels = [];
        [ObservableProperty]
        private ObservableCollection<MapLayer> _Layers = [];
        [ObservableProperty]
        private Size _Size = new(10, 20);
        [ObservableProperty]
        private bool _ShowGrid = true;
        [ObservableProperty]
        private GRID_PARAMETER _GridParameter = new()
        {
            CellWidth = 32,
            CellHeight = 32,
            CellBorderColor = Color.Black
        };
        [ObservableProperty]
        private Color _BackgroundColor = Color.DeepSkyBlue;

        [ObservableProperty]
        private List<BaseMap> childMaps = [];

        public BaseMap() { }
        public BaseMap(string name)
        {
            Name = name;
        }

        protected override void _Draw(SpriteBatchExtend sb)
        {
            // Draw the map here
            // This is where you would implement the logic to draw the map using the provided SpriteBatchExtend instance.
            // For example, you might loop through the tiles in the map and draw them using sb.Draw() method.

            sb.FillRectangle(new Rectangle(0, 0, Size.Width*GridParameter.CellWidth, Size.Height*GridParameter.CellHeight), BackgroundColor);

            if (ShowGrid)
            {
                for (int i = 0; i < Size.Width* GridParameter.CellWidth; i += GridParameter.CellWidth)
                {
                    for (int j = 0; j < Size.Height * GridParameter.CellHeight; j += GridParameter.CellHeight)
                    {
                        sb.DrawRectangle(new Rectangle(i, j, GridParameter.CellWidth, GridParameter.CellHeight), GridParameter.CellBorderColor);
                    }
                }
            }

            foreach (var layer in Layers.OrderBy(l => l.ZIndex))
            {
                if (layer.Visible)
                {
                    layer.Draw(sb);
                }
            }
        }

        public void AddLayer(MapLayer layer)
        {
            Layers.Add(layer);
            layer.Parent = this;
            _OnAddedChild();
        }

        public void RemoveLayer(MapLayer layer)
        {
            Layers.Remove(layer);
            layer.Parent = null;
            _OnRemovedChild();
        }

        public void SelectLayer(MapLayer layer)
        {
            foreach (var l in Layers)
            {
                l.IsSelected = false;
            }
            layer.IsSelected = true;
        }

        public void SelectLayer(int index)
        {
            if (index < 0 || index >= Layers.Count)
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            SelectLayer(Layers[index]);
        }

        public MapLayer? GetSelectedLayer()
        {
            return Layers.FirstOrDefault(l => l.IsSelected);
        }

        protected override void _Update(GameTime gameTime)
        {
            // Nothing here for now, need to think about what the use of this function could be for the map
        }

        public BaseMap CreateChildMap(string MapName)
        {
            BaseMap Child = new(MapName);

            ChildMaps.Add(Child);

            EngineCore.Instance.Data.EditedProject.GameData.Maps.Add(Child);

            return Child;
        }
    }
}
