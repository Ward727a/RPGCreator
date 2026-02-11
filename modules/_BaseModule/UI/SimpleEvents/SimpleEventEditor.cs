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

using _BaseModule.SimpleEvents.Conditions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions;
using RPGCreator.SDK.Assets.Definitions.SimpleEvent;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Modules.Definition;
using RPGCreator.SDK.Modules.SimpleEvents;

namespace _BaseModule.UI.SimpleEvents;

public class SimpleEventEditor : UserControl
{
    private Grid _editorGrid = null!;
    
    private Grid _topBarGrid = null!;
    private AutoCompleteBox _autoCompleteBox = null!;
    private Button _addButton = null!;
    
    public SimpleEventEditor(List<Ulid>? simpleEventsDefinitionIds = null)
    {
        CreateComponents();
        RegisterEvents();
        
        if(simpleEventsDefinitionIds != null)
            LoadSimpleEvents(simpleEventsDefinitionIds);
    }

    private void LoadSimpleEvents(List<Ulid> simpleEventsDefinitionIds)
    {
    }

    private void CreateComponents()
    {
        _editorGrid = new Grid()
        {
            RowDefinitions = new RowDefinitions("Auto, *, Auto"),
            RowSpacing = 5
        };
        Content = _editorGrid;

        _topBarGrid = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("*, Auto"),
            ColumnSpacing = 5
        };
        _editorGrid.Children.Add(_topBarGrid);

        _autoCompleteBox = new AutoCompleteBox() { };
        _topBarGrid.Children.Add(_autoCompleteBox);
        
        _addButton = new Button() { Content = "New event" };
        _topBarGrid.Children.Add(_addButton);
        Grid.SetColumn(_addButton, 1);
        
        var testEntry = new SimpleEventEntry();
        _editorGrid.Children.Add(testEntry);
        Grid.SetRow(testEntry, 1);

    }

    private void RegisterEvents()
    {
    }

}

public class SimpleEventEntry : UserControl
{
    
    #region SubClass

    private class SimpleEventConditionEntry : UserControl
    {

        private class ConditionUiEntry : UserControl
        {

            public event Action<ConditionUiEntry> OnRemovedAsked;
            
            private TextBlock _conditionEntryPanel = null!;

            private List<SimpleEventPropertyDescriptor> _availableProperties;

            public BaseSimpleEventCondition Condition { get; private set; }
            public CustomData CurrentParameters { get; private set; }

            private bool _isInEditMode = false;

            public ConditionUiEntry(BaseSimpleEventCondition condition, Action<ConditionUiEntry>? onRemovedAsked)
            {
                Condition = condition;
                CurrentParameters = condition.Parameters;
                _availableProperties = condition.GetConditionProperties();

                CreateComponents();
            }

            private void AddRemoveBlock()
            {
                var textBlock = new TextBlock()
                {
                    Text = "Remove",
                    Foreground = Brushes.IndianRed,
                    Cursor = new(StandardCursorType.Hand),
                    FontWeight = FontWeight.SemiBold,
                    Margin = new Thickness(5, 0, 0, -4),
                    VerticalAlignment = VerticalAlignment.Center
                };
                ToolTip.SetTip(textBlock, "Remove this condition from the event.");
                textBlock.PointerPressed += (s, e) =>
                {
                    Logger.Debug("Remove asked for condition {conditionName}", Condition.Name);
                    OnRemovedAsked?.Invoke(this);
                };
                _conditionEntryPanel.Inlines!.Add(new InlineUIContainer(textBlock));
            }
            
            private void AddTextBlock(string text)
            {
                _conditionEntryPanel.Inlines!.Add(
                    new TextBlock
                    {
                        Text = text,
                        Margin = new Thickness(0, 0, 0, -4)
                    });
            }

            private void AddPropertyTextBlock(SimpleEventPropertyDescriptor property, string text)
            {
                var textBlock = new TextBlock()
                {
                    Text = $"{text}",
                    Foreground = Brushes.LightSkyBlue,
                    Cursor = new(StandardCursorType.Hand),
                    Margin = new Thickness(0, 0, 0, -4)
                };
                textBlock.PointerPressed += (s, e) =>
                {
                    Logger.Debug("Clicked on property {propertyName} of condition {conditionName}",
                        property.DisplayName, Condition.Name);
                    OpenEditorFor(property.Key, textBlock, ShowEdit);
                };
                _conditionEntryPanel.Inlines!.Add(new InlineUIContainer(textBlock));
            }

            private async void OpenEditorFor(string key, TextBlock target, Action OnEdited)
            {
                var prop = _availableProperties.FirstOrDefault(p => p.Key == key);
                if (prop == null)
                {
                    Logger.Error(
                        "[ConditionUiEntry] Failed to find property with key '{key}' for condition '{conditionName}'!",
                        key, Condition.Name);
                    return;
                }

                var obj = await prop.GetInputControl(CurrentParameters);

                if (obj == null)
                {
                    Logger.Info("[ConditionUiEntry] No editor returned for property '{propertyName}' of condition '{conditionName}', skipping editor opening.",
                        prop.DisplayName, Condition.Name);
                    return;
                }

                if (obj.GetType().IsAssignableTo(typeof(BaseAssetDef)))
                {
                    CurrentParameters.Set(prop.Key, (obj as BaseAssetDef)?.Unique ?? Ulid.Empty);
                    ShowEdit();
                    return;
                }

                if (obj is Control control)
                {
                    var flyout = new Flyout()
                    {
                        Content = control,
                        Placement = PlacementMode.BottomEdgeAlignedLeft,
                        ShowMode = FlyoutShowMode.Standard,
                    };
                    flyout.ShowAt(target);
                    flyout.Closed += (s, e) => OnEdited();
                }
            }

            private void CreateComponents()
            {
                _conditionEntryPanel = new TextBlock()
                {
                    Inlines = new InlineCollection()
                };
                Content = _conditionEntryPanel;
                ShowDisplay();
            }

            public bool ToggleEdit()
            {
                _isInEditMode = !_isInEditMode;
                
                _conditionEntryPanel.Inlines!.Clear();
                if (_isInEditMode)
                    ShowEdit();
                else
                    ShowDisplay();
                return _isInEditMode;
            }

            private void ShowEdit()
            {
                _conditionEntryPanel.Inlines!.Clear();
                foreach (var part in Condition.GetConditionsTextAsPart(CurrentParameters))
                {
                    var key = part.Key;
                    var text = part.Value;

                    if (key.StartsWith("@"))
                    {
                        AddTextBlock(text);
                        continue;
                    }
                    
                    var property = _availableProperties.FirstOrDefault(p => p.Key == key);
                    if (property != null)
                    {
                        AddPropertyTextBlock(property, text);
                        AddTextBlock(" | ");
                    }
                    else
                    {
                        Logger.Error(
                            "[ConditionUiEntry] Failed to find property with key '{key}' for condition '{conditionName}'!",
                            key, Condition.Name);
                        AddTextBlock($"[ERROR KEY NOT FOUND: {key}]");
                    }
                }
                AddRemoveBlock();
            }

            private void ShowDisplay()
            {
                _conditionEntryPanel.Inlines!.Clear();
                _conditionEntryPanel.Inlines.Add(new Run(Condition.GetConditionText(CurrentParameters)));
            }
        }

        
        private WrapPanel _conditionNameTextBlock = null!;

        private InlineCollection _inlineCollection = new();

        private ConditionUiEntry test;
        
        private Border _AddCondBlock = null!;
        private Border _AddThenActionBlock = null!;
        private Border _AddElseActionBlock = null!;
        
        private List<BaseSimpleEventCondition> _conditions = new List<BaseSimpleEventCondition>();
        private List<BaseSimpleEventAction> _thenActions = new List<BaseSimpleEventAction>();
        private List<BaseSimpleEventAction> _elseActions = new List<BaseSimpleEventAction>();
        
        public SimpleEventConditionEntry()
        {
            test = new ConditionUiEntry(new StatValueSimpleCondition(), entry =>
            {
                _conditionNameTextBlock.Children.Remove(entry);
            });
            
            CreateComponents();
            _conditionNameTextBlock.Children.Add(AddIfBlock());
            _conditionNameTextBlock.Children.Add(MakeBasicBorder(test));
            _conditionNameTextBlock.Children.Add(_AddCondBlock);
            _conditionNameTextBlock.Children.Add(_AddThenActionBlock);
            _conditionNameTextBlock.Children.Add(_AddElseActionBlock);
            RegisterEvents();
        }
        
        private Border MakeBasicBorder(Control children)
        {
            var border = new Border()
            {
                BorderBrush = Brushes.DimGray,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(5),
                Margin = new Thickness(0, 5, 0, 5),
                CornerRadius = new CornerRadius(5),
            };
            var blockPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Spacing = 5
            };
            border.Child = blockPanel;
            blockPanel.Children.Add(children);
            return border;
        }
        
        private Border AddIfBlock()
        {
            var border = new Border()
            {
                BorderBrush = Brushes.DimGray,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(5),
                Margin = new Thickness(0, 5, 0, 5),
                CornerRadius = new CornerRadius(5),
            };
            var blockPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Spacing = 5
            };
            border.Child = blockPanel;
            var ifText = new TextBlock()
            {
                Text = "IF",
                FontWeight = FontWeight.Bold,
                Foreground = Brushes.LightGreen
            };
            blockPanel.Children.Add(ifText);
            return border;
        }
        
        private Border AddThenBlock()
        {
            var border = new Border()
            {
                BorderBrush = Brushes.DimGray,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(5),
                Margin = new Thickness(0, 5, 0, 5),
                CornerRadius = new CornerRadius(5),
            };
            var blockPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Spacing = 5
            };
            border.Child = blockPanel;
            var ifText = new TextBlock()
            {
                Text = "THEN",
                FontWeight = FontWeight.Bold,
                Foreground = Brushes.LightGreen
            };
            blockPanel.Children.Add(ifText);
            return border;
        }
        
        private Border AddElseBlock()
        {
            var border = new Border()
            {
                BorderBrush = Brushes.DimGray,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(5),
                Margin = new Thickness(0, 5, 0, 5),
                CornerRadius = new CornerRadius(5),
            };
            var blockPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Spacing = 5
            };
            border.Child = blockPanel;
            var ifText = new TextBlock()
            {
                Text = "ELSE",
                FontWeight = FontWeight.Bold,
                Foreground = Brushes.LightGreen
            };
            blockPanel.Children.Add(ifText);
            return border;
        }

        private Border AddAndBlock()
        {
            var border = new Border()
            {
                BorderBrush = Brushes.DimGray,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(5),
                Margin = new Thickness(0, 5, 0, 5),
                CornerRadius = new CornerRadius(5),
            };
            var blockPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Spacing = 5
            };
            border.Child = blockPanel;
            var ifText = new TextBlock()
            {
                Text = "AND",
                FontWeight = FontWeight.Bold,
                Foreground = Brushes.LightGreen
            };
            blockPanel.Children.Add(ifText);
            return border;
        }
        
        private void CreateComponents()
        {
            _conditionNameTextBlock = new WrapPanel()
            {
                ItemSpacing = 5,
                LineSpacing = 5
            };
            Content = _conditionNameTextBlock;

            {
                _AddCondBlock = new Border()
                {
                    BorderBrush = Brushes.DimGray,
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(5),
                    Margin = new Thickness(0, 5, 0, 5),
                    CornerRadius = new CornerRadius(5),
                    Cursor = new(StandardCursorType.Hand),
                    IsVisible = false
                };
                var blockPanel = new StackPanel()
                {
                    Orientation = Orientation.Vertical,
                    Spacing = 5
                };
                _AddCondBlock.Child = blockPanel;
                var ifText = new TextBlock()
                {
                    Text = "+ Add cond.",
                    FontWeight = FontWeight.Bold,
                    Foreground = Brushes.DodgerBlue
                };
                ToolTip.SetTip(ifText, "Add a condition to this event.");
                blockPanel.Children.Add(ifText);
                _AddCondBlock.PointerPressed += (s, e) => { };
            }

            {
                _AddThenActionBlock = new Border()
                {
                    BorderBrush = Brushes.DimGray,
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(5),
                    Margin = new Thickness(0, 5, 0, 5),
                    CornerRadius = new CornerRadius(5),
                    Cursor = new(StandardCursorType.Hand),
                    IsVisible = false
                };
                var blockPanel = new StackPanel()
                {
                    Orientation = Orientation.Vertical,
                    Spacing = 5
                };
                _AddThenActionBlock.Child = blockPanel;
                var ifText = new TextBlock()
                {
                    Text = "+ Add THEN",
                    FontWeight = FontWeight.Bold,
                    Foreground = Brushes.DodgerBlue
                };
                ToolTip.SetTip(ifText, "Add a THEN action to this event.");
                blockPanel.Children.Add(ifText);
                _AddThenActionBlock.PointerPressed += (s, e) => { };
            }
            
            {
                _AddElseActionBlock = new Border()
                {
                    BorderBrush = Brushes.DimGray,
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(5),
                    Margin = new Thickness(0, 5, 0, 5),
                    CornerRadius = new CornerRadius(5),
                    Cursor = new(StandardCursorType.Hand),
                    IsVisible = false
                };
                var blockPanel = new StackPanel()
                {
                    Orientation = Orientation.Vertical,
                    Spacing = 5
                };
                _AddElseActionBlock.Child = blockPanel;
                var ifText = new TextBlock()
                {
                    Text = "+ Add ELSE",
                    FontWeight = FontWeight.Bold,
                    Foreground = Brushes.DodgerBlue
                };
                ToolTip.SetTip(ifText, "Add an ELSE action to this event.");
                blockPanel.Children.Add(ifText);
                _AddElseActionBlock.PointerPressed += (s, e) => { };
            }
        }
        
        public bool ToggleEdit()
        {
            var val = test.ToggleEdit();

            if (val)
            {
                _AddCondBlock.IsVisible = true;
                _AddThenActionBlock.IsVisible = true;
                _AddElseActionBlock.IsVisible = true;
            }
            else
            {
                _AddCondBlock.IsVisible = false;
                _AddThenActionBlock.IsVisible = false;
                _AddElseActionBlock.IsVisible = false;
            }
            
            return val;
        }

        private void RegisterEvents()
        {
        }
        
    }
    
    #endregion
    
    private BaseSimpleEventDefinition _definition;
    
    private Grid _entryGrid = null!;
    
    private Grid _topBarGrid = null!;
    private TextBox _eventTag = null!;
    private Button _editButton = null!;
    private Button _removeButton = null!;
    
    public SimpleEventEntry(BaseSimpleEventDefinition? definition = null)
    {
        _definition = definition ?? EngineServices.AssetsManager.CreateAsset<BaseSimpleEventDefinition>();
        
        CreateComponents();
        RegisterEvents();
    }

    private void CreateComponents()
    {
        
        _entryGrid = new Grid()
        {
            RowDefinitions = new RowDefinitions("Auto, Auto"),
            RowSpacing = 5
        };
        Content = _entryGrid;
        
        _topBarGrid = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("*, Auto, Auto"),
            ColumnSpacing = 5
        };
        _entryGrid.Children.Add(_topBarGrid);
        
        _eventTag = new TextBox()
        {
            Text = _definition.Tag,
            IsEnabled = false
        };
        _topBarGrid.Children.Add(_eventTag);
        
        _removeButton = new Button()
        {
            Content = "Remove",
        };
        _topBarGrid.Children.Add(_removeButton);
        Grid.SetColumn(_removeButton, 1);

        _editButton = new Button()
        {
            Content = "Edit",
        };
        _topBarGrid.Children.Add(_editButton);
        Grid.SetColumn(_editButton, 2);
        
        var testConditionEntry = new SimpleEventConditionEntry();
        _entryGrid.Children.Add(testConditionEntry);
        Grid.SetRow(testConditionEntry, 1);

        _editButton.Click += (s, e) =>
        {
            _eventTag.IsEnabled = testConditionEntry.ToggleEdit();
        };
    }

    private void RegisterEvents()
    {
    }

}