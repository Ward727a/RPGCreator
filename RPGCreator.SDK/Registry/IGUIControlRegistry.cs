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
using RPGCreator.SDK.GameUI.Controls;
using RPGCreator.SDK.GameUI.Core;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Registry;

public interface IGuiControlRegistry
{
    public int ControlCount { get; }
    public IReadOnlyCollection<GuiControlMeta> Controls { get; }
    
    public Result RegisterControl<T>(bool overwriteIfExists = false) where T : BaseControl;
    public Result UnregisterControl<T>() where T : BaseControl;
    
    public Result ScanAndRegisterControls(Assembly assembly, bool overwriteIfExists = false);
    public Result ScanAndUnregisterControls(Assembly assembly);
    
    public Result ScanAndRegisterControls(IEnumerable<Assembly> assemblies, bool overwriteIfExists = false);
    public Result ScanAndUnregisterControls(IEnumerable<Assembly> assemblies);
    
    public Result<GuiControlMeta> GetControl(URN controlUrn);
    
    public bool HasControl<T>() where T : BaseControl;
    public bool HasControl(Type controlType);
    public bool HasControl(URN controlUrn);
    
    public Result<BaseControl> CreateControl(URN controlUrn, params object[] parameters);

    public void Clear();
}