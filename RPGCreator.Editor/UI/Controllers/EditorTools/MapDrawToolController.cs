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
using RPGCreator.Editor.Tools;
using RPGCreator.Editor.UI.Models.EditorTools;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.Registry;
using RPGCreator.SDK.Tools;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.Editor.UI.Controllers.EditorTools;

[EditorUiController]
public class MapDrawToolController(MapDrawToolModel state, IToolRegistry toolRegistry, ToolManager toolManager)
{
    IEditorUiStoreContext Store;
    public const string SelectedToolKey = "selected_tool";

    public void Init(IEditorUiStoreContext store)
    {
        Store = store;
        State.SelectedTool = store.GetOrDefault(SelectedToolKey, URN.Empty);
        
        if(State.SelectedTool != URN.Empty)
            toolManager.SelectTool(State.SelectedTool);
    }
    
    public readonly MapDrawToolModel State = state;

    public IEnumerable<Tool> GetTools()
    {
        return toolRegistry.RegisteredTools;
    }
    
    private URN _lastFailedAttemptUrn = URN.Empty;
    
    public Tool? GetTool(URN toolUrn)
    {
        if(toolUrn == _lastFailedAttemptUrn)
            return null;

        var returnValue = toolRegistry.GetTool(toolUrn);
        
        if(returnValue == null)
            _lastFailedAttemptUrn = toolUrn;

        return returnValue;
    }

    public void SelectTool(URN toolUrn)
    {
        State.SelectedTool = toolUrn;
        Store.AddOrEdit(SelectedToolKey, State.SelectedTool);
        toolManager.SelectTool(State.SelectedTool);
    }
}