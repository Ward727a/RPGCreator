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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hexa.NET.ImGui;
using Microsoft.Extensions.DependencyInjection;
using RPGCreator.SDK.ActorFeatures;
using RPGCreator.SDK.Assets.Actors;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Enums;

namespace RPGCreator.Editor.Handlers;

[EngineService(typeof(IActorFeatureDrawerRegistry), ServiceLifetime.Singleton, EngineModeFlags.Editor)]
public class ActorFeatureDrawerRegistry(IServiceProvider serviceProvider) : IActorFeatureDrawerRegistry
{
    private readonly Dictionary<Type, IActorFeatureDrawer> _drawers = new();

    public void RegisterDrawer(Type featureType, Type drawerType)
    {
        if (!typeof(IActorFeatureDrawer).IsAssignableFrom(drawerType))
        {
            Logger.Error("Type {0} does not implement IActorFeatureDrawer.", drawerType.Name);
            return;
        }

        if (ActivatorUtilities.CreateInstance(serviceProvider, drawerType) is IActorFeatureDrawer drawerInstance)
        {
            _drawers[featureType] = drawerInstance;
        }
    }

    public void UnregisterDrawer(Type featureType)
    {
        _drawers.Remove(featureType);
    }

    public bool DrawFeature(IActorFeatureData feature)
    {
        var featureType = feature.GetType();
        if (_drawers.TryGetValue(featureType, out var drawer))
        {
            return drawer.Draw(feature);
        }

        // Fallback: Generic reflection inspector if the modder didn't provide a custom drawer
        return DrawGenericFallback(feature);
    }

    private static bool DrawGenericFallback(IActorFeatureData feature)
    {
        bool changed = false;
        var properties = feature.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0);

        foreach (var prop in properties)
        {
            var val = prop.GetValue(feature);

            if (prop.PropertyType == typeof(int) && val is int intVal)
            {
                if (ImGui.InputInt(prop.Name, ref intVal))
                {
                    prop.SetValue(feature, intVal);
                    changed = true;
                }
            }
            else if (prop.PropertyType == typeof(byte) && val is byte byteVal)
            {
                int temp = byteVal;
                if (ImGui.SliderInt(prop.Name, ref temp, 0, 255))
                {
                    prop.SetValue(feature, (byte)Math.Clamp(temp, 0, 255));
                    changed = true;
                }
            }
            else if (prop.PropertyType == typeof(float) && val is float floatVal)
            {
                if (ImGui.DragFloat(prop.Name, ref floatVal, 1f))
                {
                    prop.SetValue(feature, floatVal);
                    changed = true;
                }
            }
            else if (prop.PropertyType == typeof(double) && val is double doubleVal)
            {
                if (ImGui.InputDouble(prop.Name, ref doubleVal))
                {
                    prop.SetValue(feature, doubleVal);
                    changed = true;
                }
            }
            else if (prop.PropertyType == typeof(bool) && val is bool boolVal)
            {
                if (ImGui.Checkbox(prop.Name, ref boolVal))
                {
                    prop.SetValue(feature, boolVal);
                    changed = true;
                }
            }
            else if (prop.PropertyType == typeof(string))
            {
                string strVal = (val as string) ?? string.Empty;
                if (ImGui.InputText(prop.Name, ref strVal, 256))
                {
                    prop.SetValue(feature, strVal);
                    changed = true;
                }
            }
            else if (prop.PropertyType.IsEnum && val != null)
            {
                if (ImGui.BeginCombo(prop.Name, val.ToString()))
                {
                    foreach (var enumVal in Enum.GetValues(prop.PropertyType))
                    {
                        if (ImGui.Selectable(enumVal.ToString(), enumVal.Equals(val)))
                        {
                            prop.SetValue(feature, enumVal);
                            changed = true;
                        }
                    }
                    ImGui.EndCombo();
                }
            }
        }

        return changed;
    }
}