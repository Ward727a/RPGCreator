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
using RPGCreator.Core.Managers.RTP.BrushManagers.Brushs;
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
        public class SEditorSettings()
        {

            public event Action? IsDrawingChanged;
            private bool _isDrawing = false;
            public bool IsDrawing { get => _isDrawing; set
                {
                    if (_isDrawing != value)
                    {
                        _isDrawing = value;
                        IsDrawingChanged?.Invoke();
                    }
                }
            }

            public event Action? ShowCollisionChanged;
            private bool _showCollisionLayer = true;
            public bool ShowCollisionLayer { get => _showCollisionLayer; set
                {
                    if (_showCollisionLayer != value)
                    {
                        _showCollisionLayer = value;
                        ShowCollisionChanged?.Invoke();
                    }
                }
            }
            public event Action? ShowEntityChanged;
            private bool _showEntityLayer = true;
            public bool ShowEntityLayer { get => _showEntityLayer; set
                {
                    if (_showEntityLayer != value)
                    {
                        _showEntityLayer = value;
                        ShowEntityChanged?.Invoke();
                    }
                }
            }

            public event Action? BrushTypeChanged;
            private IBrush? _brushType = null;
            public IBrush? BrushType { get => _brushType; set
                {
                    if (_brushType != value)
                    {
                        _brushType = value;
                        BrushTypeChanged?.Invoke();
                    }
                }
            }
        }

        public event EventHandler? EditedMapChanged;
        public event EventHandler<SelectedLayerChangedEventArgs>? SelectedLayerChanged;
        public event EventHandler<SelectedTileChangedEventArgs>? SelectedTileChanged;

        private BaseMap? _editedMap;
        private MapLayer? _selectedLayer;
        private Tile? _selectedTile;
        public SEditorSettings EditorSettings { get; } = new SEditorSettings();

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
