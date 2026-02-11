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
using RPGCreator.RTP;

namespace RPGCreator.UI.Test;

public sealed class MonoGameControlTest : Control
{
  public static readonly DirectProperty<MonoGameControlTest, IBrush> FallbackBackgroundProperty = AvaloniaProperty.RegisterDirect<MonoGameControlTest, IBrush>(nameof (FallbackBackground), (Func<MonoGameControlTest, IBrush>) (o => o.FallbackBackground), (Action<MonoGameControlTest, IBrush>) ((o, v) => o.FallbackBackground = v));
  public static readonly DirectProperty<MonoGameControlTest, Game?> GameProperty = 
    AvaloniaProperty.RegisterDirect<MonoGameControlTest, Game>(
      nameof (Game), 
      (Func<MonoGameControlTest, Game>) (o => o.Game), 
      (Action<MonoGameControlTest, Game>) ((o, v) => o.Game = v));
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

  public MonoGameControlTest() => this.Focusable = true;

  public IBrush FallbackBackground { get; set; } = (IBrush) Brushes.Purple;

  public Game? Game
  {
    get;
    set
    {
      if (field == value)
        return;
      field = value;
      if (!this._isInitialized)
        return;
      this.Initialize();
    }
  }

  public override void Render(DrawingContext context)
  {
    Game game = this.Game;
    if (game != null)
    {
      GraphicsDevice graphicsDevice = this.Game?.GraphicsDevice;
      if (graphicsDevice != null && this._bitmap != null)
      {
        Rect bounds = this.Bounds;
        if ((bounds.Width >= 1.0 || bounds.Height >= 1.0) && this.HandleDeviceReset(graphicsDevice))
        {
          this.RunFrame(game);
        }
      }
    }
  }

  protected override Size ArrangeOverride(Size finalSize)
  {
    finalSize = base.ArrangeOverride(finalSize);
    Size size1 = finalSize;
    Size? size2 = this._bitmap?.Size;
    if ((size2.HasValue ? (size1 != size2.GetValueOrDefault() ? 1 : 0) : 1) != 0)
    {
      GraphicsDevice graphicsDevice = this.Game?.GraphicsDevice;
      if (graphicsDevice != null)
        this.ResetDevice(graphicsDevice, finalSize);
    }
    return finalSize;
  }

  protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
  {
    base.OnAttachedToVisualTree(e);
    this.Start();
  }

  private bool HandleDeviceReset(GraphicsDevice device)
  {
    if (device.GraphicsDeviceStatus == GraphicsDeviceStatus.NotReset)
      this.ResetDevice(device, this.Bounds.Size);
    return device.GraphicsDeviceStatus == GraphicsDeviceStatus.Normal;
  }

  private void Initialize()
  {
    this.TrySetWindowHandle();
    Game game = this.Game;
    if (game == null)
      return;
    GraphicsDevice graphicsDevice = game.GraphicsDevice;
    if (graphicsDevice != null)
      this.ResetDevice(graphicsDevice, this.Bounds.Size);
    this.RunFrame(game);
  }

  private void Start()
  {
    if (this._isInitialized)
      return;
    this.Initialize();
    this._stopwatch.Start();
    this._isInitialized = true;
  }

  private void ResetDevice(GraphicsDevice device, Size newSize)
  {
    if (this._presentationParameters.DeviceWindowHandle == IntPtr.Zero)
    {
      this.TrySetWindowHandle();
      if (this._presentationParameters.DeviceWindowHandle == IntPtr.Zero)
        return;
    }
    int width1 = Math.Max(1, (int) Math.Ceiling(newSize.Width));
    int height1 = Math.Max(1, (int) Math.Ceiling(newSize.Height));
    device.Viewport = new Viewport(0, 0, width1, height1);
    this._presentationParameters.BackBufferWidth = width1;
    this._presentationParameters.BackBufferHeight = height1;
    device.Reset(this._presentationParameters);
    this._bitmap?.Dispose();
    Viewport viewport = device.Viewport;
    int width2 = viewport.Width;
    viewport = device.Viewport;
    int height2 = viewport.Height;
    this._bitmap = new WriteableBitmap(new PixelSize(width2, height2), new Vector(96.0, 96.0), new PixelFormat?(PixelFormat.Rgba8888), new AlphaFormat?(AlphaFormat.Opaque));
  }

  private void TrySetWindowHandle()
  {
    if (!(this.GetVisualRoot() is Window visualRoot) || visualRoot.PlatformImpl == null)
      return;
    IntPtr? handle = visualRoot.TryGetPlatformHandle()?.Handle;
    if (!handle.HasValue)
      return;
    this._presentationParameters.DeviceWindowHandle = handle.GetValueOrDefault();
  }

  private void RunFrame(Game game)
  {
    this._gameTime.ElapsedGameTime = this._stopwatch.Elapsed;
    this._gameTime.TotalGameTime += this._gameTime.ElapsedGameTime;
    this._stopwatch.Restart();
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
      if (this._bufferData.Length < num)
        Array.Resize<byte>(ref this._bufferData, num);
      device.GetBackBufferData<byte>(this._bufferData, 0, num);
      Marshal.Copy(this._bufferData, 0, lockedFramebuffer.Address, num);
    }
  }
}
