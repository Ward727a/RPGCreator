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

namespace RPGCreator.MainGenerator;

public static class GeneratorLogger
{
    private static SourceProductionContext? _spc;
    public static void Init(SourceProductionContext spc)
    {
        _spc = spc;
    }
    
    private static DiagnosticDescriptor _warningMisc = new DiagnosticDescriptor(
        "RPG001",
        "RPG Creator Warning",
        "RPG Creator Warning: {0}",
        "RPG Creator",
        DiagnosticSeverity.Warning,
        true);

    private static DiagnosticDescriptor _errorMisc = new DiagnosticDescriptor(
        "RPG002",
        "RPG Creator Error",
        "RPG Creator Error: {0}",
        "RPG Creator",
        DiagnosticSeverity.Error,
        true
    );

    private static DiagnosticDescriptor _infoMisc = new DiagnosticDescriptor(
        "RPG003",
        "RPG Creator Info",
        "RPG Creator Info: {0}",
        "RPG Creator",
        DiagnosticSeverity.Info,
        true
    );

    public static void ErrorMisc(string message, Location? location = null)
    {
        Log(_errorMisc, location ?? Location.None, message);
    }
    public static void WarningMisc(string message, Location? location = null)
    {
        Log(_warningMisc, location ?? Location.None, message);
    }
    public static void InfoMisc(string message, Location? location = null)
    {
        Log(_infoMisc, location ?? Location.None, message);
    }
    
    public static void Log(Diagnostic diagnostic) => _spc?.ReportDiagnostic(diagnostic);
    public static void Log(DiagnosticDescriptor descriptor, Location location, params object[] args) => Log(Diagnostic.Create(descriptor, location, args));
}