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

using System.Linq;
using Microsoft.CodeAnalysis;
using RPGCreator.MainGenerator.InternalDependancies;
using RPGCreator.MainGenerator.MetaData;

namespace RPGCreator.MainGenerator;

public static class MetadataParser
{
    public static Result<EPropertyData> ParseEProperty(IFieldSymbol fieldSymbol)
    {
        var attribute = fieldSymbol.GetAttributes().FirstOrDefault(a => IsEProperty(a));

        if (attribute is null)
        {
            return Result.Fail(
                $"Property {fieldSymbol.Name} is not decorated with EPropertyAttribute or returned null when trying to get it.");
        }

        var data = new EPropertyData(fieldSymbol);
        data = Enumerable.Aggregate(attribute.NamedArguments, data, (current, arg) => arg.Key switch
        {
            "Description" when arg.Value.Value is string namedDesc => current with
            {
                Description = namedDesc.Replace("\"", "\\\"")
            },
            "Category" when arg.Value.Value is string namedPath => current with
            {
                Category = namedPath.Replace("\"", "\\\"")
            },
            "Dirtying" when arg.Value.Value is bool dirtying => current with { Dirtying = dirtying },
            "DisplayName" when arg.Value.Value is string displayName => current with
            {
                DisplayName = displayName.Replace("\"", "\\\"")
            },
            "IsSerializable" when arg.Value.Value is bool isSerializable => current with
            {
                Serializable = isSerializable
            },
            "EditorHint" when arg.Value.Value is string editorHint => current with
            {
                EditorHint = editorHint.Replace("\"", "\\\"")
            },
            _ => current
        });

        if (string.IsNullOrWhiteSpace(data.DisplayName))
        {
            data = data with { DisplayName = fieldSymbol.Name };
        }

        return data;
    }

    public static Result<EClassData> ParseEClass(INamedTypeSymbol classSymbol)
    {
        var attribute = classSymbol.GetAttributes().FirstOrDefault(a =>
            a.AttributeClass?.ToDisplayString() == "RPGCreator.SDK.Attributes.EClassAttribute");

        if (attribute is null)
        {
            return Result.Fail(
                $"Class {classSymbol.Name} is not decorated with EClassAttribute or returned null when trying to get it.");
        }

        INamedTypeSymbol? parent = classSymbol.BaseType;
        bool parentHasEClass = false;
        bool parentHasDirtyFlag = false;
        bool parentHasSerializable = false;

        var currentBase = classSymbol.BaseType;
        while (currentBase != null && currentBase.SpecialType != SpecialType.System_Object)
        {
            var eClassAttr = currentBase.GetAttributes()
                .FirstOrDefault(ad => ad.AttributeClass?.Name == "EClassAttribute");

            if (eClassAttr != null)
            {
                var attrData = ParseEClass(currentBase);

                if (attrData.IsFailure)
                {
                    currentBase = currentBase.BaseType;
                    continue;
                }

                if (attrData.Value.SupportDirtyFlag) parentHasDirtyFlag = true;
                if (attrData.Value.SupportSerialization) parentHasSerializable = true;
                parentHasEClass = true;
            }

            currentBase = currentBase.BaseType;
        }

        var data = new EClassData(classSymbol);
        data = Enumerable.Aggregate(attribute.NamedArguments, data, (current, arg) => arg.Key switch
        {
            "Name" when arg.Value.Value is string name => current with { Name = name.Replace("\"", "\\\"") },
            "DisplayName" when arg.Value.Value is string displayName => current with
            {
                DisplayName = displayName.Replace("\"", "\\\"")
            },
            "Description" when arg.Value.Value is string namedDesc => current with
            {
                Description = namedDesc.Replace("\"", "\\\"")
            },
            "Icon" when arg.Value.Value is string icon => current with { Icon = icon.Replace("\"", "\\\"") },
            "Category" when arg.Value.Value is string namedPath => current with
            {
                Category = namedPath.Replace("\"", "\\\"")
            },
            "SerializationPath" when arg.Value.Value is string serializationPath => current with
            {
                SerializationPath = serializationPath.Replace("\"", "\\\"")
            },
            "SerializeInProjectFolder" when arg.Value.Value is bool serializeInProjectFolder => current with
            {
                SerializeInProjectFolder = serializeInProjectFolder
            },
            "SupportSerialization" when arg.Value.Value is bool supportSerialization => current with
            {
                SupportSerialization = supportSerialization
            },
            "SupportDirtyFlag" when arg.Value.Value is bool supportDirtyFlag => current with
            {
                SupportDirtyFlag = supportDirtyFlag
            },
            "UrnNamespace" when arg.Value.Value is string urnNamespace => current with
            {
                UrnNamespace = urnNamespace.Replace("\"", "\\\"")
            },
            "UrnModule" when arg.Value.Value is string urnModule => current with
            {
                UrnModule = urnModule.Replace("\"", "\\\"")
            },
            _ => current
        });

        if (string.IsNullOrWhiteSpace(data.Name))
        {
            data = data with { Name = classSymbol.Name };
        }

        if (string.IsNullOrWhiteSpace(data.DisplayName))
        {
            data = data with { DisplayName = data.Name };
        }

        if (string.IsNullOrWhiteSpace(data.SerializationPath))
        {
            data = data with { SerializationPath = classSymbol.Name };
        }

        if (string.IsNullOrWhiteSpace(data.Category))
        {
            data = data with { Category = "EngineClass" };
        }

        if (string.IsNullOrWhiteSpace(data.UrnNamespace))
        {
            data = data with { UrnNamespace = classSymbol.ContainingNamespace.ToDisplayString().Replace(" ", "_") };
        }


        if (string.IsNullOrWhiteSpace(data.UrnModule))
        {
            data = data with { UrnModule = data.Category };
        }

        data = data with
        {
            ParentIsEBaseClass = parentHasEClass, 
            ParentSupportsDirtyFlag = parentHasDirtyFlag,
            ParentSupportsSerialization = parentHasSerializable
        };


        return data;
    }

    public static bool IsEProperty(AttributeData attr)
    {
        return attr.AttributeClass?.ToDisplayString() == "RPGCreator.SDK.Attributes.EPropertyAttribute";
    }
}