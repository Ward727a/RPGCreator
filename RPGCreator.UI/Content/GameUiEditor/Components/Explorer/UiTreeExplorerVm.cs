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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.Input;
using FontStashSharp;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.SDK;
using RPGCreator.SDK.GameUI;
using RPGCreator.SDK.GameUI.Controls;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.RuntimeService;
using RPGCreator.SDK.Types;

namespace RPGCreator.UI.Content.GameUiEditor.Components.Explorer;

public class UiBaseControlVm : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public Ulid Id { get; private set; }

    public string Name => Model.Name;

    public bool IsExpanded
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsSelected
    {
        get;
        set => SetField(ref field, value);
    } = false;

    public bool IsLocked
    {
        get;
        set => SetField(ref field, value);
    } = false;

    public BaseControl Model { get; set; }

    public ObservableCollection<UiBaseControlVm> Children { get; set; } = new ObservableCollection<UiBaseControlVm>();

    public UiTreeExplorerVm TreeExplorerVm { get; set; }

    public UiBaseControlVm(BaseControl model, UiTreeExplorerVm treeExplorerVm)
    {
        Id = Ulid.NewUlid();
        Model = model;
        TreeExplorerVm = treeExplorerVm;
        LinkToControl();

        TreeExplorerVm.ControlsIdMapping[Model] = Id;
        TreeExplorerVm.ControlVms[Id] = this;

        foreach (var child in Model.Children.Where(c => !c.IsInternal))
            Children.Add(new UiBaseControlVm(child, treeExplorerVm));
    }

    protected void LinkToControl()
    {
        Model.AddedChildren += (child) => Children.Add(new UiBaseControlVm(child, TreeExplorerVm));
        Model.RemovedChildren += (child) => Children.Remove(Children.First(x => x.Model == child));
        Model.PropertyDescriptorsExposed += OnPropertyDescriptorsExposed;

        void OnPropertyDescriptorsExposed()
        {
            void OnNamePropertyOnPropertyChanged(object? sender, PropertyChangedEventArgs args) => OnPropertyChanged(nameof(Name));

            Model.NameProperty.PropertyChanged -= OnNamePropertyOnPropertyChanged;
            Model.NameProperty.PropertyChanged += OnNamePropertyOnPropertyChanged;
            
            Model.PropertyDescriptorsExposed -= OnPropertyDescriptorsExposed;
        }

    }

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

public class PropertyGroupViewModel
{
    public string GroupName { get; set; }
    public List<ControlPropertyDescriptor> Properties { get; set; } = new();
    public Dictionary<string, PropertyGroupViewModel> SubGroups { get; set; } = new();

    public IEnumerable<PropertyGroupViewModel> IterableGroups => SubGroups.Values;
}

public class EventGroupViewModel
{
    public string GroupName { get; set; }
    public List<ControlEventDescriptor> Events { get; set; } = new();
    public Dictionary<string, EventGroupViewModel> SubGroups { get; set; } = new();

    public IEnumerable<EventGroupViewModel> IterableGroups => SubGroups.Values;
}

public class UiTreeExplorerVm : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public event Action<BaseControl>? OnControlSelected;
    public event Action<BaseControl>? OnControlUnselected;

    public RelayCommand AddControlCommand { get; }

    public BaseControl? SelectedControl
    {
        get;
        set => SetField(ref field, value);
    }

    public UiBaseControlVm? SelectedControlVm
    {
        get;
        set => SetField(ref field, value);
    }
    
    public ObservableCollection<PropertyGroupViewModel> ControlProperties { get; set; } = new ObservableCollection<PropertyGroupViewModel>();
    public ObservableCollection<EventGroupViewModel> ControlEvents { get; set; } = new ObservableCollection<EventGroupViewModel>();

    public ObservableCollection<UiBaseControlVm> RootsControls { get; set; } =
        new ObservableCollection<UiBaseControlVm>();

    public Dictionary<BaseControl, Ulid> ControlsIdMapping { get; set; } = new Dictionary<BaseControl, Ulid>();
    public Dictionary<Ulid, UiBaseControlVm> ControlVms { get; set; } = new Dictionary<Ulid, UiBaseControlVm>();

    private UiEditorContext _context;
    private readonly IRpgFont _font18;
    
    public UiTreeExplorerVm(UiEditorContext context, IEnumerable<BaseControl> rootControls)
    {
        _context = context;
        AddControlCommand = new RelayCommand(AddControl);
        foreach (var control in rootControls.Where(c => !c.IsInternal))
            RootsControls.Add(new UiBaseControlVm(control, this));
    }

    public void AddControl()
    {
        var dialog = new DialogUiBrowser();

        dialog.ControlSubmitted += OnControlSubmitted;
        
        dialog.ShowDialog(TopLevel.GetTopLevel(_context.EditorControl) as Window ?? throw new InvalidOperationException("No window found to show the dialog"));
        
        return;

        void OnControlSubmitted(BaseControl control)
        {
            var createdControl = control.Create();

            if (SelectedControl != null)
            {
                _context.RaiseControlAdded(this, createdControl);
                SelectedControl.AddChild(createdControl);
            }
            else
            {
                _context.RaiseRootControlAdded(this, createdControl);
            }
            
            dialog.ControlSubmitted -= OnControlSubmitted;
        }
    }

    public bool HasControl(BaseControl control) => ControlsIdMapping.ContainsKey(control);

    public void SelectControl(BaseControl? control)
    {
        if (SelectedControl == control) return;

        if (control == null)
        {
            UnselectControl();
            return;
        }

        if (!HasControl(control))
        {
            Logger.Error("Control not found in the explorer: {controlName}", control.Name);
            return;
        }

        if (SelectedControl != null)
            UnselectControl();

        SelectedControl = control;
        SelectedControlVm = ControlVms[ControlsIdMapping[control]];
        SelectedControlVm.IsSelected = true;
        ControlProperties.Clear();
        ControlEvents.Clear();
        ExplodePropertiesIntoGroups();
        ExplodeEventsIntoGroups();

        OnControlSelected?.Invoke(control);
    }

    public void ExplodePropertiesIntoGroups()
    {
        var properties = SelectedControl?.GetExposedProperties();
        var mapping = new Dictionary<string, PropertyGroupViewModel>();

        foreach (var property in properties)
        {
            PropertyGroupViewModel? group = null;
            foreach (var segment in property.Path.Segments)
            {
                if (group == null)
                {
                    if (!mapping.TryGetValue(segment, out var existingGroup))
                    {
                        existingGroup = new PropertyGroupViewModel {GroupName = segment};
                        mapping.Add(segment, existingGroup);
                    }
                    group = existingGroup;
                }
                else
                {
                    if (!group.SubGroups.TryGetValue(segment, out var existingSubGroup))
                    {
                        existingSubGroup = new PropertyGroupViewModel {GroupName = segment};
                        group.SubGroups.Add(segment, existingSubGroup);
                    }
                    group = existingSubGroup;
                }
            }
            group.Properties.Add(property);
        }
        
        ControlProperties.AddRange(mapping.Values);
    }
    
    public void ExplodeEventsIntoGroups()
    {
        var properties = SelectedControl?.GetExposedEvents();
        var mapping = new Dictionary<string, EventGroupViewModel>();

        foreach (var property in properties)
        {
            EventGroupViewModel? group = null;
            foreach (var segment in property.Category.Segments)
            {
                if (group == null)
                {
                    if (!mapping.TryGetValue(segment, out var existingGroup))
                    {
                        existingGroup = new EventGroupViewModel {GroupName = segment};
                        mapping.Add(segment, existingGroup);
                    }
                    group = existingGroup;
                }
                else
                {
                    if (!group.SubGroups.TryGetValue(segment, out var existingSubGroup))
                    {
                        existingSubGroup = new EventGroupViewModel {GroupName = segment};
                        group.SubGroups.Add(segment, existingSubGroup);
                    }
                    group = existingSubGroup;
                }
            }
            group.Events.Add(property);
        }
        
        ControlEvents.AddRange(mapping.Values);
    }

    public void UnselectControl()
    {
        if (SelectedControl == null) return;

        OnControlUnselected?.Invoke(SelectedControl);
        SelectedControlVm?.IsSelected = false;

        SelectedControl = null;
        SelectedControlVm = null;
    }

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