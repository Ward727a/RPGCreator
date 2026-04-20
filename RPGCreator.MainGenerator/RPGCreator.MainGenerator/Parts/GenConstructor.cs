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

public static class GenConstructor
{
    public static void Write(GenerationContext context)
    {
        if (!context.InClass)
        {
            GeneratorLogger.ErrorMisc("Cannot write constructor outside of a class.", context.ClassData.Class.Locations.FirstOrDefault());
            return;
        }
        
        context.Writer.WriteLine("private " + context.ClassData.Class.Name + "()");
        context.Writer.WriteLine("{");
        context.Indent++;
        {
            context.Writer.WriteLine("Id = Ulid.NewUlid();");
            context.Writer.WriteLine($"Urn = new URN(ClassUrn, Id.ToString());");
        }
        context.Indent--;
        context.Writer.WriteLine("}");
        
        context.Writer.WriteLine("private " + context.ClassData.Class.Name + "(Ulid id)");
        context.Writer.WriteLine("{");
        context.Indent++;
        
        context.Writer.WriteLine($"Id = id;");
        context.Writer.WriteLine($"Urn = new URN(ClassUrn, Id.ToString());");
        
        context.Indent--;
        context.Writer.WriteLine("}");
        
        context.Writer.WriteLine("[JsonConstructor]");
        context.Writer.WriteLine("private " + context.ClassData.Class.Name + "(Ulid id, URN urn)");
        context.Writer.WriteLine("{");
        context.Indent++;
        
        context.Writer.WriteLine($"Id = id;");
        context.Writer.WriteLine($"Urn = urn;");
        
        context.Indent--;
        context.Writer.WriteLine("}");
    }
}