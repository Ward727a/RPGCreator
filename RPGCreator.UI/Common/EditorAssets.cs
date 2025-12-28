using System;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace RPGCreator.UI.Common;

public static class EditorAssets
{
    private static readonly Lazy<Bitmap> _fallbackBitmap = new(() => 
        CreateCheckerboard(32, 32));

    public static Bitmap FallbackImage => _fallbackBitmap.Value;

    private static Bitmap CreateCheckerboard(int width, int height)
    {
        var bitmap = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96), PixelFormat.Rgba8888, AlphaFormat.Opaque);
        using (var lockedBitmap = bitmap.Lock())
        {
            unsafe
            {
                byte* buffer = (byte*)lockedBitmap.Address;
                int stride = lockedBitmap.RowBytes;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int index = y * stride + x * 4;
                        bool isWhite = ((x / 8) + (y / 8)) % 2 == 0;
                        
                        // pink and black colors for a missing texture
                        buffer[index + 0] = isWhite ? (byte)255 : (byte)0;   // R
                        buffer[index + 1] = isWhite ? (byte)0 : (byte)0;     // G
                        buffer[index + 2] = isWhite ? (byte)255 : (byte)0;   // B
                        buffer[index + 3] = 255; // A
                    }
                }
            }
        }
        return bitmap;
    }
}
