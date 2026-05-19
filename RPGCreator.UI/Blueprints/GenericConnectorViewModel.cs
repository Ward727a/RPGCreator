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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using RPGCreator.SDK;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Graph.LOGIC;
using RPGCreator.UI.Content.Blueprint;

namespace RPGCreator.UI.Blueprints;

public class GenericConnectorViewModel : INotifyPropertyChanged
{
    public INodeLogic ParentNodeLogic { get; init; }
    public int ConnectorIndex { get; init; }
    public bool IsInput { get; init; }
    public IConnectorLogic ConnectorLogic =>
        IsInput ? ParentNodeLogic.Inputs[ConnectorIndex] : ParentNodeLogic.Outputs[ConnectorIndex];

    public GenericConnectorViewModel(INodeLogic nodeLogic, int connectorIndex, bool isInput)
    {
        ParentNodeLogic = nodeLogic;
        ConnectorIndex = connectorIndex;
        IsInput = isInput;
        ConnectorLogic.RuntimeId = Ulid.NewUlid();
        Type = RegistryServices.BpConnector.GetConnectorTypeId(ConnectorLogic.ValueType);
        if (Type == -1)
        {
            Logger.Error($"Unknown connector type for {ConnectorLogic.ValueType}");
            Type = 0;
            Logger.Error("Defaulting to 'exec' connector type (0).");
        }
    }
    
    public Ulid Id => ConnectorLogic.RuntimeId;

    public Ulid NodeId
    {
        get => ConnectorLogic.RuntimeNodeId;
        set => ConnectorLogic.RuntimeNodeId = value;
    }

    public Ulid ParentNodeId
    {
        get => ConnectorLogic.RuntimeParentConnector;
        set => ConnectorLogic.RuntimeParentConnector = value;
    }
    
    public bool HasParent => ConnectorLogic.RuntimeHasParent;
    
    public int AllowedConnections => ConnectorLogic.AllowedConnections;
    public bool HasAnyAllowedConnections => AllowedConnections > 0;
    
    public int Type { get; init; }

    public bool IsOutput { get; set; } = false;

    /// <summary>
    /// Only true if:
    /// - The connector is an 'in' connector
    /// - The connector has no connections
    /// - The connector logic has AllowManualInput set to true
    /// If on true, then the user can manually input a value.
    /// If on false, then the user cannot manually input a value.
    /// </summary>
    public bool CanHaveManualInput => ConnectorLogic.AllowManualInput && !IsOutput && TotalConnections == 0;
    
    public object? Value
    {
        get => ConnectorLogic.GetRawValue();
        set
        {
            if (Equals(ConnectorLogic.GetRawValue(), value)) return;
        
            ConnectorLogic.SendValue(value);
        
            OnPropertyChanged();
        }
    }

    public List<ConnectionViewModel> Connections { get; } = new();
    public int TotalConnections => Connections.Count;

    public Point Anchor
    {
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Anchor)));
        }
        get;
    }

    public bool IsConnected
    {
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnected)));
        }
        get;
    }

    public string Title => ConnectorLogic.Title;
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}