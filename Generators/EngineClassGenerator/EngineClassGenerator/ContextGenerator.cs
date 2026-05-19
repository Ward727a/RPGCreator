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
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace EngineClassGenerator;

[Generator]
public class ContextGenerator : IIncrementalGenerator
{
    
    private static DiagnosticDescriptor _descriptor = new DiagnosticDescriptor(
        id: "SG001",
        title: "Debug Log",
        messageFormat: "Log message: {0}",
        category: "SourceGenerator",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclaration = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is ClassDeclarationSyntax,
                transform: static (ctx, token) => GetSemanticTargetForGeneration(ctx, token))
            .Where(static m => m is not null);
        
        context.RegisterSourceOutput(classDeclaration.Collect(), Execute);
    }
    
    private static ClassToGenerate? GetSemanticTargetForGeneration(GeneratorSyntaxContext ctx, CancellationToken token)
    {var classDeclaration = (ClassDeclarationSyntax)ctx.Node;
        var classSymbol = ctx.SemanticModel.GetDeclaredSymbol(classDeclaration, token) as INamedTypeSymbol;

        if (classSymbol == null || !ContainsTargetedAttribute(classSymbol)) return null;

        var currentBase = classSymbol.BaseType;

        bool baseHasClassUrnField = false;
        bool baseHasUniqueField = false;
        
        while (currentBase != null)
        {
            if (ContainsTargetedAttribute(currentBase))
            {
                baseHasClassUrnField = true;
                baseHasUniqueField = true;
                break;
            }
            
            if (!baseHasClassUrnField)
                baseHasClassUrnField = currentBase.GetMembers().Any(m => m.Name.Equals("ClassUrn", StringComparison.Ordinal));
            
            if (!baseHasUniqueField)
                baseHasUniqueField = currentBase.GetMembers().Any(m => m.Name.Equals("Unique", StringComparison.Ordinal));

            currentBase = currentBase.BaseType;
        }

        var attrData = GetAttributeData(classSymbol);

        return new ClassToGenerate(
            classSymbol.Name,
            classSymbol.ContainingNamespace.ToDisplayString(),
            attrData.ClassUrn.ToImmutableArray(),
            ShouldGenerateClassUrnField: !baseHasClassUrnField && !classSymbol.GetMembers().Any(m => m.Name == "ClassUrn"),
            ShouldGenerateUniqueField: !baseHasUniqueField && !classSymbol.GetMembers().Any(m => m.Name == "Unique")
        );
    }
    
    public record struct ClassToGenerate(
        string Name, 
        string Namespace, 
        ImmutableArray<string> UrnParts,
        bool ShouldGenerateClassUrnField,
        bool ShouldGenerateUniqueField);
    
    private static bool ContainsTargetedAttribute(INamedTypeSymbol symbol)
    {
        return symbol.GetAttributes().Any(attribute =>
            attribute.AttributeClass?.Name == "EngineClassAttribute"
            || attribute.AttributeClass?.Name == "EngineClass");
    }

    private static void Execute(SourceProductionContext spc, ImmutableArray<ClassToGenerate?> sources)
    {
        
        spc.ReportDiagnostic(Diagnostic.Create(_descriptor, Location.None, $"Found {sources.Length} class"));
        foreach (var source in sources)
        {
            if (source is null) continue;
            
            var sourceCode = GenerateClass(spc, source.Value);
            
            spc.AddSource($"{source.Value.Name}_gen.cs", SourceText.From(sourceCode, Encoding.UTF8));
        }
    }
    
    private static string GenerateClass(SourceProductionContext spc, ClassToGenerate classSymbol)
    {
        var stringWriter = new StringWriter(new StringBuilder());
        using var sb = new IndentedTextWriter(stringWriter, "    ");
        
        GenerateHeader(classSymbol, sb);
        
        spc.ReportDiagnostic(Diagnostic.Create(_descriptor, Location.None, $"Found {classSymbol.Name} class"));
        
        sb.WriteLine("{");
        {
            sb.WriteLine("// This partial class was auto-generated by the ContextGenerator source generator. Do not modify this file directly.");
            sb.Indent++;
            
            var className = classSymbol.Name;
            
            sb.WriteLine($"public partial class {className} : IEngineClass");
            sb.WriteLine("{");
            sb.Indent++;
            sb.Write("static public URN ClassURN = new URN(");
            foreach (var urn in classSymbol.UrnParts)
            {
                sb.Write($"\"{urn}\"");
                if(classSymbol.UrnParts.Last() != urn)
                {
                    sb.Write(", ");
                }
            }
            sb.WriteLine(");");
            
            if(classSymbol.ShouldGenerateClassUrnField || classSymbol.ShouldGenerateUniqueField)
            {
                sb.WriteLine();
            }
            
            if (classSymbol.ShouldGenerateClassUrnField)
            {
                sb.WriteLine("public URN ClassUrn {get; set; }");
            }

            if (classSymbol.ShouldGenerateUniqueField)
            {
                sb.WriteLine("public Ulid Unique { get; set; }");
            }

            sb.Indent--;
            sb.WriteLine("}");
        }
        sb.Indent--;
        sb.WriteLine("}");
        return stringWriter.ToString();
    }

    private static void GenerateHeader(ClassToGenerate classSymbol, IndentedTextWriter sb)
    {
        var baseNamespace = classSymbol.Namespace;


        sb.WriteLine("// <auto-generated/>");
        sb.WriteLine("#nullable enable");
        sb.WriteLine("using System;");
        sb.WriteLine("using System.Collections.Generic;");
        sb.WriteLine("using RPGCreator.SDK.GameUI;");
        sb.WriteLine("using RPGCreator.SDK.GameUI.Interfaces;");
        sb.WriteLine("using RPGCreator.Shared.Types;");
        sb.WriteLine("using RPGCreator.SDK.Types.Internals;");
        sb.WriteLine();
        sb.WriteLine($"namespace {baseNamespace}");
    }
    
    private record struct AttributeData(string[] ClassUrn);

    private static AttributeData GetAttributeData(INamedTypeSymbol classSymbol)
    {
        var targetAttribute = classSymbol.GetAttributes().FirstOrDefault(a => 
            a.AttributeClass?.Name == "EngineClass" || 
            a.AttributeClass?.Name == "EngineClassAttribute");

        if (targetAttribute == null) return new AttributeData(Array.Empty<string>());

        var urnParts = targetAttribute.ConstructorArguments
            .SelectMany(arg => arg.Kind == TypedConstantKind.Array 
                ? arg.Values.Select(v => v.Value?.ToString()) 
                : new[] { arg.Value?.ToString() })
            .Where(v => v != null)
            .Cast<string>()
            .ToArray();

        return new AttributeData(urnParts);
    }
}