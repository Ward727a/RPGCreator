using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Microsoft.Xna.Framework.Graphics;
using Serilog;

namespace RPGCreator.Core.Type;

public class UnifiedImage : IDisposable
{
    #region Default
    // This is a magenta square 32x32
    // This is used as a default image when an image fails to load
    
    private static readonly Lazy<Bitmap> _lazyPinkSquare = new(() => CreateSolidBitmap(Colors.Magenta, 32));
    /// <summary>
    /// Default UI image (magenta square) used when an image fails to load.
    /// </summary>
    public static Bitmap DefaultUI => _lazyPinkSquare.Value;

    private static Bitmap CreateSolidBitmap(Color color, int size)
    {
        var pixelSize = new PixelSize(size, size);
        var dpi = new Vector(96, 96);

        var target = new RenderTargetBitmap(pixelSize, dpi);

        using (var ctx = target.CreateDrawingContext())
        {
            ctx.FillRectangle(new SolidColorBrush(color), new Rect(0, 0, size, size));
        }

        return target;
    }
    
    #endregion

    private string _filePath = string.Empty;
    private GraphicsDevice GraphicsDevice { get; set; } = null!;

    private Bitmap? _avaloniaBitmap = null!;
    private Texture2D? _monoGameTexture = null!;
    
    public Bitmap UI
    {
        get
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    Log.Warning("UnifiedImage: File not found at path {FilePath}. Using default image.", _filePath);
                    return DefaultUI;
                }

                if (_avaloniaBitmap == null)
                    _avaloniaBitmap = new Bitmap(_filePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "UnifiedImage: Failed to load image from path {FilePath}. Using default image.", _filePath);
                return DefaultUI;
            }

            return _avaloniaBitmap;
        }
    }

    public Texture2D Game
    {
        get
        {
            if(GraphicsDevice == null)
                throw new InvalidOperationException("GraphicsDevice not set for UnifiedImage.");
            
            if (_monoGameTexture == null)
            {
                using var stream = new FileStream(_filePath, FileMode.Open);
                _monoGameTexture = Texture2D.FromStream(GraphicsDevice, stream);
            }
            return _monoGameTexture;
        }
    }

    public UnifiedImage(string filePath)
    {
        _filePath = filePath;
    }
    
    public UnifiedImage(string filePath, GraphicsDevice graphicsDevice)
    {
        _filePath = filePath;
        GraphicsDevice = graphicsDevice;
    }
    
    /// <summary>
    /// Useful for setting the GraphicsDevice after construction.<br/>
    /// (e.g., when the GraphicsDevice is not available at the time of creating the UnifiedImage, but will be later)
    /// </summary>
    /// <param name="graphicsDevice"></param>
    /// <returns></returns>
    public UnifiedImage SetGraphicsDevice(GraphicsDevice graphicsDevice)
    {
        GraphicsDevice = graphicsDevice;
        return this;
    }

    public void Dispose()
    {
        _avaloniaBitmap?.Dispose();
        _monoGameTexture?.Dispose();
    }
    
    public static implicit operator Bitmap(UnifiedImage unifiedImage)
    {
        return unifiedImage.UI;
    }
    
    public static implicit operator Texture2D(UnifiedImage unifiedImage)
    {
        return unifiedImage.Game;
    }
    
}