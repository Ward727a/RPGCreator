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

using System.Numerics;
using Newtonsoft.Json;
using RPGCreator.SDK.Graph.LOGIC;
using RPGCreator.SDK.Modules.Definition;
using RPGCreator.SDK.Serializer.Converter;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Assets.Definitions.Blueprints;

[JsonObject]
public class NodeData
{
    public Ulid Id { get; set; } = Ulid.Empty;
    public Vector2 Location { get; set; }
    [JsonConverter(typeof(StringUrnConverter))]
    public URN NodeUrn { get; set; } = URN.Empty;
    public bool IsFolded { get; set; } = false;
    public Dictionary<int, object?> ConnectorData { get; private set; } = new Dictionary<int, object?>();
    public CustomData StoredData { get; private set; } = new CustomData();

    public static NodeData Create(INodeLogic logic, bool isFolded = false, Vector2 location = default)
    {
        var nodeData = Create(logic.RuntimeId, logic.Urn, isFolded, location);
        var nodeStoredData = logic.CustomData;
        nodeData.StoredData = nodeStoredData;
        return nodeData;
    }

    public bool ShouldSerializeStoredData()
    {
        return StoredData.Keys.Any();
    }
    
    public bool ShouldSerializeConnectorData()
    {
        return ConnectorData.Count > 0;
    }
    
    public static NodeData Create(Ulid nodeRuntimeId, URN nodeUrn, bool isFolded = false, Vector2 location = default)
    {
        return new NodeData()
        {
            Id = nodeRuntimeId,
            NodeUrn = nodeUrn,
            IsFolded = isFolded,
            Location = location
        };
    }
}