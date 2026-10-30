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
using RPGCreator.SDK.GameUI.Core;
using RPGCreator.SDK.GameUI.Events.Actions;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Registry;

public interface IGuiActionRegistry
{
    public int ActionCount { get; }
    public IReadOnlyCollection<GuiActionMeta> Actions { get; }
    
    public Result RegisterAction<T>(bool overwriteIfExists = false) where T : IGuiAction;
    public Result UnregisterAction<T>() where T : IGuiAction;
    
    public Result ScanAndRegisterActions(Assembly assembly, bool overwriteIfExists = false);
    public Result ScanAndUnregisterActions(Assembly assembly);
    
    public Result ScanAndRegisterActions(IEnumerable<Assembly> assemblies, bool overwriteIfExists = false);
    public Result ScanAndUnregisterActions(IEnumerable<Assembly> assemblies);
    
    public Result<GuiActionMeta> GetAction(URN actionUrn);
    
    public bool HasAction<T>() where T : IGuiAction;
    public bool HasAction(Type actionType);
    public bool HasAction(URN actionUrn);
    
    public Result<IGuiAction> CreateAction(URN actionUrn);
    public Result<IGuiAction> CreateAction(URN actionUrn, params object[] parameters);
    public T CreateAction<T>() where T : IGuiAction;
    public T CreateAction<T>(params object[] args) where T : IGuiAction;
}