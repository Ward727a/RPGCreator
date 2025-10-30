using Avalonia.Media.Imaging;
using MonoGame.Extended.Graphics;
using RPGCreator.Core.Common;
using RPGCreator.Core.Type.Internal;
using Serilog;

namespace RPGCreator.Core.Type.Assets.Animations;

public class AnimationInstance
{
    public AnimationDef Definition { get; }
    
    private List<CroppedBitmap> _frames = new List<CroppedBitmap>();
    public IReadOnlyList<CroppedBitmap> Frames => _frames;
    
    private Bitmap _animationImageSource;
    public Texture2DAtlas? TextureAtlas { get; private set; }
    
    public AnimationInstance(AnimationDef definition)
    {
        Definition = definition;
        _animationImageSource = Definition.GetOrLoadAnimationImage();
        Definition.AnimationPathChanged += OnAnimationPathChanged;
        for (int i = 0; i < Definition.TotalFrames; i++)
        {
            int currentFrame = i;
            var frame = this.GetFrame(currentFrame);
            Byte[] bytes = ImageUtil.ExtractPixelsFromCroppedBitmap(frame);
                
            // Ensure directory exists
            
            var dir = $"frames_test_animation/{Path.GetFileNameWithoutExtension(Definition.AnimationPath)}";
            
            Directory.CreateDirectory(dir);
                
            ImageUtil.SavePixelsAsPng(bytes, (int)frame.Size.Width, (int)frame.Size.Height, $"{dir}/frame_{currentFrame}.png");

            var data = ImageUtil.BuildFromFrames(EngineCore.Instance.Data.RTPGame.GraphicsDevice, dir);
                
            using (var fs = File.OpenWrite($"{dir}/atlas.png"))
                data.Atlas.SaveAsPng(fs, data.Atlas.Width, data.Atlas.Height);
                
            Log.Debug("Saved frame {0} to {1}/frame_{frame}.png", currentFrame, dir, currentFrame);
            
            if(TextureAtlas == null)
                TextureAtlas = new Texture2DAtlas(data.Atlas);
        }
    }

    private void OnAnimationPathChanged(string obj)
    {
        
        _animationImageSource = Definition.GetOrLoadAnimationImage();
        
        _frames.Clear();
        
        for (int i = 0; i < Definition.TotalFrames; i++)
        {
            int currentFrame = i;
            var frame = this.GetFrame(currentFrame);
            Byte[] bytes = ImageUtil.ExtractPixelsFromCroppedBitmap(frame);
                
            // Ensure directory exists
            
            var dir = $"frames_test_animation/{Path.GetFileNameWithoutExtension(Definition.AnimationPath)}";
            
            Directory.CreateDirectory(dir);
                
            ImageUtil.SavePixelsAsPng(bytes, (int)frame.Size.Width, (int)frame.Size.Height, $"{dir}/frame_{currentFrame}.png");

            var data = ImageUtil.BuildFromFrames(EngineCore.Instance.Data.RTPGame.GraphicsDevice, dir);
                
            using (var fs = File.OpenWrite($"{dir}/atlas.png"))
                data.Atlas.SaveAsPng(fs, data.Atlas.Width, data.Atlas.Height);
                
            Log.Debug("Saved frame {0} to {1}/frame_{frame}.png", currentFrame, dir, currentFrame);
            
            if(TextureAtlas == null)
                TextureAtlas = new Texture2DAtlas(data.Atlas);
        }
            
        
    }

    public CroppedBitmap GetFrame(int frame)
    {
        
        if(frame < 0 || frame > Definition.TotalFrames)
        {
            Log.Error($"Frame index {frame} is out of range. Total frames: {Definition.TotalFrames}");
            frame = Definition.TotalFrames - 1;
        }
        
        int x = (frame * Definition.FrameSize.Width) % Definition.AnimationImageSize.Width;
        int y = ((frame * Definition.FrameSize.Width) / Definition.AnimationImageSize.Width) * Definition.FrameSize.Height;
        var cropRect = new Avalonia.PixelRect(x, y, Definition.FrameSize.Width, Definition.FrameSize.Height);
        var croppedBitmap = new CroppedBitmap(_animationImageSource, cropRect);
        return croppedBitmap;
    }
}