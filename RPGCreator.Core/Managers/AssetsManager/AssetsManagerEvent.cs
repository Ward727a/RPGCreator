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
using RPGCreator.Core.Events.EventArgs;
using RPGCreator.Core.Managers.AssetsManager.EventsArgs;
using RPGCreator.Core.Type.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.Core.Managers.AssetsManager
{
    public class AssetsManagerEvent : BaseEventArgs
    {

        public event EventHandler<AssetsManagerAddingPackArgs>? AddingPack; // Should be inside the AssetsPackManager, not here
        public virtual void OnAddingPack(AssetsManagerAddingPackArgs args)
        {
            AddingPack?.Invoke(this, args);
        }

        public event EventHandler<AssetsManagerAddedPackArgs>? AddedPack; // Should be inside the AssetsPackManager, not here
        public virtual void OnAddedPack(AssetsManagerAddedPackArgs args)
        {
            AddedPack?.Invoke(this, args);
        }

        public event EventHandler<AssetsManagerRemovingPackArgs>? RemovingPack; // Should be inside the AssetsPackManager, not here
        public virtual void OnRemovingPack(AssetsManagerRemovingPackArgs args)
        {
            RemovingPack?.Invoke(this, args);
        }

        public event EventHandler<AssetsManagerRemovedPackArgs>? RemovedPack; // Should be inside the AssetsPackManager, not here
        public virtual void OnRemovedPack(AssetsManagerRemovedPackArgs args)
        {
            RemovedPack?.Invoke(this, args);
        }

        public event EventHandler<AssetsManagerGettingAssetArgs>? GettingAsset;
        public virtual void OnGettingAsset(AssetsManagerGettingAssetArgs args)
        {
            GettingAsset?.Invoke(this, args);
        }

        public event EventHandler<AssetsManagerGotAssetArgs>? GotAsset;
        public virtual void OnGotAsset(AssetsManagerGotAssetArgs args)
        {
            GotAsset?.Invoke(this, args);
        }

        public event EventHandler<AssetsManagerAddingAssetArgs>? AddingAsset;
        public virtual void OnAddingAsset(AssetsManagerAddingAssetArgs args)
        {
            AddingAsset?.Invoke(this, args);
        }

        public event EventHandler<AssetsManagerAddedAssetArgs>? AddedAsset;
        public virtual void OnAddedAsset(AssetsManagerAddedAssetArgs args)
        {
            AddedAsset?.Invoke(this, args);
        }

        public event EventHandler<AssetsManagerMovingAssetArgs>? MovingAsset;
        public virtual void OnMovingAsset(AssetsManagerMovingAssetArgs args)
        {
            MovingAsset?.Invoke(this, args);
        }

        public event EventHandler<AssetsManagerMovedAssetArgs>? MovedAsset;
        public virtual void OnMovedAsset(AssetsManagerMovedAssetArgs args)
        {
            MovedAsset?.Invoke(this, args);
        }

        public event EventHandler<AssetsManagerRemovingAssetArgs>? RemovingAsset;
        public virtual void OnRemovingAsset(AssetsManagerRemovingAssetArgs args)
        {
            RemovingAsset?.Invoke(this, args);
        }

        public event EventHandler<AssetsManagerRemovedAssetArgs>? RemovedAsset;
        public virtual void OnRemovedAsset(AssetsManagerRemovedAssetArgs args)
        {
            RemovedAsset?.Invoke(this, args);
        }

        public event EventHandler<AssetsManagerSwitchingAssetArgs>? SwitchingAsset;
        public virtual void OnSwitchingAsset(AssetsManagerSwitchingAssetArgs args)
        {
            SwitchingAsset?.Invoke(this, args);
        }

        public event EventHandler<AssetsManagerSwitchedAssetArgs>? SwitchedAsset;
        public virtual void OnSwitchedAsset(AssetsManagerSwitchedAssetArgs args)
        {
            SwitchedAsset?.Invoke(this, args);
        }

        public event EventHandler<AssetsManagerUpdatingAssetArgs>? UpdatingAsset;
        public virtual void OnUpdatingAsset(AssetsManagerUpdatingAssetArgs? args = null)
        {
            UpdatingAsset?.Invoke(this, args ?? new(BaseAsset.TYPE.UNKNOWN));
        }
        public event EventHandler<AssetsManagerUpdatedAssetArgs?>? UpdatedAsset;
        public virtual void OnUpdatedAsset(AssetsManagerUpdatedAssetArgs? args = null)
        {
            UpdatedAsset?.Invoke(this, args ?? new(BaseAsset.TYPE.UNKNOWN, null));
        }
        public virtual void OnUpdatedAsset(AssetsManagerUpdatingAssetArgs preArgs)
        {
            var args = preArgs.ToPost();
            UpdatedAsset?.Invoke(this, args);
        }
    }
}
