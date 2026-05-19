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

using System.Text.Json.Serialization;
using CommunityToolkit.HighPerformance.Buffers;
using RPGCreator.SDK.GameUI;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Graph;

public record BlueprintParameters(
    Ulid Id,
    string Name,
    URN TypeUrn,
    // Define if the parameter has been set as readonly and then can't be modified by a node.
    bool IsReadOnly = false,
    // Define if the parameter has been added by the system (aka the engine) and then can't be deleted by the user.
    bool BySystem = false,
    // Define if the parameter is an argument of the blueprint. In general, only the system can create this.
    bool IsArgument = false
)
{
    public string Name { get; set; } = Name;
    public bool IsReadOnly { get; set; } = IsReadOnly;
    public object? DefaultValue { get; set; } = null;
    
    [JsonIgnore]
    public Type Type => RegistryServices.Types.GetType(TypeUrn).Value ?? throw new Exception($"Type not found: {TypeUrn}");

    [JsonIgnore]
    public string TypeDisplayName
    {
        get
        {
            return Type switch
            {
                _ when Type == typeof(string) => "String",
                _ when Type == typeof(float) => "Float",
                _ when Type == typeof(bool) => "Bool",
                _ when Type == typeof(int) => "Int",
                _ => "Unknown"
            };
        }
    }

    [JsonIgnore]
    public string TypeSystemName
    {
        get
        {
            return Type switch
            {
                _ when Type == typeof(string) => "string",
                _ when Type == typeof(float) => "float",
                _ when Type == typeof(bool) => "bool",
                _ when Type == typeof(int) => "int",
                _ => "unknown"
            };
        }
    }

    public string ToCSharpString(StringPool pool)
    {
        var isReadonly = IsReadOnly ? pool.GetOrAdd(" readonly") : "";
        var typeSystemName = pool.GetOrAdd(TypeSystemName);
        var defaultValue = DefaultValue == null ? "" : $" = {DefaultValue}";
        var prefix = pool.GetOrAdd("global_var_");
        
        if (Type == typeof(string))
        {
            defaultValue = DefaultValue == null ? "" : $" = \"{DefaultValue}\"";
        }
        else if (Type == typeof(float))
        {
            defaultValue = DefaultValue == null ? "" : $" = {DefaultValue:F2}";
        }
        else if (Type == typeof(bool))
        {
            defaultValue = DefaultValue == null ? "" : $" = {((bool) DefaultValue ? "true" : "false")}";
        }

        if (IsArgument)
        {
            prefix = pool.GetOrAdd("bp_arg_");
            isReadonly = ""; // Arguments cannot be readonly as they need to be set when executing the blueprint.
        }
        
        return $"private{isReadonly} {typeSystemName} {prefix}{Id}{defaultValue}; // {Name}";
    }
}

public static class BlueprintParametersExtensions
{
    public static ControlPropertyDescriptor GetDescriptor(this BlueprintParameters parameter)
    {
        // We do not take into account the "IsReadOnly" boolean as it's only for node logic.
        return new EditableControlPropertyDescriptor(parameter.Name, parameter.Type, () => parameter.DefaultValue,
            "generated".ToPipedPath().Extend("bp_g_v").Extend(parameter.Id.ToString()),
            setter: (value) => { parameter.DefaultValue = value; },
            validate: (value) =>
            {
                if (value == null) return false;
                return parameter.Type.IsInstanceOfType(value);
            });
    }
}