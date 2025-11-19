using Avalonia.Media.Imaging;
using Microsoft.Xna.Framework.Graphics;

namespace RPGCreator.Core.Type;

public class UnifiedImage : IDisposable
{
    
    private string _filePath = string.Empty;
    public GraphicsDevice GraphicsDevice { private get; set; } = null!;

    private Bitmap? _avaloniaBitmap = null!;
    private Texture2D? _monoGameTexture = null!;
    
    public Bitmap UI
    {
        get
        {
            if(_avaloniaBitmap == null)
                _avaloniaBitmap = new Bitmap(_filePath);
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