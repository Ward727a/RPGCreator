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
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using RPGCreator.RuntimeLib.MethodsExtension;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.EntitySpawner;
using RPGCreator.SDK.Assets.Tile;
using RPGCreator.SDK.Assets.Tilesets;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Common.MethodExtensions;
using RPGCreator.SDK.Editor;
using RPGCreator.SDK.Mediator.Messages;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Services.EngineService;
using RPGCreator.SDK.Services.RuntimeService;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Size = RPGCreator.Shared.Types.Size;

namespace RPGCreator.RTP.Services;

public class RenderService : IRenderService
{
    
    private float _globalOpacity = 1f;
    
    private readonly SpriteBatch _spriteBatch;

    private readonly IResourceService _resourceService;
    private readonly IAssetsManager _assetsManager;
    private readonly ICameraService _cameraService;
    private readonly IPathResolver _pathResolver;
    
    public RenderService(SpriteBatch spriteBatch, IPathResolver pathResolver, IResourceService resourceService, IAssetsManager assetsManager, ICameraService cameraService)
    {
        _spriteBatch = spriteBatch;
        _pathResolver = pathResolver;
        _resourceService = resourceService;
        _assetsManager = assetsManager;
        _cameraService = cameraService;
    }

    public void DrawEntitySpawner(EntitySpawner entitySpawner, Vector2 position)
    {
        var entityDef = entitySpawner.EntityDefinition.Get(_assetsManager).Value;
        var texture = _resourceService.Load<Texture2D>(_pathResolver.Resolve(entityDef.IconReference.Get(_assetsManager).Value.SourcePath));

        
        float scaleX = (float)32 / texture?.Height ?? 32;
        float scaleY = (float)32 / texture?.Width ?? 32;

        _spriteBatch.Draw(
            texture,
            position.ToXnaFast(),
            null,
            Color.White.ToMgColor() * _globalOpacity,
            0f,
            Vector2.Zero,
            new Microsoft.Xna.Framework.Vector2(scaleX, scaleY),
            SpriteEffects.None,
            0.4f
        );
    }

    public IPaintTarget? CurrentPreviewTarget { get; set; }

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

    public void DrawTile(SimpleTile tileDef, Vector2 tilePositionInChunk)
    {
        if (tileDef.FromInstance != null)
            _spriteBatch.Draw(
                _resourceService.Load<Texture2D>(
                    _pathResolver.Resolve(tileDef.FromInstance.Definition.ImageReference.Get(_assetsManager).Value.SourcePath)),
                tilePositionInChunk.ToXnaFast(),
                tileDef.Bounds.ToMgRectFast(),
                Microsoft.Xna.Framework.Color.White * _globalOpacity,
                0f,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                0.5f
            );
        else
        {
            Logger.Error("Error while drawing tile: TileDefinition is null. This tile will not be rendered. Position: {position}", tilePositionInChunk);
        }
    }

    public void DrawDebugRect(Vector2 worldPos, Size size, Color? color = null, float thickness = 2f)
    {
        var camera = _cameraService;
        var adjustedThickness = thickness / camera.ZoomLevel;
        
        var w = (int)(size.Width);
        var h = (int)(size.Height);

        var finalColor = color??Color.Violet;
        var xnaColor = finalColor.ToMgColor();

        _spriteBatch.DrawRectangle(
            new Rectangle(
                (int)worldPos.X,
                (int)worldPos.Y,
                w,
                h
            ),
            xnaColor * _globalOpacity,
            adjustedThickness
        );
    }

    public void DrawDebugLine(Vector2 startPos, Vector2 endPos, float thickness = 1, Color? color = null)
    {
        
        var camera = _cameraService;
        var adjustedThickness = thickness / camera.ZoomLevel;
        
        var finalColor = color??Color.Red;
        var xnaColor = finalColor.ToMgColor();
        
        _spriteBatch.DrawLine(
            startPos.ToXnaFast(),
            endPos.ToXnaFast(),
            xnaColor * _globalOpacity,
            adjustedThickness
        );
    }

    public void DrawDebugPoint(Vector2 position, Color? color = null, float size = 4, float thickness = 2f)
    {
        var camera = _cameraService;
        var adjustedSize = size / camera.ZoomLevel;
        var adjustedThickness = thickness / camera.ZoomLevel;
        
        var finalColor = color??Color.Yellow;
        var xnaColor = finalColor.ToMgColor();
        
        _spriteBatch.DrawCircle(
            position.ToXnaFast(),
            adjustedSize / 2,
            6,
            xnaColor * _globalOpacity,
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
            TransformMatrix = _cameraService.GetViewMatrix().ToXnaFast()
        };
        
        _spriteBatch.Begin(
            sortMode: trueSortMode,
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp,
            depthStencilState: DepthStencilState.Default,
            rasterizerState: RasterizerState.CullNone,
            transformMatrix: _cameraService.GetViewMatrix().ToXnaFast()
        );
    }

    public void PauseDrawing()
    {
        DrawingStateStack.Push(CurrentDrawingState);
        _spriteBatch.End();
    }
    
    public void ResumeDrawing()
    {
        if (DrawingStateStack.Count == 0)
            return;

        var previousState = DrawingStateStack.Pop();
        CurrentDrawingState = previousState;
        
        _spriteBatch.Begin(
            sortMode: previousState.SortMode,
            blendState: previousState.BlendState,
            samplerState: previousState.SamplerState,
            depthStencilState: previousState.DepthStencilState,
            rasterizerState: previousState.RasterizerState,
            transformMatrix: _cameraService.GetViewMatrix().ToXnaFast()
        );
    }

    public void DirectDraw(string texturePath, Vector2 position, Rect? sourceRect = null, Color? tint = null, float rotation = 0, Vector2 origin = default,
        Vector2? scale = null, float layerDepth = 0, SDK.ECS.Components.SpriteEffects effects = SDK.ECS.Components.SpriteEffects.None)
    {
        var texture = _resourceService.Load<Texture2D>(texturePath);
        var xnaColor = (tint ?? Color.White).ToMgColor();
        var xnaEffects = SpriteEffects.None;
        if (effects.HasFlag(SDK.ECS.Components.SpriteEffects.FlipHorizontally))
            xnaEffects |= SpriteEffects.FlipHorizontally;
        if (effects.HasFlag(SDK.ECS.Components.SpriteEffects.FlipVertically))
            xnaEffects |= SpriteEffects.FlipVertically;
        
        var scaleValue = scale ?? Vector2.One;
        var finalScale = scaleValue.ToXnaFast();

        Rectangle? finalSourceRect = null;

        if (sourceRect.HasValue)
        {
            var rectValue = sourceRect.Value;
            finalSourceRect = rectValue.ToMGRect();
        }
        
        _spriteBatch.Draw(
            texture,
            position.ToXnaFast(),
            finalSourceRect,
            xnaColor * _globalOpacity,
            rotation,
            origin.ToXnaFast(),
            finalScale,
            xnaEffects,
            layerDepth
        );
    }

    public void DirectDraw(object textureObject, Vector2 position, Rect? sourceRect = null, Color? tint = null, float rotation = 0,
        Vector2 origin = default, Vector2? scale = null, float layerDepth = 0, SDK.ECS.Components.SpriteEffects effects = SDK.ECS.Components.SpriteEffects.None)
    {
        var texture = (Texture2D)textureObject;
        var xnaColor = (tint ?? Color.White).ToMgColor();
        var xnaEffects = SpriteEffects.None;
        if (effects.HasFlag(SDK.ECS.Components.SpriteEffects.FlipHorizontally))
            xnaEffects |= SpriteEffects.FlipHorizontally;
        if (effects.HasFlag(SDK.ECS.Components.SpriteEffects.FlipVertically))
            xnaEffects |= SpriteEffects.FlipVertically;
        
        var scaleValue = scale ?? Vector2.One;
        var finalScale = scaleValue.ToXnaFast();

        Rectangle? finalSourceRect = null;

        if (sourceRect.HasValue)
        {
            var rectValue = sourceRect.Value;
            finalSourceRect = rectValue.ToMGRect();
        }

        _spriteBatch.Draw(
            texture,
            position.ToXnaFast(),
            finalSourceRect,
            xnaColor * _globalOpacity,
            rotation,
            origin.ToXnaFast(),
            finalScale,
            xnaEffects,
            layerDepth
        );
    }

    public void FinishDrawing()
    {
        _spriteBatch.End();
    }

    public void SetGlobalOpacity(float opacity)
    {
        _globalOpacity = Math.Clamp(opacity, 0f, 1f);
    }

    public Rect GetTileSourceRect(ITileDef tileDef)
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
}