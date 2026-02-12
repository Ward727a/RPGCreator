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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.RTP.Extensions;
using RPGCreator.RTP.GameUI;
using RPGCreator.RTP.GameUI.BaseControls;
using RPGCreator.RTP.GameUI.BaseControls.Box;
using RPGCreator.RTP.GameUI.BaseControls.Containers;
using RPGCreator.RTP.GameUI.DefaultControls;
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
    private RenderCore _core;
    private UiRenderer _uiRenderer;

    private BaseLayer UiLayer;
    private ContainerControl simpleTestControl;
    private ContainerControl simpleChildControl;
    
    public GameViewport(RenderTarget2D renderTarget)
    {
        RenderTarget = renderTarget;
    }

    private static bool IsFormsInitialized = false;
    
    public void LoadContent(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, RenderCore core, UiRenderer uiRenderer)
    {
        _graphicsDevice = graphicsDevice;
        _spriteBatch = spriteBatch;
        _core = core;
        _uiRenderer = uiRenderer;
        
        UiLayer = new BaseLayer("MainUILayer", _graphicsDevice, _spriteBatch);
        UiLayer.SetParentViewport(this);
        
        simpleTestControl = new ContainerControl()
        {
            Position = new Vector2(50, 50),
            Size = new Size(200, 100),
            ClipsToBounds = true
        };
        UiLayer.SetRootControl(simpleTestControl);
        
        simpleChildControl = new ContainerControl()
        {
            Name = "ChildControl",
            Position = new Vector2(20, 20),
            Size = new Size(100, 50)
        };
        simpleChildControl.Origin = new Vector2(simpleChildControl.Width / 2, simpleChildControl.Height / 2);
        simpleTestControl.AddChild(simpleChildControl);

        var testTextControl = new TextControl()
        {
            Position = new Vector2(10, 10),
            Size = new Size(180, 80),
        };
        simpleTestControl.AddChild(testTextControl);
        testTextControl.Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

        var testSimpleColorBox2 = new SimpleColorBox()
        {
            BackgroundColor = Color.Beige,
            Anchors = ControlAnchors.AnchorFull,
        };
        simpleTestControl.AddChild(testSimpleColorBox2);
        
        var testSimpleColorBox = new TextButton()
        {
            Size = new Size(0, 20),
            Anchors = ControlAnchors.AnchorFullHorizontal,
        };
        simpleTestControl.AddChild(testSimpleColorBox);
        testSimpleColorBox.OnClicked += () =>
        {
            Logger.Debug("Button clicked!");
        };
        
        _pixelTexture = new Texture2D(RenderTarget.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
    }
    
    private Color bgColor = Color.CornflowerBlue;
    
    
    public override void UpdateAvaloniaControl(IntPtr bitmapControlAddress)
    {
        _bitmapControlAddress = bitmapControlAddress;
    }

    public void Draw(TimeSpan deltaTime)
    {
        var rs = new RasterizerState { ScissorTestEnable = true };
        _graphicsDevice.Clear(bgColor);
        UiLayer.Draw();
        
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