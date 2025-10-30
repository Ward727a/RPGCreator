using System.Drawing;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using Serilog;
using SkiaSharp;
using Color = Avalonia.Media.Color;

namespace RPGCreator.Core.Common;

public static class ImageUtil
{
    
    public static byte[] ExtractPixelsFromCroppedBitmap(CroppedBitmap cropped)
    {
        var time = System.Diagnostics.Stopwatch.StartNew();
        if (cropped == null)
            throw new ArgumentNullException(nameof(cropped));

        int width = (int)cropped.Size.Width;
        int height = (int)cropped.Size.Height;
        int stride = width * 4;
        int bufferSize = stride * height;

        var pixels = new byte[bufferSize];

        if (cropped.Source is Bitmap bitmap)
        {
            var sourceRect = cropped.SourceRect;
            IntPtr bufferPtr = Marshal.UnsafeAddrOfPinnedArrayElement(pixels, 0);

            bitmap.CopyPixels(
                sourceRect,
                bufferPtr,
                bufferSize,
                stride
            );
        }
        else
        {
            throw new InvalidOperationException("CroppedBitmap source is null or not a Bitmap.");
        }

        for (int i = 0; i < pixels.Length; i += 4)
        {
            byte b = pixels[i + 0];
            byte g = pixels[i + 1];
            byte r = pixels[i + 2];
            byte a = pixels[i + 3];

            pixels[i + 0] = r;
            pixels[i + 1] = g;
            pixels[i + 2] = b;
            pixels[i + 3] = a;
        }

        time.Stop();
        Log.Debug("Extracted pixels from CroppedBitmap of size {width}x{height} in {time}", width, height, time.Elapsed);
        return pixels;
    }
    
    public static (Texture2D Atlas, List<Rectangle> Frames)
        BuildFromFrames(GraphicsDevice device, string framesFolder)
    {
        var files = Directory.GetFiles(framesFolder, "frame_*.png");
        var textures = new List<Texture2D>();
        var rectangles = new List<Rectangle>();

        // charge les textures une par une
        foreach (var file in files.OrderBy(f => f.ToLower()))
        {
            using var fs = File.OpenRead(file);
            var tex = Texture2D.FromStream(device, fs);
            textures.Add(tex);
        }

        // on suppose que toutes ont la même taille
        int frameWidth = textures[0].Width;
        int frameHeight = textures[0].Height;

        int atlasWidth = frameWidth * textures.Count;
        int atlasHeight = frameHeight;
        var atlas = new Texture2D(device, atlasWidth, atlasHeight, false, SurfaceFormat.Color);

        // buffer pour fusionner
        var atlasData = new Color[atlasWidth * atlasHeight];

        for (int i = 0; i < textures.Count; i++)
        {
            var data = new Color[frameWidth * frameHeight];
            textures[i].GetData(data);

            for (int y = 0; y < frameHeight; y++)
            {
                for (int x = 0; x < frameWidth; x++)
                {
                    int src = y * frameWidth + x;
                    int dst = y * atlasWidth + (x + i * frameWidth);
                    atlasData[dst] = data[src];
                }
            }

            rectangles.Add(new Rectangle(i * frameWidth, 0, frameWidth, frameHeight));
        }

        atlas.SetData(atlasData);

        return (atlas, rectangles);
    }
    
    public static void SavePixelsAsPng(byte[] rgba, int width, int height, string path)
    {
        // Create a time log to measure how long it takes to save the image
        Log.Debug("Saving PNG image to {path} with size {width}x{height}", path, width, height);
        
        var timer = System.Diagnostics.Stopwatch.StartNew();
        
        using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        var ptr = bitmap.GetPixels();
        Marshal.Copy(rgba, 0, ptr, rgba.Length);

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(path);
        data.SaveTo(stream);
        timer.Stop();
        Log.Debug("Saved PNG image to {path} in {time}", path, timer.Elapsed);
    }
    //
    // public static Texture2DAtlas CreateTextureAtlas(Bitmap bitmap)
    // {
    //     
    //     var data = new byte[bitmap.PixelSize.Height * bitmap.PixelSize.Width * 4];
    //     int bufferSize = data.Length;
    //     IntPtr ptr = System.Runtime.InteropServices.Marshal.AllocHGlobal(bufferSize);
    //     PixelRect rect = new PixelRect(0, 0, bitmap.PixelSize.Width, bitmap.PixelSize.Height);
    //     bitmap.CopyPixels(rect, ptr, bufferSize, bitmap.PixelSize.Width * 4);
    //     System.Runtime.InteropServices.Marshal.Copy(ptr, data, 0, bufferSize);
    //     System.Runtime.InteropServices.Marshal.FreeHGlobal(ptr);
    //     var texture = new Texture2D(EngineCore.Instance.GraphicsDevice,
    //         bitmap.PixelSize.Width, bitmap.PixelSize.Height,
    //         false,
    //         Microsoft.Xna.Framework.Graphics.SurfaceFormat.Color);
    //     texture.SetData(data);
    //     
    //     var atlas = new Texture2DAtlas(texture);
    //     return atlas;
    // }
    
}