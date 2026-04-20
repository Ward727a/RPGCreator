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

using System.IO;
using System.Linq;

namespace RPGCreator.MainGenerator.Parts;

public static class GenSerialization
{
    public static void Write(GenerationContext context)
    {
        if (!context.InClass)
        {
            GeneratorLogger.ErrorMisc("Cannot write serialization code outside of a class",
                context.ClassData.Class.Locations.FirstOrDefault());
            return;
        }

        if (!context.ClassData.SupportSerialization)
        {
            context.Writer.WriteLine();
            context.Writer.WriteLine("// SupportSerialization set to false for this class.");
            context.Writer.WriteLine();
            return;
        }

        context.Writer.WriteLine("#region Serialization Management");
        context.Writer.WriteLine();
        WriteProperty(context);
        context.Writer.WriteLine();
        WriteMethod(context);
        context.Writer.WriteLine();
        WritePartial(context);
        context.Writer.WriteLine();
        context.Writer.WriteLine("#endregion");
    }

    private static void WriteProperty(GenerationContext context)
    {
        context.Writer.WriteLine("[JsonIgnore]");
        var pathParts = context.ClassData.SerializationPath.Split(['/', '|']);
        var path = "Assets";
        foreach (var pathPart in pathParts)
        {
            path = Path.Combine(path, pathPart);
        }
        context.Writer.WriteLine($"private static readonly string BaseSerializationFolder = @\"{path}\";");
    }

    private static void WriteMethod(GenerationContext context)
    {
        context.Writer.WriteLine($"public{(context.ClassData.ParentSupportsSerialization ? " override" : " virtual")} void Serialize()");
        context.Writer.WriteLine("{");
        context.Indent++;
        context.Writer.WriteLine("OnBeforeSerialize();");
        if (context.ClassData.SerializeInProjectFolder)
        {
            context.Writer.WriteLine("var project = GlobalStates.ProjectState.CurrentProject;");
            context.Writer.WriteLine("if (project == null)");
            context.Indent++;
            context.Writer.WriteLine("return;");
            context.Indent--;
            context.Writer.WriteLine($"var folder = Path.Combine(project.Path, {context.ClassData.Class.ToDisplayString()}.BaseSerializationFolder);");
        }
        else
        {
            context.Writer.WriteLine($"var folder = Path.Combine(RpgEnv.Path.ApplicationData, {context.ClassData.Class.ToDisplayString()}.BaseSerializationFolder);");
        }
        context.Writer.WriteLine("if(Directory.Exists(folder) == false)");
        context.Writer.Indent++;
        context.Writer.WriteLine("Directory.CreateDirectory(folder);");
        context.Writer.Indent--;
        context.Writer.WriteLine(
            "EngineServices.Serializer.SerializeTo(this, Path.Combine(folder, $\"{Id}.json\"));");

        context.Indent--;
        context.Writer.WriteLine("}");
        
        context.Writer.WriteLine();
        
        context.Writer.WriteLine($"public{(context.ClassData.ParentSupportsSerialization ? " override" : " virtual")} void OnBeforeSerialize()");
        context.Writer.WriteLine("{");
        context.Indent++;
        context.Writer.WriteLine("_onBeforeSerialize();");
        context.Indent--;
        context.Writer.WriteLine("}");
    }

    private static void WritePartial(GenerationContext context)
    {
        context.Writer.WriteLine("partial void _onBeforeSerialize();");
    }
}