using Avalonia;
using Avalonia.Media.Imaging;
using MonoGame.Extended.Graphics;
using RPGCreator.Core.Common;
using RPGCreator.Core.Managers.AssetsManager;
using RPGCreator.Core.Type.Internal;
using Serilog;

namespace RPGCreator.Core.Type.Assets.Animations;

public class AnimationInstance: IResettable<AnimationDef>, ICleanable
{
    public AnimationDef Definition { get; private set; }
    
    private CroppedBitmap?[] _frames = Array.Empty<CroppedBitmap?>();
    public IReadOnlyList<CroppedBitmap> Frames => _frames;

    private SpritesheetDef? _sourceSpritesheet;
    public Texture2DAtlas? TextureAtlas { get; private set; }
    
    public AnimationInstance(AnimationDef definition)
    {
        Definition = definition;
        _frames = new CroppedBitmap?[Definition.TotalFrames];
    }

    public CroppedBitmap GetFrame(int frame)
    {
        if (_sourceSpritesheet == null)
        {
            Log.Error("Source spritesheet is null for animation \"{0}\"", Definition.Urn);
            return new CroppedBitmap(UnifiedImage.DefaultUI, new PixelRect(0, 0, 32, 32));
        }
        
        if(frame < 0 || frame >= Definition.TotalFrames)
        {
            Log.Error($"Frame index {frame} is out of range. Total frames: {Definition.TotalFrames}");
            frame = Definition.TotalFrames - 1;
            
            if (frame < 0)
            {
                frame = 0;
            }
        }
        
        if(frame >= _frames.Length)
        {
            Log.Error("Frame index {0} exceeds cached frames length {1} for animation \"{2}\"", frame, _frames.Length, Definition.Urn);
            return new CroppedBitmap(UnifiedImage.DefaultUI, new PixelRect(0, 0, 32, 32));
        }
        
        if(_frames[frame] != null)
            return _frames[frame];
        
        var cropRect = _sourceSpritesheet.GetFrameRect(Definition.FrameIndexes[frame]);
        Bitmap bitmap = _sourceSpritesheet.SourceImage.Image;
        var croppedBitmap = new CroppedBitmap(bitmap, cropRect);
        _frames[frame] = (croppedBitmap);
        return croppedBitmap;
    }

    public void Clean()
    {
        Array.Clear(_frames, 0, _frames.Length);
        TextureAtlas = null;
        _sourceSpritesheet = null;
    }

    public void ResetFrom(AnimationDef def, params object[] parameters)
    {
        Definition = def;

        if (_frames.Length != def.TotalFrames)
        {
            _frames = new CroppedBitmap?[def.TotalFrames];
        }
        else
        {
            Array.Clear(_frames, 0, _frames.Length);
        }

        if (parameters.Length > 0 && parameters[0] is SpritesheetDef spritesheetDef)
        {
            _sourceSpritesheet = spritesheetDef;
        }
        else
        {
            Log.Error("AnimationInstance.ResetFrom: Missing SpritesheetDef parameter.");
            _sourceSpritesheet = null;
        }
    }
}