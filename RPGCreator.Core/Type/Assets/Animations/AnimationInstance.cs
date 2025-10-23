using Avalonia.Media.Imaging;
using RPGCreator.Core.Type.Internal;
using Serilog;

namespace RPGCreator.Core.Type.Assets.Animations;

public class AnimationInstance
{
    public AnimationDef Definition { get; }
    
    private List<CroppedBitmap> _frames = new List<CroppedBitmap>();
    public IReadOnlyList<CroppedBitmap> Frames => _frames;
    
    private Bitmap _animationImageSource;
    
    public AnimationInstance(AnimationDef definition)
    {
        Definition = definition;
        _animationImageSource = Definition.GetOrLoadAnimationImage();
    }

    public CroppedBitmap GetFrame(int frame)
    {
        
        if(frame < 0 || frame >= Definition.TotalFrames)
        {
            frame = Definition.TotalFrames - 1;
            Log.Error($"Frame index {frame} is out of range. Total frames: {Definition.TotalFrames}");
        }
        
        int x = (frame % Definition.FrameWidth) * Definition.FrameSize.Width;
        int y = (frame / Definition.FrameWidth) * Definition.FrameSize.Height;
        var cropRect = new Avalonia.PixelRect(x, y, Definition.FrameSize.Width, Definition.FrameSize.Height);
        var croppedBitmap = new CroppedBitmap(_animationImageSource, cropRect);
        return croppedBitmap;
    }
}