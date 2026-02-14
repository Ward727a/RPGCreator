// RPG Creator - Open-source RPG Engine.
// (c) 2026 Ward
// 
// This file is part of RPG Creator and is distributed under the Apache 2.0 License.
// You are free to use, modify, and distribute this file under the terms of the Apache 2.0 License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence Apache 2.0.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence Apache 2.0.
// Voir LICENSE pour plus de détails.
// 
// Contact:
// => Mail: Ward727a@gmail.com
//    Please use this object: "RPG Creator [YourObject]"
// => Discord: ward727
// 
// For urgent inquiries, sending both an email and a message on Discord is highly recommended for a quicker response.

using System;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.RTP.Extensions;
using RPGCreator.RTP.GameUI;
using RPGCreator.RTP.GameUI.BaseControls;
using RPGCreator.RTP.GameUI.BaseControls.Box;
using RPGCreator.RTP.GameUI.BaseControls.Containers;
using RPGCreator.RTP.GameUI.BaseControls.Inputs;
using RPGCreator.RTP.GameUI.Enums;
using RPGCreator.RTP.GameUI.Layers;
using RPGCreator.SDK;
using RPGCreator.SDK.Editor.Rendering;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types;
using Size = MonoGame.Extended.Size;

namespace RPGCreator.RTP.Viewport;

public class GameViewport : BaseGameViewport
{
    private static ScopedLogger _logger = Logger.ForContext<GameViewport>();
    
    
    public RenderTarget2D? RenderTarget { get; set; }
    private uint[]? _internalBuffer;
    private IntPtr? _bitmapControlAddress;
    private Texture2D _pixelTexture;
    
    private GraphicsDevice _graphicsDevice;
    private SpriteBatch _spriteBatch;
    private UiRenderer _uiRenderer;

    private BaseLayer UiLayer;
    private Panel simpleTestControl;
    private ContainerControl simpleChildControl;
    private TextButton simpleTextControl;
    
    public GameViewport(RenderTarget2D renderTarget)
    {
        RenderTarget = renderTarget;
    }

    private static bool IsFormsInitialized = false;
    private StackPanel stackPanel;
    private TextButton buttonTest;
    
    private RenderTarget2D _UiCache;
    
    public void LoadContent(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, FontSystem fontSystem)
    {
        _graphicsDevice = graphicsDevice;
        _spriteBatch = spriteBatch;
        
        UiLayer = new BaseLayer("MainUILayer", _graphicsDevice, _spriteBatch, fontSystem);
        UiLayer.SetParentViewport(this);
        
        simpleTestControl = new Panel()
        {
            Position = new Vector2(50, 50),
            Size = new Size(600, 300),
            ClipToBounds = true,
            BackgroundColor = new Color(255, 0, 0, 128)
        };
        UiLayer.SetRootControl(simpleTestControl);
        
        // simpleChildControl = new ContainerControl()
        // {
        //     Name = "ChildControl",
        //     Position = new Vector2(20, 20),
        //     Size = new Size(100, 50)
        // };
        // simpleChildControl.Origin = new Vector2(simpleChildControl.Width / 2, simpleChildControl.Height / 2);
        // simpleTestControl.AddChild(simpleChildControl);
        //
        // var testTextControl = new TextControl()
        // {
        //     Position = new Vector2(10, 10),
        //     Size = new Size(180, 80),
        // };
        // simpleTestControl.AddChild(testTextControl);
        // testTextControl.Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
        //
        // var testSimpleColorBox2 = new SimpleColorBox()
        // {
        //     BackgroundColor = Color.Beige,
        //     Anchors = ControlAnchors.AnchorFull,
        // };
        // simpleTestControl.AddChild(testSimpleColorBox2);
        //
        // simpleTextControl = new TextButton()
        // {
        //     Size = new Size(0, 20),
        //     Anchors = ControlAnchors.AnchorFullHorizontal,
        // };
        // simpleTestControl.AddChild(simpleTextControl);
        // simpleTextControl.OnClicked += () =>
        // {
        //     Logger.Debug("Button clicked!");
        //     simpleTextControl.Text = "Clicked!";
        // };

        stackPanel = new StackPanel()
        {
            AutoSize = true,
            BackgroundColor = Color.Transparent,
            ClipToBounds = true
        };
        simpleTestControl.AddChild(stackPanel);
        text = new TextControl()
        {
            Text = "Hello!",
            Wrapping = TextWrapping.Wrap,
            Anchors = ControlAnchors.AnchorFullHorizontal,
            Position = new(10, 50)
        };
        stackPanel.AddChild(new TextControl()
        {
            Text = "World!"
        });
        buttonTest = new TextButton()
        {
            Size = new Size(100, 20),
            Text = "Click me!",
        };
        buttonTest.OnLeftClicked += () =>
        {
            Logger.Debug("Button clicked!");
            buttonTest.Text = "Clicked!";
            text.Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.Lorem ipsum dolor sit amet, consectetur adipiscing elit.";
            simpleTestControl.Position = new Vector2(simpleTestControl.Position.X + 10, simpleTestControl.Position.Y + 10);
        }; 
        stackPanel.AddChild(buttonTest);
        
        testScroll = new ScrollContainer()
        {
            Position = new Vector2(300, 20),
            Size = new Size(200, 100)
        };
        simpleTestControl.AddChild(testScroll);
        testScroll.SetContent(text);
        
        _pixelTexture = new Texture2D(RenderTarget.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
    }
    
    private Color bgColor = Color.CornflowerBlue;
        ScrollContainer testScroll;
        TextControl text;
    
    
    public override void UpdateAvaloniaControl(IntPtr bitmapControlAddress)
    {
        _bitmapControlAddress = bitmapControlAddress;
    }

    public void RefreshUiCache()
    {
        bool needsRedraw = false;
        if (_UiCache == null)
        {
            _UiCache = new RenderTarget2D(_graphicsDevice, RenderTarget.Width, RenderTarget.Height);
            needsRedraw = true;
        }
        else
        {
            if (_UiCache.Width != RenderTarget.Width || _UiCache.Height != RenderTarget.Height)
            {
                _UiCache.Dispose();
                _UiCache = new RenderTarget2D(_graphicsDevice, RenderTarget.Width, RenderTarget.Height);
                needsRedraw = true;
            }
        }

        if (UiLayer.RootControl.IsDirty || needsRedraw)
        {
            _graphicsDevice.SetRenderTarget(_UiCache);
            _graphicsDevice.Clear(Color.Transparent);
            UiLayer.Draw();
            _graphicsDevice.SetRenderTarget(null);
        }
    }

    public void Draw(TimeSpan deltaTime)
    {
        RefreshUiCache();
        _graphicsDevice.SetRenderTarget(RenderTarget);
        _graphicsDevice.Clear(bgColor);
        
        _spriteBatch.Begin();
        
        _spriteBatch.Draw(_UiCache, Vector2.Zero, Color.White);

        if (_uiRenderer.CursorTexture == null)
        {
            _uiRenderer.CursorTexture = _pixelTexture;
        }
        _spriteBatch.Draw(_uiRenderer.CursorTexture, UiLayer._mousePosition, Color.White);
        
        _uiRenderer.DrawDebugBounds(_spriteBatch, text);
        // _uiRenderer.DrawDebugBounds(_spriteBatch, testScroll.ContentPresenter);
        // _uiRenderer.DrawDebugBounds(_spriteBatch, testScroll.ScrollBarThumb);
        // _uiRenderer.DrawDebugBounds(_spriteBatch, testScroll.ScrollBarBackground);
        // Logger.Debug(testScroll.ToString());
        
        _spriteBatch.End();
    }
    
    public void SetRenderer(UiRenderer renderer)
    {
        _uiRenderer = renderer;
        _uiRenderer.GraphicsDevice.ScissorRectangle = _spriteBatch.GraphicsDevice.Viewport.Bounds;
    }

    public void Update(TimeSpan deltaTime)
    {
        UiLayer.Update(deltaTime);
    }

    #region InternalMethods - DO NOT TOUCH
    protected override void UpdatingFrame(TimeSpan deltaTime)
    {
        Draw(deltaTime);
        if (DrawFrameByFrame) return;
        DoNewFrame();
    }

    protected override void UpdatingLoop(TimeSpan deltaTime)
    {
        Update(deltaTime);
    }

    protected override void AskForNewFrame()
    {
        if(RenderTarget == null) return;
        if(_bitmapControlAddress == null || !_bitmapControlAddress.HasValue) return;
        _inDrawing = true;

        var address = _bitmapControlAddress.Value;
        
        int totalPixels = (Size.Width * Size.Height);
        
        if (_internalBuffer == null || _internalBuffer.Length != totalPixels)
        {
            _internalBuffer = new uint[totalPixels];
        }
        
        RenderTarget.GetData(_internalBuffer);

        try
        {
            unsafe
            {
                fixed (uint* pSource = _internalBuffer)
                {
                    long bytesToCopy = (long)totalPixels * sizeof(uint);
                    Buffer.MemoryCopy(pSource, address.ToPointer(),
                        bytesToCopy,
                        bytesToCopy);

                }
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions that may occur during memory copy
            _logger.Error($"Error copying frame data: {ex.Message}");
        }
        finally
        {
            OnceUpdatedAction?.Invoke();
            _inDrawing = false;
        }
    }

    protected override void Disposing()
    {
        RenderTarget?.Dispose();
        _internalBuffer = null;
        _bitmapControlAddress = null;
    }
    #endregion
}