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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Graph.ConnectorLogics;
using RPGCreator.SDK.Graph.LOGIC;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;
using RPGCreator.UI.Content.Blueprint;

namespace RPGCreator.UI.Blueprints.Nodes.Variables;


public class GetVarNode(Ulid parameterUlid) : BaseNodeLogic
{
    private Ulid _parameterUlid = parameterUlid;
    private BlueprintParameters? _parameters;
    public override FlowType FlowType => FlowType.Pure;
    public override string Title => $"Get {_parameters?.Name ?? "[ERROR]"}";
    public override PipedPath Category { get; } = SpecialCategory.Extend("Variables");
    public override URN Urn => DefaultUrnModule.ToUrnModule("rpgc").ToUrn("get_variable");
    public override IReadOnlyList<IConnectorLogic> Outputs { get; set; } = [ ];

    public override void PlacedInGraph(object? nodeEditorViewModel)
    {
        if (CustomData.Has("parameter"))
        {
            _parameterUlid = CustomData.GetAs<Ulid>("parameter");
        }
        
        if (_parameterUlid != Ulid.Empty)
        {
            CustomData.Set("parameter", _parameterUlid);
        }
        else
        {
            Logger.Error($"GetVarNode with RuntimeId {RuntimeId} has an empty parameter Ulid. This node will not function correctly.");
            return;
        }
        
        if (nodeEditorViewModel is not NodeEditorViewModel editor)
            return;

        var parameterVm = editor.ParametersVm;
        if (parameterVm.ParametersCache.Keys.Contains(_parameterUlid))
        {
            var optionalParam = parameterVm.ParametersCache.Lookup(_parameterUlid);
            
            if(!optionalParam.HasValue)
                throw new ArgumentException($"Parameter with Ulid {_parameterUlid} not found in the blueprint parameters.");
            
            _parameters = optionalParam.Value;
            
        }

        if (_parameters == null)
        {
            Logger.Error($"GetVarNode with RuntimeId {RuntimeId} has an empty parameter. This node will not function correctly.");
            return;
        }

        switch (Type.GetTypeCode(_parameters.Type))
        {
            case TypeCode.Int32:
            {
                Outputs = [new IntConnectorLogic("Value")];
                break;
            }
            case TypeCode.Single:
            {
                Outputs = [new FloatConnectorLogic("Value")];
                break;
            }
            case TypeCode.String:
            {
                Outputs = [new StringConnectorLogic("Value")];
                break;
            }
            case TypeCode.Boolean:
            {
                Outputs = [new BoolConnectorLogic("Value")];
                break;
            }
            default:
            {
                Logger.Error($"GetVarNode with RuntimeId {RuntimeId} has an unsupported parameter type {_parameters.Type}. This node will not function correctly.");
                return;
            }
        }
    }

    public override void GenerateCode(CodeContext context, IndentedTextWriter writer)
    {
        string varName = $"global_var_{_parameterUlid}";
        if (_parameters.IsArgument)
        {
            varName = $"bp_arg_{_parameterUlid}";
        }
        context.SetVariableName(Outputs[0].RuntimeId, varName);
    }
}