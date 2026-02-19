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

using System.Collections.ObjectModel;
using RPGCreator.SDK.GlobalState;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Registry;

public interface IToolRegistry : IService
{
    
    ObservableCollection<ToolLogic> RegisteredTools { get; }
    
    public void ActivateTool(URN toolUrn);
    public void ActivateTool(ToolLogic toolLogic);
    public void DeactivateTool(URN toolUrn);
    public void DeactivateTool(ToolLogic toolLogic);
    public ToolLogic? GetActiveTool();
    
    public void RegisterTool(ToolLogic toolLogic, bool overrideIfExists = false);
    public void UnregisterTool(URN toolUrn);
    public ToolLogic? GetTool(URN toolUrn);
    public bool HasTool(URN toolUrn);
}