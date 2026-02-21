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
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Editor.Brushes;
using RPGCreator.SDK.GlobalState;
using RPGCreator.SDK.Projects;

namespace RPGCreator.Core
{
    public class EditorState : BaseState, IEditorState
    {
        public bool InEditorMode
        {
            get;
            set => SetProperty(ref field, value);
        } = false;

        public ITileDef? CurrentTile { 
            get;
            set => SetProperty(ref field, value);
        } = null;

        public override void Reset()
        {
            
        }
    }
    
    public class ProjectState : BaseState, IProjectState
    {
        public IBaseProject? CurrentProject { 
            get;
            set => SetProperty(ref field, value);
        } = null;
        
        public override void Reset()
        {
            
        }
    }
}
