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

using System.Text.Json;
using System.Text.Json.Serialization;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Types;

public enum ESerializableDataType
{
    None,
    Type,
    Class
}

public class SerializableData
{
    protected ESerializableDataType DataType { get; set; } = ESerializableDataType.None;
    
    public SerializableData()
    {
    }

    public SerializableData(Type type, object? data)
    {
        if(RegistryServices.Types.HasType(type))
            DataType = ESerializableDataType.Type;
        
        if(ClassesRegistry.HasType(type))
            DataType = ESerializableDataType.Class;
        
        if (DataType == ESerializableDataType.None)
        {
            Logger.Critical("Cannot create SerializableData for type {0} because it is not registered in the TypesRegistry nor in the ClassesRegistry.", type.Name);
            Logger.Critical("Please ensure the type {0} is registered in the TypesRegistry or in the ClassesRegistry.", type.Name);
            Logger.Critical("Either by scanning the asm, or register it manually.");
            throw new Exception($"Cannot create SerializableData for type {type.Name} because it is not registered in the TypesRegistry nor in the ClassesRegistry.");
        }

        var urnType = GetTypeUrn(type);

        if (urnType.Equals(URN.Empty))
        {
            Logger.Critical("Cannot create SerializableData for type {0} because its URN is empty.", type.Name);
            Logger.Critical("Please report this error to the RPG Creator team.");
            throw new Exception($"Cannot create SerializableData for type {type.Name} because its URN is empty.");
        }
        
        UrnType = urnType;
        ObjData = data;
    }

    protected URN GetTypeUrn(Type type)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type), "Type cannot be null.");
        }
        
        return DataType switch
        {
            ESerializableDataType.Type => RegistryServices.Types.GetKey(type).Value,
            ESerializableDataType.Class => ClassesRegistry.GetUrn(type).Value,
            _ => throw new Exception($"Cannot get URN for type {type.Name} because its data type is not recognized.")
        };
    }

    [JsonConstructor]
    public SerializableData(URN UrnType, object? ObjData)
    {
        if(RegistryServices.Types.HasKey(UrnType))
            DataType = ESerializableDataType.Type;
        
        if(ClassesRegistry.HasUrn(UrnType))
            DataType = ESerializableDataType.Class;
        
        if (DataType == ESerializableDataType.None)
        {
            Logger.Critical("Cannot create SerializableData for urn {0} because it is not registered in the TypesRegistry.", UrnType);
            Logger.Critical("Please ensure the urn {0} is registered in the TypesRegistry.", UrnType);
            Logger.Critical("Either by scanning the asm, or register it manually.");
            throw new Exception($"Cannot create SerializableData for urn {UrnType} because it is not registered in the TypesRegistry.");
        }
        
        this.UrnType = UrnType;

        if (ObjData is not JsonElement jsonElement)
        {
            this.ObjData = ObjData;
            return;
        }
        
        var returnType = this.Type;
        if (returnType != null)
            this.ObjData = EngineServices.Serializer.Deserialize(jsonElement.GetRawText(), returnType);
        else
        {
            Logger.Critical("Cannot create SerializableData for urn {0} because it is not registered in the TypesRegistry.", UrnType);
            Logger.Critical("Please ensure the urn {0} is registered in the TypesRegistry.", UrnType);
            Logger.Critical("Either by scanning the asm, or register it manually.");
            throw new Exception($"Cannot create SerializableData for urn {UrnType} because it is not registered in the TypesRegistry.");
        }
    }
    
    [JsonInclude]
    [JsonPropertyName("Value")]
    protected object? ObjData { get; set; }
    
    [JsonPropertyName("Type")]
    public URN UrnType { get; protected set; }

    [JsonIgnore]
    public Type? Type
    {
        get
        {
            if (field is null)
            {
                if (DataType == ESerializableDataType.Type)
                {
                    var result = RegistryServices.Types.GetType(UrnType);
                    if (result.IsSuccess)
                    {
                        field = result.Value;
                        return field;
                    }

                    Logger.Error("Failed to get type for urn {0}, error: {1}", UrnType, result.Error);
                    return null;
                }

                if (DataType == ESerializableDataType.Class)
                {
                    var result = ClassesRegistry.GetType(UrnType);
                    if (result.IsSuccess)
                    {                        
                        field = result.Value;
                        return field;
                    }
                    Logger.Error("Failed to get class for urn {0}, error: {1}", UrnType, result.Error);
                    return null;
                }
            }
            return field;
        }
    } = null;

    public Result<TDataType> GetValue<TDataType>()
    {
        if (ObjData is TDataType data)
        {
            return Result<TDataType>.Success(data);
        }
        
        return Result<TDataType>.Failure($"Failed to cast value to {typeof(TDataType).Name}");
    }
    
    internal object? GetDirectData() => ObjData;
    
    public bool IsNull() => ObjData == null;
    
    public void SetData<TDataType>(TDataType data)
    {
        ObjData = data;
    }
}

public class SerializableData<TDataType> : SerializableData
{
    private static ITypesRegistry TypesRegistry => RegistryServices.Types;
    
    public SerializableData()
    {
        var type = typeof(TDataType);
        
        if(RegistryServices.Types.HasType(type))
            DataType = ESerializableDataType.Type;
        
        if(ClassesRegistry.HasType(type))
            DataType = ESerializableDataType.Class;
        
        if (DataType == ESerializableDataType.None)
        {
            Logger.Critical("Cannot create SerializableData for type {0} because it is not registered in the TypesRegistry nor in the ClassesRegistry.", type.Name);
            Logger.Critical("Please ensure the type {0} is registered in the TypesRegistry or in the ClassesRegistry.", type.Name);
            Logger.Critical("Either by scanning the asm, or register it manually.");
            throw new Exception($"Cannot create SerializableData for type {type.Name} because it is not registered in the TypesRegistry nor in the ClassesRegistry.");
        }

        var urnType = GetTypeUrn(type);

        if (urnType.Equals(URN.Empty))
        {
            Logger.Critical("Cannot create SerializableData for type {0} because its URN is empty.", type.Name);
            Logger.Critical("Please report this error to the RPG Creator team.");
            throw new Exception($"Cannot create SerializableData for type {type.Name} because its URN is empty.");
        }
        
        UrnType = urnType;
    }
    
    public SerializableData(TDataType data) : this()
    {
        ObjData = data;
    }

    [JsonIgnore]
    public TDataType? Data
    {
        get => (TDataType?) ObjData;
        set => ObjData = value;
    }
    
    public Result<TDataType> GetValue()
    {
        return base.GetValue<TDataType>();
    }
    
    public void SetData(TDataType data)
    {
        ObjData = data;
    }
}