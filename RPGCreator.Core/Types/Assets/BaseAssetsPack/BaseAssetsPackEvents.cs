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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGCreator.Core.Types.Assets.BaseAssetsPack.EventsArgs;

namespace RPGCreator.Core.Types.Assets.BaseAssetsPack
{
    public class BaseAssetsPackEvents
    {
        public event EventHandler<BaseAssetsPackAddingArgs>? AssetAdding;
        public virtual void OnAssetAdding(BaseAssetsPackAddingArgs e)
        {
            AssetAdding?.Invoke(this, e);
        }
        public event EventHandler<BaseAssetsPackAddedArgs>? AssetAdded;
        public virtual void OnAssetAdded(BaseAssetsPackAddedArgs e)
        {
            AssetAdded?.Invoke(this, e);
        }
        public event EventHandler<BaseAssetsPackRemovingArgs>? AssetRemoving;
        public virtual void OnAssetRemoving(BaseAssetsPackRemovingArgs e)
        {
            AssetRemoving?.Invoke(this, e);
        }
        public event EventHandler<BaseAssetsPackRemovedArgs>? AssetRemoved;
        public virtual void OnAssetRemoved(BaseAssetsPackRemovedArgs e)
        {
            AssetRemoved?.Invoke(this, e);
        }
        public event EventHandler<BaseAssetsPackGettingArgs>? AssetGetting;
        public virtual void OnAssetGetting(BaseAssetsPackGettingArgs e)
        {
            AssetGetting?.Invoke(this, e);
        }
        public event EventHandler<BaseAssetsPackGotArgs>? AssetGot;
        public virtual void OnAssetGot(BaseAssetsPackGotArgs e)
        {
            AssetGot?.Invoke(this, e);
        }

        public event EventHandler<BaseAssetsPackMovingArgs>? AssetMoving;
        public virtual void OnAssetMoving(BaseAssetsPackMovingArgs e)
        {
            AssetMoving?.Invoke(this, e);
        }

        public event EventHandler<BaseAssetsPackMovedArgs>? AssetMoved;
        public virtual void OnAssetMoved(BaseAssetsPackMovedArgs e)
        {
            AssetMoved?.Invoke(this, e);
        }
    }
}
