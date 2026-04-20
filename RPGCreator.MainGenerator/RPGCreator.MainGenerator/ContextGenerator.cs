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

using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using RPGCreator.MainGenerator.MetaData;
using RPGCreator.MainGenerator.Parts;

namespace RPGCreator.MainGenerator;

[Generator]
public class ContextGenerator : IIncrementalGenerator
{
    public static Dictionary<string, EClassData> EClassDataList = new Dictionary<string, EClassData>();
    public static HashSet<string> Namespaces = new HashSet<string>();
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var eClassDeclaration = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, token) => s is ClassDeclarationSyntax { Members.Count: > 0} && HasEClassAttribute(s),
                transform: static (ctx, token) => GetEClassSemanticTarget(ctx, token))
            .Where(static m => m is not null);


        context.RegisterSourceOutput(eClassDeclaration.Collect(), static (spc, sources) =>
        {
            GeneratorLogger.Init(spc);
            GenerateEClass(spc, sources);
        });
        
        var moduleStarterDeclaration = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, token) => s is ClassDeclarationSyntax && HasEModuleStarterAttribute(s),
                transform: static (ctx, token) => GetEModuleStarterSemanticTarget(ctx, token))
            .Where(static m => m is not null);
        
        context.RegisterSourceOutput(moduleStarterDeclaration.Collect(), static (spc, sources) =>
        {
            GeneratorLogger.Init(spc);
            if (sources.Count() > 1)
            {
                foreach (var namedTypeSymbol in sources)
                {
                    GeneratorLogger.ErrorMisc("Only one EModuleStarter can be declared per project!", namedTypeSymbol?.Locations.FirstOrDefault());
                    return;
                }
            }
            
            GenModule.GenerateModuleStarter(spc, sources);
        });
    }


    private static void GenerateEClass(SourceProductionContext spc, ImmutableArray<INamedTypeSymbol?> sources)
    {
        foreach (var source in sources)
        {
            if (source is null) continue;

            var sourceCode = GenerateClass(spc, source);
            
            spc.AddSource($"{source.Name}.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
        }
    }

    private static string GenerateClass(SourceProductionContext spc, INamedTypeSymbol classSymbol)
    {
        var stringWriter = new StringWriter(new StringBuilder());
        using var sb = new IndentedTextWriter(stringWriter, "    ");
        var classData = MetadataParser.ParseEClass(classSymbol);

        var properties = GetProperties(classSymbol);

        if (classData.IsFailure)
        {
            GeneratorLogger.ErrorMisc(classData.Error, classSymbol.Locations.FirstOrDefault());
            if (properties.Count != 0)
            {
                foreach (var property in properties)
                {
                    GeneratorLogger.ErrorMisc(
                        $"Property {property.Field.Name} has EProperty attribute, " +
                        $"but parent class does not have EClass attribute, or generator " +
                        $"couldn't generate metadata.",
                        property.Field.Locations.FirstOrDefault());
                }
            }
        }

        var context = new GenerationContext(sb, classData.Value);

        GenNamespaces.Begin(context);
        Namespaces.Add(GenNamespaces.GetNamespace(context));
        
        GenClass.Begin(context);
        
        GenFactory.Write(context);
        
        GenDirty.Write(context);
        context.Writer.WriteLine();
        
        GenSerialization.Write(context);
        context.Writer.WriteLine();
        
        GenProperties.WriteBaseProperties(context);
        GenConstructor.Write(context);

        foreach (var property in properties)
        {
            GenProperties.WriteProperty(context, property);
        }
        
        GenProperties.WritePropertiesGetter(context, properties);
        
        GenClass.End(context);
        GenNamespaces.End(context);
        
        EClassDataList[classData.Value.Class.ToDisplayString()] = classData.Value;
        
        return stringWriter.ToString();
    }

    private static List<EPropertyData> GetProperties(INamedTypeSymbol classSymbol)
    {
        return classSymbol.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(f => f.GetAttributes().Any(MetadataParser.IsEProperty))
            .Select(f =>
            {
                var ret = MetadataParser.ParseEProperty(f);

                if (ret.IsFailure)
                {
                    GeneratorLogger.ErrorMisc(ret.Error, f.Locations.FirstOrDefault());
                    return default;
                }
                return ret.Value;
            })
            .ToList();
    }

    private static INamedTypeSymbol? GetEClassSemanticTarget(GeneratorSyntaxContext ctx, CancellationToken token)
    {
        var classDecl = (ClassDeclarationSyntax)ctx.Node;

        if (ctx.SemanticModel.GetDeclaredSymbol(classDecl, token) is not INamedTypeSymbol classSymbol) return null;

        var hasAttribute = classSymbol.GetAttributes().Any(attr => 
            attr.AttributeClass?.ToDisplayString() == "RPGCreator.SDK.Attributes.EClassAttribute");

        return hasAttribute ? classSymbol : null;
    }

    private static INamedTypeSymbol? GetEModuleStarterSemanticTarget(GeneratorSyntaxContext ctx,
        CancellationToken token)
    {
        var classDeclaration = (ClassDeclarationSyntax)ctx.Node;
        
        if(ctx.SemanticModel.GetDeclaredSymbol(classDeclaration, token) is not INamedTypeSymbol classSymbol) return null;
        
        var hasAttribute = classSymbol.GetAttributes().Any(attr => 
            attr.AttributeClass?.ToDisplayString() == "RPGCreator.SDK.Attributes.EModuleStarterAttribute");
        
        return hasAttribute ? classSymbol : null;
    }
    
    private static bool FieldHasAttribute(IFieldSymbol field, string attributeFullName)
    {
        return field.GetAttributes().Any(a => 
            a.AttributeClass?.ToDisplayString() == attributeFullName);
    }

    private static bool HasEClassAttribute(SyntaxNode node)
    {
        if (node is not ClassDeclarationSyntax classDecl) return false;

        return classDecl.AttributeLists
            .SelectMany(list => list.Attributes)
            .Any(attr => 
            {
                var name = attr.Name.ToString();
                return name is "EClass" or "EClassAttribute";
            });
    }

    private static bool HasEModuleStarterAttribute(SyntaxNode node)
    {
        if (node is not ClassDeclarationSyntax classDeclaration) return false;

        return classDeclaration.AttributeLists
            .SelectMany(list => list.Attributes)
            .Any(attr =>
            {
                var name = attr.Name.ToString();
                return name is "EModuleStarter" or "EModuleStarterAttribute";
            });
    }
}