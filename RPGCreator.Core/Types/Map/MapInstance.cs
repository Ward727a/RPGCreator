#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
// 
// This file is part of RPG Creator and is distributed under the Apache 2.0 License.
// You are free to use, modify, and distribute this file under the terms of the Apache 2.0 License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence Apache 2.0.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence Apache 2.0.
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

using CommunityToolkit.Diagnostics;
using RPGCreator.Core.Types.Map.Layers;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Maps.AutoLayer;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers.AutoLayer;

namespace RPGCreator.Core.Types.Map
{

    public partial class MapInstance : IMapInstance
    {
        // The Ulid Identifier can be used to sort map by creation date
        // See more: https://github.com/ulid/spec
        public Ulid Identifier { get; private set; } = Ulid.NewUlid();
        
        public MapDefinition Definition { get; private set; }
        private List<TileLayerInstance> _tileLayers = new List<TileLayerInstance>();
        public IReadOnlyList<IMapLayerInstance> TileLayers => _tileLayers.AsReadOnly();

        public IMapLayerInstance PreviewLayer { get; set; } = new TileLayerInstance(new TileLayerDefinition()
        {
            Name = "Preview Layer",
            ZIndex = 1000, // High ZIndex to ensure it is drawn on top of other layers
            VisibleByDefault = true,
        });

        public MapInstance() { }
        public MapInstance(IMapDef definition)
        {
            Guard.IsOfType(definition, typeof(MapDefinition));
            Definition = (MapDefinition)definition;
            if (Definition == null)
            {
                throw new ArgumentNullException(nameof(definition), "Map definition cannot be null.");
            }
            
            // Initialize the map with the provided definition
            foreach (var layerDef in Definition.TileLayers)
            {
                if (layerDef is TileLayerDefinition tileLayerDef)
                {
                    var layerInstance = EngineCore.Instance.Managers.Assets.TileLayerFactory.Create(tileLayerDef);
                    _tileLayers.Add(layerInstance);
                }
            }
            
            // Subscribe to events for layer management
            Definition.TileLayerAdded += OnTileLayerAdded;
            Definition.TileLayerRemoved += OnTileLayerRemoved;
        }

        private void OnTileLayerRemoved(BaseLayerDef e)
        {
            var layerToRemove = _tileLayers.FirstOrDefault(l => l.Definition.Unique == e.Unique);
            if (layerToRemove != null)
            {
                _tileLayers.Remove(layerToRemove);
                EngineCore.Instance.Managers.Assets.TileLayerFactory.Release(layerToRemove);
            }
            else
            {
                throw new InvalidOperationException("Layer to remove not found in the map instance.");
            }
        }

        private void OnTileLayerAdded(BaseLayerDef e)
        {
            switch (e)
            {
                case null:
                    throw new ArgumentNullException(nameof(e), "Tile layer definition cannot be null.");
                case AutoLayerDefinition autoLayerDefinition:
                {
                    var newLayer = EngineCore.Instance.Managers.Assets.TileLayerFactory.Create(autoLayerDefinition.InternalTileLayer);
                    _tileLayers.Add(newLayer);
                    return;
                }
                case TileLayerDefinition tileLayerDefinition:
                {
                    var newLayer = EngineCore.Instance.Managers.Assets.TileLayerFactory.Create(tileLayerDefinition);
                    _tileLayers.Add(newLayer);
                    break;
                }
            }
        }

    }
}
