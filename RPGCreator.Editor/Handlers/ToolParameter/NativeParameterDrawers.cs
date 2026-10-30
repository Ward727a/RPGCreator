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
using Hexa.NET.ImGui;
using RPGCreator.SDK.Tools;
using RPGCreator.SDK.Tools.ParameterTypes;

namespace RPGCreator.Editor.Handlers.ToolParameter;

public static class NativeParameterDrawers
{
    [ToolParameterDrawer(typeof(IntParameter))]
    public static void DrawInt(IntParameter parameter)
    {
        if (ImGui.InputInt(parameter.DisplayName, ref parameter.ValueRef, parameter.Step))
        {
            parameter.ValueRef = Math.Clamp(parameter.ValueRef, parameter.Min, parameter.Max);
        }
        if (!string.IsNullOrWhiteSpace(parameter.Description))
        {
            ImGui.SetItemTooltip(parameter.Description);
        }
    }

    [ToolParameterDrawer(typeof(FloatParameter))]
    public static void DrawFloat(FloatParameter parameter)
    {
        if (ImGui.InputFloat(parameter.DisplayName, ref parameter.ValueRef, parameter.Step))
        {
            parameter.ValueRef = Math.Clamp(parameter.ValueRef, parameter.Min, parameter.Max);
        }
        if (!string.IsNullOrWhiteSpace(parameter.Description))
        {
            ImGui.SetItemTooltip(parameter.Description);
        }
    }
    
    [ToolParameterDrawer(typeof(DoubleParameter))]
    public static void DrawDouble(DoubleParameter parameter)
    {
        if (ImGui.InputDouble(parameter.DisplayName, ref parameter.ValueRef, parameter.Step))
        {
            parameter.ValueRef = Math.Clamp(parameter.ValueRef, parameter.Min, parameter.Max);
        }
        if (!string.IsNullOrWhiteSpace(parameter.Description))
        {
            ImGui.SetItemTooltip(parameter.Description);
        }
    }
    
    [ToolParameterDrawer(typeof(BoolParameter))]
    public static void DrawBool(BoolParameter parameter)
    {
        ImGui.Checkbox(parameter.DisplayName, ref parameter.ValueRef);
        if (!string.IsNullOrWhiteSpace(parameter.Description))
        {
            ImGui.SetItemTooltip(parameter.Description);
        }
    }
    
    [ToolParameterDrawer(typeof(StringParameter))]
    public static void DrawString(StringParameter parameter)
    {
        ImGui.InputText(parameter.DisplayName, ref parameter.ValueRef, parameter.MaxLength);
        if (!string.IsNullOrWhiteSpace(parameter.Description))
        {
            ImGui.SetItemTooltip(parameter.Description);
        }
    }
}