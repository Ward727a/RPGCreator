using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using RPGCreator.Core;
using RPGCreator.Core.Common;
using RPGCreator.Core.Type.Assets.Animations;
using Serilog;
using Ursa.Controls;

namespace RPGCreator.UI.Common;

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
    
    
    private List<CroppedBitmap> _frames = new List<CroppedBitmap>();
    public IReadOnlyList<CroppedBitmap> Frames => _frames;
    
    private bool IsFraming { get; set; } = false;
    
    private AnimationDef _animationDef = new AnimationDef();
    public AnimationInstance? _animationInstance;
    
    private string _animationPath = string.Empty;

    public string AnimationPath
    {
        get => _animationPath;
        set
        {
            _animationPath = value;
            // _animationDef.FrameSize = new Core.Type.Internal.Size(48, 64);
            // _animationDef.AnimationPath = value;
            _animationInstance ??= new AnimationInstance(_animationDef);
            AnimationPathChanged?.Invoke(_animationPath);
        }
    }

    public bool IsPlaying { get; private set; }
    public bool IsPaused { get; private set; }
    public int CurrentFrame { get; private set; }
    public int TotalFrames { get; private set; }
    /// <summary>
    /// Milliseconds per frame
    /// </summary>
    private double FrameDuration { get; set; } = 100;

    public int FPS
    {
        get => (int)(1000 / FrameDuration);
        set
        {
            FrameDuration = 1000.0 / value;
            if(AnimationTimer != null)
                AnimationTimer.Interval = FrameDuration;
        }
    }
    
    public System.Timers.Timer? AnimationTimer { get; private set; }
    
    public Size FrameSize { get; set; } = new Size(64, 64);
    public Size AnimationImageSize { get; private set; }

    private Bitmap _animationImageSource;
    
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
    
    #region Constructors
    public AnimationPreviewer()
    {
        
        CreateComponents();
        AnimationTimer = new System.Timers.Timer(FrameDuration);
        RegisterEvents();
        
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
        
        AnimationTimer.Elapsed += (s, e) =>
        {
            if (IsPlaying && !IsPaused)
            {
                CurrentFrame++;
                if (CurrentFrame >= _animationDef.TotalFrames)
                {
                    CurrentFrame = 0; // Loop back to the first frame
                }

                UpdateFrame();
            }
        };
        AnimationTimer.AutoReset = true;
    }

    public void Play()
    {
        if(_animationPath == string.Empty) return;
        if (IsPlaying) return;
        IsPlaying = true;
        IsPaused = false;
        AnimationTimer.Start();
        PlayStarted?.Invoke();
        // Start animation timer logic here
    }
    
    public void Pause()
    {
        if (!IsPlaying || IsPaused) return;
        IsPaused = true;
        IsPlaying = false;
        AnimationTimer.Stop();
        Paused?.Invoke();
        // Pause animation timer logic here
    }
    
    public void Stop(bool resetFrame = true)
    {
        if (!IsPlaying) return;
        IsPlaying = false;
        IsPaused = false;
        AnimationTimer.Stop();
        CurrentFrame = 0;
        if(resetFrame)
            UpdateFrame(0);
        Stopped?.Invoke();
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

    public void UpdateFrame(int frameIndex = -1)
    {
        
        if(frameIndex == -1)
            frameIndex = CurrentFrame;

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            animationImage.Source = _animationInstance.GetFrame(frameIndex);
        });
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
    
    private void OnAnimationPathChanged(string newPath)
    {
        Stop();
        // Load animation from newPath and set TotalFrames accordingly
        // Reset CurrentFrame to 0
        CurrentFrame = 0;
        // Update animationImage source here
        _animationImageSource = new Bitmap(newPath);
        animationImage.Source = _animationImageSource; // Placeholder for actual image loading logic
        AnimationImageSize = new Size(_animationImageSource.PixelSize.Width, _animationImageSource.PixelSize.Height);
        TotalFrames = (int)(AnimationImageSize.Width / FrameSize.Width) * (int)(AnimationImageSize.Height / FrameSize.Height);
        _frames.Clear();
        UpdateFrame(0);
    }
    
    #endregion
}