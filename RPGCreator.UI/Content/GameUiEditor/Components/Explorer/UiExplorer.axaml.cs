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
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.GameUI;
using RPGCreator.SDK.GameUI.Controls;
using RPGCreator.UI.Common;
using RPGCreator.UI.Content.Blueprint;

namespace RPGCreator.UI.Content.GameUiEditor.Components.Explorer;

public partial class UiExplorerVm : ViewModelBase
{

    [ObservableProperty] private UiEditorContext _context;
    public UiTreeExplorerVm TreeExplorerVm { get; }

    public UiExplorerVm(UiEditorContext context, IEnumerable<BaseControl> rootControls)
    {
        Context = context;
        TreeExplorerVm = new UiTreeExplorerVm(context, rootControls);
    }

    public void OnDragOver(object? sender, DragEventArgs e)
    {
        Logger.Debug("DragOver");
        e.DragEffects = DragDropEffects.Move;
    }
}

public partial class UiCtxMenuForControlTree : UserControl
{
    private Control? _openedForControl;
    private readonly ContextMenu _contextMenu = new();
    public UiCtxMenuForControlTree()
    {
        InitializeIfNeeded();
        _contextMenu.Items.Add(new MenuItem { Header = "Delete" });
        _contextMenu.Items.Add(new MenuItem { Header = "Duplicate" });
        _contextMenu.Placement = PlacementMode.Right;
    }

    public void Open(object? sender)
    {
        if (sender is Control ctrl)
        {
            _openedForControl = ctrl;
            _contextMenu.Open(control: ctrl);
        }
    }
}

public partial class UiExplorer : UserControl
{
    private UiEditorContext _context;
    private UiCtxMenuForControlTree _ctxMenuForControlTree;
    
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
        
        _ctxMenuForControlTree = new UiCtxMenuForControlTree();
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        Logger.Debug("DragOver");
        e.DragEffects = DragDropEffects.Move;
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

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        _ctxMenuForControlTree.Open(sender: sender);
    }

    private async void TreeView_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is not Control control || control.FindAncestorOfType<TreeViewItem>() is not { } item) return;
        
        var data = new DataTransfer();
        var dataItem = new DataTransferItem();
        dataItem.Set(DragDropCustomFormats.TreeViewItemFormat, item);
        data.Add(dataItem);

        var result = await DragDrop.DoDragDropAsync(e, data, DragDropEffects.Copy);
    }
}