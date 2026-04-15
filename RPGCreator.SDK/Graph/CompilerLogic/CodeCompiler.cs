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
using System.Text;
using CommunityToolkit.HighPerformance.Buffers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.MethodExtensions;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Graph.CompilerLogic;

public static class CodeCompiler
{
    private static IConfig _config;

    static CodeCompiler()
    {
        if (!EngineServices.Config.TryFrom("blueprint", true, out _config))
        {
            if (!EngineServices.Config.CreateConfig("blueprint", new BaseConfig(), true))
            {
                throw new Exception("Failed to create config");
            }

            _config = EngineServices.Config.From("blueprint", true)!;
        }
    }

    public static Result<string> CompileToCSharp(BlueprintCompilerData data)
    {
        var usings = new SortedSet<string>();
        var (success, startNode, inDegree, metadataMap) = CodePreCompiler.SortConnectors(data);

        var codeContext = new CodeContext(data, metadataMap);

        var debugInfo = _config.GetBool("compile_debug_info", true);
        if (debugInfo)
        {
            codeContext.InDebugMode = true;
            usings.Add("CommunityToolkit.Diagnostics");
            usings.Add("System.Diagnostics");
            usings.Add("System.Globalization");
            usings.Add("RPGCreator.SDK.Graph");
            usings.Add("RPGCreator.SDK.Logging");
        }

        var defaultUsings = _config.Get<string[]>("compile_default_usings", []);
        foreach (var usingNamespace in defaultUsings)
            usings.Add(usingNamespace);

        var allowAddingUsings = _config.GetBool("compile_allow_adding_usings", false);
        if (allowAddingUsings)
        {
            Logger.Warning("ALLOWING USING STATEMENTS IN COMPILED CODE!");
            Logger.Warning("If you are using any external plugins, this can lead to arbitrary code execution!");
            Logger.Warning("This is a security risk, and is disabled by default.");
            Logger.Warning("If you want to disable this:");
            Logger.Warning("1. Go inside the 'Settings' window in the editor.");
            Logger.Warning("2. Click on the 'Blueprint' tab.");
            Logger.Warning("3. Disable 'Allow adding using statements in compiled code'.");
            Logger.Warning("4. Save and RE-BUILD the blueprint or else it will do nothing!");
            codeContext.AddUsing = s => { usings.Add(s); };
        }

        try
        {
            var sb = new StringBuilder();
            using var stringWriter = new StringWriter(sb);
            using (IndentedTextWriter writer = new IndentedTextWriter(stringWriter, "    "))
            {
                writer.WriteLine($"public class bp_class_{data.Id} : BaseBpCompiledLogic");
                writer.WriteLine("{");
                writer.Indent++;

                // Generating global bp variables
                {
                    writer.WriteLine("// GLOBAL VARIABLES");
                    writer.WriteLine("#region GlobalVariables");
                    var stringPool = new StringPool();
                    foreach (var parameter in data.Parameters)
                    {
                        writer.WriteLine(parameter.ToCSharpString(stringPool));
                    }

                    writer.WriteLine("#endregion");
                    writer.WriteLine();
                }

#if DEBUG
                {
                    data.Arguments.Add(new BlueprintParameters(Ulid.NewUlid(), "testArgString", typeof(string),
                        IsArgument: true));
                }
#endif

                // Generating args bp variables
                {
                    writer.WriteLine("// ARGUMENTS");
                    writer.WriteLine("#region Arguments");
                    var stringPool = new StringPool();
                    foreach (var argument in data.Arguments)
                    {
                        writer.WriteLine(argument.ToCSharpString(stringPool));
                    }

                    writer.WriteLine("#endregion");
                }
                writer.WriteLine("public override bool Validate(ICompiledBpContext context)");
                writer.WriteLine("{");
                writer.Indent++;


                {
                    if (data.Arguments.Count > 0)
                    {
                        writer.WriteLine("// VALIDATE");
                        writer.Write("return ");
                        writer.Write($"context.Arguments != null && context.Arguments.Count == {data.Arguments.Count}");
                        writer.Indent++;
                        for (int i = 0; i < data.Arguments.Count; i++)
                        {
                            writer.WriteLine();
                            var argument = data.Arguments[i];
                            writer.Write("&& ");
                            writer.Write(
                                $"!object.Equals(context.Arguments[{i}], default(BpValue)) && context.Arguments.IsArgumentOfType({i}, typeof({argument.TypeSystemName})) // arg: {argument.Name}");
                        }

                        writer.WriteLine();
                        writer.WriteLine(";");
                        writer.Indent--;
                    }
                    else
                    {
                        writer.WriteLine("return true;");
                    }
                }

                writer.Indent--;
                writer.WriteLine("}");

                writer.WriteLine("public override void Execute(ICompiledBpContext context)");
                writer.WriteLine("{");
                writer.Indent++;

                writer.WriteLine();
                writer.WriteLine("// ARGUMENT ASSIGNATION");
                writer.WriteLine("#region ArgumentAssignation");
                for (int i = 0; i < data.Arguments.Count; i++)
                {
                    var argument = data.Arguments[i];
                    writer.WriteLine(
                        $"bp_arg_{argument.Id} = context.Arguments[{i}].As{(argument.TypeSystemName.ToCapitalize())}();");
                }

                writer.WriteLine("#endregion");
                writer.WriteLine();

                writer.WriteLine("// PURE NODES");
                foreach (var pure in inDegree.Where(d => d.Value == 0))
                {
                    if (pure.Key == startNode.RuntimeId) continue;
                    codeContext.GeneratePure(codeContext, writer, pure.Key);
                }

                writer.WriteLine();
                writer.WriteLine("// NODES");
                codeContext.GenerateCode(codeContext, writer, startNode.RuntimeId);

                writer.Indent--;
                writer.WriteLine("}");
                writer.Indent--;
                writer.WriteLine("}");
                if (writer.Indent > 0)
                {
                    Logger.Error("Indentation error in EndNode.GenerateCode");
                    Logger.Error("Indentation: " + writer.Indent + " awaited 0.");
                    Logger.Error(
                        "Please report this issue to the RPG Creator team ONLY if you do not have any external plugin installed!");
                    return Result<string>.Fail(
                        "Indentation error. Please report this issue to the RPG Creator team ONLY if you do not have any external plugin installed!");
                }
            }

            var finalCompiledCode = new StringBuilder();
            finalCompiledCode.AppendLine("// THIS FILE WAS AUTO-GENERATED BY THE RPG CREATOR ENGINE");
            finalCompiledCode.AppendLine("// ANY CHANGES MADE TO THIS FILE WILL BE LOST!");
            finalCompiledCode.AppendLine();
            finalCompiledCode.AppendLine("using System;");
            foreach (var usingNamespace in usings)
            {
                var finalNamespace = usingNamespace.Replace("using ", "").TrimEnd(';');
                if (finalNamespace.Contains(' '))
                {
                    Logger.Error($"Invalid namespace '{finalNamespace}' in code compilation. Skipping.");
                    Logger.Error($"Be aware that if this namespaces is used inside the compiled code, it will CRASH!!");
                    continue;
                }

                finalCompiledCode.AppendLine($"using {finalNamespace};");
            }

            finalCompiledCode.AppendLine();
            finalCompiledCode.AppendLine("namespace RPGCreator.SDK.Graph.Blueprints;");

            finalCompiledCode.Append(sb);

            return Result<string>.Ok(finalCompiledCode.ToString());
        }
        catch (Exception e)
        {
            Logger.Error(e, "Error while compiling blueprint.");
            return Result<string>.Fail($"ERROR WHILE COMPILING BLUEPRINT.\n{e.Message}");
        }
    }

    private static CSharpCompilationOptions _defaultCompilationOptions =
        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithOptimizationLevel(OptimizationLevel.Release).WithPlatform(Platform.AnyCpu);

    public static async Task<Result<byte[]>> CompileToDll(string assemblyName, string[] sourceFiles, string outputPath,
        List<string>? references = null, CSharpCompilationOptions? options = null,
        IProgress<float>? progress = null)
    {
        List<SyntaxTree> syntaxTrees = [];
        List<MetadataReference> metadataReferences = [];
        
        progress?.Report(0);

        if (options == null)
            options = _defaultCompilationOptions;
        
        var syntaxTreeResult = (await GetSyntaxTrees(sourceFiles, progress, 0f, 0.3f))
            .OnSuccess((localSyntaxTrees) => { syntaxTrees = localSyntaxTrees; }).OnFailure((errorMessage) =>
            {
                Logger.Error($"Error while getting syntax trees: {errorMessage}");
            });
        
        if (syntaxTreeResult.IsFailure)
            return Result<byte[]>.Fail(syntaxTreeResult.Error);

        var trustedAssemblies = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
            .Split(Path.PathSeparator).ToList();

        if (references != null)
        {
            foreach (var reference in references)
            {
                trustedAssemblies.Add(reference);
            }
        }

        progress?.Report(0.4f);

        var metadataReferencesResult = (await GetMetadataReferences(trustedAssemblies, progress, 0.4f, 0.7f))
            .OnSuccess((localReferences) => { metadataReferences = localReferences; }).OnFailure((errMsg) =>
            {
                Logger.Error($"Error while getting metadata references: {errMsg}");
            });

        if (metadataReferencesResult.IsFailure)
            return Result<byte[]>.Fail(metadataReferencesResult.Error);

        var compilation = CSharpCompilation.Create(assemblyName, syntaxTrees, metadataReferences, options);
        
        progress?.Report(0.8f);

        using var ms = new MemoryStream();
        EmitResult result = compilation.Emit(ms);

        if (!result.Success)
        {
            var failures = result.Diagnostics
                .Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error)
                .Select(d => $"{d.Id}: {d.GetMessage()} ({d.Location})").ToArray();

            return Result<byte[]>.Fail(
                $"Compilation failed with {failures.Length} errors: \n{string.Join(",\n", failures)}");
        }
        progress?.Report(1f);

        return Result<byte[]>.Ok(ms.ToArray());
    }

    private static async Task<Result<List<SyntaxTree>>> GetSyntaxTrees(string[] sourceFiles, IProgress<float>? progress, float startRange, float endRange)
    {
        var syntaxTreeLock = new object();
        List<SyntaxTree> syntaxTrees = [];
        int processed = 0;

        var boolLock = new object();
        var hasErrors = false;
        
        var option = new ParallelOptions()
        {
            MaxDegreeOfParallelism = 3
        };

        await Parallel.ForEachAsync(sourceFiles, option, async (s, token) =>
        {
            (await GetSourceFileContent(s))
                .OnSuccess((content) =>
                {
                    lock (syntaxTreeLock)
                    {
                        syntaxTrees.Add(CSharpSyntaxTree.ParseText(content));
                        processed++;
                    
                        var currentProgress = startRange + (float)processed / sourceFiles.Length * (endRange - startRange);
                        progress?.Report(currentProgress);
                    }
                })
                .OnFailure((errorMsg) =>
                {
                    lock (boolLock)
                        hasErrors = true;
                    Logger.Error($"Error while parsing source file '{s}': {errorMsg}");
                });
        }).ConfigureAwait(false);

        return hasErrors ? Result<List<SyntaxTree>>.Fail("One or more errors occurred while parsing source files") : Result<List<SyntaxTree>>.Ok(syntaxTrees);
    }

    private static async Task<Result<string>> GetSourceFileContent(string sourceFile)
    {
        var isPath = sourceFile.IndexOfAny(Path.GetInvalidPathChars()) == -1 && Path.IsPathRooted(sourceFile) && Path.IsPathFullyQualified(sourceFile);
        var isFile = sourceFile.EndsWith(".cs");

        if (!isPath && !isFile) return Result<string>.Ok(sourceFile);

        if (!isFile)
            return Result<string>.Fail($"Source file '{sourceFile}' is not a valid C# file. Need to end with '.cs'");

        if (!File.Exists(sourceFile))
            return Result<string>.Fail($"Source file '{sourceFile}' does not exist.");

        return Result<string>.Ok(await File.ReadAllTextAsync(sourceFile).ConfigureAwait(false));
    }

    private static async Task<Result<List<MetadataReference>>> GetMetadataReferences(List<string> references, IProgress<float>? progress, float startRange, float endRange)
    {
        var listLock = new object();
        List<MetadataReference> metadataReferences = [];
        int processed = 0;
        
        var boolLock = new object();
        var hasErrors = false;

        var option = new ParallelOptions()
        {
            MaxDegreeOfParallelism = 3
        };

        await Parallel.ForEachAsync(references, option, async (r, token) =>
            {
                (await GetMetadataReference(r))
                    .OnSuccess((reference) =>
                    {
                        lock (listLock)
                        {
                            metadataReferences.Add(reference);
                            processed++;
                            
                            var currentProgress = startRange + (float)processed / references.Count * (endRange - startRange);
                            progress?.Report(currentProgress);
                        }
                    }).OnFailure((errorMsg) =>
                    {
                        lock (boolLock)
                            hasErrors = true;
                        Logger.Error($"Error while getting metadata reference '{r}': {errorMsg}");
                    });
            })
            .ConfigureAwait(false);

        return hasErrors ? Result<List<MetadataReference>>.Fail("One or more errors occurred while getting metadata references") : Result<List<MetadataReference>>.Ok(metadataReferences);
    }

    private static async Task<Result<MetadataReference>> GetMetadataReference(string reference)
    {
        if (reference.EndsWith(".dll") || reference.EndsWith(".exe"))
            return Result<MetadataReference>.Ok(MetadataReference.CreateFromFile(reference));

        return Result<MetadataReference>.Fail($"Reference '{reference}' is not a valid assembly.");
    }
}