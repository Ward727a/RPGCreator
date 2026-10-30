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
using System.Linq;
using MediatR;
using RPGCreator.SDK.Mediator.Messages;
using RPGCreator.SDK.Modules.Editor.Ui;

namespace RPGCreator.Editor;

public static class EditorWindowManager
{
    private static Dictionary<string, Dictionary<string, IEditorUiWindow>> _windows = [];

    private static IMediator? _mediator;
    
    public static void Init(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    public static void Open(IEditorUiWindow uiWindow, string group = "default")
    {
        CommandQueue.Enqueue(() => {
            if (!_windows.TryGetValue(group, out var groupWindows))
            {
                groupWindows = new Dictionary<string, IEditorUiWindow>();
                _windows.Add(group, groupWindows);
            }
            
            if (!groupWindows.TryAdd(uiWindow.Name, uiWindow))
            {
                groupWindows[uiWindow.Name].Open();
                return;
            }

            uiWindow.Open();
            _mediator?.Publish(new UiMessage.OpenedView(uiWindow.ViewId));
        });
    }

    public static void Close(IEditorUiWindow uiWindow, string group = "default")
    {
        CommandQueue.Enqueue(() => {
            if (!_windows.TryGetValue(group, out var groupWindows))
                return;
            
            if (!groupWindows.Remove(uiWindow.Name))
                return;

            uiWindow.Close();
            _mediator?.Publish(new UiMessage.ClosedView(uiWindow.ViewId));
        });
    }
    
    public static Queue<Action> CommandQueue { get; } = new Queue<Action>();

    public static void Draw(string group = "default")
    {
        if(!_windows.ContainsKey(group))
            goto processCommand;

        foreach (var window in _windows[group].Values.Where(window => window.IsOpen))
        {
            var wasOpen = window.IsOpen;
            window.Draw(EDrawAs.Window);
            if (wasOpen != window.IsOpen)
            {
                Close(window, group);
                _mediator?.Publish(new UiMessage.ClosedView(window.ViewId));
            }
        }

        processCommand:
        if(group == "default")
            ProcessCommand();
    }

    private static void ProcessCommand()
    {
        while (CommandQueue.Count > 0)
        {
            var command = CommandQueue.Dequeue();
            command.Invoke();
        }
    }
}