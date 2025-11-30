using Avalonia.Media;

public static class ColorExtensions
{
    public static Color Invert(this Color color)
    {
        return Color.FromArgb(
            color.A,
            (byte)(255 - color.R),
            (byte)(255 - color.G),
            (byte)(255 - color.B)
        );
    }
    public static Color GetContrastingColor(this Color color)
    {
        double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;

        return luminance > 0.5 ? Colors.Black : Colors.White;
    }
    public static Color GetAutoContrastingColor(this Color color, double amount = 0.3)
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

        var newHsl = new HslColor(hsl.A, hsl.H, hsl.S, newLightness);

        return newHsl.ToRgb();
    }
}