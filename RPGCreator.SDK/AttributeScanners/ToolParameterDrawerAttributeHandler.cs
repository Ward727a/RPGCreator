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
using Microsoft.Extensions.DependencyInjection;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Tools;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Enums;
using RPGCreator.Shared.Interfaces;

namespace RPGCreator.SDK.AttributeScanners;

[EngineService(typeof(IAttributeScannerHandler<ToolParameterDrawerAttribute>), ServiceLifetime.Singleton, EngineModeFlags.Editor)]
public class ToolParameterDrawerAttributeHandler(IToolParameterUiRegistry toolParameterUiRegistry) : IAttributeScannerHandler<ToolParameterDrawerAttribute>
{
    private static readonly ScopedLogger Logger =
        Common.Logging.Logger.ForContext<ToolParameterDrawerAttributeHandler>();
    public void Scan(Assembly assembly)
    {
        var methods = assembly.GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            .Where(m => m.GetCustomAttribute<ToolParameterDrawerAttribute>() != null);

        var baseRegisterMethod = typeof(IToolParameterUiRegistry)
            .GetMethods()
            .First(m => m.Name == "Register" && m.GetParameters().Length == 1);

        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<ToolParameterDrawerAttribute>();
            Type paramType = attribute!.ParameterType;

            Type actionType = typeof(Action<>).MakeGenericType(paramType);
            Delegate compiledDelegate = Delegate.CreateDelegate(actionType, method);

            MethodInfo typedRegisterMethod = baseRegisterMethod.MakeGenericMethod(paramType);
            typedRegisterMethod.Invoke(toolParameterUiRegistry, [compiledDelegate]);
            
            Logger.Debug($"Registered drawer: {paramType.FullName}");
        }
    }
}