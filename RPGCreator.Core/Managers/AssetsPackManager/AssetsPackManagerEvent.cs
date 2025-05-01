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
using RPGCreator.Core.Managers.AssetsPackManager.EventsArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.Core.Managers.AssetsPackManager
{
    public class AssetsPackManagerEvent
    {
        event EventHandler<AssetsPackManagerAddingPackArgs>? AddingPack;
        public virtual void OnAddingPack(AssetsPackManagerAddingPackArgs args)
        {
            AddingPack?.Invoke(this, args);
        }

        event EventHandler<AssetsPackManagerAddedPackArgs>? AddedPack;
        public virtual void OnAddedPack(AssetsPackManagerAddedPackArgs args)
        {
            AddedPack?.Invoke(this, args);
        }

        event EventHandler<AssetsPackManagerRemovingPackArgs>? RemovingPack;
        public virtual void OnRemovingPack(AssetsPackManagerRemovingPackArgs args)
        {
            RemovingPack?.Invoke(this, args);
        }

        event EventHandler<AssetsPackManagerRemovedPackArgs>? RemovedPack;
        public virtual void OnRemovedPack(AssetsPackManagerRemovedPackArgs args)
        {
            RemovedPack?.Invoke(this, args);
        }
    }
}
