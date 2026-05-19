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
using RPGCreator.SDK.GameUI.Interfaces;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.GameUI.Visual;

public class ButtonVisual : BaseVisual
{
    public float CornerRadius { get; set; } = 10f;
    public float BorderThickness { get; set; } = 0f;
    public Color BorderColor { get; set; } = Color.Transparent;
    public Color BackgroundColor { get; set; } = new(69, 72, 77);
    
    protected override void DrawVisualAt(IUiRendererContext context, Vector2 drawPosition, Vector2 drawSize, out bool handledChildren)
    {
        context.DrawRectangle(drawPosition, drawSize, BackgroundColor, 0f, true, 1.5f, CornerRadius);
        context.DrawRectangle(drawPosition, drawSize, BorderColor, BorderThickness, false, 1.5f, CornerRadius);
        handledChildren = false;
    }
}