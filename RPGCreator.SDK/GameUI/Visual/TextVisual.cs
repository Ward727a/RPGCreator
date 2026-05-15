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

using System.Numerics;
using RPGCreator.SDK.GameUI.Controls;
using RPGCreator.SDK.GameUI.Interfaces;

namespace RPGCreator.SDK.GameUI.Visual;

public class TextVisual : BaseVisual
{
    public Vector2 TextOffset { get; set; } = Vector2.Zero;
    private TextControl _textControl => (TextControl) Control;
    
    protected override void DrawVisualAt(IUiRendererContext context, Vector2 drawPosition, Vector2 drawSize, out bool handledChildren)
    {
        handledChildren = false;
        context.DrawText(text: _textControl.Text, drawPosition + TextOffset, _textControl.TextColor, font: _textControl.Font);
    }
}