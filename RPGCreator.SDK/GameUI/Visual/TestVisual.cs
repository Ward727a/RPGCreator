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
using RPGCreator.SDK.GameUI.Enums;
using RPGCreator.SDK.GameUI.Interfaces;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.GameUI.Visual;

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

    public bool Filled => (bool)Control.GetExposedProperty("Appearance".ToPipedPath().Extend("Background"), "Filled").Get()!;

    public TestVisual()
    {
        X = 0; // 0 pixels from the left
        Y = 50; // 50% from the bottom
        XUnit = EPositionUnitType.Pixels;
        YUnit = EPositionUnitType.PixelsFromBottom;
        Width = 100; // 100% wide
        WidthUnit = ESizeUnitType.Percentage;
        Height = 50; // 50 pixels tall
    }

    protected override void DrawVisualAt(IUiRendererContext context, Vector2 drawPosition, Vector2 drawSize, out bool handledChildren)
    {
        handledChildren = false;
        context.DrawRectangle(drawPosition, drawSize, RectColor, filled: Filled);
    }
}