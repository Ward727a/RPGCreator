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
using RPGCreator.Core.Types.Internal;
using RPGCreator.Core.Types.Map;
using RPGCreator.Core.Types.Editor.Context;

namespace RPGCreator.Core.Managers.RTP.BrushManagers.Brushs
{
    public interface IBrush
    {


        void Draw(Point clickPos, MapEditorContext context);

        protected static bool InBorder(Point at, MapInstance mapInstance)
        {
            if (mapInstance == null)
            {
                return false;
            }

            int cellSize = mapInstance.Definition.GridParameter.CellWidth;
            float horizontalCells = mapInstance.Definition.Size.Width;
            float verticalCells = mapInstance.Definition.Size.Height;

            // Check if the point is within the bounds of the map
            if (at.X < 0 || at.Y < 0 || at.X >= horizontalCells * cellSize || at.Y >= verticalCells * cellSize)
            {
                return false;
            }
            return true;
        }
    }
}
