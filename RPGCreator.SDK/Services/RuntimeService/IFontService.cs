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
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.RuntimeService;

public enum FontSpecialEffects
{
    None,
    Blurry,
    Stroked,
}

public interface IRpgFont
{
    public string FontPath { get; }
    public string FontName { get; }
    public float FontSize { get; }
    public int LineHeight { get; }

    public (float x1, float y1, float x2, float y2) TextBounds(
        string text,
        Vector2 position,
        Vector2? scale = null,
        float characterSpacing = 0.0f,
        float lineSpacing = 0.0f,
        FontSpecialEffects effect = FontSpecialEffects.None,
        int effectAmount = 0);
    public Vector2 MeasureString(string text, Vector2? scale = null, float characterSpacing = 0f, float lineSpacing = 0f, FontSpecialEffects specialEffects = FontSpecialEffects.None, int effectAmount = 0);
};

public interface IFontService : IService
{
    public void LoadFont(string fontPath, string fontName);
    public void UnloadFont(string fontName);
    
    public Result<IRpgFont> GetFont(string fontName, float fontSize = 16f);
}