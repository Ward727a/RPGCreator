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
using RPGCreator.Core.Managers.RTP.BrushManagers.Brushs;
using RPGCreator.Core.Type.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.Core.Managers.RTP.BrushManagers
{
    public class BrushManager
    {
        public BrushManagerEvent Event { get; } = new BrushManagerEvent();
        internal BrushManager()
        {

        }

        protected void RegisterEvents()
        { }

        public void ClickAt(Point at)
        {

            if(!EngineCore.Instance.Data.EditorSettings.IsDrawing)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Drawing is not enabled. Please enable drawing in the toolbar before clicking.");
                Console.ResetColor();
                return;
            }

            if (EngineCore.Instance.Data.EditedProject == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No project is currently loaded. Please load a project before clicking.");
                Console.ResetColor();
                return;
            }

            // Convert the point to a valid position in the tile width and height
            if (EngineCore.Instance.Data.SelectedTile == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No tile is currently selected. Please select a tile before clicking.");
                Console.ResetColor();
                return;
            }
            var tileWidth = EngineCore.Instance.Data.SelectedTile.Tileset.tile_width;
            var tileHeight = EngineCore.Instance.Data.SelectedTile.Tileset.tile_height;

            int tileX = (at.X / tileWidth) * tileWidth;
            int tileY = (at.Y / tileHeight) * tileHeight;

            at = new Point(tileX, tileY);

            // Handle the click at the specified point
            // This is where you would implement the logic for what happens when a brush is clicked at a specific point
            Console.WriteLine($"Brush clicked at: {at}");
            if(EngineCore.Instance.Data.EditorSettings.BrushType == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No brush type is currently selected. Please select a brush type before clicking.");
                Console.ResetColor();
                return;
            }
            Event.OnClickedAt(at, EngineCore.Instance.Data.EditorSettings.BrushType);
        }

        public void PreviewAt(Point at)
        {
            if (!EngineCore.Instance.Data.EditorSettings.IsDrawing)
            {
                //Console.ForegroundColor = ConsoleColor.Red;
                //Console.WriteLine("Drawing is not enabled. Please enable drawing in the toolbar before clicking.");
                //Console.ResetColor();
                return;
            }

            if (EngineCore.Instance.Data.EditedProject == null)
            {
                //Console.ForegroundColor = ConsoleColor.Red;
                //Console.WriteLine("No project is currently loaded. Please load a project before clicking.");
                //Console.ResetColor();
                return;
            }

            // Convert the point to a valid position in the tile width and height
            if (EngineCore.Instance.Data.SelectedTile == null)
            {
                //Console.ForegroundColor = ConsoleColor.Red;
                //Console.WriteLine("No tile is currently selected. Please select a tile before clicking.");
                //Console.ResetColor();
                return;
            }
            var tileWidth = EngineCore.Instance.Data.SelectedTile.Tileset.tile_width;
            var tileHeight = EngineCore.Instance.Data.SelectedTile.Tileset.tile_height;

            int tileX = (at.X / tileWidth) * tileWidth;
            int tileY = (at.Y / tileHeight) * tileHeight;

            at = new Point(tileX, tileY);

            // Handle the click at the specified point
            // This is where you would implement the logic for what happens when a brush is clicked at a specific point
            //Console.WriteLine($"Brush previewed at: {at}");
            if (EngineCore.Instance.Data.EditorSettings.BrushType == null)
            {
                //Console.ForegroundColor = ConsoleColor.Red;
                //Console.WriteLine("No brush type is currently selected. Please select a brush type before clicking.");
                //Console.ResetColor();
                return;
            }

            if(EngineCore.Instance.Data.EditorSettings.BrushType is IBrushPreviewFeature previewBrush)
            {
                Event.OnPreviewAt(at, previewBrush);
                return;
            }

            //Console.ForegroundColor = ConsoleColor.Red;
            //Console.WriteLine("The selected brush type does not support previewing. Please select a brush type that supports previewing.");
            //Console.ResetColor();

        }

        public void ClearPreview()
        {
            //Console.WriteLine("Clearing brush preview.");
            Event.OnClearPreview();
        }

        public Point NormalizedPositionToTile(Point position)
        {
            if (EngineCore.Instance.Data.SelectedTile == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No tile is currently selected. Please select a tile before clicking.");
                Console.ResetColor();
                return new Point(-1, -1);
            }
            var tileWidth = EngineCore.Instance.Data.SelectedTile.Tileset.tile_width;
            var tileHeight = EngineCore.Instance.Data.SelectedTile.Tileset.tile_height;
            int tileX = (position.X / tileWidth) * tileWidth;
            int tileY = (position.Y / tileHeight) * tileHeight;
            return new Point(tileX, tileY);
        }
    }
}
