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
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Avalonia.Controls.Templates;
using RPGCreator.SDK.Graph.ConnectorLogics;
using RPGCreator.SDK.Graph.LOGIC;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Registry;
using RPGCreator.SDK.Types;

namespace RPGCreator.UI.Blueprints;

public struct ConnectorTemplateRegistrationItem(in IDataTemplate inTemplateObj, in IDataTemplate outTemplateObj)
{
    public readonly IDataTemplate InConnectorTemplate = inTemplateObj;
    public readonly IDataTemplate OutConnectorTemplate = outTemplateObj;
}

public class ConnectorRegistry : IBpConnectorRegistry
{
    private readonly Dictionary<URN, IConnectorLogic> _connectors = new();

    private readonly Dictionary<Type, int> _connectorTypeMapping = new()
    {
        { typeof(IComboBoxLogic), -3 },
        { typeof(Action), -2 },
        { typeof(void), 0 },
        { typeof(bool), 1 },
        { typeof(int), 2 },
        { typeof(float), 3 },
        { typeof(string), 4 },
        { typeof(Vector2), 5 }
    };
    
    private readonly Dictionary<int, ConnectorTemplateRegistrationItem> _connectorTemplates = new();
    
    public void RegisterConnector(IConnectorLogic connector, bool overwriteIfExists = false)
    {
        if (HasConnector(connector.Urn) && !overwriteIfExists)
            return;
        
        _connectors[connector.Urn] = connector;
    }

    public void UnregisterConnector(URN connectorUrn)
    {
        if (!HasConnector(connectorUrn))
            return;
        
        _connectors.Remove(connectorUrn);
    }

    public void UnregisterConnector(IConnectorLogic connector) => UnregisterConnector(connector.Urn);

    public void UnregisterAllConnectors() => _connectors.Clear();

    public IConnectorLogic? GetConnector(URN connectorUrn) => _connectors.GetValueOrDefault(connectorUrn);

    public bool TryGetConnector(URN connectorUrn, out IConnectorLogic? connector) => _connectors.TryGetValue(connectorUrn, out connector);

    public bool HasConnector(URN connectorUrn) => _connectors.ContainsKey(connectorUrn);

    public int ConnectorCount => _connectors.Count;
    public IEnumerable<IConnectorLogic> Connectors => _connectors.Values;
    public IEnumerable<URN> Urns => _connectors.Keys;
    public int RegisterConnectorType(Type connectorType)
    {
        if (_connectorTypeMapping.TryGetValue(connectorType, out var type))
            return type;
        
        _connectorTypeMapping[connectorType] = _connectorTypeMapping.Count;
        return _connectorTypeMapping[connectorType];
    }
    
    public void UnregisterConnectorType(Type connectorType) => _connectorTypeMapping.Remove(connectorType);
    public bool HasConnectorType(Type connectorType) => _connectorTypeMapping.ContainsKey(connectorType);
    public bool TryGetConnectorTypeId(Type connectorType, out int connectorTypeId) => _connectorTypeMapping.TryGetValue(connectorType, out connectorTypeId);

    public void RegisterConnectorTemplate(int connectorTypeId, ConnectorTemplateObjRegistrationItem template, bool overwriteIfExists = false)
    {
        
        var inTemplateObj = template.InConnectorTemplate;
        var outTemplateObj = template.OutConnectorTemplate;
        
        if(inTemplateObj is not IDataTemplate inDataTemplate || outTemplateObj is not IDataTemplate outDataTemplate)
        {
            Logger.Error("Invalid connector template type for {0}. Expected IDataTemplate, got {1}.", connectorTypeId, template.GetType().Name);
            return;
        }
        
        if (_connectorTemplates.ContainsKey(connectorTypeId) && !overwriteIfExists)
            return;
        
        _connectorTemplates[connectorTypeId] = new ConnectorTemplateRegistrationItem(inDataTemplate, outDataTemplate);
    }

    public void UnregisterConnectorTemplate(int connectorTypeId) => _connectorTemplates.Remove(connectorTypeId);
    public bool HasConnectorTemplate(int connectorTypeId) => _connectorTemplates.ContainsKey(connectorTypeId);
    public bool TryGetConnectorTemplate(int connectorTypeId, bool isInput, [NotNullWhen(true)] out object? templateObj)
    {
        if (_connectorTemplates.TryGetValue(connectorTypeId, out var template))
        {
            templateObj = isInput ? template.InConnectorTemplate : template.OutConnectorTemplate;
            return true;
        }
        templateObj = null;
        return false;
    }

    public int GetConnectorTypeId(Type connectorType)
    {
        if (_connectorTypeMapping.TryGetValue(connectorType, out var id))
            return id;
        
        return -1;
    }

    public object? GetConnectorTemplate(int connectorTypeId, bool isInput)
    {
        if (TryGetConnectorTemplate(connectorTypeId, isInput, out var templateObj))
            return templateObj;
        
        return null;
    }
}