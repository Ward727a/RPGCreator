#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
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
// 
// 
#endregion

using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Data.Core.Plugins;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using LiveMarkdown.Avalonia;
using RPGCreator.UI.Content.Launcher;
using RPGCreator.UI.Styles;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.MaterialDesign;
using RPGCreator.RTP.Services;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Blueprints;
using RPGCreator.SDK.EditorUiService;
using RPGCreator.SDK.GameUI;
using RPGCreator.SDK.Graph.ConnectorLogics;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Registry;
using RPGCreator.SDK.Types;
using RPGCreator.UI.Blueprints;
using RPGCreator.UI.Blueprints.Connectors;
using RPGCreator.UI.Blueprints.Nodes.Debug;
using RPGCreator.UI.Blueprints.Nodes.FlowControl;
using RPGCreator.UI.Blueprints.Nodes.Math.Int;
using RPGCreator.UI.Common.IconsProvider;
using RPGCreator.UI.Ressources;
using RPGCreator.UI.Services;
using Ursa.Controls;
using Color = Avalonia.Media.Color;
using IResourceService = RPGCreator.SDK.Resources.IResourceService;

namespace RPGCreator.UI;

public class App : Application
{
    // TODO: Move this style static variable to a more appropriate place, like a StylesManager or similar.
    public static readonly BaseStyle style = new DefaultStyle();
    public override void Initialize()
    {
        EditorUiServices.DialogService = new DialogService();
        EditorUiServices.MenuService = new MenuService();
        EditorUiServices.NotificationService = new NotificationService();
        EditorUiServices.ExtensionManager = new UiExtensionManager();
        EditorUiServices.DocService = new DocService();
        EditorUiServices.MonogameViewport = new MonogameViewportService();
        EditorUiServices.BpCompiler = new BpCompilerService();        
        
        EditorUiServices.OnceServiceReady((IDocService docService) =>
        {
            var assembly = Assembly.GetExecutingAssembly();
            
            Logger.Info("Embedded resources in assembly: {0}", args: assembly.FullName);
            
            foreach (var name in assembly.GetManifestResourceNames()) 
            {
                Logger.Info("Resource found: {0}", args: name);
                if(name.EndsWith(".md"))
                {
                    var nameWithoutExtension = Path.GetFileNameWithoutExtension(name).Split('.').Last();
                    using Stream? stream = assembly.GetManifestResourceStream(name);
                    
                    if (stream == null)            
                    {
                        Logger.Error("Failed to load embedded documentation resource: {ResourceName}", args: name);
                        continue;
                    }
                    
                    using StreamReader reader = new StreamReader(stream);
                    
                    string markdownContent = reader.ReadToEnd();
                    
                    docService.AddDocumentation(new URN("rpgc", "docs", nameWithoutExtension), markdownContent);
                    Logger.Debug("Loaded embedded documentation resource: {ResourceName} as {key}", args: [name, nameWithoutExtension]);
                }
            }
        });
        
        // DEV NOTE
        // The section below does lots of registration.
        // I should probably change the place to some more specialized class.
        
        var bpConnector = new ConnectorRegistry();
        RegistryServices.BpConnector = bpConnector;
        bpConnector.RegisterConnector(new PlayerConnectorLogic());
        var typeId = bpConnector.RegisterConnectorType(typeof(PlayerMock));

        var playerTemplate = new FuncDataTemplate<GenericConnectorViewModel>((param, _) =>
        {
            return new TextBlock()
            {
                Text = param.Title
            };
        });
        
        bpConnector.RegisterConnectorTemplate(typeId, new ConnectorTemplateObjRegistrationItem(playerTemplate, playerTemplate));

        Border MakeVector2Badge()
        {
            var borderBadge = new Border()
            {
                Background = new SolidColorBrush(Color.Parse("#D06E401E".AsSpan())),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(4, 2),
            };
            ToolTip.SetTip(borderBadge, "A position value define 2 decimal value, one for 'X' (left = negative value, right = positive value), one for 'Y' (bottom = negative value, top = positive value).");
            var typeText = new TextBlock()
            {
                Text = "Pos",
                FontSize = 10,
                VerticalAlignment = VerticalAlignment.Center
            };
            borderBadge.Child = typeText;
            
            return borderBadge;
        }
        
        var inVector2Template = new FuncDataTemplate<GenericConnectorViewModel>((param, _) =>
        {
            if (param.ConnectorLogic is Vector2ConnectorLogic vector2Logic)
            {
                
                var mainPanel = new StackPanel()
                {
                    Spacing = 8,
                };
                mainPanel.Bind(Control.IsVisibleProperty, new Binding
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor)
                    {
                        AncestorType = typeof(Nodify.Node) 
                    },
                    Path = "DataContext.IsFolded",
                    Converter = new FuncValueConverter<bool, bool>(x => !x)
                });
                var headPanel = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 8
                };
                var inputPanel = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 8,
                    IsVisible = param.CanHaveManualInput
                };
                mainPanel.Children.Add(headPanel);
                mainPanel.Children.Add(inputPanel);

                var headTitle = new TextBlock()
                {
                    Text = param.Title
                };
                headPanel.Children.Add(headTitle);
                
                var badge = MakeVector2Badge();
                headPanel.Children.Add(badge);

                var xInput = new NumericFloatUpDown()
                {
                    InnerLeftContent = "X: ",
                    Value = vector2Logic.Value.X
                };
                var yInput = new NumericFloatUpDown()
                {
                    InnerRightContent = "Y: ",
                    Value = vector2Logic.Value.Y
                };

                void OnInputChanged(object? o, ValueChangedEventArgs<float> valueChangedEventArgs)
                {
                    vector2Logic.Value = new Vector2(xInput.Value ?? 0f, yInput.Value ?? 0f);
                }

                xInput.ValueChanged += OnInputChanged;
                yInput.ValueChanged += OnInputChanged;
                
                inputPanel.Children.Add(xInput);
                inputPanel.Children.Add(yInput);
                
                return mainPanel;
            }

            return new TextBlock()
            {
                Text = "ERROR - Vector2 invalid connector logic."
            };
        });
        
        var outVector2Template = new FuncDataTemplate<GenericConnectorViewModel>((param, _) =>
        {
            if (param.ConnectorLogic is Vector2ConnectorLogic vector2Logic)
            {
                var mainPanel = new StackPanel()
                {
                    Spacing = 8,
                };
                
                mainPanel.Bind(Control.IsVisibleProperty, new Binding
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor)
                    {
                        AncestorType = typeof(Nodify.Node) 
                    },
                    Path = "DataContext.IsFolded",
                    Converter = new FuncValueConverter<bool, bool>(x => !x)
                });
                var headPanel = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 8
                };
                mainPanel.Children.Add(headPanel);

                var badge = MakeVector2Badge();
                headPanel.Children.Add(badge);
                
                var headTitle = new TextBlock()
                {
                    Text = param.Title
                };
                headPanel.Children.Add(headTitle);
                
                
                return mainPanel;
            }

            return new TextBlock()
            {
                Text = "ERROR - Vector2 invalid connector logic."
            };
        });
        bpConnector.RegisterConnectorTemplate(5, new ConnectorTemplateObjRegistrationItem(inVector2Template, outVector2Template));
        
        var bpNode = new NodeRegistry();
        RegistryServices.BpNodes = bpNode;
        
        // FLOW CONTROL
        bpNode.RegisterNode(new StartNode());
        bpNode.RegisterNode(new EndNode());
        bpNode.RegisterNode(new IfNode());
        bpNode.RegisterNode(new CombineExecNode());
        
        // DEBUG
        bpNode.RegisterNode(new PrintNode());
        
        // MATHS
        bpNode.RegisterNode(new Blueprints.Nodes.Math.Int.AddNode());
        
        // COMPARISONS
        bpNode.RegisterNode(new Blueprints.Nodes.Comparisons.Int.EqualNode());
        bpNode.RegisterNode(new Blueprints.Nodes.Comparisons.Int.NotEqualNode());
        bpNode.RegisterNode(new Blueprints.Nodes.Comparisons.Int.GreaterThanNode());
        bpNode.RegisterNode(new Blueprints.Nodes.Comparisons.Int.LessThanNode());
        
        // CONVERTERS
        bpNode.RegisterNode(new Blueprints.Nodes.Converter.IntTo.IntToStringNode());
        bpNode.RegisterNode(new Blueprints.Nodes.Converter.IntTo.IntToBoolNode());
        bpNode.RegisterNode(new Blueprints.Nodes.Converter.IntTo.IntToFloatNode());
        bpNode.RegisterNode(new Blueprints.Nodes.Converter.FloatTo.FloatToStringNode());
        bpNode.RegisterNode(new Blueprints.Nodes.Converter.FloatTo.FloatToBoolNode());
        bpNode.RegisterNode(new Blueprints.Nodes.Converter.FloatTo.FloatToIntNode());
        bpNode.RegisterNode(new Blueprints.Nodes.Converter.BoolTo.BoolToStringNode());
        bpNode.RegisterNode(new Blueprints.Nodes.Converter.BoolTo.BoolToFloatNode());
        bpNode.RegisterNode(new Blueprints.Nodes.Converter.BoolTo.BoolToIntNode());
        bpNode.RegisterNode(new Blueprints.Nodes.Converter.StringTo.StringToBoolNode());
        bpNode.RegisterNode(new Blueprints.Nodes.Converter.StringTo.StringToFloatNode());
        bpNode.RegisterNode(new Blueprints.Nodes.Converter.StringTo.StringToIntNode());
        
        // GAMES
        // // PLAYER
        bpNode.RegisterNode(new Blueprints.Nodes.Game.Player.GetPlayerNode());
        bpNode.RegisterNode(new Blueprints.Nodes.Game.Player.MovePlayerToNode());
        
        // // SOUNDS
        bpNode.RegisterNode(new Blueprints.Nodes.Game.Sounds.PlaySoundNode());
        
        // REGISTRATION FOR PROPERTY EDITOR CONTROL
        var propEditCtrl = RegistryServices.PropertyEditorRegistry;
        propEditCtrl.RegisterPropertyEditor<BlueprintData>((descriptor) =>
        {

            if (descriptor is ReadOnlyControlPropertyDescriptor readOnlyDescriptor)
            {
                return GenerateReadonlyBpDataControl(readOnlyDescriptor);
            }

            if (descriptor is EditableControlPropertyDescriptor editableDescriptor)
            {
                return GenerateEditableBpDataControl(editableDescriptor);
            }

            return new TextBlock { Text = "ERROR - Given descriptor is not a readOnly nor editable descriptor." };
        });
        
        
        EngineServices.OnceServiceReady((IResourceService ResourcesService) =>
        {
            ResourcesService.RegisterLoader<Avalonia.Media.Imaging.Bitmap>(new AvaloniaBitmapLoader());
        });

        return;

        Control GenerateReadonlyBpDataControl(ReadOnlyControlPropertyDescriptor descriptor)
        {
            var grid = new Grid()
            {
                RowDefinitions = new RowDefinitions("*, *, Auto"),
                ColumnDefinitions = new ColumnDefinitions("*, *"),
                ColumnSpacing = 4
            };

            var label = new TextBlock()
            {
                Text = descriptor.Get<BlueprintData>()?.Name ?? "No BP selected",
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            if (!string.IsNullOrEmpty(descriptor.Get<BlueprintData>()?.Name))
            {
                ToolTip.SetTip(label, descriptor.Get<BlueprintData>()?.Name);
            }
            
            var selectBp = new Button()
            {
                Content = "Select",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                IsEnabled = false
            };

            var createBp = new Button()
            {
                Content = "Create",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                IsEnabled = false
            };
            
            ToolTip.SetTip(createBp, "The property is read-only.");
            ToolTip.SetTip(selectBp, "The property is read-only.");

            var divider = new Separator();
            
            grid.Children.Add(label);
            Grid.SetColumnSpan(label, 2);
            
            grid.Children.Add(selectBp);
            Grid.SetColumn(selectBp, 0);
            Grid.SetRow(selectBp, 2);
            grid.Children.Add(createBp);
            Grid.SetColumn(createBp, 1);
            Grid.SetRow(createBp, 2);
            
            grid.Children.Add(divider);
            Grid.SetRow(divider, 3);
            Grid.SetColumnSpan(divider, 2);
            
            
            return grid;
        }
        
        Control GenerateEditableBpDataControl(EditableControlPropertyDescriptor descriptor)
        {
            var grid = new Grid()
            {
                RowDefinitions = new RowDefinitions("*, *, Auto"),
                ColumnDefinitions = new ColumnDefinitions("*, *"),
                ColumnSpacing = 4
            };

            var label = new TextBlock()
            {
                Text = descriptor.Get<BlueprintData>()?.Name ?? "No BP selected",
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            if (!string.IsNullOrEmpty(descriptor.Get<BlueprintData>()?.Name))
            {
                ToolTip.SetTip(label, descriptor.Get<BlueprintData>()?.Name);
            }

            var selectBp = new Button()
            {
                Content = "Select",
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var createBp = new Button()
            {
                Content = "Create",
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var divider = new Separator();
            
            grid.Children.Add(label);
            Grid.SetColumnSpan(label, 2);
            
            grid.Children.Add(selectBp);
            Grid.SetColumn(selectBp, 0);
            Grid.SetRow(selectBp, 1);
            grid.Children.Add(createBp);
            Grid.SetColumn(createBp, 1);
            Grid.SetRow(createBp, 1);
            
            grid.Children.Add(divider);
            Grid.SetRow(divider, 2);
            Grid.SetColumnSpan(divider, 2);
            
            return grid;
        }
    }

    
    
    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        MarkdownNode.Register<MathInlineNode>();
        MarkdownNode.Register<MathBlockNode>();
        IconProvider.Current
            .Register<MaterialDesignIconProvider>()
            .Register<GameIconProvider>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new LauncherWindow();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView ??= new LauncherWindowControl();;
        }

        base.OnFrameworkInitializationCompleted();
        #if DEBUG
        this.AttachDeveloperTools();
        #endif
    }

}
