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

using System.Reflection;
using Avalonia.Controls;
using Avalonia.Input;
using RPGCreator.SDK.Graph.LOGIC;

namespace RPGCreator.UI.Content.Blueprint;

/// <summary>
/// Because Avalonia doesn't support custom data formats,
/// and their method is private, we have to use reflection to create our own.
/// </summary>
public static class DragDropCustomFormats
{
    public static readonly DataFormat<INodeLogic> NodeLogicFormat = CreateCustomFormat<INodeLogic>("rpgcreator.nodelogic");
    public static readonly DataFormat<TreeViewItem> TreeViewItemFormat = CreateCustomFormat<TreeViewItem>("rpgcreator.treeviewitem");

    private static DataFormat<T> CreateCustomFormat<T>(string identifier) where T : class
    {
        var method = typeof(DataFormat).GetMethod("CreateApplicationFormat", 
            BindingFlags.Static | BindingFlags.NonPublic);

        var genericMethod = method?.MakeGenericMethod(typeof(T));

        return (DataFormat<T>)genericMethod?.Invoke(null, new object[] { identifier })!;
    }
}