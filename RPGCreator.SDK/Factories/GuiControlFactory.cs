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
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.GameUI.Controls;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Factories;

[EngineService(typeof(IGuiControlFactory))]
public class GuiControlFactory(IServiceProvider provider) : IGuiControlFactory
{
    public Result<BaseControl> CreateControl(Type controlType, params object[] parameters)
    {
        if (typeof(BaseControl).IsAssignableFrom(controlType))
        {
            try
            {
                return (BaseControl)ActivatorUtilities.CreateInstance(provider, controlType, parameters);
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Error while trying to create control of type {controlType.Name}!");
                return Result<BaseControl>.Fail($"Error while trying to create control of type {controlType.Name}! Exception: {e.Message} - Maybe you have a service that is not registered in the DI container?");
            }
        }

        Logger.Error(new ArgumentException($"Type {controlType.Name} can't be assigned to type BaseControl."), $"Error while trying to create control of type {controlType.Name}!");
        return Result<BaseControl>.Fail($"Type {controlType.Name} can't be assigned to type BaseControl.");
    }

    public Result<BaseControl> CreateControl(Type controlType)
    {
        if (typeof(BaseControl).IsAssignableFrom(controlType))
        {
            try
            {
                return (BaseControl)ActivatorUtilities.CreateInstance(provider, controlType);
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Error while trying to create control of type {controlType.Name}!");
                return Result<BaseControl>.Fail($"Error while trying to create control of type {controlType.Name}! Exception: {e.Message} - Maybe you forgot to add a parameterless constructor or you have a service that is not registered in the DI container?");
            }
        }

        Logger.Error(new ArgumentException($"Type {controlType.Name} can't be assigned to type BaseControl."), $"Error while trying to create control of type {controlType.Name}!");
        return Result<BaseControl>.Fail($"Type {controlType.Name} can't be assigned to type BaseControl.");
    }
    
    public T CreateControl<T>() where T : BaseControl
    {
        return ActivatorUtilities.CreateInstance<T>(provider);
    }
    
    public T CreateControl<T>(params object[] parameters) where T : BaseControl
    {
        return ActivatorUtilities.CreateInstance<T>(provider, parameters);
    }
}