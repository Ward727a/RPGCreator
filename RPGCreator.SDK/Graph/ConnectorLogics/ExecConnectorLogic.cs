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

using RPGCreator.SDK.Graph.LOGIC;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Graph.ConnectorLogics;

public class ExecConnectorLogic(string title = "") : IConnectorLogic
{
    public Type ValueType => typeof(void);
    public bool AllowManualInput { get; set; } = false; // Don't need to explain why we can't manually input a void connector.
    public URN Urn { get; }
    public string Title { get; set; } = title;
    public Ulid RuntimeNodeId { get; set; } = Ulid.Empty;
    public Ulid RuntimeId { get; set; } = Ulid.NewUlid();
    public Ulid RuntimeParentConnector { get; set; } = Ulid.Empty;
    public int AllowedConnections { get; set; } = 1;

    public object? GetRawValue()
    {
        return null;
    }

    public string GetStringValue()
    {
        return "";
    }

    public void SendValue(object? value)
    {
    }

    public IConnectorLogic Clone()
    {
        return (ExecConnectorLogic)MemberwiseClone();
    }
}