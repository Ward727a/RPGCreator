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
using CommunityToolkit.Mvvm.Input;
using LiveMarkdown.Avalonia;
using RPGCreator.SDK;
using RPGCreator.SDK.EditorUiService;

namespace RPGCreator.UI.Content;

public class AboutWindow : Window
{
    private readonly ScrollViewer _scrollViewer;
    private readonly Grid _mainGrid;
    private readonly MarkdownRenderer _markdownViewer;
    private ObservableStringBuilder mkBuilder { get; set; } = new ObservableStringBuilder();

    public AboutWindow()
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
            LinkCommand = new RelayCommand<LinkClickedEventArgs>(OpenLinkCommand)
        };
        _markdownViewer.MarkdownBuilder = mkBuilder;
        mkBuilder.Append(
            """
            # Welcome to RPG Creator (Pre-Alpha)
            
            First, I want to thank you for trying out this (very) early build of RPG Creator. This is a project that I started in my free time, and I'm kinda happy to share it now.
            
            But before you start, I want to give you a few warnings:
            
            - This build is a **pre-alpha**, which means that it's not stable, and that you should not expect it to be able to create your game with it **for now**. It's more of a "see what I'm working on" kind of build, and also a way for me to get early feedback on the direction of the project.
            
            - There are still a **lot** of features that are not implemented yet, and a lot of things that are not working yet. So don't be surprised if you encounter bugs, or if you can't find a feature that you think should be there.
            
            With that in mind, and for those who are interested in that information, this is a nearly 2 years project, there was a lot of work done on the engine side, but the editor is still in its early stages, there is still a lots of 'WIP' things (UI, UX, and features wise) but I want to first build something, and then iterate on it, rather than trying to make something perfect from the start, and end up with nothing to show for a long time (long time = Never).
            
            You also need to know that the project is open-source, and as such uses some open-source libraries, here are a list of them:
            
            - [Avalonia](https://avaloniaui.net/) (UI Framework - MIT License)
            - [Avalonia.AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit) (Text Editor - MIT License)
            - [Avalonia.Controls.DataGrid](https://github.com/AvaloniaUI/Avalonia) (Data Grid Control - MIT License)
            - [Avalonia.Desktop](https://avaloniaui.net/) (Desktop Integration - MIT License)
            - [Avalonia.Diagnostics](https://avaloniaui.net/) (Debugging Tools - MIT License)
            - [Avalonia.Fonts.Inter](https://avaloniaui.net/) (Font - MIT License)
            - [AvaloniaEdit.TextMate](https://github.com/AvaloniaUI/AvaloniaEdit) (TextMate Integration - MIT License)
            - [AvaloniaInside.MonoGame](https://github.com/AvaloniaInside/AvaloniaInside.MonoGame/) (MonoGame Integration - MIT License)
            - [CommunityToolkit.Diagnostics](https://github.com/CommunityToolkit/dotnet) (Diagnostics Tools - MIT License)
            - [CommunityToolkit.HighPerformance](https://github.com/CommunityToolkit/dotnet) (High Performance libs - MIT License)
            - [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) (MVVM Helpers - MIT License)
            - [Dock.Avalonia](https://github.com/wieslawsoltes/Dock) (Docking System - MIT License)
            - [FontStashSharp](https://github.com/rds1983/FontStashSharp) (Font Rendering - Zlib License)
            - [Irihi.Ursa](https://github.com/irihitech/Ursa.Avalonia) (UI Controls - MIT License)
            - [LiteDB](https://github.com/litedb-org/LiteDB) (Embedded Database - MIT License)
            - [LiveMarkdown.Avalonia](https://github.com/DearVa/LiveMarkdown.Avalonia) (Markdown Renderer - MIT License)
            - [MetadataExtractor](https://github.com/drewnoakes/metadata-extractor-dotnet) (Image Metadata Extraction - Apache 2.0 License)
            - [Microsoft.CodeAnalysis](https://github.com/dotnet/roslyn-analyzers) (Code Analysis - MIT License)
            - [MonoGame.Extended](https://github.com/craftworkgames/MonoGame.Extended) (MonoGame Extensions - MIT License)
            - [MonoGame.Framework](https://github.com/MonoGame/MonoGame) (Game Framework - Microsoft Public License)
            - [NETStandard.Library](https://github.com/dotnet/standard) (Standard Library (used for Source Generators) - MIT License)
            - [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) (JSON Serialization - MIT License)
            - [Projektanker.Icons.Avalonia](https://github.com/Projektanker/Icons.Avalonia) (Icon Rendering - MIT License)
            - [MaterialDesign Icons](https://github.com/Templarian/MaterialDesign) (Icon Library - Apache 2.0 License)
            - [Serilog](https://github.com/serilog/serilog) (Logging - Apache 2.0 License)
            - [Ulid](https://github.com/Cysharp/Ulid) (ULID Generation - MIT License)
            
            *Note: If you are an open-source library owner, and we are using your library, but you don't see it in the list, please let me know so I can add it to the list and give you the credit you deserve.*
            """);
        _scrollViewer.Content = (_markdownViewer);

        Width = 600;
        Height = 400;
        
        Content = _mainGrid;
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