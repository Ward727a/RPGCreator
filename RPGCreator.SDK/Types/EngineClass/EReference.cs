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

using System.Text.Json.Serialization;
using CommunityToolkit.Diagnostics;

namespace RPGCreator.SDK.Types.EngineClass;

public class EReference<TValue> : IDisposable where TValue : EBaseClass
{
    private readonly Ulid _refId;
    private readonly URN _refClassUrn;
    
    [JsonIgnore]
    private TValue? _value;

    public bool IsNull => _value == null && _refId == Ulid.Empty;
    public bool IsResolved => _value != null;
    
    public EReference(TValue? value)
    {
        Guard.IsNotNull(value);
        _refId = value.Id;
        _refClassUrn = value.GetClassUrn();
        _value = value;
    }

    [JsonConstructor]
    public EReference(URN classUrn, Ulid id)
    {
        _refClassUrn = classUrn;
        _refId = id;
    }

    /// <summary>
    /// Return the value of the reference.
    /// </summary>
    /// <returns>
    /// The value of the reference, or an error if the reference is null or the value is not of the expected type.
    /// </returns>   
    public Result<TValue> GetValue()
    {
        if (IsNull)
        {
            return Result.Fail($"Reference {_refId} is null!");
        }
        
        if (!IsResolved)
        {
            if(!RegistryServices.References.HasReference(_refId))
                return Result.Fail($"Reference {_refId} not found!");
            var result = RegistryServices.References.GetReference(_refId);
            if (result.IsFailure)
            {
                return Result.Fail(result.Error);
            }

            if (result.Value is not TValue typedValue)
            {
                return Result.Fail($"Reference {_refId} is not of type {typeof(TValue)}");
            }

            _value = typedValue;
        }
        
        if(_value == null)
            return Result.Fail($"Reference {_refId} is null!");
        
        return Result<TValue>.Ok(_value);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not EReference<TValue> other)
            return false;

        return _refId == other._refId; // Compare Ulid, if both ulid are same, it's the same reference
    }

    protected bool Equals(EReference<TValue> other)
    {
        return _refId == other._refId;
    }

    public override string ToString()
    {
        if (IsNull)
            return $"Ref({_refClassUrn}){{ Id: [NULL], Value: [NULL] }}";
        if (!IsResolved)
            return $"Ref({_refClassUrn}){{ Id: {_refId}, Value: [NOT RESOLVED] }}";
        
        return $"Ref({_refClassUrn}){{ Id: {_refId}, Value: {_value} }}";
    }

    public override int GetHashCode()
    {
        return _refId.GetHashCode();
    }

    public static implicit operator TValue?(EReference<TValue> reference)
    {
        if(reference.IsNull)
            return null;
        
        var result = reference.GetValue();
        if (result.IsFailure)
        {
            throw new NullReferenceException($"Failed to resolve reference {reference._refId}: {result.Error}");
        }
        
        return result.Value;
    }

    public void Unload()
    {
        _value = null;
    }

    public void Dispose()
    {
        _value = null;
    }
}