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
using System.Numerics;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Nodify;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Blueprints;
using RPGCreator.SDK.Commands;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Graph.LOGIC;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types;
using RPGCreator.UI.Blueprints;
using RPGCreator.UI.Services;
using Color = Avalonia.Media.Color;
using Connection = RPGCreator.SDK.Assets.Definitions.Blueprints.Connection;
using ICommand = System.Windows.Input.ICommand;

namespace RPGCreator.UI.Content.Blueprint;

public class ConnectionViewModel
{
    public ConnectionViewModel(GenericConnectorViewModel source, GenericConnectorViewModel target)
    {
        Source = source;
        Target = target;

        Source.IsConnected = true;
        Target.IsConnected = true;
    }

    public GenericConnectorViewModel Source { get; set; }
    public GenericConnectorViewModel Target { get; set; }
}

public class DelegateCommand<T> : ICommand
{
    private readonly Action<T> _action;
    private readonly Func<T, bool>? _condition;

    public event EventHandler? CanExecuteChanged;

    public DelegateCommand(Action<T> action, Func<T, bool>? executeCondition = default)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
        _condition = executeCondition;
    }

    public bool CanExecute(object? parameter)
    {
        if (parameter is T value)
        {
            return _condition?.Invoke(value) ?? true;
        }

        return _condition?.Invoke(default!) ?? true;
    }

    public void Execute(object? parameter)
    {
        if (parameter is T value)
        {
            _action(value);
        }
        else
        {
            _action(default!);
        }
    }

    public void RaiseCanExecuteChanged()
        => CanExecuteChanged?.Invoke(this, new EventArgs());
}

public class PendingConnectionViewModel : INotifyPropertyChanged
{
    public string ToolTipMessage
    {
        get;
        private set
        {
            if (field == value) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToolTipMessage)));
        }
    } = "";

    public bool ToolTipIsOpen
    {
        get;
        private set
        {
            if (field == value) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToolTipIsOpen)));
        }
    } = false;

    public PendingConnectionViewModel(NodeEditorViewModel editor)
    {
        StartCommand = new DelegateCommand<object>(param =>
        {
            if (param is GenericConnectorViewModel connector)
            {
                _source = connector;
            }
        });

        FinishCommand = new DelegateCommand<GenericConnectorViewModel?>(target =>
        {
            if (_source != null && target != null)
            {
                if (target.Type != _source.Type) return;
                if (target.IsOutput == _source.IsOutput) return;
                editor.Connect(_source, target);
            }

            _source = null;
        });

        PropertyChanged += OnPreviewTargetChanged;
    }

    private GenericConnectorViewModel? _source;

    public Point StartPoint
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartPoint)));
        }
    }

    public SolidColorBrush StrokeBrush
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StrokeBrush)));
        }
    } = new(Colors.CornflowerBlue);

    private readonly SolidColorBrush _invalidBrush = new(Colors.Red);
    private readonly SolidColorBrush _validBrush = new(Colors.CornflowerBlue);

    public AvaloniaList<float> DashArray
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DashArray)));
        }
    } = [5, 5];

    private readonly AvaloniaList<float> _validDashArray = [5, 5];
    private readonly AvaloniaList<float> _invalidDashArray = [.2f, 2f];

    private object? _previewTarget;

    public object? PreviewTarget
    {
        get => _previewTarget;
        set
        {
            if (_previewTarget == value) return;
            _previewTarget = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PreviewTarget)));
        }
    }

    private const string ErrorTwoBothPut = "Cannot connect two pin that are both {0}.";
    private const string ErrorTwoDiffType = "Cannot connect two pin that are not of the same type.";
    private const string ErrorTwoSameNode = "Cannot connect two pin on the same node.";

    private void OnPreviewTargetChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(PreviewTarget)) return;

        StrokeBrush = _validBrush;
        DashArray = _validDashArray;

        if (_previewTarget is GenericConnectorViewModel nodeViewModel)
        {
            if (_source?.NodeId == nodeViewModel.NodeId)
            {
                StrokeBrush = _invalidBrush;
                DashArray = _invalidDashArray;
                ToolTipIsOpen = true;
                ToolTipMessage = ErrorTwoSameNode;
                return;
            }

            if (nodeViewModel.IsOutput == _source?.IsOutput)
            {
                StrokeBrush = _invalidBrush;
                DashArray = _invalidDashArray;
                ToolTipIsOpen = true;
                ToolTipMessage = string.Format(ErrorTwoBothPut, _source.IsOutput ? "outputs" : "inputs");
                return;
            }

            if (_source?.Type != nodeViewModel.Type ||
                _source?.ConnectorLogic.ValueType != nodeViewModel.ConnectorLogic.ValueType)
            {
                StrokeBrush = _invalidBrush;
                DashArray = _invalidDashArray;
                ToolTipIsOpen = true;
                ToolTipMessage = ErrorTwoDiffType;
                return;
            }
        }

        ToolTipIsOpen = false;
    }

    public ICommand StartCommand { get; }
    public ICommand FinishCommand { get; }
    public event PropertyChangedEventHandler? PropertyChanged;
}

public class NodeEditorViewModel : INotifyPropertyChanged
{
    public event Action<string>? BlueprintBuilded;
    public event Action<BlueprintData>? BlueprintSaved;

    #region UndoRedoCommands

    private class ConnectCommand : BaseCommand
    {
        const string ConnectFormat = "Connect {0} from {1}";

        private readonly GenericConnectorViewModel _source;
        private readonly GenericConnectorViewModel _target;
        private readonly NodeEditorViewModel _viewModel;

        public override string Name { get; }

        public ConnectCommand(GenericConnectorViewModel source, GenericConnectorViewModel target,
            NodeEditorViewModel viewModel)
        {
            _source = source;
            _target = target;
            _viewModel = viewModel;
            Name = string.Format(ConnectFormat, _source.Title, _target.Title);
        }

        protected override void OnExecute()
        {
            _viewModel._connect(_source, _target);
        }

        protected override void OnUndo()
        {
            _viewModel._disconnect(_source, _target);
        }
    }

    private class DisconnectCommand : BaseCommand
    {
        const string DisconnectFormat = "Disconnect {0} from {1}";

        private readonly GenericConnectorViewModel _source;
        private readonly GenericConnectorViewModel _target;
        private readonly NodeEditorViewModel _viewModel;

        public override string Name { get; }

        public DisconnectCommand(GenericConnectorViewModel source, GenericConnectorViewModel target,
            NodeEditorViewModel viewModel)
        {
            _source = source;
            _target = target;
            _viewModel = viewModel;
            Name = string.Format(DisconnectFormat, _source.Title, _target.Title);
        }

        protected override void OnExecute()
        {
            _viewModel._disconnect(_source, _target);
        }

        protected override void OnUndo()
        {
            _viewModel._connect(_source, _target);
        }
    }

    private class DisconnectConnectionCommand : BaseCommand
    {
        const string DisconnectFormat = "Disconnect {0} from {1}";
        private readonly ConnectionViewModel _connection;
        private readonly NodeEditorViewModel _viewModel;

        public override string Name { get; }

        public DisconnectConnectionCommand(ConnectionViewModel connection, NodeEditorViewModel viewModel)
        {
            _connection = connection;
            _viewModel = viewModel;
            Name = string.Format(DisconnectFormat, _connection.Source.Title, _connection.Target.Title);
        }

        protected override void OnExecute()
        {
            _viewModel._disconnect(_connection);
        }

        protected override void OnUndo()
        {
            _viewModel._connect(_connection.Source, _connection.Target);
        }
    }

    private class DisconnectAllConnectionCommand : BaseCommand
    {
        const string DisconnectAllFormat = "Disconnect {0} connection{1} {2} {3}";

        private readonly GenericConnectorViewModel _connector;
        private readonly NodeEditorViewModel _viewModel;

        private List<ConnectionViewModel> _connections = [];
        public override string Name { get; }

        public DisconnectAllConnectionCommand(GenericConnectorViewModel connector, NodeEditorViewModel viewModel)
        {
            _connector = connector;
            _viewModel = viewModel;
            Name = string.Format(DisconnectAllFormat, _connector.Connections.Count,
                _connector.Connections.Count > 1 ? "s" : "", _connector.IsOutput ? "to" : "from", _connector.Title);
        }

        protected override void OnExecute()
        {
            _connections = _connector.Connections.ToList();
            _viewModel._disconnect(_connector);
        }

        protected override void OnUndo()
        {
            foreach (var connection in _connections)
            {
                _viewModel._connect(connection.Source, connection.Target);
            }
        }
    }

    #endregion

    public Ulid CurrentId { get; private set; } = Ulid.Empty;
    
    /// <summary>
    /// If true, the editor will be in dev mode.<br/>
    /// In dev mode, the editor will show the 'produced source code' button, allowing to have a preview of what the BP editor has produced.
    /// </summary>
    public bool IsInDevMode
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsInDevMode)));
        }
    } = false;
    
    public bool IsDragging
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDragging)));
        }
    } = false;

    public event PropertyChangedEventHandler? PropertyChanged;

    public BlueprintListVm BlueprintListVm { get; set; }

    private NodeRegistry? _nodeRegistry;
    private IUndoRedoService _undoRedoService;
    public ObservableCollection<GenericNodeViewModel> Nodes { get; } = new();
    public ObservableCollection<ConnectionViewModel> Connections { get; } = new();
    public PendingConnectionViewModel PendingConnection { get; }
    public List<GenericNodeViewModel> SelectedNodes { get; set; } = new();
    public ICommand DisconnectConnectorCommand { get; }

    public RelayCommand BuildBlueprintCommand { get; private set; }
    public RelayCommand SaveBlueprintCommand { get; private set; }
    public RelayCommand ShowProducedSourceCommand { get; private set; }
    public RelayCommand DeleteSelectedNodesCommand { get; private set; }
    public RelayCommand HideToggleSelectedNodesCommand { get; private set; }
    public RelayCommand CreateCommentGroupNodeCommand { get; private set; }

    public NodeEditorViewModel()
    {
        if (RegistryServices.BpNodes is NodeRegistry nodeRegistry)
        {
            _nodeRegistry = nodeRegistry;
        }

        BuildBlueprintCommand = new RelayCommand(BuildBlueprint);
        SaveBlueprintCommand = new RelayCommand(SaveBlueprint);
        ShowProducedSourceCommand = new RelayCommand(ShowProducedSource);
        DeleteSelectedNodesCommand = new RelayCommand(OnDeleteSelectedNodes);
        HideToggleSelectedNodesCommand = new RelayCommand(HideToggleSelectedNodes);
        CreateCommentGroupNodeCommand = new RelayCommand(CreateCommentGroupNode);

        BlueprintListVm = new BlueprintListVm(_nodeRegistry);

        _undoRedoService = EngineServices.UndoRedoService;
        DisconnectConnectorCommand = new DelegateCommand<object?>(connector =>
        {
            if (connector is ConnectionViewModel connectionViewModel)
            {
                Disconnect(connectionViewModel);
            }
            else if (connector is GenericConnectorViewModel connectorViewModel)
            {
                Disconnect(connectorViewModel);
            }
        });

        PendingConnection = new PendingConnectionViewModel(this);

        if (_nodeRegistry != null)
        {
            var urnModule = "rpgc".ToUrnNamespace().ToUrnModule("bp_nodes");
            var start = _nodeRegistry.GetNodeViewModel(urnModule.ToUrn("start_node"));
            var end = _nodeRegistry.GetNodeViewModel(urnModule.ToUrn("end_node"));
            var print = _nodeRegistry.GetNodeViewModel(urnModule.ToUrn("print_node"));
            end.Location = new Point(400, 0);
            print.Location = new Point(200, 0);
            Nodes.Add(start);
            Nodes.Add(print);
            Nodes.Add(end);

            _connect(start.Outputs[0], print.Inputs[0]);
            _connect(print.Outputs[0], end.Inputs[0]);
        }
        else
        {
            Logger.Error("Node registry is null");
        }
    }

    public void SaveBlueprint()
    {
        BlueprintData data;

        if (CurrentId != Ulid.Empty)
        {
            data = BlueprintData.Create(CurrentId);
            data.Name = "My blueprint";
        }
        else
        {
            data = BlueprintData.Create();
            data.Name = "My blueprint";
            CurrentId = data.Id;
        }
        
        foreach (var connectionViewModel in Connections)
        {
            var from = connectionViewModel.Source;
            var to = connectionViewModel.Target;

            var connectionData = ConnectionData.Create(
                new Connection(from.NodeId, from.ConnectorIndex),
                new Connection(to.NodeId, to.ConnectorIndex)
            );

            data.Connections.Add(connectionData);
        }

        foreach (var node in Nodes)
        {
            var nodeData = NodeData.Create(node.Id, node.NodeLogic.Urn, node.IsFolded,
                new Vector2((float)node.Location.X, (float)node.Location.Y));

            foreach (var input in node.Inputs)
            {
                if (input.Value != null)
                    nodeData.ConnectorData.Add(input.ConnectorIndex, input.Value);
            }

            data.Nodes.Add(nodeData);
        }


        BlueprintSaved?.Invoke(data);
    }

    private void HideToggleSelectedNodes()
    {
        if (SelectedNodes == null || SelectedNodes.Count == 0)
        {
            return;
        }

        var firstNode = SelectedNodes.First();
        var newStatus = !firstNode.IsFolded;

        foreach (var node in SelectedNodes)
        {
            node.IsFolded = newStatus;
        }
    }

    private void CreateCommentGroupNode()
    {
        var mousePosition = GlobalStates.MouseState.Position;
    }

    private Window? _producedSourceWindow = null;
    private TextBlock _producedSource;

    private void ShowProducedSource()
    {
        if (_producedSourceWindow != null)
        {
            return;
        }

        _producedSourceWindow = new Window
        {
            Title = "DEBUG - Produced source",
            Width = 800,
            Height = 600,
            Content = new ScrollViewer
            {
                Content = _producedSource = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                }
            }
        };
        _producedSourceWindow.Closed += (sender, args) => _producedSourceWindow = null;
        _producedSourceWindow.Show();
    }

    public void BuildBlueprint()
    {
        SaveBlueprint();
        var compilerService = new BpCompilerService();
        var buildSource = compilerService.Compile(this);

        Logger.Debug($"\n{buildSource}");

        if (_producedSourceWindow != null)
        {
            _producedSource.Text = "GENERATED CODE - This code was generated from a blueprint!\n\n";
            _producedSource.Text += buildSource;
        }
        
        BlueprintBuilded?.Invoke(buildSource);
    }

    private void OnDeleteSelectedNodes()
    {
        Logger.Debug($"Deleted {SelectedNodes.Count} nodes");
        foreach (var node in SelectedNodes.ToList())
        {
            if (node.NodeLogic.Category.StartsWith("@"))
            {
                Logger.Debug($"Skipping deletion of node '{node.Title}' due to '@' prefix");
                continue;
            }

            foreach (var input in node.Inputs.Where(i => i.IsConnected))
            {
                _disconnect(input);
            }

            foreach (var output in node.Outputs.Where(o => o.IsConnected))
            {
                _disconnect(output);
            }

            Nodes.Remove(node);
        }

        SelectedNodes.Clear();
    }

    public void Connect(GenericConnectorViewModel source, GenericConnectorViewModel target)
    {
        var cmd = new ConnectCommand(source, target, this);
        _undoRedoService.ExecuteCommand(cmd);
    }


    private void _connect(GenericConnectorViewModel source, GenericConnectorViewModel target)
    {
        if (!source.IsOutput)
        {
            (target, source) = (source, target);
        }


        var connection = new ConnectionViewModel(source, target);

        if (target.TotalConnections == 1)
        {
            _disconnect(target.Connections.Last());
        }

        if (source.TotalConnections == source.AllowedConnections)
        {
            _disconnect(source.Connections.Last());
        }

        Connections.Add(connection);
        target.Connections.Add(connection);
        source.Connections.Add(connection);
        source.IsConnected = true;
        target.IsConnected = true;

        target.ConnectorLogic.RuntimeParentConnector = source.ConnectorLogic.RuntimeId;
    }

    public void Disconnect(GenericConnectorViewModel connector)
    {
        var cmd = new DisconnectAllConnectionCommand(connector, this);
        _undoRedoService.ExecuteCommand(cmd);
    }

    public void Disconnect(GenericConnectorViewModel source, GenericConnectorViewModel target)
    {
        var cmd = new DisconnectCommand(source, target, this);
        _undoRedoService.ExecuteCommand(cmd);
    }

    public void Disconnect(ConnectionViewModel connection)
    {
        var cmd = new DisconnectConnectionCommand(connection, this);
        _undoRedoService.ExecuteCommand(cmd);
    }

    /// <summary>
    /// Disconnects a connector from ALL its connections
    /// </summary>
    /// <param name="connector">
    /// The connector to disconnect.
    /// </param>
    private void _disconnect(GenericConnectorViewModel connector)
    {
        foreach (var connection in connector.Connections.ToList())
        {
            Connections.Remove(connection);

            connection.Source.Connections.Remove(connection);
            connection.Target.Connections.Remove(connection);

            connection.Source.IsConnected = connection.Source.TotalConnections > 0;
            connection.Target.IsConnected =
                false; // Target always has only 1 connection, so if we disconnect it, it's not connected anymore at all.

            connection.Target.ParentNodeId = Ulid.Empty;
            connection.Target.ConnectorLogic.RuntimeParentConnector = Ulid.Empty;
        }
    }

    private void _disconnect(GenericConnectorViewModel source, GenericConnectorViewModel target)
    {
        var connection = Connections.First(x => x.Source == source && x.Target == target);
        _disconnect(connection);
    }

    private void _disconnect(ConnectionViewModel connection)
    {
        var source = connection.Source;
        var target = connection.Target;
        Connections.Remove(connection);

        source.Connections.Remove(connection);
        target.Connections.Remove(connection);

        source.IsConnected = source.TotalConnections > 0;
        target.IsConnected = false;

        target.ParentNodeId = Ulid.Empty;
        target.ConnectorLogic.RuntimeParentConnector = Ulid.Empty;
    }

    public void DraggingStart()
    {
        IsDragging = true;
    }

    public void DraggingEnd()
    {
        IsDragging = false;
    }
}

public partial class EditorWindow : Window
{
    private NodeEditorViewModel _dataContext;
    private ContentControl _dragGhost;
    private IConfig _bpConfig;

    private string _blueprintSaveFolder = string.Empty;
    private string _blueprintBuildFolder = string.Empty;
    private string _blueprintCompiledFolder = string.Empty;

    private bool _inDevMode = false;

    public EditorWindow()
    {
        _dataContext = new NodeEditorViewModel();
        
        LoadConfig();
        
        _dataContext.IsInDevMode = _inDevMode;

        DataContext = _dataContext;
        InitializeComponent();

        DragDrop.AddDropHandler(Editor, Editor_Drop);
        DragDrop.AddDragOverHandler(this, GlobalDragOver);

        _dataContext.BlueprintSaved += data =>
        {
            var blueprintId = data.Id;
            
            var fileName = Path.Combine(_blueprintSaveFolder, $"{data.Name}_{blueprintId}.json");
            
            EngineServices.Serializer.SerializeTo(data, fileName);
            
            Logger.Debug($"Blueprint saved: {fileName}");
        };

        _dataContext.BlueprintBuilded += source =>
        {
            var blueprintId = _dataContext.CurrentId;
            var fileName = Path.Combine(_blueprintBuildFolder, $"{blueprintId}.cs");

            File.WriteAllText(fileName, source);

            Logger.Debug($"Blueprint built: {fileName}");
        };
    }

    private void LoadConfig()
    {
        if (!EngineServices.Config.TryFrom("blueprint_config", false, out _bpConfig))
        {
            if (EngineServices.Config.CreateConfig("blueprint_config", new BaseConfig()))
            {
                _bpConfig = EngineServices.Config.From("blueprint_config")!;
            }
            else
            {
                throw new InvalidOperationException("Failed to create project configuration");
            }
        }

        var bpFolder = Path.Combine(GlobalStates.ProjectState.CurrentProject?.Path ?? RpgEnv.Path.ApplicationData, "Blueprints");
        
        _blueprintSaveFolder = _bpConfig.GetString("save_folder",
            Path.Combine(bpFolder, "Data"));
        
        _blueprintBuildFolder = _bpConfig.GetString("build_folder",
            Path.Combine(bpFolder, "Build"));
        
        _blueprintCompiledFolder = _bpConfig.GetString("compiled_folder",
            Path.Combine(bpFolder, "Compiled"));
        
        _inDevMode = _bpConfig.GetBool("dev_mode_enabled", false);
        
        if (!_bpConfig.HasString("save_folder"))
        {
            _bpConfig.SetString("save_folder", _blueprintSaveFolder);
            Logger.Info("Created default blueprint save folder (at: {path})", _blueprintSaveFolder);
        }
        
        if (!_bpConfig.HasString("build_folder"))
        {
            _bpConfig.SetString("build_folder", _blueprintBuildFolder);
            Logger.Info("Created default blueprint build folder (at: {path})", _blueprintBuildFolder);
        }
        
        if (!_bpConfig.HasString("compiled_folder"))
        {
            _bpConfig.SetString("compiled_folder", _blueprintCompiledFolder);
            Logger.Info("Created default blueprint compiled folder (at: {path})", _blueprintCompiledFolder);
        }

        if (!_bpConfig.HasBool("dev_mode_enabled"))
        {
            _bpConfig.SetBool("dev_mode_enabled", _inDevMode);
        }
        
        if (!Directory.Exists(_blueprintSaveFolder))
        {
            Directory.CreateDirectory(_blueprintSaveFolder);
        }
        
        if (!Directory.Exists(_blueprintBuildFolder))
        {
            Directory.CreateDirectory(_blueprintBuildFolder);
        }
        
        if (!Directory.Exists(_blueprintCompiledFolder))
        {
            Directory.CreateDirectory(_blueprintCompiledFolder);
        }
    }


    private void GlobalDragOver(object? sender, DragEventArgs e)
    {
        if (_dragGhost != null)
        {
            var pos = e.GetPosition(this);

            Canvas.SetLeft(_dragGhost, pos.X + 0);
            Canvas.SetTop(_dragGhost, pos.Y + 0);
        }
    }

    private void Editor_Drop(object? sender, DragEventArgs e)
    {
        var data = e.DataTransfer.TryGetValue<INodeLogic>(BlueprintFormats.NodeLogicFormat);
        if (data is not null && sender is NodifyEditor && DataContext is NodeEditorViewModel vm &&
            _dragGhost.Content is GenericNodeViewModel nodeVm)
        {
            var position = ScreenToViewport(e.GetPosition(Editor));
            nodeVm.Location = position;
            vm.Nodes.Add(nodeVm);
        }
    }

    private Point ScreenToViewport(Point screenPos)
    {
        double x = (screenPos.X / Editor.ViewportZoom) + Editor.ViewportLocation.X;
        double y = (screenPos.Y / Editor.ViewportZoom) + Editor.ViewportLocation.Y;

        return new Point(x, y);
    }

    private async void AddNode_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is NodeEditorViewModel vm && e.Source is Control control)
        {
            vm.DraggingStart();
            var listVm = vm.BlueprintListVm;

            if (listVm.SelectedItem == null || listVm.SelectedItem.Node is not INodeLogic logic)
                return;

            Logger.Debug("Start drag & drop");
            var _vm = ((NodeRegistry)RegistryServices.BpNodes).GetNodeViewModel(logic.Urn);
            _dragGhost = new ContentControl()
            {
                Content = _vm,
                ContentTemplate = this.FindResource("NodeTemplate") as IDataTemplate,
                IsHitTestVisible = false,
                Opacity = 0.6
            };

            if (GhostOverlay != null)
            {
                GhostOverlay.Children.Add(_dragGhost);
            }

            var data = new DataTransfer();
            var item = new DataTransferItem();
            item.Set(BlueprintFormats.NodeLogicFormat, logic);
            data.Add(item);

            var result = await DragDrop.DoDragDropAsync(e, data, DragDropEffects.Copy); // Nettoyage final
            vm.DraggingEnd();
            GhostOverlay.Children.Clear();
            _dragGhost = null;
        }
    }
}