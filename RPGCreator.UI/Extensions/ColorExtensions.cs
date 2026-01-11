using System;
using ToolsUtilitiesStandard.Helpers;
using AvaloniaHSLColor = Avalonia.Media.HslColor;
using AvaloniaColorsChoice = Avalonia.Media.Colors;
using AvaloniaColor = Avalonia.Media.Color;
using DrawingColor = System.Drawing.Color;

namespace RPGCreator.UI.Extensions;

public static class ColorExtensions
{
    public static AvaloniaColor ToAvalonia(this DrawingColor c)
    {
        return AvaloniaColor.FromArgb(c.A, c.R, c.G, c.B);
    }

    public static DrawingColor FromAvalonia(this DrawingColor c, AvaloniaColor color)
    {
        return DrawingColor.FromArgb(color.A, color.R, color.G, color.B);
    }
    
    public static AvaloniaColor Invert(this AvaloniaColor color)
    {
        return AvaloniaColor.FromArgb(
            color.A,
            (byte)(255 - color.R),
            (byte)(255 - color.G),
            (byte)(255 - color.B)
        );
    }
    public static AvaloniaColor GetContrastingColor(this AvaloniaColor color)
    {
        double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;

        return luminance > 0.5 ? AvaloniaColorsChoice.Black : AvaloniaColorsChoice.White;
    }
    public static AvaloniaColor GetAutoContrastingColor(this AvaloniaColor color, double amount = 0.3)
    {
        var hsl = color.ToHsl();

        double newLightness;

        if (hsl.L < 0.5) 
        {
            newLightness = Math.Clamp(hsl.L + amount, 0.0, 1.0);
        }
        else 
        {
            newLightness = Math.Clamp(hsl.L - amount, 0.0, 1.0);
        }

        var newHsl = new AvaloniaHSLColor(hsl.A, hsl.H, hsl.S, newLightness);

        return newHsl.ToRgb();
    }
}