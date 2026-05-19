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
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Graph.ConnectorLogics;

public abstract class BaseConnectorLogic<T> : IConnectorLogic<T>
{
    protected static UrnSingleModule UrnModule => "connector".ToUrnSingleModule();
    public abstract T? Value { get; set; }
    
    public Type ValueType => typeof(T);
    public virtual bool AllowManualInput { get; set; } = true;
    public abstract URN Urn { get; }
    public abstract string Title { get; set; }
    public Ulid RuntimeNodeId { get; set; }
    public Ulid RuntimeId { get; set; } = Ulid.NewUlid();
    public Ulid RuntimeParentConnector { get; set; } = Ulid.Empty;
    public virtual int AllowedConnections { get; set; } = 1;

    public object? GetRawValue()
    {
        return Value;
    }

    public virtual string GetStringValue() => Value?.ToString() ?? string.Empty;
    public void SendValue(object? value)
    {
        if(value is T typedValue)
            Value = typedValue;
    }

    public virtual IConnectorLogic Clone()
    {
        var clone = (BaseConnectorLogic<T>)MemberwiseClone();
        return clone;
    }
}