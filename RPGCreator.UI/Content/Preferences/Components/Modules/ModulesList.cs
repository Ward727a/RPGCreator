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
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Projektanker.Icons.Avalonia;
using RPGCreator.SDK;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules;
using Ursa.Controls;
using Color = Avalonia.Media.Color;
using FontWeight = Avalonia.Media.FontWeight;
using HorizontalAlignment = Avalonia.Layout.HorizontalAlignment;
using VerticalAlignment = Avalonia.Layout.VerticalAlignment;

namespace RPGCreator.UI.Content.Preferences.Components.Modules;

public class ModulesList : UserControl
{

    private class ModuleItem : UserControl
    {
        public readonly ModuleCandidate Candidate;
        
        private readonly Grid _itemGrid = new Grid();
        private readonly TextBlock _moduleName, _moduleAuthor, _moduleVersion, _moduleDescription = new TextBlock();

        private readonly Border _certifiedBorder = new Border();
        private readonly Icon _certifiedIcon = new Icon();
        
        private readonly StackPanel _tagsPanel = new StackPanel();
        
        public ModuleItem(ModuleCandidate candidate)
        {
            _moduleName = MakeTextBlock();
            _moduleAuthor = MakeTextBlock();
            _moduleVersion = MakeTextBlock();
            
            Candidate = candidate;

            ClipToBounds = false;
            CreateComponents();
            RegisterEvents();
            Content = _itemGrid;
        }

        private void CreateComponents()
        {
            _itemGrid.ColumnDefinitions = new ColumnDefinitions("Auto, *");
            _itemGrid.RowDefinitions = new RowDefinitions("Auto, Auto, Auto, Auto");
            _itemGrid.RowSpacing = 4;
            _itemGrid.ColumnSpacing = 4;
            
            _moduleName.Inlines = MakeInlines("Name: ", Candidate.Name);
            AddToGrid(_moduleName);
            
            _moduleAuthor.Inlines = MakeInlines("Author: ", Candidate.Author);
            AddToGrid(_moduleAuthor, 1);
            
            _moduleVersion.Inlines = MakeInlines("Version: ", Candidate.Version);
            AddToGrid(_moduleVersion, 2);
            
            _moduleDescription.Inlines = MakeInlines("Description: ", Candidate.Description);
            _moduleDescription.Inlines.Insert(1, new LineBreak());
            _moduleDescription.VerticalAlignment = VerticalAlignment.Top;
            _moduleDescription.TextWrapping = TextWrapping.Wrap;
            AddToGrid(_moduleDescription, 0, 1);
            Grid.SetRowSpan(_moduleDescription, 3);

            switch (Candidate.Certified)
            {
                case ModuleCandidate.CertificationState.Certified:
                {
                    _certifiedIcon.Value = "mdi-shield-check-outline";
                    _certifiedIcon.Foreground = Brushes.Green;
                    ToolTip.SetTip(_certifiedBorder, "This module has been certified by the RPG Creator team.");
                    break;
                }
                case ModuleCandidate.CertificationState.DllModified:
                {
                    _certifiedIcon.Value = "mdi-shield-remove-outline";
                    _certifiedIcon.Foreground = Brushes.Orange;
                    ToolTip.SetTip(_certifiedBorder,
                        "This module has a certification that isn't valid for this module and as such it may not be safe to use!");
                    break;
                }
                case ModuleCandidate.CertificationState.ModifiedCertification:
                {
                    _certifiedIcon.Value = "mdi-shield-alert-outline";
                    _certifiedIcon.Foreground = Brushes.Orange;
                    ToolTip.SetTip(_certifiedBorder,
                        "This module has a certification that has been modified by outside parties and as such it may not be safe to use!");
                    break;
                }
                case ModuleCandidate.CertificationState.NotCertified:
                {
                    _certifiedIcon.Value = "mdi-shield-off-outline";
                    _certifiedIcon.Foreground = Brushes.Red;
                    ToolTip.SetTip(_certifiedBorder, "This module is not certified and as such it may not be safe to use.");
                    break;
                }
            }
            
            _certifiedBorder.Background = Brushes.Transparent; // For tooltip to show
            _certifiedBorder.BorderThickness = new Thickness(0);
            _certifiedBorder.Child = _certifiedIcon;
            
            _certifiedIcon.IsHitTestVisible = false;
            _certifiedIcon.FontSize = 26;
            _certifiedBorder.HorizontalAlignment = HorizontalAlignment.Right;
            _certifiedBorder.VerticalAlignment = VerticalAlignment.Top;
            _certifiedBorder.Margin = new Thickness(0, -8, -15, 0);
            AddToGrid(_certifiedBorder, 0, 1);

            _tagsPanel.Orientation = Orientation.Horizontal;
            _tagsPanel.HorizontalAlignment = HorizontalAlignment.Right;
            _tagsPanel.Spacing = 4;
            AddToGrid(_tagsPanel, 3);
            Grid.SetColumnSpan(_tagsPanel, 2);

            if(EngineServices.ModuleManager.IsModuleStarted(Candidate.ModuleUrn))
            {
                AddActivatedTag();
            }
            else
            {
                AddDeactivatedTag();
            }

            Content = _itemGrid;
        }

        private void RegisterEvents()
        {
        }

        private void AddToGrid(Control control, int row = 0, int column = 0)
        {
            _itemGrid.Children.Add(control);
            Grid.SetRow(control, row);
            Grid.SetColumn(control, column);
        }

        private TextBlock MakeTextBlock()
        {
            return new TextBlock()
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
        }
        
        private InlineCollection MakeInlines(string title, string value)
        {
            var inlines = new InlineCollection();
            
            inlines.Add(new Run(title)
            {
                Foreground = Brushes.Gray
            });
            inlines.Add(new Run(value)
            {
            });
            return inlines;
        }

        private Border MakeTagBorder(Color color)
        {
            var borderRpg = new Shared.Types.Color(color.R, color.G, color.B, color.A).Darken(.2f);
            var borderColor = new Color(borderRpg.A, borderRpg.R, borderRpg.G, borderRpg.B);

            return new Border()
            {
                BorderThickness = new Thickness(4),
                CornerRadius = new CornerRadius(20),
                BorderBrush = new SolidColorBrush(borderColor),
                Background = new SolidColorBrush(color),
                Padding = new Thickness(20, 2)
            };
        }
        
        private void AddActivatedTag()
        {
            var border = MakeTagBorder(Color.FromUInt32(0x6400AA00));
            _tagsPanel.Children.Add(border);
            border.Child = new TextBlock()
            {
                Text = "Activated",
                Foreground = Brushes.White,
                FontSize = 12,
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            ToolTip.SetTip(border, "This module is currently activated.");
        }
        
        private void AddDeactivatedTag()
        {
            var border = MakeTagBorder(Color.FromUInt32(0x64FF0000));
            _tagsPanel.Children.Add(border);
            border.Child = new TextBlock()
            {
                Text = "Deactivated",
                Foreground = Brushes.White,
                FontSize = 12,
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            ToolTip.SetTip(border, "This module is currently deactivated.");
        }
    }

    private class ModuleParameters : UserControl
    {
        private class ModuleNotCertifiedStartConfirmModal : StackPanel
        {
            
            public event Action? YesButtonClicked;
            public event Action? NoButtonClicked;
            
            private readonly ModuleCandidate _candidate;
            
            private readonly TextBlock _description = new TextBlock();

            private readonly StackPanel _buttonsPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Spacing = 4,
                HorizontalAlignment = HorizontalAlignment.Right,
            };
            private readonly Button _yesButton = new Button()
            {
                Content = "Yes, I accept the risk, and start it anyway",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Classes = { "Danger", "Outline" }
            };

            private readonly Button _noButton = new Button()
            {
                Content = "No, don't start module",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                IsCancel = true
            };
            
            public ModuleNotCertifiedStartConfirmModal(ModuleCandidate candidate) : base()
            {
                _candidate = candidate;
                CreateComponents();
            }
            
            private void CreateComponents()
            {

                _description.Text = $"The module \"{_candidate.Name}\" hasn't been certified by the RPG Creator team.\n" +
                               $"This can lead to unexpected behavior and security risks.\n" +
                               $"Do you want to start the module anyway?\n\n" +
                               $"(If you are the creator of this module, please consider certifying it, it's free!)";
                _description.FontSize = 14;
                _description.HorizontalAlignment = HorizontalAlignment.Center;
                _description.VerticalAlignment = VerticalAlignment.Center;
                _description.Margin = new Thickness(0, 0, 0, 10);
                Children.Add(_description);

                Children.Add(_buttonsPanel);
                
                _buttonsPanel.Children.Add(_yesButton);
                _buttonsPanel.Children.Add(_noButton);
                Grid.SetColumn(_noButton, 1);
                
                _yesButton.Click += (_,_) => YesButtonClicked?.Invoke();
                _noButton.Click += (_,_) => NoButtonClicked?.Invoke();
            }
        }

        private class ModuleDllModifiedStartConfirmModal : StackPanel
        {
            
            public event Action? YesButtonClicked;
            public event Action? NoButtonClicked;
            
            private readonly ModuleCandidate _candidate;
            
            private readonly TextBlock _description = new TextBlock();

            private readonly StackPanel _buttonsPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Spacing = 4,
                HorizontalAlignment = HorizontalAlignment.Right,
            };
            private readonly Button _yesButton = new Button()
            {
                Content = "Yes, I accept the risk, and start it anyway",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Classes = { "Danger", "Outline" }
            };

            private readonly Button _noButton = new Button()
            {
                Content = "No, don't start module",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                IsCancel = true
            };
            
            public ModuleDllModifiedStartConfirmModal(ModuleCandidate candidate) : base()
            {
                _candidate = candidate;
                CreateComponents();
            }
            
            private void CreateComponents()
            {

                _description.Text = $"The module \"{_candidate.Name}\" contains a certification that wasn't made for this module!\n" +
                                    $"It may be a corrupted DLL, or an attempt from a malicious party!\n" +
                               $"This can lead to unexpected behavior and security risks.\n" +
                               $"Do you want to start the module anyway?\n\n" +
                               $"(If you are the creator of this module, please consider certifying your module again, it's free!)";
                _description.FontSize = 14;
                _description.HorizontalAlignment = HorizontalAlignment.Center;
                _description.VerticalAlignment = VerticalAlignment.Center;
                _description.Margin = new Thickness(0, 0, 0, 10);
                Children.Add(_description);

                Children.Add(_buttonsPanel);
                
                _buttonsPanel.Children.Add(_yesButton);
                _buttonsPanel.Children.Add(_noButton);
                Grid.SetColumn(_noButton, 1);
                
                _yesButton.Click += (_,_) => YesButtonClicked?.Invoke();
                _noButton.Click += (_,_) => NoButtonClicked?.Invoke();
            }
        }
        
        private class ModuleModifiedCertificationStartConfirmModal : StackPanel
        {
            
            public event Action? YesButtonClicked;
            public event Action? NoButtonClicked;
            
            private readonly ModuleCandidate _candidate;
            
            private readonly TextBlock _description = new TextBlock();

            private readonly StackPanel _buttonsPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Spacing = 4,
                HorizontalAlignment = HorizontalAlignment.Right,
            };
            private readonly Button _yesButton = new Button()
            {
                Content = "Yes, I accept the risk, and start it anyway",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Classes = { "Danger", "Outline" }
            };

            private readonly Button _noButton = new Button()
            {
                Content = "No, don't start module",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                IsCancel = true
            };
            
            public ModuleModifiedCertificationStartConfirmModal(ModuleCandidate candidate) : base()
            {
                _candidate = candidate;
                CreateComponents();
            }
            
            private void CreateComponents()
            {

                _description.Text = $"The module \"{_candidate.Name}\" contains a certification that has been modified!\n" +
                                    $"It may be a corrupted certification, or an attempt from a malicious party!\n" +
                               $"This can lead to unexpected behavior and security risks.\n" +
                               $"Do you want to start the module anyway?\n\n" +
                               $"(If you are the creator of this module, please consider certifying your module again (it's free!) or contact us!)";
                _description.FontSize = 14;
                _description.HorizontalAlignment = HorizontalAlignment.Center;
                _description.VerticalAlignment = VerticalAlignment.Center;
                _description.Margin = new Thickness(0, 0, 0, 10);
                Children.Add(_description);

                Children.Add(_buttonsPanel);
                
                _buttonsPanel.Children.Add(_yesButton);
                _buttonsPanel.Children.Add(_noButton);
                Grid.SetColumn(_noButton, 1);
                
                _yesButton.Click += (_,_) => YesButtonClicked?.Invoke();
                _noButton.Click += (_,_) => NoButtonClicked?.Invoke();
            }
        }
        private OverlayDialogHost _overlayDialogHost;
        private ModuleCandidate? _selectedModuleCandidate;
        private bool _isStarted = false;
        private ModuleCandidate.CertificationState CertificationState => _selectedModuleCandidate?.Certified ?? ModuleCandidate.CertificationState.NotCertified;

        private readonly StackPanel _bodyPanel = new StackPanel()
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Width = double.NaN,
            MaxWidth = double.MaxValue,
            Spacing = 4
        };

        private readonly Grid _startStopButtons = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("*, *"),
            ColumnSpacing = 4,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Width = double.NaN,
            MaxWidth = double.MaxValue
        };
        
        private readonly TextBlock _moduleName = new TextBlock();
        private readonly TextBlock _moduleAuthor = new TextBlock();
        private readonly TextBlock _moduleVersion = new TextBlock();
        private readonly Button _activateButton = new Button()
        {
            Content = "Start Module",
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        private readonly Button _deactivateButton = new Button()
        {
            Content = "Stop Module",
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        private readonly Button _openLocationButton = new Button()
        {
            Content = "Open Module Location (folder)",
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        
        public ModuleParameters(ModulesList modulesList)
        {
            _overlayDialogHost = modulesList._overlayDialogHost;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            Width = double.NaN;
            MaxWidth = double.MaxValue;
            CreateComponents();
            RegisterEvents(modulesList);
            SetCandidate(null);
            Content = _bodyPanel;
        }

        private void CreateComponents()
        {
            _bodyPanel.Children.Add(_moduleName);
            _bodyPanel.Children.Add(_moduleAuthor);
            _bodyPanel.Children.Add(_moduleVersion);
            _bodyPanel.Children.Add(_startStopButtons);
            
            _startStopButtons.Children.Add(_activateButton);
            _startStopButtons.Children.Add(_deactivateButton);
            Grid.SetColumn(_deactivateButton, 1);
            
            _bodyPanel.Children.Add(_openLocationButton);
        }

        private void RegisterEvents(ModulesList modulesList)
        {
            modulesList.ModuleSelectionChanged += SetCandidate;
            _activateButton.Click += StartModule;
            _deactivateButton.Click += StopModule;
            _openLocationButton.Click += OpenModuleLocation;
        }
        
        private bool CanProcessCommand()
        {
            return _selectedModuleCandidate.HasValue;
        }
        
        private void SetCandidate(ModuleCandidate? candidate)
        {
            _selectedModuleCandidate = candidate;
            _moduleName.Text = _selectedModuleCandidate?.Name ?? "No module selected";
            _moduleAuthor.Text = _selectedModuleCandidate?.Author ?? "No author specified";
            _moduleVersion.Text = _selectedModuleCandidate?.Version ?? "No version specified";
            _activateButton.IsEnabled = CanProcessCommand() && !EngineServices.ModuleManager.IsModuleStarted(_selectedModuleCandidate.Value.ModuleUrn);
            _deactivateButton.IsEnabled = CanProcessCommand() && EngineServices.ModuleManager.IsModuleStarted(_selectedModuleCandidate.Value.ModuleUrn);
            _openLocationButton.IsEnabled = CanProcessCommand();
            
            _isStarted = CanProcessCommand() && EngineServices.ModuleManager.IsModuleStarted(_selectedModuleCandidate.Value.ModuleUrn);
        }

        private void StartModule(object? sender, RoutedEventArgs e)
        {
            if (!CanProcessCommand()) return;
            if (_isStarted) return;

            switch (CertificationState)
            {
                case ModuleCandidate.CertificationState.NotCertified:
                {
                    var modal = new ModuleNotCertifiedStartConfirmModal(_selectedModuleCandidate.Value);
                    OverlayDialog.Show(modal, vm: null, hostId: _overlayDialogHost.HostId, options: new OverlayDialogOptions()
                    {
                        Buttons = DialogButton.None,
                        CanResize = false,
                        CanDragMove = false,
                        CanLightDismiss = false,
                        HorizontalAnchor = HorizontalPosition.Center,
                        VerticalAnchor = VerticalPosition.Center,
                        Title = "Module not certified - Please confirm",
                        IsCloseButtonVisible = false,
                        Mode = DialogMode.Warning,
                        FullScreen = true
                    });

                    modal.YesButtonClicked += () =>
                    {
                        _overlayDialogHost.Children.Clear();
                        EngineServices.ModuleManager.StartModule(_selectedModuleCandidate.Value.ModuleUrn, new EngineSecurityToken());
                    };
                    modal.NoButtonClicked += () =>
                    {
                        _overlayDialogHost.Children.Clear();
                    };
                    return;
                }
                case ModuleCandidate.CertificationState.DllModified:
                {
                    var modal = new ModuleDllModifiedStartConfirmModal(_selectedModuleCandidate.Value);
                    OverlayDialog.Show(modal, vm: null, hostId: _overlayDialogHost.HostId, options: new OverlayDialogOptions()
                    {
                        Buttons = DialogButton.None,
                        CanResize = false,
                        CanDragMove = false,
                        CanLightDismiss = false,
                        HorizontalAnchor = HorizontalPosition.Center,
                        VerticalAnchor = VerticalPosition.Center,
                        Title = "Module DLL corrupted or modified - Please confirm",
                        IsCloseButtonVisible = false,
                        Mode = DialogMode.Warning,
                        FullScreen = true
                    });

                    modal.YesButtonClicked += () =>
                    {
                        _overlayDialogHost.Children.Clear();
                        EngineServices.ModuleManager.StartModule(_selectedModuleCandidate.Value.ModuleUrn, new EngineSecurityToken());
                    };
                    modal.NoButtonClicked += () =>
                    {
                        _overlayDialogHost.Children.Clear();
                    };
                    return;
                }
                case ModuleCandidate.CertificationState.ModifiedCertification:
                {
                    var modal = new ModuleModifiedCertificationStartConfirmModal(_selectedModuleCandidate.Value);
                    OverlayDialog.Show(modal, vm: null, hostId: _overlayDialogHost.HostId, options: new OverlayDialogOptions()
                    {
                        Buttons = DialogButton.None,
                        CanResize = false,
                        CanDragMove = false,
                        CanLightDismiss = false,
                        HorizontalAnchor = HorizontalPosition.Center,
                        VerticalAnchor = VerticalPosition.Center,
                        Title = "Module certification corrupted or modified - Please confirm",
                        IsCloseButtonVisible = false,
                        Mode = DialogMode.Warning,
                        FullScreen = true
                    });

                    modal.YesButtonClicked += () =>
                    {
                        _overlayDialogHost.Children.Clear();
                        EngineServices.ModuleManager.StartModule(_selectedModuleCandidate.Value.ModuleUrn, new EngineSecurityToken());
                    };
                    modal.NoButtonClicked += () =>
                    {
                        _overlayDialogHost.Children.Clear();
                    };
                    return;
                }
                case ModuleCandidate.CertificationState.Certified:
                {
                    EngineServices.ModuleManager.StartModule(_selectedModuleCandidate.Value.ModuleUrn, new EngineSecurityToken());
                    return;
                }
            }
        }
        
        private void StopModule(object? sender, RoutedEventArgs e)
        {
            if (!CanProcessCommand()) return;
            if (!_isStarted) return;
            
            EngineServices.ModuleManager.StopModule(_selectedModuleCandidate.Value.ModuleUrn, new EngineSecurityToken());
        }
        
        private void OpenModuleLocation(object? sender, RoutedEventArgs e)
        {
            if (!CanProcessCommand()) return;
            
            var module = _selectedModuleCandidate.Value;
            if (!string.IsNullOrWhiteSpace(module.ModulePath) && File.Exists(module.ModulePath) && Path.GetDirectoryName(module.ModulePath) is { } moduleDirectory)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = moduleDirectory,
                        UseShellExecute = true
                    });
                }
                catch (Exception err)
                {
                    Logger.Error(err, "Could not open module location due to an unexpected error.");
                }
            }
        }
    }
    
    public event Action<ModuleCandidate?>? ModuleSelectionChanged;
    
    private Grid _bodyGrid = new Grid();

    private ScrollViewer _scrollViewer = new ScrollViewer();
    private ListBox _modulesList = new ListBox();
    
    private Grid _leftPanel = new Grid()
    {
        ColumnDefinitions = new ColumnDefinitions("22, *"),
        RowDefinitions = new RowDefinitions("*"),
        Width = 300
    };

    private Divider _leftPanelDivider = new Divider()
    {
        Orientation = Orientation.Vertical,
        VerticalAlignment = VerticalAlignment.Stretch,
        VerticalContentAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Center,
        Height = Double.NaN,
        MaxHeight = Double.MaxValue
    };

    private ModuleParameters _leftPanelContent;
    
    private OverlayDialogHost _overlayDialogHost = new OverlayDialogHost();

    private List<ModuleCandidate> _moduleCandidates;
    private ModuleCandidate? _selectedModuleCandidate;
    
    public ModulesList()
    {
        _leftPanelContent = new ModuleParameters(this);
        
        LoadModules();
        CreateComponents();
        RegisterEvents();
        Content = _bodyGrid;
    }
    
    private void CreateComponents()
    {
        _bodyGrid.ColumnDefinitions = new ColumnDefinitions("*, Auto");
        _bodyGrid.RowDefinitions = new RowDefinitions("Auto, *, Auto");
        
        _scrollViewer = new ScrollViewer();
        _bodyGrid.Children.Add(_scrollViewer);
        Grid.SetRow(_scrollViewer, 1);
        
        _modulesList = new ListBox();
        _scrollViewer.Content = _modulesList;

        _modulesList.ItemsSource = _moduleCandidates;
        _modulesList.ItemTemplate = new FuncDataTemplate<ModuleCandidate>((mc, _) => new ModuleItem(mc));
        
        _bodyGrid.Children.Add(_leftPanel);
        Grid.SetColumn(_leftPanel, 1);
        Grid.SetRow(_leftPanel, 1);
        
        _leftPanel.Children.Add(_leftPanelDivider);
        
        _leftPanel.Children.Add(_leftPanelContent);
        Grid.SetColumn(_leftPanelContent, 1);
        
        _bodyGrid.Children.Add(_overlayDialogHost);
        Grid.SetRow(_overlayDialogHost, 0);
        Grid.SetColumn(_overlayDialogHost, 0);
        Grid.SetColumnSpan(_overlayDialogHost, 2);
        Grid.SetRowSpan(_overlayDialogHost, 3);
    }

    private void LoadModules()
    {
        _moduleCandidates = EngineServices.ModuleManager.GetAllLoadedModules(new EngineSecurityToken()).ToList();
    }

    private void RefreshList()
    {
        Dispatcher.UIThread.Post(() =>
        {
            _modulesList.ItemsSource = null;
            _modulesList.ItemsSource = _moduleCandidates;
        }, DispatcherPriority.Default);
    }

    private void RegisterEvents()
    {
        EngineServices.ModuleManager.ModuleLoaded += (_) => RefreshModules();
        EngineServices.ModuleManager.ModuleUnloaded += (_) => RefreshModules();
        EngineServices.ModuleManager.ModuleStarted += (_) => RefreshModules();
        EngineServices.ModuleManager.ModuleStopped += (_) => RefreshModules();

        _modulesList.SelectionChanged += (_, _) =>
        {
            if (_modulesList.SelectedItem is ModuleCandidate selectedItem)
                _selectedModuleCandidate = selectedItem;
            else
                _selectedModuleCandidate = null;
            
            ModuleSelectionChanged?.Invoke(_selectedModuleCandidate);
        };
        
        
        void RefreshModules()
        {
            LoadModules();
            RefreshList();
        }
    }
}