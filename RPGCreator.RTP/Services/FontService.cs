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

using System.Collections.Generic;
using System.IO;
using System.Numerics;
using FontStashSharp;
using RPGCreator.RTP.Extensions;
using RPGCreator.SDK.RuntimeService;
using RPGCreator.SDK.Types;

namespace RPGCreator.RTP.Services;

public class RpgFont : IRpgFont
{
    private readonly DynamicSpriteFont _spriteFont;

    public string FontPath { get; }
    public string FontName { get; }
    public float FontSize => _spriteFont.FontSize;
    public int LineHeight => _spriteFont.LineHeight;

    public RpgFont(string fontName, string fontPath, DynamicSpriteFont spriteFont)
    {
        FontName = fontName;
        _spriteFont = spriteFont;
    }
    
    public DynamicSpriteFont GetSpriteFont() => _spriteFont;

    public (float x1, float y1, float x2, float y2) TextBounds(string text, Vector2 position, Vector2? scale = null,
        float characterSpacing = 0, float lineSpacing = 0, FontSpecialEffects effect = FontSpecialEffects.None,
        int effectAmount = 0)
    {
        var bounds = _spriteFont.TextBounds(text, position, scale, characterSpacing, lineSpacing, FontRpgEffectToSystemEffect(effect), effectAmount);
        
        return (bounds.X, bounds.Y, bounds.X2, bounds.Y2);
    }

    public Vector2 MeasureString(string text, Vector2? scale = null, float characterSpacing = 0, float lineSpacing = 0,
        FontSpecialEffects specialEffects = FontSpecialEffects.None, int effectAmount = 0)
    {
        Vector2 defaultScale = Vector2.One;
        if (scale.HasValue)
        {
            defaultScale = scale.Value;
        }
        return _spriteFont.MeasureString(text, defaultScale.ToXnaFast(), characterSpacing, lineSpacing, FontRpgEffectToSystemEffect(specialEffects), effectAmount).ToNumericFast();
    }
    
    private static FontSystemEffect FontRpgEffectToSystemEffect(FontSpecialEffects effect)
    {
        return (FontSystemEffect)((int)effect);
    }
}

public class FontService : IFontService
{
    private readonly Dictionary<string, (FontSystem font, string fontPath)> _loadedFonts = new Dictionary<string, (FontSystem font, string fontPath)>();
    private readonly Dictionary<(string, float), IRpgFont> _fontCache = new Dictionary<(string, float), IRpgFont>();
    
    public void LoadFont(string fontPath, string fontName)
    {
        fontName = fontName.ToLowerInvariant().Trim();
        
        if (_loadedFonts.ContainsKey(fontName))
        {
            return;
        }
        
        if(!File.Exists(fontPath))
        {
            throw new FileNotFoundException($"Font file not found at path: {fontPath}");
        }
        
        var fontSystem = new FontSystem();
        fontSystem.AddFont(File.ReadAllBytes(fontPath));
        _loadedFonts[fontName] = (fontSystem, fontPath);
    }

    public void UnloadFont(string fontName)
    {
        fontName = fontName.ToLowerInvariant().Trim();

        if (!_loadedFonts.Remove(fontName))
        {
            throw new KeyNotFoundException($"Font not found: {fontName}");
        }
    }

    public Result<IRpgFont> GetFont(string fontName, float fontSize = 16)
    {
        var key = (fontName, fontSize);
        if (_fontCache.TryGetValue(key, out var font))
        {
            return Result<IRpgFont>.Success(font);
        }
        
        fontName = fontName.ToLowerInvariant().Trim();

        if (!_loadedFonts.TryGetValue(fontName, out var fontData))
        {
            return Result.Fail($"Font not found: {fontName}");
        }

        var fontSystem = fontData.font;
        var fontPath = fontData.fontPath;
        
        var rpgFont = new RpgFont(fontName, fontPath, fontSystem.GetFont(fontSize));
        _fontCache[key] = rpgFont;
        return rpgFont;
    }
}