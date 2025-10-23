using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media.Imaging;

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
    
    #endregion
    
    #region Properties
    
    
    private List<CroppedBitmap> _frames = new List<CroppedBitmap>();
    public IReadOnlyList<CroppedBitmap> Frames => _frames;
    
    private bool IsFraming { get; set; } = false;
    
    private string _animationPath = string.Empty;

    public string AnimationPath
    {
        get => _animationPath;
        set
        {
            _animationPath = value;
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
    public double FrameDuration { get; set; } = 100;
    
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
        
        buttonsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
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
    }

    private void RegisterEvents()
    {
        playButton.Click += (s, e) => Play();
        pauseButton.Click += (s, e) => Pause();
        stopButton.Click += (s, e) => Stop();

        AnimationPathChanged += OnAnimationPathChanged;
        
        AnimationTimer.Elapsed += (s, e) =>
        {
            if (IsPlaying && !IsPaused)
            {
                CurrentFrame++;
                if (CurrentFrame >= TotalFrames)
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
    
    public void Stop()
    {
        if (!IsPlaying) return;
        IsPlaying = false;
        IsPaused = false;
        AnimationTimer.Stop();
        CurrentFrame = 0;
        UpdateFrame(0);
        Stopped?.Invoke();
        // Stop animation timer logic here
    }

    private void UpdateFrame(int frameIndex = -1)
    {
        
        if(frameIndex == -1)
            frameIndex = CurrentFrame;

        // If we have the frame cached, use it
        CroppedBitmap? cachedFrame;
        if (_frames.Count > frameIndex)
        {
            cachedFrame = _frames[frameIndex];
            // Call from ui thread
            Avalonia.Threading.Dispatcher.UIThread.Post(() => { animationImage.Source = cachedFrame; });
            return;
        }
        
        // Update animationImage to show the frame at frameIndex
        var frameX = (frameIndex * (int)FrameSize.Width) % (int)AnimationImageSize.Width;
        var frameY = ((frameIndex * (int)FrameSize.Width) / (int)AnimationImageSize.Width) * (int)FrameSize.Height;
        
        // Set the source rectangle of animationImage to the calculated frameX and frameY
        var sourceRect = new Avalonia.PixelRect(frameX, frameY, (int)FrameSize.Width, (int)FrameSize.Height);
        
        
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            var cropped = new Avalonia.Media.Imaging.CroppedBitmap(_animationImageSource, sourceRect);
            _frames.Add(cropped);
            animationImage.Source = cropped;
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