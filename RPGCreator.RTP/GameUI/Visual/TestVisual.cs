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
using RPGCreator.RTP.GameUI.Enums;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types;

namespace RPGCreator.RTP.GameUI.Visual;

public class TestVisual : BaseVisual
{
    public Color RectColor
    {
        get;
        set
        {
            if(RectColor == value) return;
            field = value;
            MarkDirty();
        }
    } = Color.Red;

    public TestVisual()
    {
        X = 0; // 100 pixels from the left of the parent container
        Y = 50; // 50% from the top of the parent container
        XUnit = EPositionUnitType.Pixels;
        YUnit = EPositionUnitType.PixelsFromBottom;
        Width = 100; // 200 pixels wide
        WidthUnit = ESizeUnitType.Percentage;
        Height = 50; // 100 pixels tall
    }

    protected override void DrawVisualAt(UiRendererContext context, Vector2 drawPosition, Vector2 drawSize, out bool handledChildren)
    {
        handledChildren = false;
        context.DrawRectangle(drawPosition, drawSize, RectColor, filled: true);
    }
}