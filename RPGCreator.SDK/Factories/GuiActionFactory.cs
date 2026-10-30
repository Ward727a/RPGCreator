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
using RPGCreator.SDK.GameUI.Events.Actions;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Factories;

[EngineService(typeof(IGuiActionFactory))]
public class GuiActionFactory(IServiceProvider provider) : IGuiActionFactory
{
    public Result<IGuiAction> CreateAction(Type actionType)
    {
        if (typeof(IGuiAction).IsAssignableFrom(actionType))
        {
            try
            {
                return Result<IGuiAction>.Ok((IGuiAction)ActivatorUtilities.CreateInstance(provider, actionType));
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Error while trying to create GuiAction of type {actionType.Name}!");
                return Result<IGuiAction>.Fail($"Error while trying to create GuiAction of type {actionType.Name}! Exception: {e.Message} - Maybe you have a service that is not registered in the DI container?");
            }
        }

        Logger.Error(new ArgumentException($"Type {actionType.Name} can't be assigned to type IGuiAction."), $"Error while trying to create GuiAction of type {actionType.Name}!");
        return Result<IGuiAction>.Fail($"Type {actionType.Name} can't be assigned to type IGuiAction.");
    }

    public Result<IGuiAction> CreateAction(Type actionType, params object[] args)
    {
        if (typeof(IGuiAction).IsAssignableFrom(actionType))
        {
            try
            {
                return Result<IGuiAction>.Ok((IGuiAction)ActivatorUtilities.CreateInstance(provider, actionType, args));
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Error while trying to create GuiAction of type {actionType.Name}!");
                return Result<IGuiAction>.Fail($"Error while trying to create GuiAction of type {actionType.Name}! Exception: {e.Message} - Maybe you have a service that is not registered in the DI container?");
            }
        }

        Logger.Error(new ArgumentException($"Type {actionType.Name} can't be assigned to type IGuiAction."), $"Error while trying to create GuiAction of type {actionType.Name}!");
        return Result<IGuiAction>.Fail($"Type {actionType.Name} can't be assigned to type IGuiAction.");
    }

    public T CreateAction<T>() where T : IGuiAction
    {
        return ActivatorUtilities.CreateInstance<T>(provider);
    }

    public T CreateAction<T>(params object[] args) where T : IGuiAction
    {
        return ActivatorUtilities.CreateInstance<T>(provider, args);
    }
}