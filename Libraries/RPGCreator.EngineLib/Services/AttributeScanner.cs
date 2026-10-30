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
using RPGCreator.SDK.Services.EngineService;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Enums;
using RPGCreator.Shared.Interfaces;

namespace RPGCreator.EngineLib.Services;

[EngineService(typeof(IAttributeScanner))]
public class AttributeScanner : IAttributeScanner
{
    private readonly IServiceProvider _provider;

    public AttributeScanner(IServiceProvider provider)
    {
        _provider = provider;
    }

    public void AutoScan(Assembly[] assemblies, EngineModeFlags modeFlags)
    {
        var attributeToOrder = new List<(int Order, Type Type)>();
        
        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface || !type.IsSubclassOf(typeof(Attribute))) continue;

                var attributes = type.GetCustomAttributes<EngineAttrAttribute>();
                foreach (var attr in attributes)
                {
                    if (!attr.Mode.HasFlag(modeFlags) || !attr.AutoScan)
                        continue;
                    
                    attributeToOrder.Add((attr.Order, type));
                    Logger.Info($"Found attribute {type.Name} in assembly {assembly.FullName} with order {attr.Order}");
                }
            }
        }

        attributeToOrder.Sort((a, b) => a.Order.CompareTo(b.Order));

        foreach (var (_, typeAttribute) in attributeToOrder)
        {
            foreach (var assembly in assemblies)
            {
                Logger.Info($"Processing attribute {typeAttribute.Name} in assembly {assembly.FullName}");
                ScanAndProcess(typeAttribute, assembly);
            }
        }
    }

    public void ScanAndProcess(Type attributeType, Assembly assembly)
    {
        var handlerServiceType = typeof(IAttributeHandler<>).MakeGenericType(attributeType);
        if (_provider.GetService(handlerServiceType) is IAttributeHandler handler)
        {
            ForHandler(attributeType, handler, assembly);
            return;
        }

        var scannerServiceType = typeof(IAttributeScannerHandler<>).MakeGenericType(attributeType);
        if (_provider.GetService(scannerServiceType) is IAttributeScannerHandler scannerHandler)
        {
            scannerHandler.Scan(assembly);
            return;
        }
    }
    
    public void ScanAndProcess<TAttribute>(Assembly assembly)
        where TAttribute : Attribute
        => ScanAndProcess(typeof(TAttribute), assembly);

    private bool IsHandler<TAttribute>() where TAttribute : Attribute
    {
        return _provider.GetService<IAttributeHandler<TAttribute>>() != null;
    }
    
    private bool IsScanner<TAttribute>() where TAttribute : Attribute
    {
        return _provider.GetService<IAttributeScannerHandler<TAttribute>>() != null;
    }

    private void ForScannerHandler<TAttribute>(Assembly assembly) 
        where TAttribute : Attribute
    {
        var handler = _provider.GetService<IAttributeScannerHandler<TAttribute>>();
        if (handler == null)
        {
            Logger.Error("Could not find attribute scanner handler for attribute {Attribute}", typeof(TAttribute));
            return;
        }

        handler.Scan(assembly);
    }
    
    private void ForHandler(Type attributeType, IAttributeHandler handler, Assembly assembly)
    {
        Type[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            Logger.Error("Could not load types from assembly {Assembly}: {Exception}", assembly.FullName ?? "Unknown", ex);
            return;
        }

        handler.BeforeHandle();
        foreach (var type in types)
        {
            var attributes = type.GetCustomAttributes(attributeType);
            foreach (var attribute in attributes)
            {
                handler.Handle(type, attribute);
            }
        }
        handler.AfterHandle();
    }
    
    private void ForHandler<TAttribute>(Assembly assembly) 
        where TAttribute : Attribute
    {
        
        var handler = _provider.GetService<IAttributeHandler<TAttribute>>();
        if (handler == null)
        {
            Logger.Error("Could not find attribute handler for attribute {Attribute}", typeof(TAttribute));
            return;
        }

        Type[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            Logger.Error("Could not load types from assembly {Assembly}: {Exception}", assembly.FullName ?? "Unknown", ex);
            return;
        }
        
        handler.BeforeHandle();
        foreach (var type in types)
        {
            var attributes = type.GetCustomAttributes<TAttribute>();
            foreach (var attribute in attributes)
            {
                handler.Handle(type, attribute);
            }
        }
        handler.AfterHandle();
    }
}