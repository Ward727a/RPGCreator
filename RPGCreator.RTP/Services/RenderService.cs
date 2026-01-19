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
using System.Drawing;
using System.IO;
using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using RenderingLibrary.Graphics;
using RPGCreator.Core.Types.Map.Chunks;
using RPGCreator.RTP.Extensions;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.RuntimeService;
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

    public void DrawTile(ITileDef tileDef, Vector2 worldPosition)
    {
        var camera = RuntimeServices.CameraService;
        var zoom = camera.ZoomLevel;

        var worldPixels = worldPosition * _cellSizeAsVector;
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
            worldPixels.ToXnaFast(),
            sourceRect,
            Microsoft.Xna.Framework.Color.White,
            0f,             // Rotation
            Vector2.Zero,   // Origin
            zoom,           // Scale (Zoom)
            SpriteEffects.None,
            0f              // LayerDepth
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

        var finalColor = color??Color.BlueViolet;
        var xnaColor = finalColor.ToXnaFast();

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
        var xnaColor = finalColor.ToXnaFast();
        
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
        
        var finalColor = color??Color.GreenYellow;
        var xnaColor = finalColor.ToXnaFast();
        
        spriteBatch.DrawCircle(
            position.ToXnaFast(),
            adjustedSize / 2,
            6,
            xnaColor,
            adjustedThickness
        );
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