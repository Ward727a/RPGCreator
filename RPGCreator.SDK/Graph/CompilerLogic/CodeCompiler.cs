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
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Logging;

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
    
    public static string CompileToCSharp(BlueprintCompilerData data)
    {
        var usings = new SortedSet<string>();
        var (success, startNode, inDegree, metadataMap) = CodePreCompiler.SortConnectors(data);

        var codeContext = new CodeContext(data, metadataMap);
        
        var debugInfo = _config.GetBool("compile_debug_info", true);
        if (debugInfo)
        {
            codeContext.InDebugMode = true;
            usings.Add("System.Diagnostics");
            usings.Add("System.Globalization");
            usings.Add("CommunityToolkit.Diagnostics");
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
            codeContext.AddUsing = s =>
            {
                usings.Add(s);
            };
        }

        try
        {
            var sb = new StringBuilder();
            using var stringWriter = new StringWriter(sb);
            using IndentedTextWriter writer = new IndentedTextWriter(stringWriter, "    ");
            {

                writer.WriteLine($"public class bp_class_{startNode.RuntimeId} : BaseCompiledLogic");
                writer.WriteLine("{");
                writer.Indent++;
                writer.WriteLine("public override void Execute(ICompiledBpContext context)");
                writer.WriteLine("{");
                writer.Indent++;
                foreach (var pure in inDegree.Where(d => d.Value == 0))
                {
                    if (pure.Key == startNode.RuntimeId) continue;
                    codeContext.GeneratePure(codeContext, writer, pure.Key);
                }

                codeContext.GenerateCode(codeContext, writer, startNode.RuntimeId);
            }

            writer.Indent--;
            writer.WriteLine("}");
            writer.Indent--;
            writer.WriteLine("}");
            if (writer.Indent > 0)
            {
                Logger.Error("Indentation error in EndNode.GenerateCode");
                Logger.Error("Indentation: " + writer.Indent + " awaited 0.");
                Logger.Error("Please report this issue to the RPG Creator team ONLY if you do not have any external plugin installed!");
                return "/// ERROR ON CODE GENERATION: Indentation error. Please report this issue to the RPG Creator team ONLY if you do not have any external plugin installed!";
            }
        
        
            var finalCompiledCode = new StringBuilder();
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
        
            return finalCompiledCode.ToString();
        }
        catch(Exception e)
        {
            Logger.Error(e, "Error while compiling blueprint.");
            return $"ERROR WHILE COMPILING BLUEPRINT.\n{e.Message}";
        }

    }
}