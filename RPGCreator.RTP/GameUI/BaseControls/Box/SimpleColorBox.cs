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

namespace RPGCreator.RTP.GameUI.BaseControls.Box;

public class SimpleColorBox : BaseControl
{
    #region Events
    
    public event EventHandler<ControlPropertyChangingEventArgs<Color>>? OnBackgroundColorChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<Color>>? OnBackgroundColorChanged;
    
    #endregion

    protected override string _Name { get; set; } = "SimpleColorBox";

    protected override bool _canReceiveMouseEvents { get; set; } = false;

    public Color BackgroundColor
    {
        get;
        set
        {
            if (Equals(field, value)) return;
            var old = field;
            OnBackgroundColorChanging?.Invoke(this, new (old, value));
            field = value;
            OnBackgroundColorChanged?.Invoke(this, new (old, value));
            Invalidate();
        }
    } = Color.White;

    public override void Draw(SpriteBatch sb)
    {
        if (!ShouldDrawn || !AbsoluteVisibility || OwningLayer == null)
            return;

        sb.Draw(OwningLayer.PixelTexture, GlobalsBounds, BackgroundColor * (AbsoluteAlpha / 255f));
    }
}