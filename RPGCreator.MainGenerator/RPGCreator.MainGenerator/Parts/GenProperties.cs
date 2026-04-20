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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using RPGCreator.MainGenerator.MetaData;
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
        
        context.Writer.WriteLine($"public static new readonly URN ClassUrn = new URN(\"{context.ClassData.UrnNamespace}\", \"{context.ClassData.UrnModule}\", \"{context.ClassData.Name}\");");
        context.Writer.WriteLine($"public static new readonly EClassData ClassData = new EClassData(\"{context.ClassData.Name}\", \"{context.ClassData.DisplayName}\", \"{context.ClassData.Icon}\", \"{context.ClassData.Description}\", \"{context.ClassData.Category}\", \"{context.ClassData.SerializationPath}\", ClassUrn, {(context.ClassData.SupportSerialization ? "true" : "false")}, {(context.ClassData.SupportDirtyFlag ? "true" : "false" )});");
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
    
    public static void WritePropertiesGetter(GenerationContext context, List<EPropertyData> properties)
    {
        if (!context.InClass)
        {
            GeneratorLogger.ErrorMisc("Cannot write properties getter outside of a class.", context.ClassData.Class.Locations.FirstOrDefault());
            return;
        }

        context.Writer.Write("private static Dictionary<StringName, EPropertyContext> _properties = new()");

        context.Writer.WriteLine("{");
        context.Indent++;

        foreach (var propertyData in properties)
        {
            context.Writer.WriteLine("{");
            context.Writer.Indent++;
            context.Writer.WriteLine($"new StringName(\"{FormatPublicName(propertyData.Field.Name)}\"),");
            context.Writer.WriteLine("new EPropertyContext()");
            context.Writer.WriteLine("{");
            context.Indent++;
            context.Writer.WriteLine($"PropertyName = \"{FormatPublicName(propertyData.Field.Name)}\",");
            context.Writer.WriteLine($"PropertyType = typeof({propertyData.Field.Type.ToDisplayString()}),");
            context.Writer.WriteLine($"Getter = (instance) => {{");
            context.Writer.WriteLine($"if(instance is {context.ClassData.Class.Name} typedInstance)");
            context.Writer.Indent++;
            context.Writer.WriteLine(
                $"return Result<object>.Ok(typedInstance.{FormatPublicName(propertyData.Field.Name)});");
            context.Writer.Indent--;
            context.Writer.WriteLine(
                $"return Result.Fail($\"Instance of type '{{instance.GetType()}}' and not of type '{context.ClassData.Class.Name}'\");");
            context.Writer.WriteLine("},");
            context.Writer.WriteLine($"Setter = (instance, value) => {{");
            context.Writer.WriteLine(
                $"if(value is {propertyData.Field.Type.ToDisplayString()} typedValue && instance is {context.ClassData.Class.Name} typedInstance)");
            context.Writer.WriteLine("{");
            context.Writer.Indent++;
            context.Writer.WriteLine($"typedInstance.{FormatPublicName(propertyData.Field.Name)} = typedValue;");
            context.Writer.WriteLine("return Result.Ok();");
            context.Writer.Indent--;
            context.Writer.WriteLine("}");
            context.Writer.WriteLine(
                $"return Result.Fail($\"Value is of type '{{value.GetType()}}' while the property '{FormatPublicName(propertyData.Field.Name)}' await a type '{propertyData.Field.Type.ToDisplayString()}'\");");
            context.Writer.WriteLine("},");
            context.Writer.WriteLine($"IsDirtying = {(propertyData.Dirtying ? "true" : "false")},");
            context.Writer.WriteLine($"IsSerializable = {(propertyData.Serializable ? "true" : "false")}");
            context.Indent--;
            context.Writer.WriteLine("}");
            context.Writer.Indent--;
            context.Writer.WriteLine("},");
        }

        context.Indent--;
        context.Writer.WriteLine("};");
        
        if(!context.ClassData.ParentIsEBaseClass)
        {
            context.Writer.WriteLine("public static IReadOnlyDictionary<StringName, EPropertyContext> GetProperties() => _properties.AsReadOnly();");
        }
        else
        {
            context.Writer.WriteLine("private static Dictionary<StringName, EPropertyContext> _fullProperties;");
            context.Writer.WriteLine("public new static IReadOnlyDictionary<StringName, EPropertyContext> GetProperties()");
            context.Writer.WriteLine("{");
            context.Indent++;
            context.Writer.WriteLine("if (_fullProperties == null)");
            context.Writer.WriteLine("{");
            context.Indent++;
            context.Writer.WriteLine($"var parentProperties = {context.ClassData.Class.BaseType!.ToDisplayString()}.GetProperties();");
            context.Writer.WriteLine("var combinedProperties = new Dictionary<StringName, EPropertyContext>(parentProperties);");
            context.Writer.WriteLine("foreach (var prop in _properties)");
            context.Writer.WriteLine("{");
            context.Indent++;
            context.Writer.WriteLine("combinedProperties[prop.Key] = prop.Value;");
            context.Indent--;
            context.Writer.WriteLine("}");
            context.Writer.WriteLine("_fullProperties = combinedProperties;");
            context.Indent--;
            context.Writer.WriteLine("}");
            context.Writer.WriteLine($"return _fullProperties;");
            context.Indent--;
            context.Writer.WriteLine("}");
        }

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