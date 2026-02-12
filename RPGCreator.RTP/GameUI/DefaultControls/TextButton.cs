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


using Microsoft.Xna.Framework;
using RPGCreator.RTP.GameUI.BaseControls;
using RPGCreator.RTP.GameUI.BaseControls.Box;
using RPGCreator.RTP.GameUI.Enums;
using RPGCreator.RTP.GameUI.InteractableControls;

namespace RPGCreator.RTP.GameUI.DefaultControls;

public class TextButton : InteractableControl
{
    protected override string _Name { get; set; } = "TextButton";
    protected override bool _canReceiveMouseEvents { get; set; } = true;
    protected SimpleColorBox InternalBackground { get; set; }
    protected TextControl InternalText { get; set; }
    
    public string Text
    {
        get => InternalText.Text;
        set
        {
            InternalText.Text = value;
            Invalidate();
        }
    }
    
    public int FontSize
    {
        get => InternalText.FontSize;
        set
        {
            InternalText.FontSize = value;
            Invalidate();
        }
    }
    
    public Color FontNormalColor
    {
        get;
        set;
    } = Color.White;
    
    public Color FontPressedColor
    {
        get;
        set;
    } = Color.White;
    
    public Color FontHoverColor
    {
        get;
        set;
    } = Color.White;

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
    
    public TextButton() : base()
    {
        InternalBackground = new SimpleColorBox
        {
            Name = "Internal_Background",
            Anchors = ControlAnchors.AnchorFull,
            IsInternal = true,
            BackgroundColor = BackgroundNormalColor
        };
        
        InternalText = new TextControl
        {
            Name = "Internal_Text",
            Anchors = ControlAnchors.AnchorCenter,
            IsInternal = true,
            FontSize = 16,
            FontColor = FontNormalColor,
            Text = "Button"
        };
        
        this.AddChild(InternalBackground);
        this.AddChild(InternalText);
        
        RefreshControl();
        
        OnPressed += PressedState;
        OnReleased += HoveredState;
        OnMouseEnter += HoveredState;
        OnMouseLeave += NormalState;
    }
    
    private void PressedState()
    {
        InternalBackground.BackgroundColor = BackgroundPressedColor;
        InternalText.FontColor = FontPressedColor;
    }
    
    private void HoveredState()
    {
        InternalBackground.BackgroundColor = BackgroundHoverColor;
        InternalText.FontColor = FontHoverColor;
    }
    
    private void NormalState()
    {
        InternalBackground.BackgroundColor = BackgroundNormalColor;
        InternalText.FontColor = FontNormalColor;
    }
}