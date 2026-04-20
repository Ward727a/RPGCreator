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
using Microsoft.CodeAnalysis;

namespace RPGCreator.MainGenerator.Parts;

public static class GenClass
{
    public static void Begin(GenerationContext context)
    {
        context.Writer.WriteLine(
            $"public partial class {context.ClassData.Class.Name} : {GenerateHeritage(context)}");
        context.Writer.WriteLine("{");
        context.Indent++;
        context.InClass = true;
    }

    public static string GenerateHeritage(GenerationContext context)
    {
        List<string> interfaces = [];
        if (!context.ClassData.ParentIsEBaseClass)
        {
            interfaces.Add("EBaseClass");
            interfaces.Add("IEBaseClassStatic");
        }

        interfaces.AddRange([$"IFactorable<{context.ClassData.Class.Name}>"]);
        
        if(context.ClassData is { SupportDirtyFlag: true, ParentSupportsDirtyFlag: false })
            interfaces.Add("IDirtyable");
        
        if(context.ClassData is { SupportSerialization: true, ParentSupportsSerialization: false })
            interfaces.Add($"ISerializable<{context.ClassData.Class.Name}>");
        
        return string.Join(", ", interfaces);
    }
    
    public static void End(GenerationContext context)
    {
        context.Indent--;
        context.Writer.WriteLine("}");
        context.InClass = false;
    }
}