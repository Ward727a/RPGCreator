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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FontStashSharp;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.RTP.Extensions;
using RPGCreator.RTP.GameUI.Interface;
using RPGCreator.SDK.Types;
using Color = RPGCreator.SDK.Types.Color;
using Matrix3x2 = System.Numerics.Matrix3x2;
using Vector2 = System.Numerics.Vector2;


namespace RPGCreator.RTP.GameUI;

public enum DrawCommandType
{
    SetClip,
    UnsetClip,
    SetTransform,
    UnsetTransform,
    DrawRectangle,
    DrawLine,
    DrawCircle,
    DrawTexture,
    DrawText,
    DrawTextWithString,
    DrawSprite,
    DrawSpriteWithIndex,
    DrawNineSlice,
    DrawThreeSlice
}

/// <summary>
/// This struct represents a single draw command that will be stored in the UiRendererContext and executed later in the rendering phase.<br/>
/// This is a low-level struct that is used to store the draw commands in a compact way, using explicit layout to save memory and improve performance.<br/>
/// I don't encourage you to edit this struct if you don't know exactly what you're doing, as it can easily lead to memory corruption and crashes if not used correctly.
/// </summary>
// This is working for now as I don't need to store more complex, but once I need to, I might need to switch to another approach.
// But well, for now this is working and it's very efficient
[StructLayout(LayoutKind.Explicit)]
internal readonly struct DrawCommand(
        DrawCommandType type,
        Matrix3x2 matrixData = default,
        bool boolData = default,
        Vector2 vectorData3 = default,
        Rect rectData = default,
        NineSliceInfo nineSliceData = default,
        ThreeSliceInfo threeSliceData = default,
        object? objectData = null,
        object? additionalData = null)
{
    [FieldOffset(0)] public readonly DrawCommandType Type = type;
    [FieldOffset(4)] public readonly Matrix3x2 MatrixData = matrixData; // The matrixData is used to store a matrix, but also to store other data such as colors, positions, sizes, etc. depending on the type of the command.
    [FieldOffset(28)] public readonly bool BoolData = boolData;
    
    [FieldOffset(32)] public readonly Vector2 VectorData3 = vectorData3;

    [FieldOffset(40)] public readonly Rect RectData = rectData;
    [FieldOffset(40)] public readonly NineSliceInfo NineSliceData = nineSliceData;
    [FieldOffset(40)] public readonly ThreeSliceInfo ThreeSliceData = threeSliceData;

    [FieldOffset(64)] public readonly object? ObjectData = objectData;
    [FieldOffset(72)] public readonly object? AdditionalData = additionalData;
}

public class UiRendererContext(SpriteBatch spriteBatch) : IMgUiRendererContext
{
    public SpriteBatch SpriteBatch { get; init; } = spriteBatch;

    private readonly List<DrawCommand> _drawCommands = new();
    private readonly Stack<Rect> _clipStack = new();
    private readonly Stack<Matrix3x2> _transformStack = new();

    private readonly Stack<Rect?> _execClipStack = new();
    private readonly Stack<Matrix3x2> _execTransformStack = new();
    
    private Rect? _currentClip;
    private Matrix3x2 _currentTransform = Matrix3x2.Identity;
    
    private readonly RasterizerState _clippingRasterizerState = new RasterizerState { ScissorTestEnable = true };
    private readonly RasterizerState _defaultRasterizerState = new RasterizerState { ScissorTestEnable = false };
    
    #if DEBUG
    private bool _isSpriteBatchBeginActive = false;
    #endif
    
    #region MemoryHelpers

    internal Color MatrixDataToColor(float data)
    {
        return Unsafe.As<float, Color>(ref data);
    }
    
    internal float ColorToMatrixData(Color color)
    {
        return Unsafe.As<Color, float>(ref color);
    }
    
    internal int MatrixDataToInt(float data)
    {
        return Unsafe.As<float, int>(ref data);
    }
    
    internal float IntToMatrixData(int value)
    {
        return Unsafe.As<int, float>(ref value);
    }
    
    #region DrawRectangle
    // Position = (M11 M12)
    // Size = (M21 M22)
    // Color = (M31)
    // thickness = (M32)
    internal (Vector2 position, Vector2 size, Color color, float thickness) GetRectangleData(DrawCommand command)
    {
        return (
            new Vector2(command.MatrixData.M11, command.MatrixData.M12), 
            new Vector2(command.MatrixData.M21, command.MatrixData.M22),
            MatrixDataToColor(command.MatrixData.M31),
            command.MatrixData.M32);
    }
    
    internal Matrix3x2 PutRectangleData(Vector2 position, Vector2 size, Color color, float thickness)
    {
        Matrix3x2 matrix = new Matrix3x2
        {
            M11 = position.X,
            M12 = position.Y,
            M21 = size.X,
            M22 = size.Y,
            M31 = ColorToMatrixData(color),
            M32 = thickness
        };
        return matrix;
    }
    #endregion
    
    #region DrawLine
    // Start = (M11 M12)
    // End = (M21 M22)
    // Color = (M31)
    // thickness = (M32)
    internal (Vector2 start, Vector2 end, Color color, float thickness) GetLineData(DrawCommand command)
    {
        return (
            new Vector2(command.MatrixData.M11, command.MatrixData.M12), 
            new Vector2(command.MatrixData.M21, command.MatrixData.M22),
            MatrixDataToColor(command.MatrixData.M31),
            command.MatrixData.M32);
    }
    
    internal Matrix3x2 PutLineData(Vector2 start, Vector2 end, Color color, float thickness)
    {
        Matrix3x2 matrix = new Matrix3x2
        {
            M11 = start.X,
            M12 = start.Y,
            M21 = end.X,
            M22 = end.Y,
            M31 = ColorToMatrixData(color),
            M32 = thickness
        };
        return matrix;
    }
    
    #endregion
    
    #region DrawCircle
    // center = (M11 M12)
    // radius = (M21)
    // color = (M22)
    // thickness = (M31)
    // segments = (M32)
    internal (Vector2 center, float radius, Color color, float thickness, int segments) GetCircleData(DrawCommand command)
    {
        return (
            new Vector2(command.MatrixData.M11, command.MatrixData.M12), 
            command.MatrixData.M21,
            MatrixDataToColor(command.MatrixData.M22),
            command.MatrixData.M31,
            MatrixDataToInt(command.MatrixData.M32));
    }
    
    internal Matrix3x2 PutCircleData(Vector2 center, float radius, Color color, float thickness, int segments)
    {
        Matrix3x2 matrix = new Matrix3x2
        {
            M11 = center.X,
            M12 = center.Y,
            M21 = radius,
            M22 = ColorToMatrixData(color),
            M31 = thickness,
            M32 = IntToMatrixData(segments)
        };
        return matrix;
    }
    #endregion
    
    #region DrawTexture
    // Position = (M11 M12)
    // Size = (M21 M22)
    // Color = (M31)
    internal (Vector2 position, Vector2 size, Color color) GetTextureData(DrawCommand command)
    {
        return (
            new Vector2(command.MatrixData.M11, command.MatrixData.M12),
            new Vector2(command.MatrixData.M21, command.MatrixData.M22),
            MatrixDataToColor(command.MatrixData.M31));
    }
    
    internal Matrix3x2 PutTextureData(Vector2 position, Vector2 size, Color color)
    {
        Matrix3x2 matrix = new Matrix3x2
        {
            M11 = position.X,
            M12 = position.Y,
            M21 = size.X,
            M22 = size.Y,
            M31 = ColorToMatrixData(color)
        };
        return matrix;
    }
    #endregion
    
    #region DrawTextWithRichTextLayout
    // Position = (M11 M12)
    // Color = (M21)
    internal (Vector2 position, Color color) GetTextDataWithRichTextLayout(DrawCommand command)
    {
        return (
            new Vector2(command.MatrixData.M11, command.MatrixData.M12),
            MatrixDataToColor(command.MatrixData.M21));
    }
    
    internal Matrix3x2 PutTextDataWithRichTextLayout(Vector2 position, Color color)
    {
        Matrix3x2 matrix = new Matrix3x2
        {
            M11 = position.X,
            M12 = position.Y,
            M21 = ColorToMatrixData(color)
        };
        return matrix;
    }
    #endregion
    
    #region DrawTextWithString
    // Position = (M11 M12)
    // Color = (M21)
    // FontSize = (M31)
    internal (Vector2 position, Color color, int fontSize) GetTextDataWithString(DrawCommand command)
    {
        return (
            new Vector2(command.MatrixData.M11, command.MatrixData.M12),
            MatrixDataToColor(command.MatrixData.M21),
            MatrixDataToInt(command.MatrixData.M31));
    }
    
    internal Matrix3x2 PutTextDataWithString(Vector2 position, Color color, int fontSize)
    {
        Matrix3x2 matrix = new Matrix3x2
        {
            M11 = position.X,
            M12 = position.Y,
            M21 = ColorToMatrixData(color),
            M31 = IntToMatrixData(fontSize)
        };
        return matrix;
    }
    #endregion
    
    #region DrawSpriteWithIndex
    // Position = (M11 M12)
    // Size = (M21 M22)
    // SpriteIndex = (M31)
    // Color = (M32)
    internal (Vector2 position, Vector2 size, int spriteIndex, Color color) GetSpriteDataWithIndex(DrawCommand command)
    {
        return (
            new Vector2(command.MatrixData.M11, command.MatrixData.M12),
            new Vector2(command.MatrixData.M21, command.MatrixData.M22),
            MatrixDataToInt(command.MatrixData.M31),
            MatrixDataToColor(command.MatrixData.M32));
    }
    
    internal Matrix3x2 PutSpriteDataWithIndex(Vector2 position, Vector2 size, int spriteIndex, Color color)
    {
        Matrix3x2 matrix = new Matrix3x2
        {
            M11 = position.X,
            M12 = position.Y,
            M21 = size.X,
            M22 = size.Y,
            M31 = IntToMatrixData(spriteIndex),
            M32 = ColorToMatrixData(color)
        };
        return matrix;
    }
    
    #endregion
    
    #region DrawSprite
    
    // Position = (M11 M12)
    // Size = (M21 M22)
    // Color = (M31)
    internal (Vector2 position, Vector2 size, Color color) GetSpriteData(DrawCommand command)
    {
        return (
            new Vector2(command.MatrixData.M11, command.MatrixData.M12),
            new Vector2(command.MatrixData.M21, command.MatrixData.M22),
            MatrixDataToColor(command.MatrixData.M31));
    }
    
    internal Matrix3x2 PutSpriteData(Vector2 position, Vector2 size, Color color)
    {
        Matrix3x2 matrix = new Matrix3x2
        {
            M11 = position.X,
            M12 = position.Y,
            M21 = size.X,
            M22 = size.Y,
            M31 = ColorToMatrixData(color)
        };
        return matrix;
    }
    
    #endregion
    
    #region DrawNineSlice
    // Position = (M11 M12)
    // Size = (M21 M22)
    // Color = (M31)
    internal (Vector2 position, Vector2 size, Color color) GetNineSliceData(DrawCommand command)
    {
        return (
            new Vector2(command.MatrixData.M11, command.MatrixData.M12),
            new Vector2(command.MatrixData.M21, command.MatrixData.M22),
            MatrixDataToColor(command.MatrixData.M31));
    }
    
    internal Matrix3x2 PutNineSliceData(Vector2 position, Vector2 size, Color color)
    {
        Matrix3x2 matrix = new Matrix3x2
        {
            M11 = position.X,
            M12 = position.Y,
            M21 = size.X,
            M22 = size.Y,
            M31 = ColorToMatrixData(color)
        };
        return matrix;
    }
    
    #endregion
    
    #region DrawThreeSlice
    // Position = (M11 M12)
    // Size = (M21 M22)
    // Color = (M31)
    internal (Vector2 position, Vector2 size, Color color) GetThreeSliceData(DrawCommand command)
    {
        return (
            new Vector2(command.MatrixData.M11, command.MatrixData.M12),
            new Vector2(command.MatrixData.M21, command.MatrixData.M22),
            MatrixDataToColor(command.MatrixData.M31));
    }
    
    internal Matrix3x2 PutThreeSliceData(Vector2 position, Vector2 size, Color color)
    {
        Matrix3x2 matrix = new Matrix3x2
        {
            M11 = position.X,
            M12 = position.Y,
            M21 = size.X,
            M22 = size.Y,
            M31 = ColorToMatrixData(color)
        };
        return matrix;
    }
    
    #endregion
    
    #endregion
    
    public void Execute()
    {
        if (_drawCommands.Count == 0) return;
        
        _execClipStack.Clear();
        _execTransformStack.Clear();
        
        Matrix3x2 currentTransform = Matrix3x2.Identity;
        Rect? currentClip = null;
        
        BeginBatch(ref currentTransform, ref currentClip);

        foreach (var cmd in _drawCommands)
        {
            switch (cmd.Type)
            {
                case DrawCommandType.SetClip:
                {
                    _execClipStack.Push(currentClip);
                    currentClip = Rect.Intersect(currentClip, cmd.RectData);
                    RestartBatch(ref currentTransform, ref currentClip);
                    break;
                }
                case DrawCommandType.UnsetClip:
                {
                    if (_execClipStack.Count > 0)
                    {
                        _execClipStack.Pop();
                        currentClip = _execClipStack.Count > 0 ? _execClipStack.Peek() : null;
                        RestartBatch(ref currentTransform, ref currentClip);
                    }
                    #if DEBUG
                    else
                    {
                        throw new System.InvalidOperationException("No clipping rectangle to pop.");
                    }
                    #endif
                    break;
                }
                case DrawCommandType.SetTransform:
                {
                    _execTransformStack.Push(currentTransform);
                    currentTransform = cmd.MatrixData * currentTransform;
                    RestartBatch(ref currentTransform, ref currentClip);
                    break;
                }
                case DrawCommandType.UnsetTransform:
                {
                    if (_execTransformStack.Count > 0)
                    {
                        currentTransform = _execTransformStack.Pop();
                        RestartBatch(ref currentTransform, ref currentClip);
                    }
                    #if DEBUG
                    else
                    {
                        throw new System.InvalidOperationException("No transformation to pop.");
                    }
                    #endif
                    break;
                }
                case DrawCommandType.DrawLine:
                {
                    var data = GetLineData(cmd);
                    SpriteBatch.DrawLine(data.start.ToXnaFast(), data.end.ToXnaFast(), data.color.ToMgColor(),
                        data.thickness);
                    break;
                }
                case DrawCommandType.DrawRectangle:
                {
                    var data = GetRectangleData(cmd);
                    var isFilled = cmd.BoolData;
                    if (isFilled)
                    {
                        SpriteBatch.FillRectangle(data.position.ToXnaFast(), data.size.ToXnaFast(),
                            data.color.ToMgColor());
                    }
                    else
                    {
                        SpriteBatch.DrawRectangle(data.position.ToXnaFast(), data.size.ToXnaFast(),
                            data.color.ToMgColor(), data.thickness);
                    }
                    break;
                }
                case DrawCommandType.DrawCircle: // For now, we don't support filled circles, but we can easily add it later if we need to, as the data is already in the command.
                {
                    var data = GetCircleData(cmd);
                    SpriteBatch.DrawCircle(data.center.ToXnaFast(), data.radius,data.segments, data.color.ToMgColor(),
                        data.thickness);
                    break;
                }
                case DrawCommandType.DrawSprite:
                {
                    var data = GetSpriteData(cmd);
                    var texture = (Texture2D)cmd.ObjectData;
                    var rect = cmd.RectData;
                    SpriteBatch.Draw(texture,new Rect(data.position, data.size).ToMGRect(), rect.ToMGRect(), data.color.ToMgColor());
                    break;
                }
                case DrawCommandType.DrawText:
                {
                    var data = GetTextDataWithRichTextLayout(cmd);
                    var textLayout = (RichTextLayout)cmd.ObjectData;
                    textLayout.Draw(SpriteBatch, data.position.ToXnaFast(), data.color.ToMgColor());
                    break;
                }
                case DrawCommandType.DrawTextWithString:
                {
                    var data = GetTextDataWithString(cmd);
                    var text = (string)cmd.ObjectData;
                    var font = (SpriteFontBase?)cmd.AdditionalData;
                    if(font == null)
                        #if DEBUG
                        throw new System.InvalidOperationException("NULL FONT IS NOT CURRENTLY SUPPORTED!.");
                        #else 
                        break;
                        #endif
                    font.DrawText(SpriteBatch, text, data.position.ToXnaFast(), data.color.ToMgColor());
                    break;
                }
                case DrawCommandType.DrawTexture:
                {
                    var data = GetTextureData(cmd);
                    var texture = (Texture2D)cmd.ObjectData;
                    SpriteBatch.Draw(texture, new Rect(data.position, data.size).ToMGRect(), null, data.color.ToMgColor());
                    break;
                }
                case DrawCommandType.DrawNineSlice:
                {
                    var data = GetNineSliceData(cmd);
                    var texture = (Texture2D)cmd.ObjectData;
                    var nineSliceInfo = cmd.NineSliceData;
                    RenderNineSlice(texture, data.position, data.size, nineSliceInfo, data.color);
                    break;
                }
                case DrawCommandType.DrawThreeSlice:
                {
                    var data = GetThreeSliceData(cmd);
                    var texture = (Texture2D)cmd.ObjectData;
                    var threeSliceInfo = cmd.ThreeSliceData;
                    var isHorizontal = cmd.BoolData;
                    RenderThreeSlice(texture, data.position, data.size, threeSliceInfo, isHorizontal, data.color);
                    break;
                }
                case DrawCommandType.DrawSpriteWithIndex:
                {
                    var data = GetSpriteDataWithIndex(cmd);
                    var texture = (Texture2D)cmd.ObjectData;
                    var rect = cmd.RectData;
                    var spriteIndex = data.spriteIndex;
                    var spriteSize = cmd.VectorData3;
                    var sourceRect = new Rect(
                        (spriteIndex % (int)(texture.Width / spriteSize.X)) * spriteSize.X,
                        (spriteIndex / (int)(texture.Width / spriteSize.X)) * spriteSize.Y,
                        spriteSize.X, spriteSize.Y);
                    SpriteBatch.Draw(texture, new Rect(data.position, data.size).ToMGRect(), sourceRect.ToMGRect(), data.color.ToMgColor());
                    break;
                }
            }
        }
        
        EndBatch();
        
        _drawCommands.Clear();
        _currentTransform = Matrix3x2.Identity;
        _currentClip = null;
    }

    private void BeginBatch(ref Matrix3x2 transform, ref Rect? clip)
    {
        #if DEBUG
        if (_isSpriteBatchBeginActive)
            throw new System.InvalidOperationException("SpriteBatch is already active. Nested batches are not supported.");
        _isSpriteBatchBeginActive = true;
        #endif
        
        var rasterizerState = clip.HasValue ? _clippingRasterizerState : _defaultRasterizerState;
        
        if (clip.HasValue)
        {
            SpriteBatch.GraphicsDevice.ScissorRectangle = clip.Value.ToMGRect();
        }

        _currentTransform = transform;
        _currentClip = clip;
        SpriteBatch.Begin(transformMatrix: transform.ToXna(), rasterizerState: rasterizerState);
    }

    private void EndBatch()
    {
        #if DEBUG
        if (!_isSpriteBatchBeginActive)
            throw new System.InvalidOperationException("SpriteBatch is not active. Cannot end batch.");
        _isSpriteBatchBeginActive = false;
        #endif
        SpriteBatch.End();
    }

    private void RestartBatch(ref Matrix3x2 transform, ref Rect? clip)
    {
        if (transform == _currentTransform && clip == _currentClip) return;
        EndBatch();
        BeginBatch(ref transform, ref clip);
    }

    public int PushClip(Rect clippingRect)
    {
        _drawCommands.Add(new DrawCommand(
            DrawCommandType.SetClip,
            rectData: clippingRect
        ));
        
        _clipStack.Push(clippingRect);
        return _clipStack.Count;
    }

    public void PopClip(int clipId)
    {
        if (_clipStack.Count == 0)
            throw new System.InvalidOperationException("No clipping rectangle to pop.");
        if (clipId != _clipStack.Count)
            throw new System.InvalidOperationException("Clipping rectangles must be popped in the correct order (last pushed, first popped).");
        
        _drawCommands.Add(new DrawCommand(
            DrawCommandType.UnsetClip
        ));
        
        _clipStack.Pop();
    }

    public int PushTransform(Matrix3x2 transform)
    {
        _drawCommands.Add(new DrawCommand(
            DrawCommandType.SetTransform,
            matrixData: transform
        ));
        
        _transformStack.Push(transform);
        return _transformStack.Count;
    }

    public void PopTransform(int transformId)
    {
        if (_transformStack.Count == 0)
            throw new System.InvalidOperationException("No transformation to pop.");
        if (transformId != _transformStack.Count)
            throw new System.InvalidOperationException("Transformations must be popped in the correct order (last pushed, first popped).");
        
        _drawCommands.Add(new DrawCommand(
            DrawCommandType.UnsetTransform
        ));
        
        _transformStack.Pop();
    }

    public void DrawRectangle(Vector2 position, Vector2 size, Color color, float thickness = 1, bool filled = false)
    {
        _drawCommands.Add(new DrawCommand(
            DrawCommandType.DrawRectangle,
            matrixData: PutRectangleData(position, size, color, thickness),
            boolData: filled
        ));
    }

    public void DrawLine(Vector2 start, Vector2 end, Color color, float thickness = 1)
    {
        _drawCommands.Add(new DrawCommand(
            DrawCommandType.DrawLine,
            matrixData: PutLineData(start, end, color, thickness)
        ));
    }

    public void DrawCircle(Vector2 center, float radius, Color color, float thickness = 1, int segments = 16, bool filled = false)
    {
        _drawCommands.Add(new DrawCommand(
            DrawCommandType.DrawCircle,
            matrixData: PutCircleData(center, radius, color, thickness, segments),
            boolData: filled
        ));

    }

    public void DrawTexture(Vector2 position, Vector2 size, Texture2D texture, Color? color = null)
    {
        _drawCommands.Add(new DrawCommand(
            DrawCommandType.DrawTexture,
            matrixData: PutTextureData(position, size, Color.GetOrDefault(color)),
            objectData: texture
        ));
    }

    public void DrawText(RichTextLayout textLayout, Vector2 position, Color? color = null)
    {
        _drawCommands.Add(new DrawCommand(
            DrawCommandType.DrawText,
            matrixData: PutTextDataWithRichTextLayout(position, Color.GetOrDefault(color)),
            objectData: textLayout
        ));
    }

    public void DrawText(string text, Vector2 position, Color? color = null, int fontSize = 16, SpriteFontBase? font = null)
    {
        _drawCommands.Add(new DrawCommand(
            DrawCommandType.DrawTextWithString,
            matrixData: PutTextDataWithString(position, Color.GetOrDefault(color), fontSize),
            objectData: text,
            additionalData: font
        ));
    }

    public void DrawSprite(Vector2 position, Vector2 size, int spriteIndex, Vector2 spriteSize, Texture2D atlasTexture,
        Color? color = null)
    {
        _drawCommands.Add(new DrawCommand(
            DrawCommandType.DrawSpriteWithIndex,
            matrixData: PutSpriteDataWithIndex(position, size, spriteIndex, Color.GetOrDefault(color)),
            objectData: atlasTexture,
            vectorData3: spriteSize
        ));
    }

    public void DrawSprite(Vector2 position, Vector2 size, Rect sourceRect, Texture2D atlasTexture, Color? color = null)
    {
        _drawCommands.Add(new DrawCommand(
            DrawCommandType.DrawSprite,
            matrixData: PutSpriteData(position, size, Color.GetOrDefault(color)),
            objectData: atlasTexture,
            rectData: sourceRect
        ));
    }

    public void DrawNineSlice(Vector2 position, Vector2 size, Texture2D texture, NineSliceInfo info, Color? color = null)
    {
        _drawCommands.Add(new DrawCommand(
            DrawCommandType.DrawNineSlice,
            matrixData: PutNineSliceData(position, size, Color.GetOrDefault(color)),
            objectData: texture,
            nineSliceData: info
        ));
    }

    public void DrawThreeSlice(Vector2 position, Vector2 size, Texture2D texture, ThreeSliceInfo info, bool isHorizontal,
        Color? color = null)
    {
        _drawCommands.Add(new DrawCommand(
            DrawCommandType.DrawThreeSlice,
            matrixData: PutThreeSliceData(position, size, Color.GetOrDefault(color)),
            objectData: texture,
            threeSliceData: info,
            boolData: isHorizontal
        ));
    }
    
    #region PrivateDrawMethods
    
    private void RenderNineSlice(Texture2D texture, Vector2 position, Vector2 size, NineSliceInfo info, Color color)
    {
        var x = position.X;
        var y = position.Y;
        var w = size.X;
        var h = size.Y;

        var left = info.Left;
        var right = info.Right;
        var top = info.Top;
        var bottom = info.Bottom;

        var texW = texture.Width;
        var texH = texture.Height;

        var midTexW = texW - left - right;
        var midTexH = texH - top - bottom;

        var midWidth = w - left - right;
        var midHeight = h - top - bottom;

        DrawPart(texture, new Rectangle(0, 0, left, top), new Rectangle((int)x, (int)y, left, top), color);
        DrawPart(texture, new Rectangle(texW - right, 0, right, top), new Rectangle((int)(x + w - right), (int)y, right, top), color);
        DrawPart(texture, new Rectangle(0, texH - bottom, left, bottom), new Rectangle((int)x, (int)(y + h - bottom), left, bottom), color);
        DrawPart(texture, new Rectangle(texW - right, texH - bottom, right, bottom), new Rectangle((int)(x + w - right), (int)(y + h - bottom), right, bottom), color);

        DrawPart(texture, new Rectangle(left, 0, midTexW, top), new Rectangle((int)(x + left), (int)y, (int)midWidth, top), color);
        DrawPart(texture, new Rectangle(left, texH - bottom, midTexW, bottom), new Rectangle((int)(x + left), (int)(y + h - bottom), (int)midWidth, bottom), color);
        DrawPart(texture, new Rectangle(0, top, left, midTexH), new Rectangle((int)x, (int)(y + top), left, (int)midHeight), color);
        DrawPart(texture, new Rectangle(texW - right, top, right, midTexH), new Rectangle((int)(x + w - right), (int)(y + top), right, (int)midHeight), color);

        DrawPart(texture, new Rectangle(left, top, midTexW, midTexH), new Rectangle((int)(x + left), (int)(y + top), (int)midWidth, (int)midHeight), color);
    }
    
    private void RenderThreeSlice(Texture2D texture, Vector2 position, Vector2 size, ThreeSliceInfo info, bool isHorizontal, Color color)
    {
        var x = position.X;
        var y = position.Y;
        var w = size.X;
        var h = size.Y;

        var texW = texture.Width;
        var texH = texture.Height;

        if (isHorizontal)
        {
            var left = info.Start;
            var right = info.End;
        
            var midWidth = w - left - right;
            var midTexW = texW - left - right;

            DrawPart(texture, new Rectangle(0, 0, left, texH), new Rectangle((int)x, (int)y, left, (int)h), color);
            DrawPart(texture, new Rectangle(texW - right, 0, right, texH), new Rectangle((int)(x + w - right), (int)y, right, (int)h), color);
            DrawPart(texture, new Rectangle(left, 0, midTexW, texH), new Rectangle((int)(x + left), (int)y, (int)midWidth, (int)h), color);
        }
        else
        {
            var top = info.Start;
            var bottom = info.End;

            var midHeight = h - top - bottom;
            var midTexH = texH - top - bottom;

            DrawPart(texture, new Rectangle(0, 0, texW, top), new Rectangle((int)x, (int)y, (int)w, top), color);
            DrawPart(texture, new Rectangle(0, texH - bottom, texW, bottom), new Rectangle((int)x, (int)(y + h - bottom), (int)w, bottom), color);
            DrawPart(texture, new Rectangle(0, top, texW, midTexH), new Rectangle((int)x, (int)(y + top), (int)w, (int)midHeight), color);
        }
    }
    private void DrawPart(Texture2D tex, Rectangle source, Rectangle dest, Color color)
    {
        if (dest.Width <= 0 || dest.Height <= 0) return;
        SpriteBatch.Draw(tex, dest, source, color.ToMgColor());
    }
    
    #endregion
}