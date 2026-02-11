// RPG Creator - Open-source RPG Engine.
// (c) 2026 Ward
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

using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using RPGCreator.SDK.EditorUiService;
using RPGCreator.SDK.Logging;

namespace RPGCreator.UI.Services;

public class MenuService : IMenuService
{
    private static readonly ScopedLogger Logger = SDK.Logging.Logger.ForContext<MenuService>();
    public void OpenContextMenu(object host, IEnumerable<MenuAction> actions)
    {
        if (host is not Control hostControl) return;

        var menu = new ContextMenu();
        foreach (var action in actions)
        {
            menu.Items.Add(CreateMenuItem(action));
        }

        menu.Open(hostControl);
    }

    private Control CreateMenuItem(MenuAction action)
    {
        if (action.IsSeparator)
        {
            return new Separator();
        }
        
        var item = new MenuItem { Header = action.Header };

        if (action.SubMenu != null && action.SubMenu.Any())
        {
            foreach (var subAction in action.SubMenu)
            {
                item.Items.Add(CreateMenuItem(subAction));
            }
        }
        else
        {
            item.Click += (_, _) => action.Command?.Invoke();
        }

        return item;
    }
    
    public void OpenContextMenu(object host, object control)
    {
        if (host is not Control hostControl) return;

        switch (control)
        {
            case ContextMenu contextMenu:
                contextMenu.Open(hostControl);
                return;
            case IEnumerable<MenuAction> actions:
            {
                var menu = new ContextMenu();
                foreach (var action in actions)
                {
                    menu.Items.Add(CreateMenuItem(action));
                }
                menu.Open(hostControl);
                return;
            }
            case MenuAction singleAction:
            {
                var menu = new ContextMenu();
                menu.Items.Add(CreateMenuItem(singleAction));
                menu.Open(hostControl);
                return;
            }
            default:
                Logger.Warning("MenuService: The provided control object is not a supported menu type.");
                break;
        }
    }
}