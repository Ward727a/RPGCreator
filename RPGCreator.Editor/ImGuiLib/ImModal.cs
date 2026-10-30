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
using Hexa.NET.ImGui;
using RPGCreator.SDK.Modules.Editor.Ui;

namespace RPGCreator.Editor.ImGuiLib;

public interface IModalView : IEditorUiBindableView
{
}

public static class ImModal
{
    public class ImModalInstance
    {
        public string Title { get; set; } = string.Empty;
        public required Action DrawContent { get; set; }
        public Action? OnClose { get; set; }
        
        public bool CloseRequested { get; set; }
        public bool IsOpen { get; set; } = true;
        public bool IsOpenRequested { get; set; } = true;
        public bool ShowCloseButton { get; set; } = true;
    }

    private static readonly List<ImModalInstance> _modalInstances = [];
    private static readonly List<ImModalInstance> _modalInstancesToRemove = [];

    public static void Show(string title, Action drawContent, bool showCloseButton = true, Action? onClose = null)
    {
        _modalInstances.Add(new ImModalInstance
        {
            Title = title,
            DrawContent = drawContent,
            IsOpenRequested = true,
            ShowCloseButton = showCloseButton,
            OnClose = onClose
        });
    }

    public static void CloseCurrent()
    {
        ImGui.CloseCurrentPopup();

        if (_modalInstances.Count <= 0) return;

        _modalInstances[^1].CloseRequested = true;
        _modalInstances[^1].IsOpen = false;
        _modalInstances[^1].IsOpenRequested = false;
    }

    public static void Render()
    {
        if (_modalInstances.Count <= 0) return;

        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < _modalInstances.Count; i++)
        {
            var modal = _modalInstances[i];

            if (modal.IsOpenRequested)
            {
                ImGui.OpenPopup(modal.Title);
                modal.IsOpenRequested = false;
            }

            bool isOpen = modal.IsOpen;

            var began = modal.ShowCloseButton
                ? ImGui.BeginPopupModal(modal.Title, ref isOpen)
                : ImGui.BeginPopupModal(modal.Title);

            if (began)
            {
                modal.DrawContent();
                ImGui.EndPopup();
            }

            modal.IsOpen = isOpen;

            if (modal.CloseRequested || !modal.IsOpen)
            {
                modal.OnClose?.Invoke();
                _modalInstancesToRemove.Add(modal);
            }
        }

        if (_modalInstancesToRemove.Count > 0)
        {
            foreach (var modalToRemove in _modalInstancesToRemove)
            {
                _modalInstances.Remove(modalToRemove);
            }

            _modalInstancesToRemove.Clear();
        }
    }
}