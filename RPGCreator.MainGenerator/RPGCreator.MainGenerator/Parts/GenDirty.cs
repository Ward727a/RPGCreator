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

namespace RPGCreator.MainGenerator.Parts;

public static class GenDirty
{
    public static void Write(GenerationContext context)
    {
        if (!context.InClass)
        {
            GeneratorLogger.ErrorMisc("Cannot generate dirty code outside of a class",
                context.ClassData.Class.Locations.FirstOrDefault());
            return;
        }

        if (!context.ClassData.SupportDirtyFlag)
        {
            context.Writer.WriteLine("// Dirty flag set to false for this class.");
            return;
        }

        context.Writer.WriteLine("#region Dirty Flag Management");
        context.Writer.WriteLine();
        WriteEvent(context);
        context.Writer.WriteLine();
        WriteProperty(context);
        context.Writer.WriteLine();
        WriteMethod(context);
        context.Writer.WriteLine();
        context.Writer.WriteLine("#endregion");
    }

    private static void WriteEvent(GenerationContext context)
    {
        context.Writer.WriteLine("public event Action<IDirtyable> DirtyChanged;");
    }

    private static void WriteProperty(GenerationContext context)
    {
        context.Writer.WriteLine("[JsonIgnore]");
        context.Writer.WriteLine("public bool Dirty { get; private set; } = false;");
    }

    private static void WriteMethod(GenerationContext context)
    {
        context.Writer.WriteLine("public void MarkDirty()");
        context.Writer.WriteLine("{");
        context.Indent++;
        context.Writer.WriteLine("Dirty = true;");
        context.Writer.WriteLine("DirtyChanged?.Invoke(this);");
        context.Indent--;
        context.Writer.WriteLine("}");
        
        context.Writer.WriteLine();
        
        context.Writer.WriteLine("public void MarkClean()");
        context.Writer.WriteLine("{");
        context.Indent++;
        context.Writer.WriteLine("Dirty = false;");
        context.Writer.WriteLine("DirtyChanged?.Invoke(this);");
        context.Indent--;
        context.Writer.WriteLine("}");
    }
}