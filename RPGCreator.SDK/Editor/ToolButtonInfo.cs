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

using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.GlobalState;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Editor;

public interface IToolButtonInfo : ISerializable, IDeserializable
{
    string Name { get; set; }
}

[SerializingType("ToolButtonSeparator")]
public record struct ToolButtonSeparator() : IToolButtonInfo
{
    public string Name { get; set; } = "Separator";
    public SerializationInfo GetObjectData()
    {
        return new SerializationInfo(typeof(ToolButtonSeparator))
            .AddValue(nameof(Name), Name);
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        return [];
    }

    public void SetObjectData(DeserializationInfo info)
    {
        Name = "Separator";
    }
}

[SerializingType("ToolButtonInfo")]
public record struct ToolButtonInfo : IToolButtonInfo
{
    public bool HasValidTool;
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public URN ToolUrn { get; set; }
    
    public ToolLogic? GetTool()
    {
        if (ToolUrn == URN.Empty)
            return null;
        
        var toolRegistry = RegistryServices.ToolRegistry;
        if (!toolRegistry.HasTool(ToolUrn))
        {
            Logger.Warning($"Tool with URN {ToolUrn} not found in registry.");
            return null;
        }

        return toolRegistry.GetTool(ToolUrn);
    }
    
    public SerializationInfo GetObjectData()
    {
        return new SerializationInfo(typeof(ToolButtonInfo))
            .AddValue(nameof(Name), Name)
            .AddValue(nameof(Description), Description)
            .AddValue(nameof(Icon), Icon)
            .AddValue(nameof(ToolUrn), ToolUrn);
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        return [];
    }

    public void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue(nameof(Name), out string? name);
        
        info.TryGetValue(nameof(Description), out string? description);
        info.TryGetValue(nameof(Icon), out string? icon);
        info.TryGetValue(nameof(ToolUrn), out URN? toolUrn);
        
        Name = name ?? "Unknown Tool";
        Description = description ?? "No description found in data.";
        Icon = icon ?? "mdi-alert-circle";
        ToolUrn = toolUrn ?? URN.Empty;

        HasValidTool = false;
        if (ToolUrn != URN.Empty)
            HasValidTool = RegistryServices.ToolRegistry.HasTool(ToolUrn);
    }
}