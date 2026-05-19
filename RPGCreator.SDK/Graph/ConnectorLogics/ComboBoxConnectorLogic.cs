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

using System.Collections;
using CommunityToolkit.Mvvm.Input;
using RPGCreator.SDK.Graph.LOGIC;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Graph.ConnectorLogics;

/// <summary>
/// Marker interface for ComboBox connectors.
/// </summary>
public interface IComboBoxLogic
{
}

[Obsolete("Do not use this connector, as it's not implemented yet. It may be removed in future versions without a deprecation period.")]
public class ComboBoxConnectorLogic(string title, IEnumerable? items, Action<int> onSelectionChanged) : IConnectorLogic
{
    public Type ValueType => typeof(IComboBoxLogic);
    public bool AllowManualInput { get; set; } = false; // Don't need to explain why we can't manually input a button connector.
    public URN Urn { get; }
    public string Title { get; set; } = title;
    public Ulid RuntimeNodeId { get; set; } = Ulid.Empty;
    public Ulid RuntimeId { get; set; } = Ulid.NewUlid();
    public Ulid RuntimeParentConnector { get; set; } = Ulid.Empty;
    public int AllowedConnections { get; set; } = 0; // So we hide the connector pin.

    public int SelectedIndex
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Command?.Execute(SelectedIndex);
        }
    } = 0;

    public IEnumerable? Items { get; set; } = items;

    private object? SelectedItem;
    
    public RelayCommand<int> Command => new(onSelectionChanged);
    
    public object? GetRawValue()
    {
        return Items;
    }

    public string GetStringValue()
    {
        return "";
    }

    public void SendValue(object? value)
    {
        SelectedItem = value;
    }

    public IConnectorLogic Clone()
    {
        return (ButtonConnectorLogic)MemberwiseClone();
    }
}