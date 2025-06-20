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
using Avalonia.Controls;
using RPGCreator.Modals;
using RPGCreator.UI.OLD.Views.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.Core.Services
{

    public interface IModalService
    {
        void ShowError(string message, string title = "Error!");
    }

    public class ModalService : IModalService
    {
        private readonly Window _parent;

        public ModalService(Window parentWindow)
        {
            _parent = parentWindow;
        }

        public void ShowError(string message, string title = "Error!")
        {
            ErrorDialog dialog = new(title, message);
            if (_parent.IsVisible)
                dialog.ShowDialog(_parent);
            else
                _parent.Loaded += (_, _) =>
                {
                    dialog.ShowDialog(_parent);
                };
        }
        
        public void ShowCustom<T>(Dictionary<string, object> args) where T : ModalBase<T>
        {
            var createMethod = typeof(T).GetMethod("Create", [typeof(Dictionary<string, object>)]);

            if (createMethod == null || !createMethod.IsStatic)
            {
                throw new InvalidOperationException($"{typeof(T).Name} must have a static Create method.");
            }
#pragma warning disable CS8600
            T? modal = (T)createMethod.Invoke(
                null,
                [args]
                );
#pragma warning restore CS8600

            if (_parent.IsVisible)
                modal?.ShowDialog(_parent);
            else
                _parent.Loaded += (_, _) =>
                {
                    modal?.ShowDialog(_parent);
                };
        }
    }
}
