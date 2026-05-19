using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using CommunityToolkit.Diagnostics;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Characters;
using RPGCreator.SDK.Commands;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Definition;
using RPGCreator.SDK.Modules.Features.Entity;
using RPGCreator.SDK.Services.EditorUiService;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;
using RPGCreator.UI.Common.Modal.Browser;
using Thickness = Avalonia.Thickness;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.CharactersEditor.Tabs;

public class CharacterFeaturesTab : UserControl
{
    #region Properties

    public CharacterData Data;
    private CharacterEditorWindowControl _parent;
    
    #endregion

    #region Components

    private Grid Body { get; set; } = null!;
    private Button AddFeatureButton { get; set; } = null!;
    private StackPanel FeaturesList { get; set; } = null!;

    #endregion

    #region Constructors

    public CharacterFeaturesTab(CharacterData data, CharacterEditorWindowControl parent)
    {
        Data = data;
        _parent = parent;
        Name = "Features"; // Define the name of the tab
        CreateComponents();
        RegisterEvents();
        Content = Body;
    }
    
    #endregion

    #region Methods

    private void CreateComponents()
    {
        Body = new Grid()
        {
            RowDefinitions = new RowDefinitions("Auto, *"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(10)
        };

        AddFeatureButton = new Button
        {
            Content = "Add Feature",
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 0, 10)
        };

        Body.Children.Add(AddFeatureButton);


        var scrollContainer = new ScrollViewer()
        {
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
        };

        FeaturesList = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Thickness(10)
        };
        scrollContainer.Content = FeaturesList;

        Body.Children.Add(scrollContainer);
        Grid.SetRow(scrollContainer, 1);
    }

    private void RegisterEvents()
    {
        AddFeatureButton.Click += OnAddFeatureButtonClick;
    }

    private bool InitialFeaturesLoaded;
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (!InitialFeaturesLoaded)
        {
            LoadFeatures();
            InitialFeaturesLoaded = true;
        }
    }

    private void LoadFeatures()
    {
        Guard.IsNotNull(Data);
        foreach (var featureData in Data.Features)
        {
            if (featureData.IsSubFeature) continue;
            var feature = featureData.ToEntityFeature();
            var featureControl = new FeatureItemControl(feature, featureData.InstanceId, _parent);
            FeaturesList.Children.Add(featureControl);
            featureControl.FeatureRemoved += OnFeatureRemoved;
            feature.OnAddedToUi(Data, featureControl.Context);
            DetachedFromVisualTree += (_,_) => feature.OnRemovedFromUi(Data, featureControl.Context);
        }
    }

    private void OnFeatureRemoved(Ulid instanceId, IEntityFeature feature, FeatureItemControl control)
    {
        var removeCmd = new FeatureRemovedCommand(instanceId, feature, FeaturesList, Data, control).WhenExecuted((_) =>
        {
            control.FeatureRemoved -= OnFeatureRemoved;
        }).WhenUndone((_) =>
        {
            control.FeatureRemoved += OnFeatureRemoved;
        });
        EngineServices.UndoRedoService.ExecuteCommand(removeCmd);
    }

    #endregion

    #region Events Handlers

    /// <summary>
    /// A command representing the addition of a feature to the character.<br/>
    /// So that it can be undone/redone.
    /// </summary>
    private class FeatureAddedCommand : BaseCommand
    {
        /// <summary>
        /// The instance id of the feature added.<br/>
        /// This is used to identify the feature instance in the character data.<br/>
        /// It is generated when the feature is added.
        /// </summary>
        public Ulid FeatureInstanceId { get; private set; }
        
        private readonly IEntityFeature _feature;
        private readonly StackPanel _featuresList;
        private readonly CharacterData _characterData;
        private readonly CharacterEditorWindowControl _parent;
        public FeatureItemControl? FeatureControl { get; private set; }
        public override string Name { get; }

        public FeatureAddedCommand(IEntityFeature feature, StackPanel featuresList, CharacterData characterData, CharacterEditorWindowControl parent)
        {
            _feature = feature;
            _featuresList = featuresList;
            _characterData = characterData;
            _parent = parent;

            var displayableName = string.IsNullOrWhiteSpace(_feature.FeatureName)
                ? _feature.FeatureUrn.ToString()
                : _feature.FeatureName;

            if (displayableName.Length > 100)
                displayableName = string.Concat(displayableName.AsSpan(0, 97), "...");

            Name = $"Add Feature: {displayableName}";
        }

        protected override void OnExecute()
        {
            _feature.OnAddingToDefinition(_characterData);

            if (FeatureInstanceId != Ulid.Empty)
            {
                _characterData.AddFeatureConfig(_feature.FeatureUrn, _feature.Configuration, FeatureInstanceId);
            } else
                FeatureInstanceId = _characterData.AddFeatureConfig(_feature);
            _feature.OnAddedToDefinition(_characterData, FeatureInstanceId);
            
            FeatureControl ??= new FeatureItemControl(_feature, FeatureInstanceId, _parent);

            _featuresList.Children.Add(FeatureControl);
            FeatureControl.CallAddedToUi();
        }

        protected override void OnUndo()
        {
            if (FeatureControl != null)
            {
                FeatureControl.CallRemovedFromUi();
                _featuresList.Children.Remove(FeatureControl);
                
                _feature.OnRemovedFromDefinition(_characterData);
                _characterData.RemoveFeatureConfig(FeatureInstanceId);
            }
            else
                Logger.Error("Cannot undo feature addition, feature control is null");
        }
    }

    private class FeatureRemovedCommand : BaseCommand
    {
        public override string Name { get; }

        private readonly Ulid _featureInstanceId;
        private readonly IEntityFeature _feature;
        private readonly StackPanel _featuresList;
        private readonly CharacterData _characterData;
        private readonly FeatureItemControl? _featureControl;

        private CustomData _featureConfig = new();

        public FeatureRemovedCommand(Ulid featureInstanceIdInstanceId, IEntityFeature feature, StackPanel featuresList,
            CharacterData characterData, FeatureItemControl featureControl)
        {
            _featureInstanceId = featureInstanceIdInstanceId;
            _feature = feature;
            _featuresList = featuresList;
            _characterData = characterData;
            _featureControl = featureControl;

            var displayableName = string.IsNullOrWhiteSpace(feature.FeatureName)
                ? feature.FeatureUrn.ToString()
                : feature.FeatureName;

            if (displayableName.Length > 100)
                displayableName = string.Concat(displayableName.AsSpan(0, 97), "...");

            Name = $"Remove Feature: {displayableName}";
        }

        protected override void OnExecute()
        {
            Guard.IsNotNull(_featureControl);
            Guard.IsNotEqualTo(_featureInstanceId, Ulid.Empty);

            if (IsFirstExecution)
            {
                _featureConfig = _feature.Configuration.Clone();
            }

            _featureControl.CallRemovedFromUi();
            _featuresList.Children.Remove(_featureControl);
            
            _feature.OnRemovedFromDefinition(_characterData);
            _characterData.RemoveFeatureConfig(_featureInstanceId);
        }

        protected override void OnUndo()
        {
            if (_featureControl == null)
            {
                Logger.Error("Cannot undo feature removal, feature control is null");
                #if DEBUG
                Logger.Dump(this);
                #endif
                return;
            }
            
            _feature.OnAddingToDefinition(_characterData);
            _characterData.AddFeatureConfig(_feature.FeatureUrn, _featureConfig, _featureInstanceId);
            _feature.OnAddedToDefinition(_characterData, _featureInstanceId);
            
            _featuresList.Children.Add(_featureControl);
            _featureControl.CallAddedToUi();
        }
    }

    /// <summary>
    /// Just a test list to keep track of added features in this session.<br/>
    /// This will not be kept in the final implementation.
    /// </summary>
    private readonly List<IEntityFeature> _addedFeatures = new();

    async void OnAddFeatureButtonClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            Logger.Debug("Add Feature button clicked");

            var dialog = new FeatureLibraryExplorerDialog();
            var result = await EditorUiServices.DialogService.ConfirmAsync("Add Feature", dialog,
                new DialogStyle(Height: 450, Width: 500, CanResize: true, SizeToContent: DialogSizeToContent.None));

            if (result && dialog.HasFeature)
            {
                var feature = dialog.GetSelectedFeature();
                Logger.Debug("Feature selected: {featureName}", feature.FeatureName);
                var cmd = new FeatureAddedCommand(feature, FeaturesList, Data, _parent).WhenExecuted((cmd) =>
                {
                    if(cmd is not FeatureAddedCommand addedCmd)
                    {
                        Logger.Error("Executed command is not of type FeatureAddedCommand");
                        return;
                    }
                    _addedFeatures.Add(feature);
                    addedCmd.FeatureControl?.FeatureRemoved += OnFeatureRemoved;
                    
                    EditorUiServices.NotificationService.Success("Feature Added",
                        $"The feature '{feature.FeatureName}' has been added.",
                        new NotificationOptions(5000));
                }).WhenUndone((cmd) =>
                {
                    if(cmd is not FeatureAddedCommand addedCmd)
                    {
                        Logger.Error("Undone command is not of type FeatureAddedCommand");
                        return;
                    }
                    _addedFeatures.Remove(feature);
                    addedCmd.FeatureControl?.FeatureRemoved -= OnFeatureRemoved;
                    
                    EditorUiServices.NotificationService.Info("Feature Removed",
                        $"The feature '{feature.FeatureName}' has been removed.",
                        new NotificationOptions(5000));
                });
                EngineServices.UndoRedoService.ExecuteCommand(cmd);
            }
        }
        catch (Exception error)
        {
            Logger.Error("Error while adding feature: {error}", error.Message);
        }

        CheckDependencies();
    }

    /// <summary>
    /// DO NOT USE!!!!<br/>
    /// This is just a "dev" method, this is right now not the final implementation of dependency checking.<br/>
    /// It will just log warnings and show notifications for missing dependencies!!!<br/>
    /// DO NOT USE FOR THE RELEASE!!!
    /// </summary>
    public void CheckDependencies()
    {
        var presentFeatures = new HashSet<URN>();
        foreach (var entityFeature in _addedFeatures)
        {
            presentFeatures.Add(entityFeature.FeatureUrn);
        }

        foreach (var entityFeature in _addedFeatures)
        {
            var dependencies = entityFeature.DependentFeatures;
            foreach (var dependency in dependencies)
            {
                if (!presentFeatures.Contains(dependency))
                {
                    Logger.Warning("Feature {feature} is missing dependency {dependency}", entityFeature.FeatureUrn,
                        dependency);
                    
                    // Here you could also notify the user via UI
                    EditorUiServices.NotificationService.Error("Feature Dependency Missing",
                        $"The feature '{entityFeature.FeatureName}' is missing the required dependency feature '{dependency}'.",
                        new NotificationOptions(10000));
                }
            }
        }
    }

    #endregion
}