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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGCreator.Core.Types.RTP.EventsArgs;

namespace RPGCreator.Core.Types.RTP
{
    public class RTP_GameEvents
    {
        public event EventHandler<RTPLoadingContentArgs>? RTPLoadingContent;
        public virtual void OnRTPLoadingContent(RTPLoadingContentArgs e) => RTPLoadingContent?.Invoke(this, e);
        public event EventHandler<RTPLoadedContentArgs>? RTPLoadedContent;
        public virtual void OnRTPLoadedContent(RTPLoadedContentArgs e) => RTPLoadedContent?.Invoke(this, e);

        public event EventHandler<RTPInitializingArgs>? RTPInitializing;
        public virtual void OnRTPInitializing(RTPInitializingArgs e) => RTPInitializing?.Invoke(this, e);
        public event EventHandler<RTPInitializedArgs>? RTPInitialized;
        public virtual void OnRTPInitialized(RTPInitializedArgs e) => RTPInitialized?.Invoke(this, e);

        public event EventHandler<RTPUpdateArgs>? RTPUpdate;
        public virtual void OnRTPUpdate(RTPUpdateArgs e) => RTPUpdate?.Invoke(this, e);

        public event EventHandler<RTPDrawArgs>? RTPDraw;
        public virtual void OnRTPDraw(RTPDrawArgs e) => RTPDraw?.Invoke(this, e);
    }
}
