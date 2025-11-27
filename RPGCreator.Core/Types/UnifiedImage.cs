using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Serilog;
using Color = Avalonia.Media.Color;

namespace RPGCreator.Core.Types;

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

    private static Texture2D? _defaultGameTexture;
    private static Texture2D GetDefaultGameTexture(GraphicsDevice device)
    {
        if (_defaultGameTexture == null || _defaultGameTexture.IsDisposed)
        {
            _defaultGameTexture = new Texture2D(device, 1, 1);
            _defaultGameTexture.SetData(new[] { Microsoft.Xna.Framework.Color.Magenta });
        }
        return _defaultGameTexture;
    }
    
    #endregion

    private string _filePath = string.Empty;
    public string FilePath => _filePath;
    private GraphicsDevice? GraphicsDevice { get; set; } = null;

    private Bitmap? _avaloniaImageCache = null!;
    private Texture2D? _monoGameTexture = null!;
    
    public Bitmap UI
    {
        get
        {
            
            if(_avaloniaImageCache != null)
                return _avaloniaImageCache;
            
            try
            {
                if (!File.Exists(_filePath))
                {
                    Log.Warning("UnifiedImage: File not found at path {FilePath}. Using default image.", _filePath);
                    return DefaultUI;
                }
                var loadedBitmap = new Bitmap(_filePath);
                _avaloniaImageCache = loadedBitmap;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "UnifiedImage: Failed to load image from path {FilePath}. Using default image.", _filePath);
                return DefaultUI;
            }

            return _avaloniaImageCache ?? DefaultUI;
        }
    }

    public Texture2D Game
    {
        get
        {
            if(GraphicsDevice == null)
                throw new InvalidOperationException("GraphicsDevice not set.");
            
            if (_monoGameTexture != null && !_monoGameTexture.IsDisposed) 
                return _monoGameTexture;

            try
            {
                if (!string.IsNullOrEmpty(_filePath) && File.Exists(_filePath))
                {
                    using var stream = new FileStream(_filePath, FileMode.Open);
                    _monoGameTexture = Texture2D.FromStream(GraphicsDevice, stream);
                }
                else
                {
                    Log.Warning("UnifiedImage: File not found at path {FilePath}. Using default game texture.", _filePath);
                    return GetDefaultGameTexture(GraphicsDevice);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "UnifiedImage: Texture creation failed.");
                return GetDefaultGameTexture(GraphicsDevice);
            }
            
            return _monoGameTexture;
        }
    }
    
    public UnifiedImage()
    {
    }

    public UnifiedImage(string filePath)
    {
        _filePath = filePath;
    }
    
    public UnifiedImage(string filePath, GraphicsDevice graphicsDevice) : this(filePath)
    {
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
    
    public bool HasGraphicsDevice()
    {
        return GraphicsDevice != null;
    }

    public void Dispose()
    {
        _avaloniaImageCache?.Dispose();
        _monoGameTexture?.Dispose();
    }

    public static implicit operator Texture2D(UnifiedImage unifiedImage)
    {
        return unifiedImage.Game;
    }
    
}