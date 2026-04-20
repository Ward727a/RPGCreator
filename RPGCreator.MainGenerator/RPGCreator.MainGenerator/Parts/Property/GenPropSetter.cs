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

namespace RPGCreator.MainGenerator.Parts.Property;

public static class GenPropSetter
{
    public static void Write(GenerationContext context, EPropertyData data)
    {
        context.Writer.WriteLine($"public void Set{GenProperties.FormatPublicName(data.Field.Name)}({data.Field.Type.ToDisplayString()} value)");
        context.Writer.WriteLine("{");
        context.Indent++;
            context.Writer.WriteLine($"if (Validate{GenProperties.FormatPublicName(data.Field.Name)}(value))");
            context.Writer.WriteLine("{");
            context.Indent++;
                context.Writer.WriteLine($"value = Coerce{GenProperties.FormatPublicName(data.Field.Name)}(value);");
                context.Writer.WriteLine($"_onSet{GenProperties.FormatPublicName(data.Field.Name)}(ref value);");
                context.Writer.WriteLine($"{data.Field.Name} = value;");
                if (context.ClassData.SupportDirtyFlag && data.Dirtying)
                {
                    context.Writer.WriteLine("MarkDirty();");
                }
            context.Indent--;
            context.Writer.WriteLine("}");
        context.Indent--;
        context.Writer.WriteLine("}");
        context.Writer.WriteLine();
        WritePartial(context, data);
    }
    
    private static void WritePartial(GenerationContext context, EPropertyData data)
    {
        context.Writer.WriteLine(
            $"partial void _onSet{GenProperties.FormatPublicName(data.Field.Name)}(ref {data.Field.Type.ToDisplayString()} value);");
    }
}