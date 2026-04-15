// Decompiled with JetBrains decompiler
// Type: AvaloniaInside.MonoGame.MonoGameControl
// Assembly: AvaloniaInside.MonoGame, Version=1.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: 51C24C9E-EB79-45B0-A927-F1C858093BE7
// Assembly location: C:\Users\Admin\.nuget\packages\avaloniainside.monogame\1.0.2\lib\net10.0\AvaloniaInside.MonoGame.dll

#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RPGCreator.UI.Common;

// Code from 'AvaloniaInside.MonoGame' package, slightly modified to work with a multi-viewport system.

public sealed class MonoGameControl : Control
{
  public static readonly DirectProperty<MonoGameControl, IBrush> FallbackBackgroundProperty = AvaloniaProperty.RegisterDirect<MonoGameControl, IBrush>(nameof (FallbackBackground), (Func<MonoGameControl, IBrush>) (o => o.FallbackBackground), (Action<MonoGameControl, IBrush>) ((o, v) => o.FallbackBackground = v));
  public static readonly DirectProperty<MonoGameControl, Game?> GameProperty = 
    AvaloniaProperty.RegisterDirect<MonoGameControl, Game>(
      nameof (Game), 
      (Func<MonoGameControl, Game>) (o => o.Game), 
      (Action<MonoGameControl, Game>) ((o, v) => o.Game = v));
  private readonly Stopwatch _stopwatch = new Stopwatch();
  private readonly GameTime _gameTime = new GameTime();
  private readonly PresentationParameters _presentationParameters = new PresentationParameters()
  {
    BackBufferWidth = 1,
    BackBufferHeight = 1,
    BackBufferFormat = SurfaceFormat.Color,
    DepthStencilFormat = DepthFormat.Depth24,
    PresentationInterval = PresentInterval.Immediate,
    IsFullScreen = false
  };
  private byte[] _bufferData = Array.Empty<byte>();
  private WriteableBitmap? _bitmap;
  private bool _isInitialized;

  public MonoGameControl() => Focusable = true;

  public IBrush FallbackBackground { get; set; } = (IBrush) Brushes.Purple;

  public Game? Game
  {
    get;
    set
    {
      if (field == value)
        return;
      field = value;
      if (!_isInitialized)
        return;
      Initialize();
    }
  }

  public override void Render(DrawingContext context)
  {
    Game game = Game;
    if (game != null)
    {
      GraphicsDevice graphicsDevice = Game?.GraphicsDevice;
      if (graphicsDevice != null && _bitmap != null)
      {
        Rect bounds = Bounds;
        if ((bounds.Width >= 1.0 || bounds.Height >= 1.0) && HandleDeviceReset(graphicsDevice))
        {
          RunFrame(game);
        }
      }
    }
  }

  protected override Size ArrangeOverride(Size finalSize)
  {
    finalSize = base.ArrangeOverride(finalSize);
    Size size1 = finalSize;
    Size? size2 = _bitmap?.Size;
    if ((size2.HasValue ? (size1 != size2.GetValueOrDefault() ? 1 : 0) : 1) != 0)
    {
      GraphicsDevice graphicsDevice = Game?.GraphicsDevice;
      if (graphicsDevice != null)
        ResetDevice(graphicsDevice, finalSize);
    }
    return finalSize;
  }

  protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
  {
    base.OnAttachedToVisualTree(e);
    Start();
  }

  private bool HandleDeviceReset(GraphicsDevice device)
  {
    if (device.GraphicsDeviceStatus == GraphicsDeviceStatus.NotReset)
      ResetDevice(device, Bounds.Size);
    return device.GraphicsDeviceStatus == GraphicsDeviceStatus.Normal;
  }

  private void Initialize()
  {
    TrySetWindowHandle();
    Game game = Game;
    if (game == null)
      return;
    GraphicsDevice graphicsDevice = game.GraphicsDevice;
    if (graphicsDevice != null)
      ResetDevice(graphicsDevice, Bounds.Size);
    RunFrame(game);
  }

  private void Start()
  {
    if (_isInitialized)
      return;
    Initialize();
    _stopwatch.Start();
    _isInitialized = true;
  }

  private void ResetDevice(GraphicsDevice device, Size newSize)
  {
    if (_presentationParameters.DeviceWindowHandle == IntPtr.Zero)
    {
      TrySetWindowHandle();
      if (_presentationParameters.DeviceWindowHandle == IntPtr.Zero)
        return;
    }
    int width1 = Math.Max(1, (int) Math.Ceiling(newSize.Width));
    int height1 = Math.Max(1, (int) Math.Ceiling(newSize.Height));
    device.Viewport = new Viewport(0, 0, width1, height1);
    _presentationParameters.BackBufferWidth = width1;
    _presentationParameters.BackBufferHeight = height1;
    device.Reset(_presentationParameters);
    _bitmap?.Dispose();
    Viewport viewport = device.Viewport;
    int width2 = viewport.Width;
    viewport = device.Viewport;
    int height2 = viewport.Height;
    _bitmap = new WriteableBitmap(new PixelSize(width2, height2), new Vector(96.0, 96.0), new PixelFormat?(PixelFormat.Rgba8888), new AlphaFormat?(AlphaFormat.Opaque));
  }

  private void TrySetWindowHandle()
  {
    if (TopLevel.GetTopLevel(this) is not Window visualRoot || visualRoot.PlatformImpl == null)
      return;
    IntPtr? handle = visualRoot.TryGetPlatformHandle()?.Handle;
    if (!handle.HasValue)
      return;
    _presentationParameters.DeviceWindowHandle = handle.GetValueOrDefault();
  }

  private void RunFrame(Game game)
  {
    _gameTime.ElapsedGameTime = _stopwatch.Elapsed;
    _gameTime.TotalGameTime += _gameTime.ElapsedGameTime;
    _stopwatch.Restart();
    try
    {
      game.RunOneFrame();
    }
    catch (Exception ex)
    {
      Console.WriteLine((object) ex);
    }
    finally
    {
      Dispatcher.UIThread.Post(new Action(((Visual) this).InvalidateVisual), DispatcherPriority.Render);
    }
  }

  private void CaptureFrame(GraphicsDevice device, WriteableBitmap bitmap)
  {
    using (ILockedFramebuffer lockedFramebuffer = bitmap.Lock())
    {
      int num = lockedFramebuffer.RowBytes * lockedFramebuffer.Size.Height;
      if (_bufferData.Length < num)
        Array.Resize<byte>(ref _bufferData, num);
      device.GetBackBufferData<byte>(_bufferData, 0, num);
      Marshal.Copy(_bufferData, 0, lockedFramebuffer.Address, num);
    }
  }
}
