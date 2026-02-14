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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;
using RPGCreator.RTP.Extensions;
using RPGCreator.RTP.GameUI.BaseControls;
using RPGCreator.RTP.GameUI.BaseControls.Containers;
using RPGCreator.RTP.GameUI.Enums;

namespace RPGCreator.RTP.GameUI;

public class UiRenderer
{
    public readonly GraphicsDevice GraphicsDevice;
    public readonly SpriteBatch SpriteBatch;
    public readonly Texture2D PixelTexture;
    public readonly Texture2D DashTexture;
    public readonly FontSystem FontSystem;
    public readonly Matrix4x4 TransformMatrix;
    public readonly RasterizerState ClippingRasterizerState = new RasterizerState { ScissorTestEnable = true };
    public Texture2D? CursorTexture { get; set; }
    public Dictionary<Cursors, Texture2D> CursorTextures { get; } = new Dictionary<Cursors, Texture2D>();
    public Dictionary<string, Texture2D> CustomTextures { get; } = new Dictionary<string, Texture2D>();
    
    public UiRenderer(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, FontSystem fontSystem, Matrix4x4 transformMatrix)
    {
        GraphicsDevice = graphicsDevice;
        SpriteBatch = spriteBatch;
        
        PixelTexture = new Texture2D(graphicsDevice, 1, 1);
        PixelTexture.SetData([Color.White]);
        
        DashTexture = new Texture2D(GraphicsDevice, 8, 1);
        Color[] data = new Color[8];
        for (int i = 0; i < 4; i++) data[i] = Color.White;
        for (int i = 4; i < 8; i++) data[i] = Color.Transparent;
        DashTexture.SetData(data);
        
        FontSystem = fontSystem;
        TransformMatrix = transformMatrix;
    }
    
    public void DrawDebugBounds(SpriteBatch sb, BaseControl control)
    {
        if (!control.AbsoluteVisibility) return;

        var color = GetDebugColor(control);
        
        if(!control.ShouldDrawn)
            color = new Color(color.R*.8f, color.G*.8f, color.B*.8f, (0.5f));
        
        sb.DrawRectangle(control.GlobalsBounds, color, 1f);
        sb.DrawRectangle(
            control.GlobalsBounds with
            {
                Height = control.GlobalsBounds.Height - control.Padding.Height, 
                Width = control.GlobalsBounds.Width - control.Padding.Width
            }, Color.BlueViolet , 1f);
        
        sb.DrawCircle(control.GlobalsBounds.Location.ToVector2() + control.Origin, 2f, 8, color, 2f);
        
    }

    public Color GetDebugColor(BaseControl control)
    {
        var typeName = control.GetType().Name;
        var hash = typeName.GetHashCode();
        return new Color((hash & 0xFF0000) >> 16, (hash & 0x00FF00) >> 8, hash & 0x0000FF);
    }

    public void SetCursor(Texture2D texture)
    {
        CursorTexture = texture;
    }

    public void SetCursor(Cursors cursor)
    {
        if (CursorTextures.TryGetValue(cursor, out var texture))
        {
            CursorTexture = texture;
        }
    }
    
    public void RegisterCursor(Cursors cursor, Texture2D texture)
    {
        CursorTextures[cursor] = texture;
    }

    public bool TryGetCursorTexture(Cursors cursor, [NotNullWhen(true)] out Texture2D? texture)
    {
        return CursorTextures.TryGetValue(cursor, out texture);
    }
}