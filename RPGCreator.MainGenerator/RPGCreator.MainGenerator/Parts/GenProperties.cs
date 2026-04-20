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
using Microsoft.CodeAnalysis.CodeFixes;
using RPGCreator.MainGenerator.Parts.Property;

namespace RPGCreator.MainGenerator.Parts;

public static class GenProperties
{
    public static void WriteBaseProperties(GenerationContext context)
    {
        if (!context.InClass)
        {
            GeneratorLogger.ErrorMisc("Cannot write base properties outside of a class.", context.ClassData.Class.Locations.FirstOrDefault());
            return;
        }
        
        context.Writer.WriteLine($"public static readonly URN ClassUrn = new URN(\"{context.ClassData.UrnNamespace}\", \"{context.ClassData.UrnModule}\", \"{context.ClassData.Name}\");");
        context.Writer.WriteLine($"public static readonly EClassData ClassData = new EClassData(\"{context.ClassData.Name}\", \"{context.ClassData.DisplayName}\", \"{context.ClassData.Icon}\", \"{context.ClassData.Description}\", \"{context.ClassData.Category}\", \"{context.ClassData.SerializationPath}\", ClassUrn, {(context.ClassData.SupportSerialization ? "true" : "false")}, {(context.ClassData.SupportDirtyFlag ? "true" : "false" )});");
        context.Writer.WriteLine();
        context.Writer.WriteLine("public override URN GetClassUrn() => ClassUrn;");
        context.Writer.WriteLine();
    }

    public static void WriteProperty(GenerationContext context, EPropertyData propertyData)
    {
        if (!context.InClass)
        {
            GeneratorLogger.ErrorMisc("Cannot write property outside of a class.", context.ClassData.Class.Locations.FirstOrDefault());
            return;
        }

        if (!IsPrivateAccessibility(propertyData))
        {
            GeneratorLogger.ErrorMisc($"Property is not valid for generation [EProperty]. Should be 'private', not {propertyData.Field.DeclaredAccessibility}.\n" +
                                      $"If you need to access this property publicly, use the generated property: {FormatPublicName(propertyData.Field.Name)}", propertyData.Field.Locations.FirstOrDefault());
            return;
        }

        if (!IsNameValid(propertyData))
        {
            GeneratorLogger.ErrorMisc($"Property name is not valid for generation [EProperty]. Should start with '_', not {propertyData.Field.Name}.\n" +
                                      $"Example: _{propertyData.Field.Name} is valid!", propertyData.Field.Locations.FirstOrDefault());
            return;
        }
        
        if (!IsNameLengthValid(propertyData))
        {
            GeneratorLogger.ErrorMisc($"Property name is not valid for generation with [EProperty]. Should be at least 2 characters long, not {propertyData.Field.Name}", propertyData.Field.Locations.FirstOrDefault());
            return;
        }
        
        WritePropertyField(context, propertyData);
        GenPropValidate.Write(context, propertyData);
        GenPropCoerce.Write(context, propertyData);
        GenPropSetter.Write(context, propertyData);
        GenPropGetter.Write(context, propertyData);
    }

    private static void WritePropertyField(GenerationContext context, EPropertyData propertyData)
    {
        var name = FormatPublicName(propertyData.Field.Name);
        var type = propertyData.Field.Type.ToDisplayString(); 
        context.Writer.WriteLine(
        $$"""
        public {{type}} {{name}} 
        {
            get
            {
                return Get{{name}}();
            }
            set
            {
                if(EqualityComparer<{{type}}>.Default.Equals(value, {{propertyData.Field.Name}})) return;
                Set{{name}}(value);
            }
        } 
        """);
    }
    
    #region CheckingMethods
    private static bool IsPrivateAccessibility(EPropertyData data)
    {
        return data.Field.DeclaredAccessibility == Accessibility.Private;
    }
    
    private static bool IsNameValid(EPropertyData data)
    {
        return data.Field.Name.StartsWith("_");
    }

    private static bool IsNameLengthValid(EPropertyData data)
    {
        return data.Field.Name.Length >= 2;
    }
    #endregion

    public static string FormatPublicName(string fieldName)
    {
        string cleanName = fieldName.TrimStart('_');
        if (string.IsNullOrEmpty(cleanName)) return "InvalidName";
    
        return char.ToUpper(cleanName[0]) + cleanName.Substring(1);
    }

    public static string FormatDirectType(string typeName)
    {
        return typeName.TrimEnd('?');
    }
}