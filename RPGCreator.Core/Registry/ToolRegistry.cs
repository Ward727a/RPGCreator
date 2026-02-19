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
using RPGCreator.SDK;
using RPGCreator.SDK.GlobalState;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Registry;
using RPGCreator.SDK.Types;

namespace RPGCreator.Core.Registry;

public class ToolRegistry : IToolRegistry
{
    private IToolState? _toolState;
    private readonly Dictionary<URN, ToolLogic> _tools = new();
    private static readonly ScopedLogger Logger = SDK.Logging.Logger.ForContext<ToolRegistry>();

    private IToolState GetToolState()
    {
        _toolState ??= GlobalStates.ToolState;
        return _toolState;
    }

    public ObservableCollection<ToolLogic> RegisteredTools { get; private set; } = new ObservableCollection<ToolLogic>();

    public void ActivateTool(URN toolUrn)
    {
        var toolState = GetToolState();
        if (toolState.ActiveTool?.ToolUrn == toolUrn)
            return;

        if (!_tools.TryGetValue(toolUrn, out var tool))
        {
            Logger.Warning($"Attempted to activate unregistered tool: {toolUrn}");
            return;
        }

        toolState.ActiveTool = tool;
    }
    
    public void ActivateTool(ToolLogic toolLogic)
    {
        var toolState = GetToolState();
        if (toolState.ActiveTool?.ToolUrn == toolLogic.ToolUrn)
            return;
        GetToolState().ActiveTool = toolLogic;
    }

    public void DeactivateTool(URN toolUrn)
    {
        var toolState = GetToolState();
        if (toolState.ActiveTool?.ToolUrn != toolUrn)
            return;

        toolState.ActiveTool = null;
    }
    
    public void DeactivateTool(ToolLogic toolLogic)
    {
        var toolState = GetToolState();
        if (toolState.ActiveTool?.ToolUrn != toolLogic.ToolUrn)
            return;

        toolState.ActiveTool = null;
    }

    public ToolLogic? GetActiveTool()
    {
        var toolState = GetToolState();
        return toolState.ActiveTool;
    }

    public void RegisterTool(ToolLogic toolLogic, bool overrideIfExists = false)
    {
        if (_tools.ContainsKey(toolLogic.ToolUrn))
        {
            if (!overrideIfExists)
            {
                Logger.Warning($"Attempted to register a tool with an already existing URN: {toolLogic.ToolUrn}");
                return;
            }
            else
            {
                Logger.Info($"Overriding existing tool with URN: {toolLogic.ToolUrn}");
            }
        }

        _tools[toolLogic.ToolUrn] = toolLogic;
        if (!RegisteredTools.Contains(toolLogic))
        {
            RegisteredTools.Add(toolLogic);
        }
    }

    public void UnregisterTool(URN toolUrn)
    {
        if (!_tools.Remove(toolUrn))
        {
            Logger.Warning($"Attempted to unregister a tool with a non-existing URN: {toolUrn}");
        }
    }

    public ToolLogic? GetTool(URN toolUrn)
    {
        if (_tools.TryGetValue(toolUrn, out var tool))
        {
            return tool;
        }
        
        Logger.Warning($"Attempted to get a tool with a non-existing URN: {toolUrn}");
        return null;
    }
    
    public bool HasTool(URN toolUrn)
    {
        return _tools.ContainsKey(toolUrn);
    }
}