using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace RPGCreator.UI.Common;

public static class ImageUtil
{
    public static Bitmap ConvertCroppedBitmapToBitmap(CroppedBitmap cropped)
    {
        int width = (int)Math.Max(1, cropped.Size.Width);
        int height = (int)Math.Max(1, cropped.Size.Height);

        var result = new RenderTargetBitmap(new PixelSize(width, height), new Vector(96, 96));

        var drawing = new ImageDrawing
        {
            ImageSource = cropped.Source,
            Rect = new Rect(-cropped.SourceRect.X, -cropped.SourceRect.Y, 
                cropped.Source.Size.Width, cropped.Source.Size.Height)
        };

        using (var context = result.CreateDrawingContext())
        {
            using (context.PushClip(new Rect(0, 0, width, height)))
            {
                drawing.Draw(context);
            } 
        }
        return result;
    }
}