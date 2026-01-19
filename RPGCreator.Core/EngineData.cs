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

using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Editor.Brushes;
using RPGCreator.SDK.Types.Interfaces;

namespace RPGCreator.Core
{
    public partial class EditorState : ObservableObject, IEditorState
    {
        public bool InEditorMode { get; set; } = false;
        public bool InPlacingMode { get; set; } = false;
        public bool InDrawingMode { get; set; } = false;
        public bool ShowCollisionLayer { get; set; } = false;
        public bool ShowEntityLayer { get; set; } = false;
        public MapDefinition? CurrentMap { get; set; } = null;
        public BaseLayerDef? CurrentLayer { get; set; } = null;
        public ITileDef? CurrentTile { get; set; } = null;
    }
    
    public partial class BrushState : ObservableObject, IBrushState
    {
        public IBrushInfo? CurrentBrush { get; set; } = null;
        public BrushMode CurrentMode { get; set; } = BrushMode.Tiling;
        public object? CurrentObjectToPaint { get; set; }
        public bool IsPlacing { get; set; } = false;
        public bool IsDrawing { get; set; } = false;
        public Vector2 LastDrawAt { get; set; } = Vector2.Zero;
    }

    public partial class ProjectState : ObservableObject, IProjectState
    {
        public IBaseProject? CurrentProject { get; set; } = null;
    }
}
