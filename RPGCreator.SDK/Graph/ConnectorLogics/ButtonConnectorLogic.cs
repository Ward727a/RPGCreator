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

using CommunityToolkit.Mvvm.Input;
using RPGCreator.SDK.Graph.LOGIC;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Graph.ConnectorLogics;

/// <summary>
/// A special connector that is used to represent a button on a node.<br/>
/// This can allow the node to trigger events or actions when clicked.
/// </summary>
public class ButtonConnectorLogic(string title, Action onClick) : IConnectorLogic
{
    public Type ValueType => typeof(Action); // The UI will use this to display the button.
    public bool AllowManualInput { get; set; } = false; // Don't need to explain why we can't manually input a button connector.
    public URN Urn { get; }
    public string Title { get; set; } = title;
    public Ulid RuntimeNodeId { get; set; } = Ulid.Empty;
    public Ulid RuntimeId { get; set; } = Ulid.NewUlid();
    public Ulid RuntimeParentConnector { get; set; } = Ulid.Empty;
    public int AllowedConnections { get; set; } = 0; // So we hide the connector pin.

    public RelayCommand Command => new(onClick);
    
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
        Command?.Execute(null);
    }

    public IConnectorLogic Clone()
    {
        return (ButtonConnectorLogic)MemberwiseClone();
    }
}