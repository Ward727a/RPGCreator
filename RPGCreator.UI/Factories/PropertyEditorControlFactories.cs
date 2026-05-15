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
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Projektanker.Icons.Avalonia;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Blueprints;
using RPGCreator.SDK.GameUI;
using RPGCreator.SDK.GameUI.Controls;
using RPGCreator.SDK.Registry;
using RPGCreator.SDK.Types;
using RPGCreator.UI.Content.GameUiEditor;
using RPGCreator.UI.Factories.TemplateAxaml;

namespace RPGCreator.UI.Factories;

public struct GuiAction
{
    public readonly Action<object?> Action;
    public readonly string? Tooltip;
    public readonly string? Icon;
    public readonly string? Label;
}

public static class PropertyEditorControlFactories
{
    private static class Strings
    {
        public const string ControlCantBeCreated = "This control can't be created.";
        public const string ControlInvalidArg = "Tried to create a control with invalid arguments.";
    }
    
    private class BlueprintControlTemplate : IRecyclingDataTemplate
    {
        private class Template : UserControl
        {
            private const string Create = "Create";
            private const string Select = "Select";
            private const string NoBlueprint = "No BP selected";
            private const string ReadOnlyTooltip = "The property is read-only.";
            private const string SelectTooltip = "Select a blueprint.";
            private const string CreateTooltip = "Create a new blueprint.";
            
            private const string RowDef = "*, *, Auto";
            private const string ColDef = "*, *";

            private readonly TextBlock _label;
            private readonly Button _selectBp;
            private readonly Button _createBp;
            
            public ControlPropertyDescriptor Descriptor { get; private set; }
            public bool IsReadOnly { get; private set; } = false;

            public Template(ControlPropertyDescriptor descriptor)
            {
                Descriptor = descriptor;
                IsReadOnly = Descriptor is ReadOnlyControlPropertyDescriptor;
                
                var grid = new Grid()
                {
                    RowDefinitions = new RowDefinitions(RowDef),
                    ColumnDefinitions = new ColumnDefinitions(ColDef),
                    ColumnSpacing = 4
                };

                _label = new TextBlock()
                {
                    TextTrimming = TextTrimming.CharacterEllipsis
                };

                _selectBp = new Button()
                {
                    Content = Create,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                _createBp = new Button()
                {
                    Content = Select,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                var divider = new Separator();
        
                grid.Children.Add(_label);
                Grid.SetColumnSpan(_label, 2);
        
                grid.Children.Add(_selectBp);
                Grid.SetColumn(_selectBp, 0);
                Grid.SetRow(_selectBp, 1);
                grid.Children.Add(_createBp);
                Grid.SetColumn(_createBp, 1);
                Grid.SetRow(_createBp, 1);
        
                grid.Children.Add(divider);
                Grid.SetRow(divider, 2);
                Grid.SetColumnSpan(divider, 2);
                
                Content = grid;
                
                SetData();
            }

            public Template Reset(ControlPropertyDescriptor descriptor)
            {
                if (Descriptor == descriptor)
                    return this;
                Descriptor = descriptor;
                IsReadOnly = Descriptor is ReadOnlyControlPropertyDescriptor;
                SetData();
                return this;
            }

            private void SetData()
            {
                var bpData = Descriptor.Get<BlueprintData>();
                
                _label.Text = bpData?.Name ?? NoBlueprint;
                ToolTip.SetTip(_label, !string.IsNullOrEmpty(bpData?.Name) ? bpData.Name : null);

                _selectBp.IsEnabled = !IsReadOnly;
                _createBp.IsEnabled = !IsReadOnly;
                
                ToolTip.SetTip(_selectBp, IsReadOnly ? ReadOnlyTooltip : SelectTooltip);
                ToolTip.SetTip(_createBp, IsReadOnly ? ReadOnlyTooltip : CreateTooltip);
            }
        }
        
        public Control? Build(object? param)
        {
            if(param is ControlPropertyDescriptor descriptor)
                return new Template(descriptor);
            return null;
        }

        public bool Match(object? data)
        {
            return data is ReadOnlyControlPropertyDescriptor<BlueprintData>
                or EditableControlPropertyDescriptor<BlueprintData>;
        }

        public Control? Build(object? data, Control? existing)
        {
            if (existing is Template template && data is ControlPropertyDescriptor descriptor)
                return template.Reset(descriptor);
            return null;
        }
    }
    private class GuiBaseControlTemplate : IRecyclingDataTemplate
    {
        public readonly record struct BaseControlTemplateArgs(
            ControlPropertyDescriptor Descriptor,
            UiEditorContext Context);
        private class Template : UserControl
        {
            private const string NoParent = "No parent";
            private static readonly StringName EditorHintOnlyExisting = "only-existing";
            
            [Flags]
            private enum EFlags : byte
            {
                None = 0,
                OnlyExisting = 1 << 0,
                IsReadOnly = 1 << 1,
                HasParent = 1 << 2,
            }

            private ControlPropertyDescriptor Descriptor;
            private EFlags _flags = EFlags.None;
            private readonly Button _clearBtn = null!;
            private readonly Button _changeBtn = null!;
            private readonly TextBlock _label = null!;
            private List<BaseControl> _availableControl = null!;

            public UiEditorContext Context { get; private set; } = null!;
            
            public bool IsOnlyExisting => (_flags & EFlags.OnlyExisting) == EFlags.OnlyExisting;
            public bool IsReadOnly => (_flags & EFlags.IsReadOnly) == EFlags.IsReadOnly;
            public bool HasParent => (_flags & EFlags.HasParent) == EFlags.HasParent;
            
            public string ParentName => HasParent ? (Descriptor.Get() as BaseControl)!.Name : NoParent;
            
            private EditableControlPropertyDescriptor? EditableProperty => Descriptor as EditableControlPropertyDescriptor;
            
            public Template(ControlPropertyDescriptor descriptor, object? context)
            {
                Descriptor = descriptor;
                if(!CheckCtxValidity(context))
                    return;

                SetFlag();
                
                _availableControl = [];
                
                if (IsOnlyExisting)
                {
                    _availableControl.Clear();
                    foreach (var uiContextRootControl in Context.RootControls)
                    {
                        _availableControl.AddRange(uiContextRootControl);
                    }
                }
                else
                {
                    _availableControl = RegistryServices.GuiControl.Controls.ToList();
                }

                var panel = new Grid()
                {
                    ColumnDefinitions = new ColumnDefinitions("*, Auto, Auto"),
                    ColumnSpacing = 4
                };
                
                _label = new TextBlock()
                {
                    VerticalAlignment = VerticalAlignment.Center
                };
                panel.Children.Add(_label);
                
                _changeBtn = new Button()
                {
                    Content = new Icon()
                    {
                        Value = "mdi-plus"
                    },
                    Classes = { "Small" },
                };
                panel.Children.Add(_changeBtn);
                Grid.SetColumn(_changeBtn, 1);
                ToolTip.SetTip(_changeBtn, "Change parent.");

                _clearBtn = new Button()
                {
                    Content = new Icon()
                    {
                        Value = "mdi-cancel"
                    },
                    IsEnabled = HasParent,
                    Classes = { "Small", "Danger" },
                };
                panel.Children.Add(_clearBtn);
                Grid.SetColumn(_clearBtn, 2);
                ToolTip.SetTip(_clearBtn, "Unparent this control.");

                Content = panel;
                SetEvent();
                SetData();
            }

            private void DescriptorOnPropertyChanged(object? _, PropertyChangedEventArgs args)
            {
                if (args.PropertyName != nameof(Descriptor.Value)) return;

                SetFlag();
                SetData();
            }

            public Template Reset(ControlPropertyDescriptor descriptor, object? context)
            {
                if (Descriptor == descriptor)
                    return this;
                Descriptor = descriptor;
                if (!CheckCtxValidity(context))
                    return this;
                RemoveEvent();
                
                SetFlag();
                SetData();
                SetEvent();
                return this;
            }

            private void SetData()
            {
                _availableControl = [];
                
                if (IsOnlyExisting)
                {
                    _availableControl.Clear();
                    foreach (var uiContextRootControl in Context.RootControls)
                    {
                        _availableControl.AddRange(uiContextRootControl);
                    }
                }
                else
                {
                    _availableControl = RegistryServices.GuiControl.Controls.ToList();
                }

                _label.Text = ParentName;
                _clearBtn.IsEnabled = HasParent && !IsReadOnly;
                _changeBtn.IsEnabled = !IsReadOnly;
            }

            private void SetEvent()
            {
                _clearBtn.Click += ClearBtnOnClick;
                _changeBtn.Click += OnChangeBtnClicked;
                if(!IsReadOnly)
                    EditableProperty.ValueCleaned += EditablePropertyOnValueCleaned;
                Descriptor.PropertyChanged += DescriptorOnPropertyChanged;
            }

            private void OnChangeBtnClicked(object? sender, RoutedEventArgs e)
            {
                Flyout panel = new Flyout();

                panel.Content = new GuiBaseControlFlyout(_availableControl, Context.SelectedControl);
                panel.Placement = PlacementMode.Right;
                panel.ShowMode = FlyoutShowMode.Transient;
                panel.Closing += (o, args) =>
                {
                    args.Cancel = true;
                };
                
                panel.ShowAt(_changeBtn);
            }

            private static void PopulateTreeView(TreeView treeView, List<BaseControl> controls)
            {
                treeView.Items.Add(controls);
            }

            private void EditablePropertyOnValueCleaned()
            {
                SetFlag();
                SetData();
            }

            private void RemoveEvent()
            {
                _clearBtn.Click -= ClearBtnOnClick;
                if(!IsReadOnly)
                    EditableProperty.ValueCleaned -= EditablePropertyOnValueCleaned;
                Descriptor.PropertyChanged -= DescriptorOnPropertyChanged;
            }
            
            private void ClearBtnOnClick(object? sender, RoutedEventArgs args)
            {
                if (IsReadOnly) return;
                if (Context.SelectedControl == null) return;
                EditableProperty!.AskClean();
                Context.RaiseRootControlAdded(this, Context.SelectedControl);
            }

            private static List<BaseControl> FlattenRoot(BaseControl rootControl)
            {
                var controls = new List<BaseControl>();
                controls.Add(rootControl);
                foreach (var child in rootControl.Children.Where(c => !c.IsInternal))
                {
                    controls.AddRange(FlattenRoot(child));
                }
                return controls;
            }

            private bool CheckCtxValidity(object? context)
            {
                if (context is not UiEditorContext uiContext)
                {
                    Content = new TextBlock() { Text = "ERROR - Given context is not a UiEditorContext." };
                    return false;
                }

                Context = uiContext;
                return true;
            }

            /// <summary>
            /// Sets the flag based on the editor hint and returns true if the flag changed.
            /// </summary>
            /// <param name="editorHint"></param>
            /// <returns></returns>
            private bool SetFlag()
            {
                var old = _flags;
                _flags = EFlags.None;
                if(Descriptor.EditorHint == EditorHintOnlyExisting)
                    _flags |= EFlags.OnlyExisting;
                if(Descriptor is ReadOnlyControlPropertyDescriptor)
                    _flags |= EFlags.IsReadOnly;
                if(Descriptor.Get() is BaseControl)
                    _flags |= EFlags.HasParent;
                
                return old != _flags;
            }
        }
        
        public Control? Build(object? param)
        {
            if(param is BaseControlTemplateArgs args)
                return new Template(args.Descriptor, args.Context);
            return null;
        }

        public bool Match(object? data)
        {
            if(data is BaseControlTemplateArgs)
                return true;
            return false;
        }

        public Control? Build(object? data, Control? existing)
        {
            if(data is BaseControlTemplateArgs args && existing is Template template)
                return template.Reset(args.Descriptor, args.Context);
            return null;
        }
    }
    private class GuiActionControlTemplate : IRecyclingDataTemplate
    {
        public readonly record struct Args(ControlPropertyDescriptor Descriptor, object? Context)
        {
        }

        private class Template : UserControl
        {
            private const string NotAnValidAction = "This property is not a valid action.";
            private const string NotAValidContext = "The given context is not a UiEditorContext.";
            private const string NoLabelProvided = "No label";

            private UiEditorContext _context = null!;
            private GuiAction _guiAction;
            private readonly Button _btn = new();

            public Template(ControlPropertyDescriptor descriptor, object? context)
            {
                Reset(descriptor, context);
            }

            public Template Reset(ControlPropertyDescriptor descriptor, object? context)
            {
                if (descriptor.Get() is not GuiAction action)
                {
                    Content = new TextBlock() { Text = NotAnValidAction };
                    return this;
                }

                if (context is not UiEditorContext uiContext)
                {
                    Content = new TextBlock() { Text = NotAValidContext };
                    return this;
                }
                RemoveEvent();
                _guiAction = action;
                _context = uiContext;
                SetEvent();
                return this;
            }

            private void SetData()
            {
                _btn.Content = string.IsNullOrWhiteSpace(_guiAction.Label) ? NoLabelProvided : _guiAction.Label;
                ToolTip.SetTip(_btn, string.IsNullOrWhiteSpace(_guiAction.Tooltip) ? null : _guiAction.Tooltip);
            }

            private void RemoveEvent()
            {
                _btn.Click -= OnClick;
            }

            private void SetEvent()
            {
                _btn.Click += OnClick;
            }

            private void OnClick(object? sender, RoutedEventArgs args)
            {
                _guiAction.Action(_context);
            }
        }
        
        private const string NoAValidArg = "The given argument is not a valid argument.";

        public Control? Build(object? param)
        {
            if (param is Args arg)
                return new Template(arg.Descriptor, arg.Context);
            return new TextBlock() { Text = NoAValidArg };
        }

        public bool Match(object? data)
        {
            return data is Args;
        }

        public Control? Build(object? data, Control? existing)
        {
            if (data is Args args && existing is Template template)
                return template.Reset(args.Descriptor, args.Context);
            return null;
        }
    }
    
    private static bool _isRegistered;
    
    private static readonly BlueprintControlTemplate BlueprintTemplate = new();
    private static readonly GuiBaseControlTemplate BaseControlTemplate = new();
    private static readonly GuiActionControlTemplate GuiActionTemplate = new();
    
    public static void RegisterFactories()
    {
        if (_isRegistered) return;
        _isRegistered = true;
        
        RegistryServices.OnceServiceReady<IGuiPropertyEditorRegistry>(guiRegistry =>
        {
            guiRegistry.RegisterPropertyEditor<BlueprintData>(RegisterBlueprintControl);
            guiRegistry.RegisterPropertyEditor<BaseControl>(RegisterGuiBaseControl);
            guiRegistry.RegisterPropertyEditor<GuiAction>(RegisterGuiActionControl);
        });
    }

    private static object RegisterBlueprintControl(ControlPropertyDescriptor descriptor, object? ctx)
    {
        return BlueprintTemplate.Build(descriptor) ?? new TextBlock(){ Text = Strings.ControlCantBeCreated };
    }

    private static object RegisterGuiBaseControl(ControlPropertyDescriptor descriptor, object? ctx)
    {
        if (ctx is UiEditorContext editorContext)
        {
            return BaseControlTemplate.Build(
                new GuiBaseControlTemplate.BaseControlTemplateArgs(descriptor, editorContext)) ?? new TextBlock(){ Text = Strings.ControlCantBeCreated};
        }

        return new TextBlock() { Text = Strings.ControlInvalidArg };
    }

    private static object RegisterGuiActionControl(ControlPropertyDescriptor descriptor, object? ctx)
    {
        if (ctx is UiEditorContext editorContext)
        {
            return GuiActionTemplate.Build(
                new GuiActionControlTemplate.Args(descriptor, ctx)) ?? new TextBlock() { Text = Strings.ControlCantBeCreated };
        }

        return new TextBlock() { Text = Strings.ControlInvalidArg };
    }
}