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
using RPGCreator.SDK.GameUI.Controls;
using RPGCreator.SDK.GameUI.Core;
using RPGCreator.SDK.Registry;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib.Registry;

/// <summary>
/// Gui control registry.<br/>
/// This class is used to register and unregister GUI controls.<br/>
/// GUI Controls are used to create the UI of the <b>Game</b> (not the editor!).<br/>
/// This allows the user to be able to use those controls in the game UI editor.
/// </summary>
[EngineService(typeof(IGuiControlRegistry))]
public class GUIControlRegistry(IGuiControlFactory controlFactory) : IGuiControlRegistry
{
    private readonly IGuiControlFactory _controlFactory = controlFactory;
    private readonly Dictionary<URN, GuiControlMeta> _controls = new();

    public int ControlCount => _controls.Count;
    public IReadOnlyCollection<GuiControlMeta> Controls => _controls.Values;

    public Result RegisterControl<T>(bool overwriteIfExists = false) where T : BaseControl
    {
        var metaResult = GetControlMeta(typeof(T));
        if (metaResult.IsFailure)
            return Result.Fail(metaResult.Error);

        if (_controls.ContainsKey(metaResult.Value.Urn) && !overwriteIfExists)
        {
            return Result.Fail($"Control with URN {metaResult.Value.Urn} is already registered.");
        }
        
        _controls[metaResult.Value.Urn] = metaResult.Value;
        
        return Result.Ok();
    }

    public Result UnregisterControl<T>() where T : BaseControl
    {
        var metaResult = GetControlMeta(typeof(T));
        
        if (metaResult.IsFailure)
            return Result.Fail(metaResult.Error);
        
        if (!_controls.Remove(metaResult.Value.Urn))
            return Result.Fail($"Control with URN {metaResult.Value.Urn} is not registered.");

        return Result.Ok();
    }

    public Result ScanAndRegisterControls(Assembly assembly, bool overwriteIfExists = false)
    {
        var controlTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(BaseControl).IsAssignableFrom(t))
            .Where(t => t.GetCustomAttribute<GuiControlMetaAttribute>() != null);

        List<Type> errors = new List<Type>();
        
        foreach (var type in controlTypes)
        {
            var metaResult = GetControlMeta(type);

            if (metaResult.IsFailure)
            {
                errors.Add(type);
                continue;
            }

            if (_controls.ContainsKey(metaResult.Value.Urn) && !overwriteIfExists)
            {
                Logger.Warning($"Control with URN {metaResult.Value.Urn} is already registered.");
                errors.Add(type);
                continue;
            }
            
            _controls[metaResult.Value.Urn] = metaResult.Value;
        }

        return errors.Count > 0 ? Result.Fail($"Failed to register controls:\n- {string.Join("\n- ", errors.Select(t => t.Name))}") : Result.Ok();
    }

    public Result ScanAndUnregisterControls(Assembly assembly)
    {
        var controlTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(BaseControl).IsAssignableFrom(t))
            .Where(t => t.GetCustomAttribute<GuiControlMetaAttribute>() != null);

        List<Type> errors = new List<Type>();
        
        foreach (var type in controlTypes)
        {
            var metaResult = GetControlMeta(type);
            
            if (metaResult.IsFailure)
            {
                errors.Add(type);
                continue;
            }
            
            if (!_controls.Remove(metaResult.Value.Urn))
            {
                Logger.Warning($"Control with URN {metaResult.Value.Urn} is not registered.");
                errors.Add(type);
            }
        }

        return errors.Count > 0 ? Result.Fail($"Failed to unregister controls:\n- {string.Join("\n- ", errors.Select(t => t.Name))}") : Result.Ok();
    }

    public Result ScanAndRegisterControls(IEnumerable<Assembly> assemblies, bool overwriteIfExists = false)
    {
        foreach (var assembly in assemblies)
        {
            var result = ScanAndRegisterControls(assembly, overwriteIfExists);
            if (result.IsFailure)
            {
                return Result.Fail($"Failed to scan assembly {assembly.GetName().Name} for controls. Error: {result.Error}");
            }
        }

        return Result.Ok();
    }

    public Result ScanAndUnregisterControls(IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            var result = ScanAndUnregisterControls(assembly);
            if (result.IsFailure)
            {
                return Result.Fail($"Failed to scan assembly {assembly.GetName().Name} for controls. Error: {result.Error}");
            }
        }

        return Result.Ok();
    }

    public Result<GuiControlMeta> GetControl(URN controlUrn)
    {
        _controls.TryGetValue(controlUrn, out var meta);
        
        if (meta == null)
            return Result<GuiControlMeta>.Fail($"Control with URN {controlUrn} is not registered.");

        return meta;
    }

    public bool HasControl<T>() where T : BaseControl
    {
        return HasControl(typeof(T));
    }

    public bool HasControl(Type controlType)
    {
        var metaResult = GetControlMeta(controlType);
        return metaResult.IsSuccess && _controls.ContainsKey(metaResult.Value.Urn);
    }

    public bool HasControl(URN controlUrn)
    {
        return _controls.ContainsKey(controlUrn);
    }

    public Result<BaseControl> CreateControl(URN controlUrn, params object[] parameters)
    {
        var metaResult = GetControl(controlUrn);
        if (!metaResult.IsSuccess)
            return Result<BaseControl>.Fail(metaResult.Error);

        return _controlFactory.CreateControl(metaResult.Value.ControlType, parameters);
    }

    public void Clear()
    {
        _controls.Clear();
    }

    #region Helpers

    private Result<GuiControlMeta> GetControlMeta(Type controlType)
    {
        var attr = controlType.GetCustomAttribute<GuiControlMetaAttribute>();

        if (attr == null)
        {
            return Result<GuiControlMeta>.Fail("Control type does not have a GuiControlMetaAttribute");
        }

        return Result<GuiControlMeta>.Ok(CreateMeta(attr, controlType));
    }

    private static GuiControlMeta CreateMeta(GuiControlMetaAttribute attribute, Type controlType) => new(attribute.Urn,
        controlType, attribute.DisplayName, attribute.Description, attribute.Icon);

    #endregion
}