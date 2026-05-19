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
using RPGCreator.SDK;
using RPGCreator.SDK.Graph.LOGIC;
using RPGCreator.SDK.Registry;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;

namespace RPGCreator.UI.Blueprints;

public class NodeRegistry : IBpNodesRegistry
{
    private readonly Dictionary<URN, INodeLogic> _nodes = new();
    private readonly Dictionary<URN, GenericNodeViewModel> _nodeViewModels = new();

    public void RegisterNode(INodeLogic node, bool overwriteIfExists = false)
    {
        if (HasNode(node.Urn) && !overwriteIfExists)
            return;
        _nodes[node.Urn] = node;
        _nodeViewModels[node.Urn] = new GenericNodeViewModel(node);
        CheckConnectorsToRegister(node);
    }

    private static void CheckConnectorsToRegister(INodeLogic node)
    {
        foreach (var output in node.Outputs)
        {
            RegisterConnector(output);
        }
        foreach (var input in node.Inputs)
        {
            RegisterConnector(input);
        }

        return;

        void RegisterConnector(IConnectorLogic connector)
        {
            if (RegistryServices.BpConnector.HasConnector(connector.Urn))
                return;
            RegistryServices.BpConnector.RegisterConnector(connector);
        }
    }
    
    public GenericNodeViewModel GetNodeViewModel(URN nodeUrn)
    {
        if(!HasNode(nodeUrn))
            throw new ArgumentException("Node not found", nameof(nodeUrn));

        return new GenericNodeViewModel(GetNode(nodeUrn)!);
    }

    public void UnregisterNode(URN nodeUrn)
    {
        if(!HasNode(nodeUrn)) return;
        _nodes.Remove(nodeUrn);
        _nodeViewModels.Remove(nodeUrn);
    }

    public void UnregisterNode(INodeLogic node) => UnregisterNode(node.Urn);

    public void UnregisterAllNodes()
    {
        _nodes.Clear();
        _nodeViewModels.Clear();
    }

    public INodeLogic? GetNode(URN nodeUrn)
    {
        return _nodes.GetValueOrDefault(nodeUrn);
    }

    public bool TryGetNode(URN nodeUrn, out INodeLogic? node) => _nodes.TryGetValue(nodeUrn, out node);

    public bool HasNode(URN nodeUrn) => _nodes.ContainsKey(nodeUrn);

    public int NodeCount => _nodes.Count;

    public IEnumerable<INodeLogic> Nodes => _nodes.Values;

    public IEnumerable<URN> Urns => _nodes.Keys;
}