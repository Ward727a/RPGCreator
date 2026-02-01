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

namespace RPGCreator.Core.CSharp.Extensions
{

    // TODO: Continue this class, it is not finished yet.

    static public class EventExtensions
    {
        /// <summary>
        /// This method should not be used for now, it is not working as expected.<br/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="handler"></param>
        /// <param name="subscribe"></param>
        /// <param name="unsubscribe"></param>
        static public void SubscribeOnce<T>(this EventHandler<T> source, EventHandler<T> handler, Action<EventHandler<T>> subscribe, Action<EventHandler<T>> unsubscribe)
            where T : EventArgs
        {
            EventHandler<T>? wrapper = null;
            wrapper = (sender, args) =>
            {
                unsubscribe(wrapper);
                handler(sender, args);
            };
            subscribe(wrapper);
        }
        /// <summary>
        /// This method should not be used for now, it is not working as expected.<br/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="handler"></param>
        static public void SubscribeOnce<T>(this EventHandler<T> source, EventHandler<T> handler)
            where T : EventArgs
        {
            EventHandler<T>? wrapper = null;
            wrapper = (sender, args) =>
            {
                source -= wrapper;
                handler(sender, args);
            };
            source += wrapper;
        }
    }
}
