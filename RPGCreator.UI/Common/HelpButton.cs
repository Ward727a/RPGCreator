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
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.EditorUiService;
using RPGCreator.SDK.Modules.UIModule;
using RPGCreator.SDK.Types;
using RPGCreator.UI.Contexts;
using Thickness = Avalonia.Thickness;

namespace RPGCreator.UI.Common;

public class HelpButton : UserControl
{
    
    [ExposePropToPlugin("HelpButton")]
    private Button _helpButton { get; set; } = null!;
    
    [ExposePropToPlugin("HelpButton", canSet: true)]
    private URN _helpDocsKey { get; set; } = null!;
    
    public HelpButton(URN helpDocsKey, string buttonText = "?")
    {
        CreateComponent();
        RegisterEvents();
        _helpButton!.Content = buttonText;
        Content = _helpButton;
        _helpDocsKey = helpDocsKey;
        
        HelpButtonContext.Config config = new HelpButtonContext.Config
        {
            Get_helpButton = () => _helpButton,
            Get_helpDocsKey = () => _helpDocsKey,
            Set_helpDocsKey = (newKey) => _helpDocsKey = newKey
        };
        
        EditorUiServices.ExtensionManager.ApplyExtensions(UIRegion.HelpButton, this, new HelpButtonContext(config));
    }
    
    public void SetHelpDocsKey(URN newKey)
    {
        _helpDocsKey = newKey;
    }

    private void CreateComponent()
    {
        _helpButton = new Button
        {
            Content = "?",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
        };
    }

    private void RegisterEvents()
    {
        _helpButton.Click += HelpClicked;
    }

    private void HelpClicked(object? sender, RoutedEventArgs e)
    {
        OpenHelp();
    }

    /// <summary>
    /// Opens the help window with the content retrieved from the documentation service using the provided key. If no content is found, an error notification is shown.<br/>
    /// This is the method that is used when the help button is clicked, but it can also be called directly to open the help window programmatically.
    /// </summary>
    public void OpenHelp()
    {
        var helpContent = EditorUiServices.DocService.GetDocumentation(_helpDocsKey);

        if (string.IsNullOrWhiteSpace(helpContent))
        {
            EditorUiServices.NotificationService.Error("No help available for this item.", $"The documentation for '{_helpDocsKey}' is missing or empty.");
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
        this.Content = _mainGrid;
        
        _scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = _mainGrid
        };
        _mainGrid.Children.Add(_scrollViewer);
        
        _markdownViewer = new MarkdownRenderer
        {
            Margin = new Thickness(10),
            LinkCommand = new RelayCommand<LinkClickedEventArgs>(OpenLinkCommand)
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