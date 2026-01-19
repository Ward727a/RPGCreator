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
using CommunityToolkit.Diagnostics;
using RPGCreator.Core.Managers.RTP.BrushManagers.Brushs;
using RPGCreator.Core.Types.Internal;
using RPGCreator.Core.Types.Editor.Context;
using RPGCreator.SDK;
using RPGCreator.SDK.Editor.Brushes;
using Serilog;

namespace RPGCreator.Core.Managers.RTP.BrushManagers
{
    public class BrushManager : IBrushManager
    {
        public BrushManagerEvent Event { get; } = new BrushManagerEvent();
        
        
        internal BrushManager()
        {

        }

        protected void RegisterEvents()
        { }

        public void ClickAt(Vector2 at)
        {

            if(!EngineStates.BrushState.IsDrawing)
            {
                // Log.Error("Drawing is not enabled. Please enable drawing in the toolbar before clicking.");
                return;
            }

            // Convert the point to a valid position in the tile width and height
            if (EngineStates.EditorState.CurrentTile == null && EngineStates.BrushState.CurrentObjectToPaint == null)
            {
                // Log.Error("No tile is currently selected. Please select a tile before clicking.");
                return;
            }

            Guard.IsNotNull(EngineStates.EditorState.CurrentMap, "CurrentMap");
            
            var tileWidth = EngineStates.EditorState.CurrentMap.GridParameter.CellWidth;
            var tileHeight = EngineStates.EditorState.CurrentMap.GridParameter.CellHeight;

            float tileX = (at.X / tileWidth) * tileWidth;
            float tileY = (at.Y / tileHeight) * tileHeight;

            at = new Vector2(tileX, tileY);

            // Handle the click at the specified point
            // This is where you would implement the logic for what happens when a brush is clicked at a specific point
            Log.Information($"Brush clicked at: {at}");
            if(EngineStates.BrushState.CurrentBrush == null || EngineStates.BrushState.CurrentBrush is not IBrush brush)
            {
                // Log.Error("No brush type is currently selected. Please select a brush type before clicking.");
                return;
            }

            brush.Draw(at);
            
            // Event.OnClickedAt(at, EngineCore.Instance.Data.EditorSettings.BrushType);
        }

        public void PreviewAt(Vector2 at)
        {
            if (!EngineStates.BrushState.IsDrawing)
            {
                //Console.ForegroundColor = ConsoleColor.Red;
                //Console.WriteLine("Drawing is not enabled. Please enable drawing in the toolbar before clicking.");
                //Console.ResetColor();
                return;
            }

            // Convert the point to a valid position in the tile width and height
            if (EngineStates.EditorState.CurrentTile == null)
            {
                //Console.ForegroundColor = ConsoleColor.Red;
                //Console.WriteLine("No tile is currently selected. Please select a tile before clicking.");
                //Console.ResetColor();
                return;
            }
            var tileWidth = EngineStates.EditorState.CurrentTile.TilesetDef.TileWidth;
            var tileHeight = EngineStates.EditorState.CurrentTile.TilesetDef.TileHeight;

            float tileX = (at.X / tileWidth) * tileWidth;
            float tileY = (at.Y / tileHeight) * tileHeight;

            at = new Vector2(tileX, tileY);

            // Handle the click at the specified point
            // This is where you would implement the logic for what happens when a brush is clicked at a specific point
            //Console.WriteLine($"Brush previewed at: {at}");
            if (EngineStates.BrushState.CurrentBrush == null)
            {
                //Console.ForegroundColor = ConsoleColor.Red;
                //Console.WriteLine("No brush type is currently selected. Please select a brush type before clicking.");
                //Console.ResetColor();
                return;
            }

            if(EngineStates.BrushState.CurrentBrush is IBrushPreview previewBrush && previewBrush.IsPreviewEnabled)
            {
                previewBrush.ShowPreview(at);
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

        public Vector2 NormalizedPositionToTile(Vector2 position)
        {
            if (EngineStates.EditorState.CurrentTile == null)
            {
                // Log.Error("No tile is currently selected. Please select a tile before clicking.");
                return Vector2.Zero;
            }
            var tileWidth = EngineStates.EditorState.CurrentTile.TilesetDef.TileWidth;
            var tileHeight = EngineStates.EditorState.CurrentTile.TilesetDef.TileHeight;
            float tileX = (position.X / tileWidth) * tileWidth;
            float tileY = (position.Y / tileHeight) * tileHeight;
            return new Vector2(tileX, tileY);
        }
    }
}
