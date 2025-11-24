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
using RPGCreator.Core.Types.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGCreator.Core.Types.Assets.Characters;
using RPGCreator.Core.Types.Assets.Actors;
using Point = RPGCreator.Core.Types.Internal.Point;
using Size = RPGCreator.Core.Types.Internal.Size;

namespace RPGCreator.Core.Types.Map
{

    public struct GRID_PARAMETER
    {
        public int CellWidth;
        public int CellHeight;

        public Color CellBorderColor;
    }

    public partial class MapInstance : BaseDrawable
    {
        // The Ulid Identifier can be used to sort map by creation date
        // See more: https://github.com/ulid/spec
        public Ulid Identifier { get; private set; } = Ulid.NewUlid();
        
        public MapDefinition Definition { get; private set; }
        public List<TileLayerInstance> TileLayers { get; private set; } = new List<TileLayerInstance>();
        public List<IActor> ActorsInMap { get; private set; } = [
            // new CharacterActor()
            // {
            //     CharacterData = new CharacterData()
            //     {
            //         Name = "Default Character",
            //     },
            // }
        ];

        public readonly TileLayerInstance PreviewLayer = new TileLayerInstance(new TileLayerDefinition()
        {
            Name = "Preview Layer",
            ZIndex = 1000, // High ZIndex to ensure it is drawn on top of other layers
            VisibleByDefault = true,
        });

        public MapInstance() { }
        public MapInstance(MapDefinition definition)
        {
            Definition = definition;
            if (Definition == null)
            {
                throw new ArgumentNullException(nameof(definition), "Map definition cannot be null.");
            }
            
            // Initialize the map with the provided definition
            foreach (var layerDef in Definition.TileLayers)
            {
                var layerInstance = EngineCore.Instance.Managers.Assets.TileLayerFactory.Create(layerDef);
                TileLayers.Add(layerInstance);
            }
            
            // Subscribe to events for layer management
            Definition.TileLayerAdded += OnTileLayerAdded;
            Definition.TileLayerRemoved += OnTileLayerRemoved;
        }

        private void OnTileLayerRemoved(object? sender, TileLayerDefinition e)
        {
            var layerToRemove = TileLayers.FirstOrDefault(l => l.Definition.Unique == e.Unique);
            if (layerToRemove != null)
            {
                TileLayers.Remove(layerToRemove);
                EngineCore.Instance.Managers.Assets.TileLayerFactory.Release(layerToRemove);
            }
            else
            {
                throw new InvalidOperationException("Layer to remove not found in the map instance.");
            }
        }

        private void OnTileLayerAdded(object? sender, TileLayerDefinition e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e), "Tile layer definition cannot be null.");
            }

            var newLayer = EngineCore.Instance.Managers.Assets.TileLayerFactory.Create(e);
            TileLayers.Add(newLayer);
        }

        protected override void _Draw(SpriteBatchExtend? sb)
        {
            // Draw the map here
            // This is where you would implement the logic to draw the map using the provided SpriteBatchExtend instance.
            // For example, you might loop through the tiles in the map and draw them using sb.Draw() method.

            foreach (var layer in TileLayers.OrderBy(l => l.Definition.ZIndex))
            {
                if (layer.IsVisible)
                {
                    layer.Draw(sb);
                }
            }
            // Nothing here for now, need to think about what the use of this function could be for the map
            foreach (var actor in ActorsInMap)
            {
                actor.Draw(sb);
            }
        }

        public void SelectLayer(TileLayerInstance layer)
        {
            foreach (var l in TileLayers)
            {
                l.IsSelected = false;
            }
            layer.IsSelected = true;
        }

        public void SelectLayer(int index)
        {
            if (index < 0 || index >= Definition.TileLayers.Count)
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            SelectLayer(TileLayers[index]);
        }

        public TileLayerInstance? GetSelectedLayer()
        {
            return TileLayers.FirstOrDefault(l => l.IsSelected);
        }

        protected override void _Update(GameTime gameTime)
        {
            // Nothing here for now, need to think about what the use of this function could be for the map
            foreach (var actor in ActorsInMap)
            {
                actor.Update(gameTime);
            }
        }
    }
}
