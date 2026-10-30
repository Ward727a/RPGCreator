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
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Factories;
using RPGCreator.SDK.GameUI.Core;
using RPGCreator.SDK.GameUI.Events.Actions;
using RPGCreator.SDK.Registry;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib.Registry;

[EngineService(typeof(IGuiActionRegistry))]
public class GuiActionRegistry(IGuiActionFactory actionFactory) : IGuiActionRegistry
{
    private readonly Dictionary<URN, GuiActionMeta> _actions = new();
    private readonly Dictionary<Type, List<GuiActionMeta>> _actionTypes = new();
    
    public int ActionCount => _actions.Count;
    public IReadOnlyCollection<GuiActionMeta> Actions => _actions.Values;
    public Result RegisterAction<T>(bool overwriteIfExists = false) where T : IGuiAction
    {
        var metaResult = GetActionMeta(typeof(T));
        if (metaResult.IsFailure)
            return Result.Fail(metaResult.Error);

        if (_actions.ContainsKey(metaResult.Value.Urn) && !overwriteIfExists)
        {
            return Result.Fail($"Action with URN {metaResult.Value.Urn} is already registered.");
        }

        if (_actions.TryGetValue(metaResult.Value.Urn, out var registeredAction))
        {
            foreach (var contextType in registeredAction.SupportedContexts)
            {
                _actionTypes[contextType].Remove(registeredAction);
                if (_actionTypes[contextType].Count == 0)
                    _actionTypes.Remove(contextType);
            }
        }
        
        _actions[metaResult.Value.Urn] = metaResult.Value;

        foreach (var contextType in metaResult.Value.SupportedContexts)
        {
            if (!_actionTypes.ContainsKey(contextType))
                _actionTypes[contextType] = new();
            
            _actionTypes[contextType].Add(metaResult.Value);
        }
        
        return Result.Ok();
    }

    public Result UnregisterAction<T>() where T : IGuiAction
    {
        var metaResult = GetActionMeta(typeof(T));
        if (metaResult.IsFailure)
            return Result.Fail(metaResult.Error);
        
        if (!_actions.Remove(metaResult.Value.Urn))
            return Result.Fail($"Action with URN {metaResult.Value.Urn} is not registered.");
        
        foreach (var contextType in metaResult.Value.SupportedContexts)
        {
            _actionTypes[contextType].Remove(metaResult.Value);
            if (_actionTypes[contextType].Count == 0)
                _actionTypes.Remove(contextType);
        }

        return Result.Ok();
    }

    public Result ScanAndRegisterActions(Assembly assembly, bool overwriteIfExists = false)
    {
        var actionsType = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IGuiAction).IsAssignableFrom(t))
            .Where(t => t.GetCustomAttribute<GuiActionMetaAttribute>() != null);

        List<Type> errors = new List<Type>();
        
        foreach (var type in actionsType)
        {
            var metaResult = GetActionMeta(type);
            if (metaResult.IsFailure)
            {
                errors.Add(type);
                Logger.Warning($"Failed to register action {type.Name}: {metaResult.Error}");
                continue;
            }

            if (_actions.ContainsKey(metaResult.Value.Urn) && !overwriteIfExists)
            {
                errors.Add(type);
                Logger.Warning($"Action with URN {metaResult.Value.Urn} is already registered.");
                continue;
            }

            if (_actions.TryGetValue(metaResult.Value.Urn, out var registeredAction))
            {
                foreach (var contextType in registeredAction.SupportedContexts)
                {
                    _actionTypes[contextType].Remove(registeredAction);
                    if (_actionTypes[contextType].Count == 0)
                        _actionTypes.Remove(contextType);
                }
            }
            
            _actions[metaResult.Value.Urn] = metaResult.Value;

            foreach (var contextType in metaResult.Value.SupportedContexts)
            {
                if (!_actionTypes.ContainsKey(contextType))
                    _actionTypes[contextType] = new();
            
                _actionTypes[contextType].Add(metaResult.Value);
            }
        }

        return errors.Count > 0 ? Result.Fail($"Failed to register actions:\n- {string.Join("\n- ", errors.Select(t => t.Name))}") : Result.Ok();
    }

    public Result ScanAndUnregisterActions(Assembly assembly)
    {
        var actionsType = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IGuiAction).IsAssignableFrom(t))
            .Where(t => t.GetCustomAttribute<GuiActionMetaAttribute>() != null);

        List<Type> errors = new List<Type>();
        
        foreach (var type in actionsType)
        {
            var metaResult = GetActionMeta(type);
            
            if (metaResult.IsFailure)
            {
                errors.Add(type);
                Logger.Warning($"Failed to unregister action {type.Name}: {metaResult.Error}");
                continue;
            }
            
            if (!_actions.Remove(metaResult.Value.Urn))
            {
                Logger.Warning($"Action with URN {metaResult.Value.Urn} is not registered.");
                errors.Add(type);
                continue;
            }
            
            foreach (var contextType in metaResult.Value.SupportedContexts)
            {
                _actionTypes[contextType].Remove(metaResult.Value);
                if (_actionTypes[contextType].Count == 0)
                    _actionTypes.Remove(contextType);
            }
        }

        return errors.Count > 0 ? Result.Fail($"Failed to unregister actions:\n- {string.Join("\n- ", errors.Select(t => t.Name))}") : Result.Ok();
    }

    public Result ScanAndRegisterActions(IEnumerable<Assembly> assemblies, bool overwriteIfExists = false)
    {
        foreach (var assembly in assemblies)
        {
            var result = ScanAndRegisterActions(assembly, overwriteIfExists);
            if (result.IsFailure)
            {
                return Result.Fail($"Failed to scan assembly {assembly.GetName().Name} for actions. Error: {result.Error}");
            }
        }

        return Result.Ok();
    }

    public Result ScanAndUnregisterActions(IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            var result = ScanAndUnregisterActions(assembly);
            if (result.IsFailure)
            {
                return Result.Fail($"Failed to scan assembly {assembly.GetName().Name} for actions. Error: {result.Error}");
            }
        }

        return Result.Ok();
    }

    public Result<GuiActionMeta> GetAction(URN actionUrn)
    {
        _actions.TryGetValue(actionUrn, out var meta);
        
        if (meta == null)
            return Result<GuiActionMeta>.Fail($"Action with URN {actionUrn} is not registered.");

        return Result<GuiActionMeta>.Ok(meta);
    }

    public bool HasAction<T>() where T : IGuiAction
    {
        return HasAction(typeof(T));
    }

    public bool HasAction(Type actionType)
    {
        var metaResult = GetActionMeta(actionType);
        return metaResult.IsSuccess && _actions.ContainsKey(metaResult.Value.Urn);
    }

    public bool HasAction(URN actionUrn)
    {
        return _actions.ContainsKey(actionUrn);
    }

    public Result<IGuiAction> CreateAction(URN actionUrn)
    {
        var metaResult = GetAction(actionUrn);
        
        if (metaResult.IsFailure)
            return Result<IGuiAction>.Fail(metaResult.Error);
        
        return actionFactory.CreateAction(metaResult.Value.ActionType);
    }

    public Result<IGuiAction> CreateAction(URN actionUrn, params object[] parameters)
    {
        var metaResult = GetAction(actionUrn);
        
        if (metaResult.IsFailure)
            return Result<IGuiAction>.Fail(metaResult.Error);
        
        return actionFactory.CreateAction(metaResult.Value.ActionType, parameters);
    }

    public T CreateAction<T>() where T : IGuiAction
    {
        return actionFactory.CreateAction<T>();
    }

    public T CreateAction<T>(params object[] args) where T : IGuiAction
    {
        return actionFactory.CreateAction<T>(args);
    }
    
    #region Helpers

    private Result<GuiActionMeta> GetActionMeta(Type actionType)
    {
        var attr = actionType.GetCustomAttribute<GuiActionMetaAttribute>();

        if (attr == null)
        {
            return Result<GuiActionMeta>.Fail("Action type does not have a GuiActionMetaAttribute");
        }

        return Result<GuiActionMeta>.Ok(CreateMeta(attr, actionType));
    }

    private GuiActionMeta CreateMeta(GuiActionMetaAttribute attribute, Type actionType)
    {
        return new(attribute.Urn,
            actionType, attribute.DisplayName, attribute.Description, attribute.Icon, attribute.SupportedContexts);
    }

    #endregion
}