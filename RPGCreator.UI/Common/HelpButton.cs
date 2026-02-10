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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using LiveMarkdown.Avalonia;
using RPGCreator.SDK;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Modules.UIModule;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.UiService;
using RPGCreator.UI.Contexts;

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
        
        UiServices.ExtensionManager.ApplyExtensions(UIRegion.HelpButton, this, new HelpButtonContext(config));
    }

    private void CreateComponent()
    {
        _helpButton = new Button
        {
            Content = "?",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            Margin = new Thickness(5, 0, 0, 0)
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
        var helpContent = UiServices.DocService.GetDocumentation(_helpDocsKey);

        if (string.IsNullOrWhiteSpace(helpContent))
        {
            UiServices.NotificationService.Error("No help available for this item.", $"The documentation for '{_helpDocsKey}' is missing or empty.");
            return;
        }

        UiServices.DialogService.ShowPromptAsync(
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
            Margin = new Thickness(10)
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
        UiServices.ExtensionManager.ApplyExtensions(UIRegion.HelpButtonHelpWindow, this, new HelpButtonHelpWindowContext(config));
    }
}