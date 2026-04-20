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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace RPGCreator.MainGenerator;


public class PropertyWriter
{
    public enum EAccessibility
    {
        Public,
        Private,
        Protected,
        Internal
    }
    public enum EPropertyMethodType
    {
        Get,
        Set,
        Coerce,
        Validate,
        Factory
    }

    public sealed class PropertyMethods(EPropertyMethodType type)
    {
        public EPropertyMethodType Type { get; set; } = type;
        public MethodBody Body { get; } = new();
        public MethodBody Partial { get; } = new();
        
        public bool HasBody = false;
        public bool HasPartial = false;

        public sealed class MethodBody
        {
            public EAccessibility Accessibility { get; set; } = EAccessibility.Public;
            public string Name { get; set; } = string.Empty;
            public string ReturnType { get; set; } = "void";
            public Dictionary<string, string> Parameters { get; } = new();
            public string Body { get; set; } = string.Empty;

            public void AddParameter(string type, string name) => Parameters.Add(name, type);
            public void RemoveParameter(string name) => Parameters.Remove(name);
            
            public void SetType(string type) => ReturnType = type;
            public void SetType(ITypeSymbol type) => ReturnType = type.ToDisplayString();
            public void AddToBody(string content) => Body = string.Join("\n", Body, content);
            
            public bool HasBody => !string.IsNullOrWhiteSpace(Body);
            public bool HasParameters => Parameters.Any();
            
            public string GenerateParameters()
            {
                if (!HasParameters)
                    return "";
                return string.Join(", ", Parameters.Select(x => $"{x.Value} {x.Key}"));
            }
        }
    }
    
    public string DefaultValue;
    
    private IFieldSymbol _prop;
    
    public string PropertyName => _prop.Name;
    public string PropertyType => _prop.Type.ToDisplayString();

    public string GetterMethod = "";
    public string SetterMethod = "";
    
    public bool HasGetter => !string.IsNullOrWhiteSpace(GetterMethod);
    public bool HasSetter => !string.IsNullOrWhiteSpace(SetterMethod);
    
    public EAccessibility Accessibility { get; set; } = EAccessibility.Public;
    
    private Dictionary<EPropertyMethodType, PropertyMethods> _methods = new();

    public PropertyWriter(IFieldSymbol prop, string defaultValue = "")
    {
        DefaultValue = defaultValue;
        _prop = prop;

        if (!prop.Name.StartsWith("_"))
        {
            GeneratorLogger.ErrorMisc($"Property name '{prop.Name}' does not start with an underscore.", prop.Locations.First());
            _canGenerate = false;
        }
    }

    private readonly bool _canGenerate = true;

    public bool HasMethod(EPropertyMethodType methodType) => _methods.ContainsKey(methodType);
    public bool HasMethodBody(EPropertyMethodType methodType) => _methods.TryGetValue(methodType, out var method) && method.HasBody;
    public bool HasMethodPartial(EPropertyMethodType methodType) => _methods.TryGetValue(methodType, out var method) && method.HasPartial;
    
    public PropertyMethods GetMethod(EPropertyMethodType methodType) => _methods[methodType];
    
    public void SetMethod(EPropertyMethodType methodType, PropertyMethods methods) => _methods[methodType] = methods;
    public void SetMethodBody(EPropertyMethodType methodType, string methodBody) => _methods[methodType].Body.Body = methodBody;
    public void SetMethodPartial(EPropertyMethodType methodType, string partial) => _methods[methodType].Partial.Body = partial;


    public void Write(IndentedTextWriter textWriter)
    {
        if (!_canGenerate)
            return;
        textWriter.WriteLine(GenerateProperty());
    }

    private string GenerateProperty()
    {
        return $$"""
                 {{GenerateAccessibility(Accessibility)}} {{PropertyType}} {{PropertyName}}{{GenerateGetterSetter()}}{{(string.IsNullOrWhiteSpace(DefaultValue) ? "" : $" = {DefaultValue}")}};
                 """;
    }

    private string GenerateMethods()
    {
        foreach (var method in _methods.Values)
        {
            
        }

        return "";
    }

    private string GenerateMethod(PropertyMethods.MethodBody methodData)
    {
        return $$"""

                 {{GenerateAccessibility(methodData.Accessibility)}} {{methodData.ReturnType}} {{methodData.Name}}
                 
                 """;
    }

    private string GenerateAccessibility(EAccessibility accessibility)
    {
        return accessibility switch
        {
            EAccessibility.Public => "public",
            EAccessibility.Private => "private",
            EAccessibility.Protected => "protected",
            EAccessibility.Internal => "internal",
            _ => throw new ArgumentOutOfRangeException(nameof(Accessibility), Accessibility, null)
        };
    }

    private string GenerateTypeWithoutNullable()
    {
        return _prop.Type.ToDisplayString().TrimEnd('?');
    }

    private string GenerateGetterSetter()
    {
        if (!HasGetter && !HasSetter)
            return "";

        return $$"""
                 {
                    {{GenerateGetter()}}
                    {{GenerateSetter()}}
                 }
                 """;

        string GenerateGetter()
        {
            if (!HasGetter)
                return "";
            return $$"""
                        get
                        {
                            {{GetterMethod}}
                            return {{PropertyName}};
                        }
                    """;
        }
        
        string GenerateSetter()
        {
            if (!HasSetter)
                return "";
            return $$"""
                        set
                        {
                            if(value == {{PropertyName}}) return;
                            {{SetterMethod}}
                            {{PropertyName}} = value;
                        }
                    """;
        }
    }
}