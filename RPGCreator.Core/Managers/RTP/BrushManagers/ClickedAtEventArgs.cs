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
using RPGCreator.Core.Managers.RTP.BrushManagers.Brushs;
using RPGCreator.Core.Types.Internal;

namespace RPGCreator.Core.Managers.RTP.BrushManagers
{
    public class ClickedAtEventArgs : EventArgs
    {
        public Point At { get; }
        public IBrush brush { get; private set; }
        public ClickedAtEventArgs(int x, int y)
        {
            At = new Point(x, y);
            this.brush = new SimpleBrush(); // Default brush if none is specified
        }
        public ClickedAtEventArgs(Point at)
        {
            At = at;
            this.brush = new SimpleBrush(); // Default brush if none is specified
        }
        public ClickedAtEventArgs(int x, int y, IBrush brush) : this(x, y)
        {
            this.brush = brush;
        }
        public ClickedAtEventArgs(Point at, IBrush brush) : this(at)
        {
            this.brush = brush;
        }
    }
}
