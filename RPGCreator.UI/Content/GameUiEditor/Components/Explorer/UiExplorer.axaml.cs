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
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RPGCreator.RTP.GameUI;
using RPGCreator.SDK.GameUI;
using RPGCreator.SDK.GameUI.Controls;
using RPGCreator.SDK.GameUI.Interfaces;
using RPGCreator.SDK.Logging;
using Ursa.Controls;

namespace RPGCreator.UI.Content.GameUiEditor.Components.Explorer;

public class UiExplorerVm
{

    private UiEditorContext _context;
    public UiTreeExplorerVm TreeExplorerVm { get; }

    public UiExplorerVm(UiEditorContext context, IEnumerable<BaseControl> rootControls)
    {
        _context = context;
        TreeExplorerVm = new UiTreeExplorerVm(context, rootControls);
    }
    
}

public partial class UiExplorer : UserControl
{
    private UiEditorContext _context;
    
    public UiExplorerVm Vm { get; set; } = null!;
    
    public UiExplorer(UiEditorContext context)
    {
        _context = context;
        _context.EditorReady += () =>
        {
            Vm = new UiExplorerVm(context, context.RootControls);
            
            _context.RootControlAdded += (sender, args) =>
            {
                if (Equals(sender, this)) return;
                if (args.IsCanceled) return;
                var control = args.Control;
                
                Vm.TreeExplorerVm.RootsControls.Add(new(control, Vm.TreeExplorerVm));
            };
            _context.RootControlRemoved += (sender, args) =>
            {
                if (Equals(sender, this)) return;
                if (args.IsCanceled) return;
                var control = args.Control;
                
                Vm.TreeExplorerVm.RootsControls.Remove(Vm.TreeExplorerVm.RootsControls.First(x => x.Model == control));
            };
            
            _context.ControlSelected += (sender, args) =>
            {
                if (Equals(sender, this)) return;
                if (args.IsCanceled) return;
                var control = args.Control;
                
                Vm.TreeExplorerVm.SelectControl(control);
            };
            
            DataContext = Vm;
            InitializeComponent();
        };
    }

    private void OnControlSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0) return;
        var o = e.AddedItems[0];
        if(o is not UiBaseControlVm vm) return;
        _context.RaiseControlSelected(this, vm.Model);
        Vm.TreeExplorerVm.SelectControl(vm.Model);
    }

    private async void EditEventClicked(object? sender, RoutedEventArgs e)
    {
        if(sender is not Button button || button.DataContext is not ControlEventDescriptor eventDescriptor) return;
        await new DialogEditEventControl(eventDescriptor).ShowDialog(TopLevel.GetTopLevel(this) as Window ?? throw new InvalidOperationException());
    }
}