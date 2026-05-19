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

using System.Diagnostics.CodeAnalysis;
using RPGCreator.SDK.Graph.LOGIC;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Registry;

/// <summary>
/// This represents a connector template registration item.
/// </summary>
/// <param name="inTemplateObj">The template object for the input connector (must be an Avalonia DataTemplate).</param>
/// <param name="outTemplateObj">The template object for the output connector (must be an Avalonia DataTemplate).</param>
public struct ConnectorTemplateObjRegistrationItem(in object inTemplateObj, in object outTemplateObj)
{
    /// <summary>
    /// Need to be an Avalonia DataTemplate.
    /// </summary>
    public readonly object InConnectorTemplate = inTemplateObj;
    /// <summary>
    /// Need to be an Avalonia DataTemplate.
    /// </summary>
    public readonly object OutConnectorTemplate = outTemplateObj;
}

public interface IBpConnectorRegistry : IService
{
    void RegisterConnector(IConnectorLogic connector, bool overwriteIfExists = false);
    void UnregisterConnector(URN connectorUrn);
    void UnregisterConnector(IConnectorLogic connector);
    void UnregisterAllConnectors();
    
    IConnectorLogic? GetConnector(URN connectorUrn);
    bool TryGetConnector(URN connectorUrn, out IConnectorLogic? connector);
    
    bool HasConnector(URN connectorUrn);
    int ConnectorCount { get; }
    IEnumerable<IConnectorLogic> Connectors { get; }
    IEnumerable<URN> Urns { get; }

    int RegisterConnectorType(Type connectorType);
    public void UnregisterConnectorType(Type connectorType);
    public bool HasConnectorType(Type connectorType);
    public bool TryGetConnectorTypeId(Type connectorType, out int connectorTypeId);

    public void RegisterConnectorTemplate(int connectorTypeId, ConnectorTemplateObjRegistrationItem template, bool overwriteIfExists = false);

    public void UnregisterConnectorTemplate(int connectorTypeId);
    public bool HasConnectorTemplate(int connectorTypeId);
    public bool TryGetConnectorTemplate(int connectorTypeId, bool isInput, [NotNullWhen(true)] out object? templateObj);

    public int GetConnectorTypeId(Type connectorType);

    public object? GetConnectorTemplate(int connectorTypeId, bool isInput);
}