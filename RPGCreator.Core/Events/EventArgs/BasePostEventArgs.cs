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
using RPGCreator.Core.Types.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.Core.Events.EventArgs
{
    public abstract class BasePostEventArgs<T> : BaseEventArgs where T : BasePostEventArgs<T>
    {
        /// <summary>
        /// If an error occurs, this is set to true. This is still a work in progress, but it's slowly being implemented.
        /// </summary>
        public BaseError? Error;
        public bool HasError => Error != null;

        public T SetError(bool error)
        {
            return ((T)this).SetError(error, "", "");
        }

        public T SetError(bool error, string message)
        {
            return ((T)this).SetError(error, message, "");
        }

        public T SetError(bool error, string message, string origin)
        {
            if (error)
            {
                Error = new BaseError(message, origin);
                if(EngineCore.Instance.Events != null) // TODO : In case the EngineCore.Configs get an error, Events hasn't been initialized yet.
                    EngineCore.Instance.Events.OnEngineError(new(Error));
            }
            else
                Error = null;


            return (T)this;
        }
    }
}
