using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Animations;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types.Collections;
using RPGCreator.SDK.Types.Internals;
using Ursa.Controls;

namespace RPGCreator.UI.Common;

public class AvaloniaAnimationDrawer : IDrawer<AnimationInstance>, IDisposable
{

    private Image _targetImage;
    private IAssetScope _assetScope;
    private SpritesheetDef? _cachedSpritesheet;
    private Bitmap? _cachedSpritesheetImage;
    
    public AvaloniaAnimationDrawer(Image targetImage)
    {
        _targetImage = targetImage;
        _assetScope = EngineServices.AssetsManager.CreateAssetScope();
    }
    
    public void Draw(IRenderContext context, AnimationInstance animation)
    {
        if(_cachedSpritesheet == null || _cachedSpritesheet.Unique != animation.Definition.SpritesheetId)
        {
            _cachedSpritesheet = _assetScope.Load<SpritesheetDef>(animation.Definition.SpritesheetId);
            _cachedSpritesheetImage = EngineServices.ResourcesService.Load<Bitmap>(_cachedSpritesheet.ImagePath);
            
            if (_cachedSpritesheet == null)
            {
                Logger.Error("[AvaloniaAnimationDrawer] Failed to load spritesheet with ID: " + animation.Definition.SpritesheetId);
                return;
            }
        }

        if (_cachedSpritesheetImage == null)
        {
            Logger.Error("[AvaloniaAnimationDrawer] Failed to load spritesheet image at path: " + _cachedSpritesheet.ImagePath);
            return;
        }

        var rect = _cachedSpritesheet.GetFrameRect(animation.GetCurrentSpritesheetIndex());
        var croppedBitmap = new CroppedBitmap(_cachedSpritesheetImage, new PixelRect((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height));
    
        _targetImage.Source = croppedBitmap;
    }

    public void Dispose()
    {
        _assetScope.Dispose();
    }
}

public class AnimationPreviewer : UserControl
{
    
    #region Constants
    #endregion
    
    #region Events
    
    public event Action<string>? AnimationPathChanged;
    public event Action? PlayStarted;
    public event Action? Paused;
    public event Action? Stopped;
    
    public event Action<int>? FpsChanged;
    
    #endregion
    
    #region Properties
    public AnimationInstance? AnimationInstance;
    
    private AnimationDef? _animationDef;

    public AnimationDef? AnimationDefinition
    {
        get => _animationDef;
        set
        {
            if(value == null || value == _animationDef) return;
            
            _animationDef?.PropertyChanged -= OnAnimationDefChanged;
            _animationDef = value;

            _animationDef.PropertyChanged += OnAnimationDefChanged;
            if (_animationDef.SpritesheetId == Ulid.Empty)
            {
                return;
            }

            AnimationInstance = EngineServices.GameFactory.CreateInstance<AnimationInstance>(_animationDef);
            
            UpdateFrame();
        }
    }

    private void OnAnimationDefChanged(object? s, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AnimationDef.SpritesheetId))
        {
            OnAnimationPathChanged(AnimationDefinition.SpritesheetId);
        }
    }

    public int FPS
    {
        get => (int)(1000 / _animationDef?.FrameDuration ?? 1);
        set
        {
            _animationDef?.FrameDuration = 1000.0 / value;
            if(AnimationTimer != null)
                AnimationTimer.Interval = TimeSpan.FromMilliseconds(_animationDef?.FrameDuration ?? 100);
        }
    }
    
    public Avalonia.Threading.DispatcherTimer? AnimationTimer { get; }
    
    public Size FrameSize { get; set; } = new Size(42, 64);

    #endregion
    
    #region Components
    private Border bodyBorder { get; set; }
    private StackPanel bodyPanel { get; set; }
    private Image animationImage { get; set; }
    private StackPanel buttonsPanel { get; set; }
    private Button playButton { get; set; }
    private Button pauseButton { get; set; }
    private Button stopButton { get; set; }
    private NumericIntUpDown FPSSpeedUpDown { get; set; }
    #endregion
    
    private readonly AvaloniaAnimationDrawer _drawer;
    
    #region Constructors
    public AnimationPreviewer()
    {
        
        CreateComponents();
        AnimationTimer = new Avalonia.Threading.DispatcherTimer();
        AnimationTimer.Interval = TimeSpan.FromMilliseconds(100);
        RegisterEvents();

        if (animationImage != null) _drawer = new AvaloniaAnimationDrawer(animationImage);
        
        if(_drawer == null)
            throw new Exception("Failed to create AvaloniaAnimationDrawer for AnimationPreviewer.");

        Content = bodyBorder;
    }
    #endregion
    
    #region Methods
    
    private void CreateComponents()
    {
        bodyBorder = new Border()
        {
            BorderThickness = new Thickness(1),
            BorderBrush = Avalonia.Media.Brushes.Gray,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Thickness(5)
        };
        
        bodyPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Thickness(10)
        };
        bodyBorder.Child = bodyPanel;
        
        animationImage = new Image
        {
            Width = 256,
            Height = 256,
            Margin = new Thickness(5),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        bodyPanel.Children.Add(animationImage);
        RenderOptions.SetBitmapInterpolationMode(animationImage, Avalonia.Media.Imaging.BitmapInterpolationMode.None);
        
        buttonsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(5)
        };
        bodyPanel.Children.Add(buttonsPanel);
        
        playButton = new Button
        {
            Content = "Play",
            Margin = new Thickness(5)
        };
        buttonsPanel.Children.Add(playButton);
        
        pauseButton = new Button
        {
            Content = "Pause",
            Margin = new Thickness(5)
        };
        buttonsPanel.Children.Add(pauseButton);
        
        stopButton = new Button
        {
            Content = "Stop",
            Margin = new Thickness(5)
        };
        buttonsPanel.Children.Add(stopButton);
        
        var testSaveFrame = new Button
        {
            Content = "Save Frame",
            Margin = new Thickness(5)
        };
        
        FPSSpeedUpDown = new NumericIntUpDown
        {
            Minimum = 1,
            Maximum = 60,
            Value = 10,
            InnerRightContent = "FPS",
            Margin = new Thickness(5),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        buttonsPanel.Children.Add(FPSSpeedUpDown);
    }

    private void RegisterEvents()
    {
        playButton.Click += (s, e) => Play();
        pauseButton.Click += (s, e) => Pause();
        stopButton.Click += (s, e) => Stop();
        FPSSpeedUpDown.ValueChanged += (s, e) =>
        {
            UpdateFPS();
            if(e.NewValue.HasValue)
                FpsChanged?.Invoke(e.NewValue.Value);
        };

        AnimationPathChanged += OnAnimationPathChanged;
        
        AnimationTimer?.Tick += (s, e) =>
        {
            if (AnimationInstance?.IsPlaying ?? false)
            {
                UpdateFrame();
            }
        };
        
    }

    public void Play()
    {
        if(AnimationInstance == null) return;
        if (AnimationInstance?.IsPlaying ?? false) return;
        AnimationInstance?.IsPlaying = true;
        AnimationTimer?.Start();
        PlayStarted?.Invoke();
        // Start animation timer logic here
    }
    
    public void Pause()
    {
        if (!AnimationInstance?.IsPlaying ?? true) return;
        AnimationInstance?.IsPlaying = false;
        AnimationTimer.Stop();
        Paused?.Invoke();
        // Pause animation timer logic here
    }
    
    public void Stop(bool resetFrame = true)
    {
        if (!AnimationInstance?.IsPlaying ?? true) return;
        AnimationInstance?.IsPlaying = false;
        AnimationTimer.Stop();
        if(resetFrame)
            UpdateFrame();
        Stopped?.Invoke();
        AnimationInstance.ForceSetFrame(0);
        // Stop animation timer logic here
    }
    
    public void UpdateFPS(int fps = -1)
    {
        if(fps == -1)
            fps = FPSSpeedUpDown.Value ?? 10;
        
        FPS = fps;
        if(FPSSpeedUpDown.Value != fps)
            FPSSpeedUpDown.Value = fps;
    }

    public void UpdateFrame()
    {
        if(!IsValidAnimationInstance()) return;
        
        AnimationInstance.ForceNextFrame();
        AnimationInstance?.Draw(null, _drawer);
    }

    public void ClearImage()
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            animationImage.Source = null;
        });
    }
    
    #endregion

    #region Events Handlers
    
    private void OnAnimationPathChanged(Ulid newSpriteSheetId)
    {
        if (AnimationInstance != null)
        {
            EngineServices.GameFactory.ReleaseInstance(AnimationInstance);
        }
        
        AnimationInstance = EngineServices.GameFactory.CreateInstance<AnimationInstance>(_animationDef);
        
        Stop();
        // Load animation from newSpriteSheetId and set TotalFrames accordingly
        // Reset CurrentFrame to 0
        AnimationInstance.ForceSetFrame(0);
    }
    
    private void OnAnimationPathChanged(string newPath)
    {
        if (AnimationInstance != null)
        {
            EngineServices.GameFactory.ReleaseInstance(AnimationInstance);
        }

        AnimationInstance = EngineServices.GameFactory.CreateInstance<AnimationInstance>(_animationDef);
        Stop();
        // Load animation from newPath and set TotalFrames accordingly
        // Reset CurrentFrame to 0
        AnimationInstance.ForceSetFrame(0);
    }
    
    #endregion
    
    #region Helpers

    private bool IsValidAnimationInstance()
    {
        return AnimationInstance != null && _animationDef != null && _animationDef.TotalFrames > 0;
    }
    
    #endregion
}