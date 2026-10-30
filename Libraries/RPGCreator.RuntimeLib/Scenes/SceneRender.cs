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

using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.SDK.Scenes;
using RPGCreator.Shared.Attributes;
using SpriteEffects = Microsoft.Xna.Framework.Graphics.SpriteEffects;

namespace RPGCreator.RuntimeLib.Scenes;

[EngineService(typeof(ISceneRender), ServiceLifetime.Transient)]
public class SceneRender : ISceneRender
{
    
    private int _renderItemsCount = 0;

    private RenderItem[] _renderItemsSpan = new RenderItem[512];
    private readonly SpriteBatch _spriteBatch;
    private readonly Texture2D _pixel;
    
    public SceneRender(SpriteBatch spriteBatch)
    {
        _spriteBatch = spriteBatch;
        _pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
        _pixel.Name = "WhitePixelSceneRender";
    }

    public void SubmitSprite(Texture2D texture, Vector2 position, Rectangle sourceRect, Color color, float sortY, int zIndex = 0, SpriteEffects spriteEffects = SpriteEffects.None)
    {
        EnsureCapacity();
        _renderItemsSpan[_renderItemsCount++] = new RenderItem
        {
            Type = RenderCommandType.Sprite,
            Asset = texture,
            Position = position,
            SourceRect = sourceRect,
            Color = color,
            SortY = sortY,
            ZIndex = zIndex,
            SpriteEffect = spriteEffects
        };
    }

    public void SubmitText(SpriteFont spriteFont, string text, Vector2 position, Color color, float sortY)
    {
        EnsureCapacity();
        _renderItemsSpan[_renderItemsCount++] = new RenderItem
        {
            Type = RenderCommandType.Text,
            Asset = spriteFont,
            TextPayload = text,
            Position = position,
            Color = color,
            SortY = sortY
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureCapacity()
    {
        if (_renderItemsCount >= _renderItemsSpan.Length)
        {
            Array.Resize(ref _renderItemsSpan, _renderItemsSpan.Length * 2);
        }
    }

    public void Render(in Matrix viewMatrix, in Rectangle viewBounds, double deltaTime)
    {
        if (_renderItemsCount == 0) return;

        var visibleItems = _renderItemsSpan.AsSpan(0, _renderItemsCount);

        visibleItems.Sort();

        _spriteBatch.Begin(
            sortMode: SpriteSortMode.Deferred,
            blendState: null,
            samplerState: null,
            depthStencilState: null,
            rasterizerState: null,
            effect: null,
            transformMatrix: viewMatrix
        );

        foreach (ref readonly var item in visibleItems)
        {
            if (item.Type == RenderCommandType.Sprite)
            {
                var texture = Unsafe.As<Texture2D>(item.Asset);
                _spriteBatch.Draw(texture, item.Position, item.SourceRect, item.Color, 0f, Vector2.Zero,
                    Vector2.One, item.SpriteEffect, 0f);
            }
            else if (item.Type == RenderCommandType.Text)
            {
                var font = Unsafe.As<SpriteFont>(item.Asset);
                _spriteBatch.DrawString(font, item.TextPayload, item.Position, item.Color);
            }
        }

        _spriteBatch.End();

        _renderItemsCount = 0;
    }
    
    private readonly BlendState _invertBlendState = new()
    {
        ColorBlendFunction = BlendFunction.Add,
        ColorSourceBlend = Blend.InverseDestinationColor,
        ColorDestinationBlend = Blend.Zero,
        AlphaBlendFunction = BlendFunction.Add,
        AlphaSourceBlend = Blend.One,
        AlphaDestinationBlend = Blend.Zero
    };

    public void RenderGrid(in Matrix viewMatrix, in Rectangle viewBounds, Point mapGridSize, int tileSize = 32)
    {
        int startX = Math.Max(0, (viewBounds.Left / tileSize) * tileSize);
        int endX = Math.Min(mapGridSize.X * tileSize, ((viewBounds.Right / tileSize) + 1) * tileSize);
    
        int startY = Math.Max(0, (viewBounds.Top / tileSize) * tileSize);
        int endY = Math.Min(mapGridSize.Y * tileSize, ((viewBounds.Bottom / tileSize) + 1) * tileSize);

        if (startX > endX || startY > endY) return;

        _spriteBatch.Begin(
            sortMode: SpriteSortMode.Deferred,
            blendState: _invertBlendState,
            samplerState: SamplerState.PointClamp,
            depthStencilState: null,
            rasterizerState: null,
            effect: null,
            transformMatrix: viewMatrix
        );

        for (int x = startX; x <= endX; x += tileSize)
        {
            _spriteBatch.Draw(_pixel, new Rectangle(x, startY, 1, endY - startY), Color.White);
        }

        for (int y = startY; y <= endY; y += tileSize)
        {
            _spriteBatch.Draw(_pixel, new Rectangle(startX, y, endX - startX, 1), Color.White);
        }

        _spriteBatch.End();
    }
}

public enum RenderCommandType : byte
{
    Sprite,
    Text
}

public struct RenderItem : IComparable<RenderItem>
{
    public RenderCommandType Type;
    
    public int ZIndex; 
    public float SortY; 
    
    public Vector2 Position;
    public Color Color;
    public object Asset; 
    public Rectangle SourceRect;
    public string TextPayload;
    public SpriteEffects SpriteEffect;

    public int CompareTo(RenderItem other)
    {
        int zComparison = ZIndex.CompareTo(other.ZIndex);
        
        if (zComparison != 0)
            return zComparison;

        return SortY.CompareTo(other.SortY);
    }
}