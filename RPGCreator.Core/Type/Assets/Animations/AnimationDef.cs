using Avalonia.Media.Imaging;
using RPGCreator.Core.Common;
using RPGCreator.Core.Type.Internal;

namespace RPGCreator.Core.Type.Assets.Animations;

public class AnimationDef : IHasUniqueId, IHasSavePath, ISerializable, IDeserializable
{
    public Ulid Unique { get; }
    public URN Urn { get; }
    public string SavePath { get; set; }
    
    private bool fromBitmap = false;
    
    public AnimationDef()
    {
        Unique = Ulid.NewUlid();
        Urn = new URN("animation", Unique.ToString());
        SavePath = string.Empty;
    }
    
    public AnimationDef(Bitmap bitmap)
    {
        Unique = Ulid.NewUlid();
        Urn = new URN("animation", Unique.ToString());
        SavePath = string.Empty;
        fromBitmap = true;
        _cachedAnimationImage = bitmap;
        AnimationImageSize = new Size((int)bitmap.Size.Width, (int)bitmap.Size.Height);
        // Default frame size
        FrameSize = new Size(42, 64);
        int columns = (int)Math.Floor((double)AnimationImageSize.Width / (double)FrameSize.Width);
        int rows = (int)Math.Floor((double)AnimationImageSize.Height / (double)FrameSize.Height);
        TotalFrames = columns * rows;
    }
    
    #region Events
    
    public event Action<string>? AnimationPathChanged;
    
    #endregion
    
    private string _animationPath = string.Empty;

    public string AnimationPath
    {
        get => _animationPath;
        set
        {
            _animationPath = value;
            RefreshMetadata();
            AnimationPathChanged?.Invoke(_animationPath);
        }
    }
    
    private Bitmap? _cachedAnimationImage = null;
    
    public Bitmap GetOrLoadAnimationImage()
    {
        if (_cachedAnimationImage == null)
        {
            if(!System.IO.File.Exists(_animationPath))
            {
                throw new FileNotFoundException($"Animation file not found at path: {_animationPath}");
            }
            _cachedAnimationImage = new Bitmap(_animationPath);
        }
        return _cachedAnimationImage;
    }
    
    public string AnimationSha { get; set; } = string.Empty;
    
    public int TotalFrames { get; private set; }
    public double FrameDuration { get; set; } = 100;
    public Size FrameSize { get; set; } = new Size(64, 64);
    public Size AnimationImageSize { get; private set; }
    
    public int FrameWidth => AnimationImageSize.Width / FrameSize.Width;
    public int FrameHeight => AnimationImageSize.Height / FrameSize.Height;
    
    private void RefreshMetadata()
    {
        if(!System.IO.File.Exists(_animationPath))
        {
            throw new FileNotFoundException($"Animation file not found at path: {_animationPath}");
        }
        _cachedAnimationImage = null; // Invalidate cached image
        AnimationSha = ShaUtil.ComputeSha256(_animationPath);
        var bitmap = new Avalonia.Media.Imaging.Bitmap(_animationPath);
        AnimationImageSize = new Size(bitmap.PixelSize.Width, bitmap.PixelSize.Height);
        int columns = (int)Math.Floor((double)AnimationImageSize.Width / (double)FrameSize.Width);
        int rows = (int)Math.Floor((double)AnimationImageSize.Height / (double)FrameSize.Height);
        TotalFrames = columns * rows;
    }

    public SerializationInfo GetObjectData()
    {

        var info = new SerializationInfo(typeof(AnimationDef));
        info.AddValue("Urn", Urn);
        info.AddValue("AnimationPath", _animationPath);
        info.AddValue("AnimationSha", AnimationSha);
        info.AddValue("TotalFrames", TotalFrames);
        info.AddValue("FrameDuration", FrameDuration);
        info.AddValue("FrameSize", FrameSize);
        info.AddValue("AnimationImageSize", AnimationImageSize);
        return info;

    }

    public void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue(nameof(_animationPath), out var animationPath, string.Empty, $"{nameof(_animationPath)} not found in deserialization info. Defaulting to empty string.");
        _animationPath = animationPath;
        info.TryGetValue(nameof(AnimationSha), out var animationSha, string.Empty, $"{nameof(AnimationSha)} not found in deserialization info. Defaulting to empty string.");
        AnimationSha = animationSha;
        info.TryGetValue(nameof(TotalFrames), out var totalFrames, 0, $"{nameof(TotalFrames)} not found in deserialization info. Defaulting to 0.");
        TotalFrames = totalFrames;
        info.TryGetValue(nameof(FrameDuration), out var frameDuration, 100.0, $"{nameof(FrameDuration)} not found in deserialization info. Defaulting to 100.0.");
        FrameDuration = frameDuration;
        info.TryGetValue(nameof(FrameSize), out var frameSize, new Size(64, 64), $"{nameof(FrameSize)} not found in deserialization info. Defaulting to Size(64, 64).");
        FrameSize = frameSize;
        info.TryGetValue(nameof(AnimationImageSize), out var animationImageSize, new Size(0, 0), $"{nameof(AnimationImageSize)} not found in deserialization info. Defaulting to Size(0, 0).");
        AnimationImageSize = animationImageSize;

        if (!ShaUtil.VerifySha256(_animationPath, AnimationSha))
        {
            throw new InvalidDataException("Animation data verification failed during deserialization. The animation file may be missing, corrupted or tampered with.");
        }
    }
}