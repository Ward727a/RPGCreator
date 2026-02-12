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
using Microsoft.Xna.Framework;
using RPGCreator.RTP.Extensions;
using RPGCreator.RTP.GameUI.BaseControls;
using RPGCreator.RTP.GameUI.BaseControls.Containers;
using RPGCreator.SDK;

namespace RPGCreator.RTP.GameUI.InteractableControls;

public class InteractableControl : ContainerControl
{
    public bool IsMouseOver { get; private set; } = false;
    public bool IsPressed { get; private set; } = false;

    protected List<BaseControl> ReversedChildren;

    public event Action? OnMouseEnter;
    public event Action? OnMouseLeave;
    public event Action<Vector2>? OnMouseMove;
    public event Action? OnPressed;
    public event Action? OnReleased;
    public event Action? OnClicked;
    
    public Vector2 LastMousePosition { get; private set; }

    public InteractableControl()
    {
        ReversedChildren = new List<BaseControl>(Children);
        OnAddedChildren += (_, child) =>
        {
            ReversedChildren.Insert(0, child);
        };
        OnRemovedChildren += (_, child) =>
        {
            ReversedChildren.Remove(child);
        };
    }
    
    public virtual bool UpdateInput(Vector2 mousePosition, bool isLeftButtonDown, bool isHandled = false)
    {
        if (!IsEnabled || !AbsoluteEnabled || !AbsoluteVisibility)
            return isHandled;

        foreach (var child in ReversedChildren)
        {
            if (child is InteractableControl interactableChild)
            {
                isHandled = interactableChild.UpdateInput(mousePosition, isLeftButtonDown, isHandled);
            }
        }

        bool currentlyOver = !isHandled && GlobalsBounds.Contains(mousePosition);
        
        if (currentlyOver && !IsMouseOver)
        {
            IsMouseOver = true;
            OnMouseEnter?.Invoke();
        } else if (!currentlyOver && IsMouseOver)
        {
            IsMouseOver = false;
            OnMouseLeave?.Invoke();
        }

        if (currentlyOver)
        {
            if (isLeftButtonDown && !IsPressed)
            {
                IsPressed = true;
                OnPressed?.Invoke();
                return true;
            }
            
            if (IsPressed && !isLeftButtonDown)
            {
                OnClicked?.Invoke();
                IsPressed = false;
                OnReleased?.Invoke();
                return true;
            }
        }
        else
        {
            if (!isLeftButtonDown) IsPressed = false;
        }

        if (IsMouseOver && currentlyOver && mousePosition != LastMousePosition)
        {
            OnMouseMove?.Invoke(mousePosition - GlobalsBounds.Location.ToVector2());
            LastMousePosition = mousePosition;
        }

        return isHandled;
    }
}