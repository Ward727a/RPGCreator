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
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.Core.Types.Internal;
using RPGCreator.Core.Types.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using RPGCreator.Core.Managers.AssetsManager.Factories;
using RPGCreator.Core.Types.Assets.Tilesets;
using RPGCreator.Core.Types.Editor.Context;
using RPGCreator.SDK.Editor.Brushes;
using Serilog;

namespace RPGCreator.Core.Managers.RTP.BrushManagers.Brushs
{
    public class EraserBrush : IBrush, IBrushResizeFeature
    {

        private TileFactory _tiles => EngineCore.Instance.Managers.Assets.TileFactory;
        
        public int Size { get; set; } = 1; // Default size of the brush
        public int Step => 1;

        public int MaxSize => 6;

        public int MinSize => 1;

        public void Draw(Vector2 clickPos)
        {
            Log.Error("[EraserBrush] Draw not implemented yet.");
        }

        public int GetBrushSize()
        {
            return Size;
        }

        public void ResizeBrush(int newSize)
        {
            Size = newSize;
        }

        public string Name => "Eraser Brush";
        public string Description => "Erase tiles or objects from the map.";
    }
}
