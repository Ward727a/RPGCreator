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
using Avalonia.Controls;
using Avalonia.Controls.Diagnostics;
using Avalonia.Controls.Primitives;

namespace RPGCreator.Core.Types
{
    public static class GlobalStaticUIData
    {
        public static IPopupHostProvider? CurrentContext;

        public static void OpenContext(Control? hostControl)
        {
            if(CurrentContext is Flyout flyout)
            {
                flyout.ShowAt(hostControl);
                return;
            }
            if (CurrentContext is Popup popup)
            {
                popup.Open();
                return;
            }
            if(CurrentContext is ContextMenu contextMenu)
            {
                contextMenu.Open(hostControl);
                return;
            }
        }
        
        public static void CloseContext()
        {
            if(CurrentContext is Flyout flyout)
            {
                flyout.Hide();
                return;
            }
            if (CurrentContext is Popup popup)
            {
                popup.Close();
                CurrentContext = null;
                return;
            }
            if(CurrentContext is ContextMenu contextMenu)
            {
                contextMenu.Close();
                CurrentContext = null;
                return;
            }
        }
    }
}
