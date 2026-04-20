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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Blueprints;
using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Registry;
using RPGCreator.SDK.Types;

namespace RPGCreator.Core.Registry;

public class BlueprintRegistry : IBlueprintRegistry
{
    private readonly Dictionary<Ulid, BlueprintData> _blueprints = new();
    private readonly Dictionary<Ulid, BaseBpCompiledLogic> _bpCompiledLogics = new();
    public int BlueprintCount => _blueprints.Count;

    public Result RegisterBlueprintLogic(Ulid id, BaseBpCompiledLogic logic)
    {
        if (logic == null)
            return Result.Fail("Logic cannot be null");
        
        if (!_blueprints.ContainsKey(id))
            return Result.Fail("Blueprint with ID does not exist");
        
        _bpCompiledLogics[id] = logic;
        return Result.Ok();
    }

    public Result RegisterBlueprint(BlueprintData blueprintData)
    {
        if (blueprintData == null)
            return Result.Fail("Blueprint data cannot be null");
        
        if (!_blueprints.TryAdd(blueprintData.Id, blueprintData))
            return Result.Fail("Blueprint with ID already exists");

        return Result.Ok();
    }

    public Result UnregisterBlueprint(Ulid id)
    {
        if (!_blueprints.Remove(id))
            return Result.Fail("Blueprint with ID does not exist");
        
        return Result.Ok();
    }

    public Result<BlueprintData> GetBlueprint(Ulid id)
    {
        if (!_blueprints.TryGetValue(id, out var blueprint))
            return Result<BlueprintData>.Fail("Blueprint with ID does not exist");
        
        return Result<BlueprintData>.Ok(blueprint);
    }
    
    public IEnumerable<BlueprintData> GetAllBlueprints() => _blueprints.Values;

    public bool HasBlueprint(Ulid id) => _blueprints.ContainsKey(id);

    public Result<byte[]> CompileAllBlueprints(string? blueprintFolder = null, string? toFolder = null, bool forceCompilation = false, IEnumerable<Ulid>? skipBlueprints = null)
    {
        var resultText = EditorUiServices.BpCompiler.BuildGlobalBlueprintCode(blueprintFolder ?? string.Empty);
        
        if(resultText.IsFailure)
            return Result<byte[]>.Fail(resultText.Error);
        
        File.WriteAllText(Path.Combine(toFolder ?? string.Empty, "GlobalBlueprint.cs"), resultText.Value.sourceCode);
        
        List<SyntaxTree> syntaxTrees = new()
        {
            CSharpSyntaxTree.ParseText(resultText.Value.sourceCode, path: "GlobalBlueprint.cs")
        };
        
        foreach (var referencedBlueprintPath in resultText.Value.referencedBlueprints)
        {
            var referencedBlueprint = File.ReadAllText(referencedBlueprintPath);
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(referencedBlueprint, path: referencedBlueprintPath));
        }
        
        var trustedAssemblies = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator);
        var references = trustedAssemblies
            .Select(path => MetadataReference.CreateFromFile(path))
            .Cast<MetadataReference>()
            .ToList();
        
        references.Add(MetadataReference.CreateFromFile(typeof(RegistryServices).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(BaseBpCompiledLogic).Assembly.Location));

        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithOptimizationLevel(OptimizationLevel.Release)
            .WithPlatform(Platform.AnyCpu);

        string assemblyName = $"RPGCreator.Generated.Blueprints_{DateTime.Now:yyyyMMdd_HHmm}";
        
        var compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees,
            references,
            compilationOptions);

        using var ms = new MemoryStream();
        EmitResult result = compilation.Emit(ms);

        if (!result.Success)
        {
            var failures = result.Diagnostics
                .Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error)
                .Select(d => $"{d.Id}: {d.GetMessage()} ({d.Location})");

            return Result<byte[]>.Fail(string.Join("\n", failures));
        }

        return Result<byte[]>.Ok(ms.ToArray());
    }

    

    public Result<string> BuildBlueprint(Ulid id, string? toFolder = null, bool forceBuild = false)
    {
        return GetBlueprint(id)
            .Bind(data => EditorUiServices.BpCompiler.Build(data));
    }

    public Result<string> BuildBlueprint(BlueprintData blueprintData, string? toFolder = null, bool forceBuild = false)
    {
        return EditorUiServices.BpCompiler.Build(blueprintData);
    }
}