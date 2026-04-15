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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using CommunityToolkit.Mvvm.Input;
using RPGCreator.SDK.Graph.ConnectorLogics;
using RPGCreator.SDK.Graph.LOGIC;
using RPGCreator.SDK.Types;

namespace RPGCreator.UI.Blueprints;

public class GenericNodeViewModel : BaseNodeViewModel
{
    public INodeLogic NodeLogic { get; }

    public GenericNodeViewModel(INodeLogic nodeLogic)
    {
        NodeLogic = nodeLogic.Clone();
        NodeLogic.RuntimeId = Ulid.NewUlid();
        
        for (int inputIndex = 0; inputIndex < NodeLogic.Inputs.Count; inputIndex++)
        {
            Inputs.Add(new GenericConnectorViewModel(NodeLogic, inputIndex, true)
            {
                NodeId = Id,
            });
        }

        for (int outputIndex = 0; outputIndex < NodeLogic.Outputs.Count; outputIndex++)
        {
            Outputs.Add(new GenericConnectorViewModel(NodeLogic, outputIndex, false)
            {
                NodeId = Id,
                IsOutput = true
            });
        }
    }
    
    public Ulid Id => NodeLogic.RuntimeId;

    public bool IsFolded
    {
        get;
        set => SetField(ref field, value);
    }
    
    public RelayCommand ToggleFoldedCommand => new(() => IsFolded = !IsFolded);

    public IRelayCommand<GenericConnectorViewModel> ButtonConnectorCommand =>
        new RelayCommand<GenericConnectorViewModel>(connector =>
            {
                if (connector.ConnectorLogic is ButtonConnectorLogic buttonLogic)
                {
                    buttonLogic.Command.Execute(null);
                }
            }
        );

    public string Title => NodeLogic?.Title ?? "Node";

    public URN HelpUrn => NodeLogic?.HelpUrn ?? URN.Empty;
    public bool HasHelpPage => HelpUrn != URN.Empty;
    
    public ObservableCollection<GenericConnectorViewModel> Inputs { get; set; } =
        new ObservableCollection<GenericConnectorViewModel>();

    public ObservableCollection<GenericConnectorViewModel> Outputs { get; set; } =
        new ObservableCollection<GenericConnectorViewModel>();
    
}