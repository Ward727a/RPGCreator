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
using RPGCreator.RTP.GameUI.BaseControls.Box;
using RPGCreator.RTP.GameUI.BaseControls.Containers;
using RPGCreator.RTP.GameUI.Enums;
using Size = MonoGame.Extended.Size;

namespace RPGCreator.RTP.GameUI.BaseControls.Inputs;

public class ButtonControl : SingleChildrenContainerControl
{
    
    #region Events
    
    public event EventHandler<ControlPropertyChangingEventArgs<SimpleColorBox?>>? OnBackgroundChanging; 
    public event EventHandler<ControlPropertyChangedEventArgs<SimpleColorBox?>>? OnBackgroundChanged;
    
    #endregion
    
    protected override string _Name { get; set; } = "ButtonControl";
    protected SimpleColorBox InternalBackground { get; set; }

    public Color BackgroundNormalColor
    {
        get;
        set;
    } = Color.Gainsboro;

    public Color BackgroundPressedColor
    {
        get;
        set;
    } = Color.Red;
    
    public Color BackgroundHoverColor
    {
        get;
        set;
    } = Color.Blue;
    
    public bool Autosize { get; set; } = true;
    
    public ButtonControl()
    {
        Padding = new(10, 5);
        InternalBackground = new SimpleColorBox
        {
            Name = "Internal_Background",
            Anchors = ControlAnchors.AnchorFull,
            IsInternal = true,
            BackgroundColor = BackgroundNormalColor,
            IsAffectedByParentPadding = false
        };
        
        AddInternalComponent(InternalBackground);
        
        base.RefreshControl();
        
        OnPressed += PressedState;
        OnReleased += HoveredState;
        OnMouseEnter += HoveredState;
        OnMouseLeave += NormalState;
    }

    public void SetBackgroundType<T>(T newBackground) where T : SimpleColorBox
    {
        if (Equals(newBackground, InternalBackground)) return;
        
        RemoveInternalComponent(InternalBackground);
        
        newBackground.Anchors = ControlAnchors.AnchorFull;
        newBackground.Position = Vector2.Zero;
        newBackground.Rotation = 0;
        
        var old = InternalBackground;
        OnBackgroundChanging?.Invoke(this, new (old, newBackground));
        InternalBackground = newBackground;
        OnBackgroundChanged?.Invoke(this, new (old, InternalBackground));
        
        InsertInternalComponent(0, newBackground);
    }

    public override void Measure()
    {
        base.Measure();

        if (!Autosize || Content == null)
            return;
        
        var contentSize = Content.GlobalsBounds.Size;
        Size = new Size(contentSize.X + Padding.Width, contentSize.Y + Padding.Height);
    }

    private void PressedState()
    {
        InternalBackground.BackgroundColor = BackgroundPressedColor;
    }
    
    private void HoveredState()
    {
        InternalBackground.BackgroundColor = BackgroundHoverColor;
    }
    
    private void NormalState()
    {
        InternalBackground.BackgroundColor = BackgroundNormalColor;
    }

    public void SetContent(BaseControl newContent)
    {
        Content = newContent;
    }

    public override string ToString()
    {
        var baseString = base.ToString();
        baseString = baseString.TrimEnd(')');
        return $"{baseString}, BackgroundNormalColor: {BackgroundNormalColor}, BackgroundPressedColor: {BackgroundPressedColor}, BackgroundHoverColor: {BackgroundHoverColor})";
    }
}