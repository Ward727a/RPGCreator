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
using Avalonia.Controls;
using Microsoft.Xna.Framework;
using RPGCreator.Core.Type.Map;
using RPGCreator.Core.Type.Project;
using RPGCreator.Core.Type.RTP;

namespace RPGCreator.Core
{

    public class SelectedTileChangedEventArgs : EventArgs
    {
        public Tile? OldTile { get; }
        public Tile? NewTile { get; }
        public SelectedTileChangedEventArgs(Tile? oldTile, Tile? newTile)
        {
            OldTile = oldTile;
            NewTile = newTile;
        }
    }

    public class  SelectedLayerChangedEventArgs : EventArgs
    {
        
        public MapLayer? OldLayer { get; }
        public MapLayer? NewLayer { get; }
        public SelectedLayerChangedEventArgs(MapLayer? oldLayer, MapLayer? newLayer)
        {
            OldLayer = oldLayer;
            NewLayer = newLayer;
        }

    }

    public class EngineData
    {


        public event EventHandler? EditedMapChanged;
        public event EventHandler<SelectedLayerChangedEventArgs>? SelectedLayerChanged;
        public event EventHandler<SelectedTileChangedEventArgs>? SelectedTileChanged;

        private BaseMap? _editedMap;
        private MapLayer? _selectedLayer;
        private Tile? _selectedTile;

        internal EngineData()
        { }

        public static string AppName => "RPG Creator";
        public static Version AppVersion => new(0, 1, 0);

        public BaseProject? EditedProject { get; internal set; }
        public MapLayer? SelectedLayer { get => _selectedLayer; 
            set
            {
                if (_selectedLayer != value)
                {
                    var oldLayer = _selectedLayer;
                    _selectedLayer = value;
                    SelectedLayerChanged?.Invoke(this, new SelectedLayerChangedEventArgs(oldLayer, _selectedLayer));
                }
            }
        }
        public BaseMap? EditedMap { get => _editedMap; 
            set
            {
                if (_editedMap != value)
                {
                    _editedMap = value;
                    EditedMapChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public Tile? SelectedTile { get => _selectedTile; 
            set
            {
                if (_selectedTile != value)
                {
                    var oldTile = _selectedTile;
                    _selectedTile = value;
                    SelectedTileChanged?.Invoke(this, new SelectedTileChangedEventArgs(oldTile, _selectedTile));
                }
            }
        }
        public Game? RTPGame { get; internal set; }
    }
}
