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
using Gum.Forms.Controls;
using Gum.Wireframe;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using RPGCreator.SDK.Editor.Rendering;
using RPGCreator.SDK.Logging;

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
    private GumService _gum;

    public GameViewport(RenderTarget2D renderTarget)
    {
        RenderTarget = renderTarget;
    }

    public void LoadContent(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, GumService gum, RenderCore core)
    {
        _graphicsDevice = graphicsDevice;
        _spriteBatch = spriteBatch;
        _core = core;
        _gum = gum;
        
        _pixelTexture = new Texture2D(RenderTarget.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
        var mainPanel = new Panel(_gum.Root);
        mainPanel.Width = GraphicalUiElement.CanvasWidth;
        mainPanel.Height = GraphicalUiElement.CanvasHeight;
            
        // Add text box to display when no map is selected.
        var _noMapSelectedText = new TextBox()
        {
            IsReadOnly = true,
            Text = "No map selected. Please select a map to edit.",
        };
        mainPanel.AddChild(_noMapSelectedText);
    }
    
    public override void UpdateAvaloniaControl(IntPtr bitmapControlAddress)
    {
        _bitmapControlAddress = bitmapControlAddress;
    }

    public void Draw(TimeSpan deltaTime)
    {
        _spriteBatch.Begin();
        
        _spriteBatch.Draw(_pixelTexture, new Rectangle(10, 10, 100, 100), Color.White);

        _spriteBatch.End();
    }

    public void Update(TimeSpan deltaTime)
    {
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
        } catch (Exception ex)
        {
            // Handle exceptions that may occur during memory copy
            _logger.Error($"Error copying frame data: {ex.Message}");
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