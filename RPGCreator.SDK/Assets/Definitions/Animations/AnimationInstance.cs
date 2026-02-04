using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.Assets.Definitions.Animations;

public class AnimationInstance: IResettable<AnimationDef>, ICleanable
{
    
    public AnimationDef Definition { get; private set; }
    
    public double ElapsedTime { get; private set; } = 0.0;
    public int CurrentFrameIndex { get; private set; } = 0;
    public bool IsPlaying { get; set; } = false;
    
    public AnimationInstance(AnimationDef definition)
    {
        Definition = definition;
    }

    public void Update(TimeSpan deltaTime)
    {
        if (!IsPlaying || Definition.TotalFrames <= 1) return;
        
        ElapsedTime += deltaTime.Milliseconds;

        if (ElapsedTime >= Definition.FrameDuration)
        {
            CurrentFrameIndex = (CurrentFrameIndex + 1) % Definition.TotalFrames;
            ElapsedTime = 0;
        }
    }
    
    public void ForceSetFrame(int frameIndex)
    {
        if (frameIndex < 0 || frameIndex >= Definition.TotalFrames)
            throw new ArgumentOutOfRangeException(nameof(frameIndex), "Frame index is out of range.");

        CurrentFrameIndex = frameIndex;
        ElapsedTime = 0.0;
    }
    
    public void ForceNextFrame()
    {
        CurrentFrameIndex = (CurrentFrameIndex + 1) % Definition.TotalFrames;
        ElapsedTime = 0.0;
    }

    public int GetCurrentSpritesheetIndex()
    {
        if (Definition.FrameIndexes.Count == 0) return -1;
        return Definition.FrameIndexes[CurrentFrameIndex];
    }

    public void Clean()
    {
        ElapsedTime = 0.0;
        CurrentFrameIndex = 0;
        IsPlaying = false;
    }

    public void ResetFrom(AnimationDef def, params object[] parameters)
    {
        Definition = def;
        ElapsedTime = 0.0;
        CurrentFrameIndex = 0;
        IsPlaying = false;
    }

    public void Draw(IRenderContext? context, IDrawer<AnimationInstance> renderer)
    {
        renderer.Draw(context, this);
    }
}