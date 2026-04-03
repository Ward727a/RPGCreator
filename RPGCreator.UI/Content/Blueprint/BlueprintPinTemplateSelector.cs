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

using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using RPGCreator.SDK;
using RPGCreator.SDK.Registry;
using RPGCreator.UI.Blueprints;

namespace RPGCreator.UI.Content.Blueprint;

public class BlueprintPinTemplateSelector : IDataTemplate
{
    private static IBpConnectorRegistry _registry => RegistryServices.BpConnector;
    
    public Dictionary<string, IDataTemplate> InAvailableTemplates { get; } = new();
    public Dictionary<string, IDataTemplate> OutAvailableTemplates { get; } = new();
    
    public Control? Build(object? param)
    {

        var error = new TextBlock()
        {
            Text = "ERROR"
        };
        
        if (param is GenericConnectorViewModel vm)
        {
            return vm.IsOutput ? BuildOutput(vm) : BuildInput(vm);
        }

        ToolTip.SetTip(error, $"The connector given as parameter is not a valid connector type.");
        return error;
    }

    private Control? BuildInput(GenericConnectorViewModel vm)
    {
        var error = new TextBlock()
        {
            Text = "ERROR"
        };
        
        int type = vm.Type;
            
        if (InAvailableTemplates.TryGetValue(vm.Type.ToString(), out var innerTemplate))
        {
            return innerTemplate.Build(vm);
        }
            
        if (_registry.TryGetConnectorTemplate(type, true, out var templateObj))
        {
            if (templateObj is IDataTemplate externalTemplate)
            {
                return externalTemplate.Build(vm);
            }
            ToolTip.SetTip(error, $"The template for connector type {type} is not a valid data template.\nGot: {templateObj.GetType().Name} expected: IDataTemplate.");
            return error;
        }
        ToolTip.SetTip(error, $"The type of the connector given as parameter ({type}) is not registered in the connector template registry.");
        return error;
    }

    private Control? BuildOutput(GenericConnectorViewModel vm)
    {
        var error = new TextBlock()
        {
            Text = "ERROR"
        };
        
        int type = vm.Type;
            
        if (OutAvailableTemplates.TryGetValue(vm.Type.ToString(), out var innerTemplate))
        {
            return innerTemplate.Build(vm);
        }
            
        if (_registry.TryGetConnectorTemplate(type, false, out var templateObj))
        {
            if (templateObj is IDataTemplate externalTemplate)
            {
                return externalTemplate.Build(vm);
            }
            ToolTip.SetTip(error, $"The template for connector type {type} is not a valid data template.\nGot: {templateObj.GetType().Name} expected: IDataTemplate.");
            return error;
        }
        ToolTip.SetTip(error, $"The type of the connector given as parameter ({type}) is not registered in the connector template registry.");
        return error;
    }

    public bool Match(object? data)
    {
        return data is GenericConnectorViewModel;
    }
}