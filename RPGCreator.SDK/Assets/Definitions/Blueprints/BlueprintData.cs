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


using Newtonsoft.Json;

namespace RPGCreator.SDK.Assets.Definitions.Blueprints;

[JsonObject]
public class BlueprintData
{
    public Ulid Id { get; private set; } = Ulid.Empty;
    public string Name { get; set; } = string.Empty;
    public List<ConnectionData> Connections { get; private set; } = new List<ConnectionData>();
    public List<NodeData> Nodes { get; private set; } = new List<NodeData>();

    public static BlueprintData Create()
    {
        return new BlueprintData()
        {
            Id = Ulid.NewUlid(),
        };
    }

    public static BlueprintData Create(Ulid id)
    {
        return new BlueprintData()
        {
            Id = id,
        };
    }
}