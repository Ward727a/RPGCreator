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

using Microsoft.Extensions.DependencyInjection;
using RPGCreator.SDK.AssetsPre;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Registry;
using RPGCreator.SDK.Tools;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib.Registry;

[EngineService(typeof(IToolRegistry))]
public class ToolRegistry : IToolRegistry
{
    private readonly IServiceProvider _provider;
    private readonly Dictionary<URN, Tool> _tools = new();
    private readonly Dictionary<URN, IToolLogic> _toolLogics = new();
    private static readonly ScopedLogger Logger = SDK.Common.Logging.Logger.ForContext<ToolRegistry>();

    public ToolRegistry(IServiceProvider provider)
    {
        _provider = provider;
    }

    public IEnumerable<Tool> RegisteredTools =>_tools.Values;
    public IEnumerable<IToolLogic> RegisteredToolLogics =>_toolLogics.Values;

    public void RegisterToolLogic(URN targetLayerUrn, Type toolLogicType, bool overrideIfExists = false)
    {
        if (ActivatorUtilities.CreateInstance(_provider, toolLogicType) is not IToolLogic toolLogic)
        {
            Logger.Error("Tool logic {logicType} for layer {layerUrn} is not of type IToolLogic", args: [toolLogicType, targetLayerUrn]);
            return;
        }

        if (_toolLogics.ContainsKey((targetLayerUrn)) && !overrideIfExists)
        {
            Logger.Warning("Tool logic {logicType} for layer {layerUrn} already registered", args: [toolLogicType, targetLayerUrn]);
            return;
        }
        
        _toolLogics[targetLayerUrn] = toolLogic;
        Logger.Info("Registered tool logic {logicType} for layer {layerUrn}.", args: [toolLogicType, targetLayerUrn]);
    }

    public void UnregisterToolLogic(URN targetLayerUrn)
    {
        _toolLogics.Remove(targetLayerUrn);
    }

    public void RegisterTool(URN toolUrn, Type toolLogic, bool overrideIfExists = false)
    {
        if (ActivatorUtilities.CreateInstance(_provider, toolLogic) is not Tool tool)
        {
            Logger.Error("Tool {toolUrn} of type {toolType} is not of type Tool", args: [toolUrn, toolLogic]);
            return;
        }

        if (_tools.ContainsKey(toolUrn) && !overrideIfExists)
        {
            Logger.Warning("Tool {toolUrn} already registered", args: toolUrn);
            return;
        }

        tool.ToolUrn = toolUrn;
        _tools[toolUrn] = tool;
        Logger.Info("Registered tool {toolUrn}.", args: toolUrn);
    }

    public void UnregisterTool(URN toolUrn)
    {
        _tools.Remove(toolUrn);
    }

    public IToolLogic? GetToolLogic(URN targetLayerUrn)
    {
        return _toolLogics.GetValueOrDefault(targetLayerUrn);
    }
    
    public Tool? GetTool(URN toolUrn)
    {
        return _tools.GetValueOrDefault(toolUrn);
    }
    
    public bool HasTool(URN toolUrn)
    {
        return _tools.ContainsKey(toolUrn);
    }
    
    public bool HasToolLogic(URN targetLayerUrn)
    {
        return _toolLogics.ContainsKey(targetLayerUrn);
    }
    
    #region Helpers
    public URN GetToolUrn(ITypesRegistry registryDiscriminator, Type type)
    {
        var result = registryDiscriminator.GetKey(type);
        if (result.IsFailure)
        {
            Logger.Error("Failed to get tool urn for type {type}", args: type);
            return URN.Empty;
        }
        return result.Value;
    }
    #endregion
}