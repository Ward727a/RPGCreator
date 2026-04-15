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

using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Graph.LOGIC;

public interface IConnectorLogic
{
    /// <summary>
    /// The unique identifier of the node that owns this connector.<br/>
    /// Please do not touch this property.<br/>
    /// The engine uses this property in an internal process while compiling the graph.
    /// </summary>
    Ulid RuntimeNodeId { get; set; }
    /// <summary>
    /// Unique identifier of the connector.<br/>
    /// Please do not touch this property.<br/>
    /// The engine uses this property in an internal process while compiling the graph.
    /// </summary>
    Ulid RuntimeId { get; set; }
    /// <summary>
    /// If this connector is linked from another connector, this is the ID of the other connector.
    /// Please do not touch this property.<br/>
    /// The engine uses this property in an internal process while compiling the graph.
    /// </summary>
    Ulid RuntimeParentConnector { get; set; }
    public bool RuntimeHasParent  => RuntimeParentConnector != Ulid.Empty;
    
    /// <summary>
    /// The number of connections allowed for this connector.<br/>
    /// Warning: This works only for connectors in 'out' directions. In 'in' directions, the number of connections is always 1.
    /// </summary>
    public int AllowedConnections { get; set; }
    Type ValueType { get; }
    
    /// <summary>
    /// If true, the user can manually input a value for this connector.<br/>
    /// This is only used for connectors in 'in' directions.
    /// </summary>
    public bool AllowManualInput { get; set; }
    
    URN Urn { get; }

    public string Title { get; set; }
    
    public object? RawValue => GetRawValue();

    object? GetRawValue();
    string GetStringValue();

    /// <summary>
    /// Sends a value to the connector.<br/>
    /// Depending on the connector type, the value may be converted to a different type or even be totally ignored (like the <see cref="RPGCreator.SDK.Graph.ConnectorLogics.ExecConnectorLogic">ExecConnectorLogic</see>).
    /// </summary>
    /// <param name="value">The value to send.</param>
    // Yes, I know, boxing bad, but boxing is the only way (I know of) to get a generic type from a generic method without lots of complex memory thing.
    // For now, it will do the work, later maybe we can implement a better way.
    // If you see this, and know a better way, you're welcome to open a PR! =) Ward.
    void SendValue(object? value);
    
    IConnectorLogic Clone();
}
    
public interface IConnectorLogic<T> : IConnectorLogic
{
    T? Value { get; set; }
}