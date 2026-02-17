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
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using RPGCreator.RTP.Extensions;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers.EntityLayer;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.RuntimeService;
using RPGCreator.SDK.Types;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Size = RPGCreator.SDK.Types.Size;

namespace RPGCreator.RTP.Services;

public class RenderService : IRenderService
{
    
    private readonly GraphicsDevice _graphicsDevice;
    private readonly SpriteBatch spriteBatch;
    private Size _cellSize = new(32f, 32f);
    private Vector2 _cellSizeAsVector = new(32f, 32f);
    
    public RenderService(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
    {
        _graphicsDevice = graphicsDevice;
        this.spriteBatch = spriteBatch;
        
        RuntimeServices.MapService.OnMapLoaded += (_) =>
        {
            var loadedMapData = RuntimeServices.MapService.CurrentLoadedMapData;
            _cellSize = new(loadedMapData.CellWidth, loadedMapData.CellHeight);
            _cellSizeAsVector = _cellSize;
        };
    }

    public void DrawEntitySpawner(EntitySpawner entityDef, Vector2 position)
    {
        var texture = EngineServices.ResourcesService.Load<Texture2D>(entityDef.PreviewImagePath);

        
        float scaleX = (float)32 / texture?.Height ?? 32;
        float scaleY = (float)32 / texture?.Width ?? 32;

        spriteBatch.Draw(
            texture,
            position.ToXnaFast(),
            null,
            Color.White.ToMgColor(),
            0f,
            Vector2.Zero,
            new Microsoft.Xna.Framework.Vector2(scaleX, scaleY),
            SpriteEffects.None,
            0.4f
        );
    }

    public int GetStackSize()
    {
        throw new NotImplementedException();
    }

    public void SubmitToQueue(RenderCommand command)
    {
        throw new NotImplementedException();
    }

    public void DrawSortedQueue()
    {
        throw new NotImplementedException();
    }

    public void DrawTile(ITileDef tileDef, Vector2 tilePositionInChunk)
    {
        var texture = GetTilesetTexture(tileDef.TilesetDef);
        
        Rectangle sourceRect;
        if (tileDef.Tags.TryGet<Rectangle>(out var rect))
        {
            sourceRect = rect;
        }
        else
        {
            sourceRect = new Rectangle(
                (int)tileDef.PositionInTileset.X,
                (int)tileDef.PositionInTileset.Y,
                (int)tileDef.SizeInTileset.Width,
                (int)tileDef.SizeInTileset.Height
            );
            tileDef.Tags.Set(sourceRect);
        }
        spriteBatch.Draw(
            texture,
            tilePositionInChunk.ToXnaFast(),
            sourceRect,
            Microsoft.Xna.Framework.Color.White,
            0f,
            Vector2.Zero,
            1f,
            SpriteEffects.None,
            0.5f
        );
    }

    public void DrawTileInstance(ITileInstance tileInstance)
    {
        
    }

    public void DrawDebugRect(Vector2 worldPos, Size size, Color? color = null, float thickness = 2f)
    {
        var camera = RuntimeServices.CameraService;
        var adjustedThickness = thickness / camera.ZoomLevel;
        
        var w = (int)(size.Width);
        var h = (int)(size.Height);

        var finalColor = color??Color.Violet;
        var xnaColor = finalColor.ToMgColor();

        spriteBatch.DrawRectangle(
            new Rectangle(
                (int)worldPos.X,
                (int)worldPos.Y,
                w,
                h
            ),
            xnaColor,
            adjustedThickness
        );
    }

    public void DrawDebugLine(Vector2 startPos, Vector2 endPos, float thickness = 1, Color? color = null)
    {
        
        var camera = RuntimeServices.CameraService;
        var adjustedThickness = thickness / camera.ZoomLevel;
        
        var finalColor = color??Color.Red;
        var xnaColor = finalColor.ToMgColor();
        
        spriteBatch.DrawLine(
            startPos.ToXnaFast(),
            endPos.ToXnaFast(),
            xnaColor,
            adjustedThickness
        );
    }

    public void DrawDebugPoint(Vector2 position, Color? color = null, float size = 4, float thickness = 2f)
    {
        var camera = RuntimeServices.CameraService;
        var adjustedSize = size / camera.ZoomLevel;
        var adjustedThickness = thickness / camera.ZoomLevel;
        
        var finalColor = color??Color.Yellow;
        var xnaColor = finalColor.ToMgColor();
        
        spriteBatch.DrawCircle(
            position.ToXnaFast(),
            adjustedSize / 2,
            6,
            xnaColor,
            adjustedThickness
        );
    }

    public record struct DrawingState
    {
        public SpriteSortMode SortMode;
        public BlendState BlendState;
        public SamplerState SamplerState;
        public DepthStencilState DepthStencilState;
        public RasterizerState RasterizerState;
        public Microsoft.Xna.Framework.Matrix? TransformMatrix;
    }
    
    public Stack<DrawingState> DrawingStateStack { get; } = new Stack<DrawingState>();
    public DrawingState CurrentDrawingState { get; private set; }
    
    public void PrepareDrawing(IRenderService.SpriteSortMode sortMode = IRenderService.SpriteSortMode.BackToFront)
    {
        SpriteSortMode trueSortMode;
            
        switch (sortMode)
        {
            case IRenderService.SpriteSortMode.BackToFront:
                trueSortMode = SpriteSortMode.BackToFront;
                break;
            case IRenderService.SpriteSortMode.Deferred:
                trueSortMode = SpriteSortMode.Deferred;
                break;
            case IRenderService.SpriteSortMode.FrontToBack:
                trueSortMode = SpriteSortMode.FrontToBack;
                break;
            case IRenderService.SpriteSortMode.Immediate:
                trueSortMode = SpriteSortMode.Immediate;
                break;
            case IRenderService.SpriteSortMode.Texture:
                trueSortMode = SpriteSortMode.Texture;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(sortMode), sortMode, null);
        }
        
        CurrentDrawingState = new DrawingState
        {
            SortMode = trueSortMode,
            BlendState = BlendState.AlphaBlend,
            SamplerState = SamplerState.PointClamp,
            DepthStencilState = DepthStencilState.Default,
            RasterizerState = RasterizerState.CullNone,
            TransformMatrix = RuntimeServices.CameraService.GetViewMatrix().ToXnaFast()
        };
        
        spriteBatch.Begin(
            sortMode: trueSortMode,
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp,
            depthStencilState: DepthStencilState.Default,
            rasterizerState: RasterizerState.CullNone,
            transformMatrix: RuntimeServices.CameraService.GetViewMatrix().ToXnaFast()
        );
    }

    public void PauseDrawing()
    {
        DrawingStateStack.Push(CurrentDrawingState);
        spriteBatch.End();
    }
    
    public void ResumeDrawing()
    {
        if (DrawingStateStack.Count == 0)
            return;

        var previousState = DrawingStateStack.Pop();
        CurrentDrawingState = previousState;
        
        spriteBatch.Begin(
            sortMode: previousState.SortMode,
            blendState: previousState.BlendState,
            samplerState: previousState.SamplerState,
            depthStencilState: previousState.DepthStencilState,
            rasterizerState: previousState.RasterizerState,
            transformMatrix: RuntimeServices.CameraService.GetViewMatrix().ToXnaFast()
        );
    }
    
    public void DirectDraw(string texturePath, Vector2 position, System.Drawing.Rectangle? sourceRect = null, Color? tint = null, float rotation = 0, Vector2 origin = default,
        Vector2? scale = null, float layerDepth = 0, SDK.ECS.Components.SpriteEffects effects = SDK.ECS.Components.SpriteEffects.None)
    {
        var texture = EngineServices.ResourcesService.Load<Texture2D>(texturePath);
        var xnaColor = (tint ?? Color.White).ToMgColor();
        var xnaEffects = SpriteEffects.None;
        if (effects.HasFlag(SDK.ECS.Components.SpriteEffects.FlipHorizontally))
            xnaEffects |= SpriteEffects.FlipHorizontally;
        if (effects.HasFlag(SDK.ECS.Components.SpriteEffects.FlipVertically))
            xnaEffects |= SpriteEffects.FlipVertically;
        // Si scale est null, on utilise Vector2.One (1,1)
        var finalScale = scale?.ToXnaFast() ?? Microsoft.Xna.Framework.Vector2.One;
        
        var finalSourceRect = sourceRect?.ToXnaFast() ?? null;
        
        spriteBatch.Draw(
            texture,
            position.ToXnaFast(),
            finalSourceRect,
            xnaColor,
            rotation,
            origin.ToXnaFast(),
            finalScale,
            xnaEffects,
            layerDepth
        );
    }

    public void FinishDrawing()
    {
        spriteBatch.End();
    }

    public System.Drawing.Rectangle GetTileSourceRect(ITileDef tileDef)
    {
        throw new NotImplementedException();
    }
    //
    // public void DrawDebugString(string text, Vector2 position, Color? color = null, float scale = 1f)
    // {
    //     var finalColor = color ?? Color.White;
    //     var xnaColor = finalColor.ToXnaFast();
    //
    //     var font = RuntimeServices.FontService.DefaultFont;
    //     spriteBatch.DrawString(
    //         font,
    //         text,
    //         position.ToXnaFast(),
    //         xnaColor,
    //         0f,
    //         Vector2.Zero,
    //         scale,
    //         SpriteEffects.None,
    //         0f
    //     );
    // }


    #region Helpers
    
    private BaseTilesetDef? _lastTilesetDef;
    private Texture2D? _lastTilesetTexture;

    private Texture2D GetTilesetTexture(BaseTilesetDef tilesetDef)
    {
        if(_lastTilesetDef == tilesetDef && _lastTilesetTexture != null)
        {
            return _lastTilesetTexture;
        }

        if (tilesetDef.Tags.TryGet<Texture2D>(out var texture))
        {
            _lastTilesetDef = tilesetDef;
            _lastTilesetTexture = texture;
            return texture;
        }

        texture = EngineServices.ResourcesService.Load<Texture2D>(tilesetDef.ImagePath);

        tilesetDef.Tags.Set(texture ?? throw new FileNotFoundException($"Tileset texture could not be loaded from path: {tilesetDef.ImagePath}"));
        _lastTilesetDef = tilesetDef;
        _lastTilesetTexture = texture;
        return texture;
    }
    
    #endregion
}