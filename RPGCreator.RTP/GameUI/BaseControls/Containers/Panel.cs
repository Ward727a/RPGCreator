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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.RTP.GameUI.BaseControls.Box;
using RPGCreator.RTP.GameUI.Enums;

namespace RPGCreator.RTP.GameUI.BaseControls.Containers;

public class Panel : MultiChildrenContainerControl
{
    #region Events
    
    public event EventHandler<ControlPropertyChangingEventArgs<SimpleColorBox?>>? OnBackgroundChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<SimpleColorBox?>>? OnBackgroundChanged;
    
    #endregion
    
    protected override string _Name { get; set; } = "Panel";

    public SimpleColorBox Background
    {
        get;
        protected set;
    }
    
    public Color BackgroundColor 
    {
        get => Background.BackgroundColor;
        set => Background.BackgroundColor = value;
    }

    public Panel() : base()
    {
        Background = new SimpleColorBox
        {
            Name = "Internal_Background",
            Anchors = ControlAnchors.AnchorFull,
            IsInternal = true
        };
        
        AddInternalComponent(Background);
    }

    public override void AddChild(BaseControl child)
    {
        if (child == Background) return;
        base.AddChild(child);
    }
    
    public override void RemoveChild(BaseControl child)
    {
        if (child == Background) return;
        base.RemoveChild(child);
    }

    public void SetBackground<T>(T newBackground) where T : SimpleColorBox
    {
        if (Equals(newBackground, Background)) return;
        
        RemoveInternalComponent(Background);
        Background.SetParent(null);
        
        newBackground.Anchors = ControlAnchors.AnchorFull;
        newBackground.Position = Vector2.Zero;
        newBackground.Rotation = 0;
        
        var old = Background;
        OnBackgroundChanging?.Invoke(this, new (old, newBackground));
        Background = newBackground;
        OnBackgroundChanged?.Invoke(this, new (old, Background));
        
        InsertInternalComponent(0, newBackground);
    }
}