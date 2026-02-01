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

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using CommunityToolkit.Diagnostics;
using RPGCreator.Core.Managers.BrushManagers.Brushs;
using RPGCreator.SDK;
using RPGCreator.SDK.Editor.Brushes;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Types;
using Serilog;

namespace RPGCreator.Core.Managers.BrushManagers
{
    public class BrushManager : IBrushManager
    {
        private readonly Dictionary<URN, IBrushInfo> _brushes = new();
        
        internal BrushManager()
        {

        }

        public IBrushState State => EngineStates.BrushState;

        public IBrushInfo? GetBrush(URN brushUrn)
        {
            return !HasBrush(brushUrn) ? null : _brushes[brushUrn];
        }

        public bool TryGetBrush(URN brushName, [NotNullWhen(true)] out IBrushInfo? brush)
        {
            if (!HasBrush(brushName))
            {
                brush = null;
                return false;
            }

            brush = _brushes[brushName];
            return true;
        }

        public void AddBrush(IBrushInfo brush, bool overwriteIfExists = false)
        {
            if (HasBrush(brush.UniqueName) && !overwriteIfExists)
                return;
            _brushes[brush.UniqueName] = brush;
        }

        public bool TryAddBrush(IBrushInfo brush)
        {
            if (HasBrush(brush.UniqueName))
                return false;
            _brushes[brush.UniqueName] = brush;
            return true;
        }

        public void RemoveBrush(IBrushInfo brush)
        {
            _brushes.Remove(brush.UniqueName);
        }

        public bool TryRemoveBrush(URN brushUrn)
        {
            return _brushes.Remove(brushUrn);
        }

        public bool HasBrush(URN brushUrn)
        {
            return _brushes.ContainsKey(brushUrn);
        }

        public bool SelectBrush(URN brushUrn)
        {
            if (!HasBrush(brushUrn))
                return false;
            EngineStates.BrushState.CurrentBrush = _brushes[brushUrn];
            return true;
        }

        public void SelectBrush(IBrushInfo brush)
        {
            EngineStates.BrushState.CurrentBrush = brush;
        }

        public IBrushInfo? GetSelectedBrush()
        {
            return EngineStates.BrushState.CurrentBrush;
        }

        public bool IsBrushAbleTo<BrushFeature>(URN brushUrn) where BrushFeature : IBrushFeature
        {
            // Check if brushUrn use the BrushFeature
            if (!HasBrush(brushUrn))
                return false;
            
            var brush = _brushes[brushUrn];
            return brush is BrushFeature;
        }

        public IEnumerable<IBrushFeature> GetBrushFeatures(URN brushUrn)
        {
            if (!HasBrush(brushUrn))
                yield break;
            
            var brush = _brushes[brushUrn];
            foreach (var feature in brush.GetType().GetInterfaces())
            {
                if (typeof(IBrushFeature).IsAssignableFrom(feature) && feature != typeof(IBrushInfo))
                {
                    yield return (IBrushFeature)brush;
                }
            }
        }

        public IEnumerable<URN> GetAllBrushes()
        {
            return _brushes.Keys;
        }

        public void DrawAt(Vector2 at)
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

            at = RuntimeServices.MapService.WorldToMapCoordinates(at);

            // Handle the click at the specified point
            // This is where you would implement the logic for what happens when a brush is clicked at a specific point
            Log.Information($"Brush clicked at: {at}");
            EngineStates.BrushState.CurrentBrush = new SimpleBrush();
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
            }

            //Console.ForegroundColor = ConsoleColor.Red;
            //Console.WriteLine("The selected brush type does not support previewing. Please select a brush type that supports previewing.");
            //Console.ResetColor();

        }

        public void ClearPreview()
        {
            //Console.WriteLine("Clearing brush preview.");
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
