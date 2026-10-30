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

using System.Diagnostics.CodeAnalysis;

namespace RPGCreator.Shared.Types;

/// <summary>
/// A helper class for managing validation states.<br/>
/// It can be used to store and manage errors and warnings related to validation for the UI.
/// </summary>
public class ValidationState
{
    private readonly Dictionary<string, string> _errors = new();
    private readonly Dictionary<string, string> _warnings = new();

    public IReadOnlyList<string> Errors => [.. _errors.Values];
    public IReadOnlyList<string> Warnings => [.. _warnings.Values];
    public bool HasErrors => _errors.Count > 0;
    public bool HasWarnings => _warnings.Count > 0;

    public void Clear()
    {
        _errors.Clear();
        _warnings.Clear();
    }

    public void AddError(string fieldName, string message) => _errors[fieldName] = message;
    public void AddWarning(string fieldName, string message) => _warnings[fieldName] = message;

    public void ClearField(string fieldName)
    {
        _errors.Remove(fieldName);
        _warnings.Remove(fieldName);
    }

    public bool TryGetError(string fieldName, [NotNullWhen(true)]out string? error) => _errors.TryGetValue(fieldName, out error);
    public bool TryGetWarning(string fieldName, [NotNullWhen(true)]out string? warning) => _warnings.TryGetValue(fieldName, out warning);
    
    public bool HasError(string fieldName) => _errors.ContainsKey(fieldName);
    public bool HasWarning(string fieldName) => _warnings.ContainsKey(fieldName);
}