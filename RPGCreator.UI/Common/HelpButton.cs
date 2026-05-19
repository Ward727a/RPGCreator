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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using LiveMarkdown.Avalonia;
using RPGCreator.SDK;
using RPGCreator.SDK.Common.Attributes;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.UIModule;
using RPGCreator.SDK.Services.EditorUiService;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;
using RPGCreator.UI.Contexts;
using TextMateSharp.Grammars;
using Thickness = Avalonia.Thickness;

namespace RPGCreator.UI.Common;

public class HelpButton : UserControl
{
    public static readonly StyledProperty<URN> HelpDocsKeyProperty =
        AvaloniaProperty.Register<HelpButton, URN>(nameof(HelpDocsKey));
    
    public static readonly StyledProperty<int> ButtonFontSizeProperty =
        AvaloniaProperty.Register<HelpButton, int>(nameof(ButtonFontSizeProperty), 12);
    
    public static readonly StyledProperty<Thickness> ButtonPaddingProperty =
        AvaloniaProperty.Register<HelpButton, Thickness>(nameof(ButtonPaddingProperty), new Thickness(4));
    
    public static readonly StyledProperty<string> ButtonTextProperty =
        AvaloniaProperty.Register<HelpButton, string>(nameof(ButtonTextProperty), "?");
    
    public static readonly StyledProperty<CornerRadius> ButtonCornerRadiusProperty =
        AvaloniaProperty.Register<HelpButton, CornerRadius>(nameof(ButtonCornerRadiusProperty), new CornerRadius(4));
    
    public static readonly StyledProperty<Button> ButtonProperty =
        AvaloniaProperty.Register<HelpButton, Button>(nameof(ButtonProperty), null);
    
    [ExposePropToPlugin("HelpButton", canSet: true)]
    public URN HelpDocsKey
    {
        get => GetValue(HelpDocsKeyProperty);
        set => SetValue(HelpDocsKeyProperty, value);
    }

    [ExposePropToPlugin("HelpButton", canSet: true)]
    public int ButtonFontSize
    {
        get => GetValue(ButtonFontSizeProperty);
        set => SetValue(ButtonFontSizeProperty, value);
    }
    
    [ExposePropToPlugin("HelpButton", canSet: true)]
    public Thickness ButtonPadding
    {
        get => GetValue(ButtonPaddingProperty);
        set => SetValue(ButtonPaddingProperty, value);
    }
    
    [ExposePropToPlugin("HelpButton", canSet: true)]
    public string ButtonText
    {
        get => GetValue(ButtonTextProperty);
        set
        {
            if (value == field) return;
            if (value == null) value = "?";
            SetValue(ButtonTextProperty, value);
            if(Button != null)
                Button.Content = value;
        }
    }
    
    [ExposePropToPlugin("HelpButton")]
    public CornerRadius ButtonCornerRadius
    {
        get => GetValue(ButtonCornerRadiusProperty);
        set
        {
            if (value == field) return;
            SetValue(ButtonCornerRadiusProperty, value);
            if(Button != null)
                Button.CornerRadius = value;
        }
            
    }

    [ExposePropToPlugin("HelpButton")]
    public Button Button
    {
        get => GetValue(ButtonProperty);
        set
        {
            if (value == field) return;
            if(value != null)
            {
                value.Click -= Clicked;
            }
            SetValue(ButtonProperty, value);
            if(value != null)
            {
                value.Click += Clicked;
            }
            Content = value;
        }
    }

    public HelpButton()
    {
        InitializeIfNeeded();
        CreateComponent();
        RegisterEvents();
        Content = Button;
    }
    
    public HelpButton(URN helpDocsKey, string buttonText = "?")
    {
        CreateComponent();
        RegisterEvents();
        Button.Content = buttonText;
        Content = Button;
        HelpDocsKey = helpDocsKey;
        
        HelpButtonContext.Config config = new HelpButtonContext.Config
        {
            GetButton = () => Button,
            GetHelpDocsKey = () => HelpDocsKey,
            SetHelpDocsKey = (newKey) => HelpDocsKey = newKey,
            GetButtonFontSize = () => ButtonFontSize,
            SetButtonFontSize = (value)=> ButtonFontSize = value,
            GetButtonPadding = () => ButtonPadding,
            SetButtonPadding = (value)=> ButtonPadding = value,
            GetButtonText = () => ButtonText,
            SetButtonText = (value)=> ButtonText = value,
        };
        
        EditorUiServices.ExtensionManager.ApplyExtensions(UIRegion.HelpButton, this, new HelpButtonContext(config));
    }
    
    public void SetHelpDocsKey(URN newKey)
    {
        HelpDocsKey = newKey;
    }

    private void CreateComponent()
    {
        Button = new Button
        {
            FontSize = ButtonFontSize,
            Padding = ButtonPadding,
            Content = ButtonText,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };
    }

    private void RegisterEvents()
    {
        Button.Click += Clicked;
    }

    private void Clicked(object? sender, RoutedEventArgs e)
    {
        OpenHelp();
    }

    /// <summary>
    /// Opens the help window with the content retrieved from the documentation service using the provided key. If no content is found, an error notification is shown.<br/>
    /// This is the method that is used when the help button is clicked, but it can also be called directly to open the help window programmatically.
    /// </summary>
    public void OpenHelp()
    {
        var helpContent = EditorUiServices.DocService.GetDocumentation(HelpDocsKey);

        if (string.IsNullOrWhiteSpace(helpContent))
        {
            EditorUiServices.NotificationService.Error("No help available for this item.", $"The documentation for '{HelpDocsKey}' is missing or empty.");
            return;
        }

        EditorUiServices.DialogService.ShowPromptAsync(
            "Help window", 
            new HelpWindow(helpContent),
            new DialogStyle(800, 600, CanResize:true, SizeToContent:DialogSizeToContent.None)
        );
    }
}

public class HelpWindow : UserControl
{
    [ExposePropToPlugin("HelpButton.HelpWindow")]
    private Grid _mainGrid { get; set; } = null!;
    [ExposePropToPlugin("HelpButton.HelpWindow")]
    private ScrollViewer _scrollViewer { get; set; } = null!;
    [ExposePropToPlugin("HelpButton.HelpWindow")]
    private MarkdownRenderer _markdownViewer { get; set; } = null!;
    [ExposePropToPlugin("HelpButton.HelpWindow")]
    private ObservableStringBuilder mkBuilder { get; set; } = new ObservableStringBuilder();
    
    public HelpWindow(string content)
    {
        _mainGrid = new Grid()
        {
            RowDefinitions = new RowDefinitions("*")
        };
        Content = _mainGrid;
        
        _scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = _mainGrid
        };
        _mainGrid.Children.Add(_scrollViewer);
        
        _markdownViewer = new MarkdownRenderer
        {
            Margin = new Thickness(10),
            LinkCommand = new RelayCommand<LinkClickedEventArgs>(OpenLinkCommand),
            CodeBlockColorTheme = ThemeName.Abbys,
        };
        _markdownViewer.MarkdownBuilder = mkBuilder;
        mkBuilder.Append(content);
        _scrollViewer.Content = (_markdownViewer);
        HelpButtonHelpWindowContext.Config config = new HelpButtonHelpWindowContext.Config
        {
            Get_mainGrid =  () => _mainGrid,
            Get_scrollViewer =  () => _scrollViewer,
            Get_markdownViewer = () => _markdownViewer,
            GetmkBuilder = () => mkBuilder
        };
        EditorUiServices.ExtensionManager.ApplyExtensions(UIRegion.HelpButtonHelpWindow, this, new HelpButtonHelpWindowContext(config));
    }

    private async void OpenLinkCommand(LinkClickedEventArgs? url)
    {

        if (url.HRef?.OriginalString.StartsWith("goto:") ?? false)
        {
            // In this case, we need to switch to another documentation (if it exists!)
            var urn = URN.Parse(url.HRef.OriginalString.Substring(5));
            if (EditorUiServices.DocService.HasDocumentation(urn))
            {
                mkBuilder.Clear();
                mkBuilder.Append(EditorUiServices.DocService.GetDocumentation(urn));
            }
            else
            {
                EditorUiServices.NotificationService.Error("Invalid link", "The link you clicked is invalid.");
                Logger.Error("Invalid link: ", url.HRef.OriginalString, ". No documentation found for it.");
            }

            return;
        }
        
        try
        {
            if(url == null)
                EditorUiServices.NotificationService.Error("Invalid link", "The link you clicked is invalid.");
            else
            {
                var urlText = url.HRef?.AbsoluteUri;

                if (urlText == null)
                {
                    EditorUiServices.NotificationService.Error("Invalid link", "The link you clicked is invalid.");
                    return;
                }

                try
                {
                    var confirm = await EditorUiServices.DialogService.ConfirmAsync(
                        "Open Link",
                        $"You are about to open the following link in your default web browser:\n\n{urlText}\n\nDo you want to proceed?\n\n(Note: Always be cautious when opening links from unknown sources.)",
                        new DialogStyle(Width: 600, SizeToContent:DialogSizeToContent.HeightOnly, CanResize:true), confirmButtonText:"Yes, open this link");

                    if (confirm)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = urlText,
                            UseShellExecute = true
                        });
                    }
                }
                catch (Exception ex)
                {
                    EditorUiServices.NotificationService.Error("Failed to open link",
                        $"An error occurred while trying to open the link: {ex.Message}");
                }
            }
        }
        catch (Exception e)
        {
            EditorUiServices.NotificationService.Error("Failed to open link", $"An error occurred while trying to open the link: {e.Message}");
        }
    }
}