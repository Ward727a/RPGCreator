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
using RPGCreator.Core.Types.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.Core.Managers.RTP.BrushManagers
{
    public partial class BrushManagerEvent
    {

        internal BrushManagerEvent()
        {
        }

        public event Action? ClearPreview;
        internal void OnClearPreview()
        {
            ClearPreview?.Invoke();
        }

        public event EventHandler<PreviewAtEventArgs>? PreviewAt;
        internal void OnPreviewAt(PreviewAtEventArgs args)
        {
            PreviewAt?.Invoke(this, args);
        }
        internal void OnPreviewAt(int x, int y)
        {
            PreviewAt?.Invoke(this, new PreviewAtEventArgs(x, y));
        }
        internal void OnPreviewAt(Point at)
        {
            PreviewAt?.Invoke(this, new PreviewAtEventArgs(at));
        }
        internal void OnPreviewAt(int x, int y, IBrushPreviewFeature brush)
        {
            PreviewAt?.Invoke(this, new PreviewAtEventArgs(x, y, brush));
        }
        internal void OnPreviewAt(Point at, IBrushPreviewFeature brush)
        {
            PreviewAt?.Invoke(this, new PreviewAtEventArgs(at, brush));
        }

        public event EventHandler<ClickedAtEventArgs>? ClickedAt;
        internal void OnClickedAt(ClickedAtEventArgs args)
        {
            ClickedAt?.Invoke(this, args);
        }
        internal void OnClickedAt(int x, int y)
        {
            ClickedAt?.Invoke(this, new ClickedAtEventArgs(x, y));
        }
        internal void OnClickedAt(Point at)
        {
            ClickedAt?.Invoke(this, new ClickedAtEventArgs(at));
        }
        internal void OnClickedAt(int x, int y, IBrush brush)
        {
            ClickedAt?.Invoke(this, new ClickedAtEventArgs(x, y, brush));
        }
        internal void OnClickedAt(Point at, IBrush brush)
        {
            ClickedAt?.Invoke(this, new ClickedAtEventArgs(at, brush));
        }
    }
}
