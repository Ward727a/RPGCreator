using Avalonia;
using Avalonia.Media.Imaging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RPGCreator.Core.Types;

public class UnifiedCroppedImage
{
    public UnifiedImage Source { get; }
    
    public PixelRect CropRect { get; }

    private CroppedBitmap? _avaloniaCache;

    public UnifiedCroppedImage(UnifiedImage source, PixelRect cropRect)
    {
        Source = source;
        CropRect = cropRect;
    }

    public CroppedBitmap UI
    {
        get
        {
            if (_avaloniaCache != null)
                return _avaloniaCache;
            
            _avaloniaCache = new CroppedBitmap(Source.UI, CropRect);
            return _avaloniaCache;
        }
    }

    public Texture2D Texture => Source.Game;
    
    public Rectangle SourceRectangle =>
        new(CropRect.X, CropRect.Y, CropRect.Width, CropRect.Height);
}