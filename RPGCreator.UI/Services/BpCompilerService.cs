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
using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Graph.CompilerLogic;
using RPGCreator.SDK.Graph.LOGIC;
using RPGCreator.UI.Content.Blueprint;

namespace RPGCreator.UI.Services;

public class BpCompilerService : IBpCompilerService
{
    public string Compile(string blueprintFilePath)
    {
        return "COMPILING FROM A FILE PATH IS NOT IMPLEMENTED YET";
    }

    public string Compile(object blueprintData)
    {
        if(blueprintData is NodeEditorViewModel data)
            return Compile(data);
        else
            throw new ArgumentException("Unsupported blueprint data type");
    }

    private string Compile(NodeEditorViewModel vm)
    {
        var bpData = new BlueprintCompilerData();

        foreach (var nodeViewModel in vm.Nodes)
        {
            bpData.Nodes.Add(nodeViewModel.NodeLogic);
        }

        foreach (var connectionViewModel in vm.Connections)
        {
            var source = connectionViewModel.Source;
            var target = connectionViewModel.Target;
            var sourceConnectorId = source.Id;
            var targetConnectorId = target.Id;
            var sourceNodeId = source.NodeId;
            var targetNodeId = target.NodeId;
            bpData.Connections.Add(new ConnectionLogic(sourceConnectorId, targetConnectorId, sourceNodeId, targetNodeId));
        }

        var data = CodeCompiler.CompileToCSharp(bpData);
        
        return data;
    }
}