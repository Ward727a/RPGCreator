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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Apos.Shapes;
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


public class UiRendererContext(SpriteBatch spriteBatch, ShapeBatch shapeBatch) : IMgUiRendererContext
{

    public class RendererMemoryBuffer
    {

        public ref struct MemoryBufferWriter
        {
            private readonly RendererMemoryBuffer _parent;
            private Span<byte> _span;
            private int _localPos;
            private bool _submitted;

            public MemoryBufferWriter(RendererMemoryBuffer parent, Span<byte> span)
            {
                _parent = parent;
                _span = span;
                _localPos = 0;
                _submitted = false;
            }

            public void EnsureCapacity(int requiredAdditionalSize)
            {
                if (_localPos + requiredAdditionalSize > _span.Length)
                {
                    _span = _parent.GetFullSpanFrom(_parent._currentPosition, _localPos + requiredAdditionalSize);
                }
            }
            
            public void Align(int alignment = 4)
            {
                _localPos = (_localPos + (alignment - 1)) & ~(alignment - 1);
            }

            public void Write<T>(T value) where T : unmanaged
            {
                int size = Unsafe.SizeOf<T>();
                Align(Math.Min(size, 8));

                Unsafe.WriteUnaligned(ref _span[_localPos], value);
                _localPos += size;
            }

            public void WriteObject(object value)
            {
                var refIndex = _parent.PushObjectReference(value);
                WriteInt(refIndex);
            }

            public void WriteByte(byte value)
            {
                _span[_localPos] = value;
                _localPos += 1;
            }

            public void WriteFloat(float value)
            {
                Write(value);
            }

            public void WriteBool(bool value)
            {
                Write(value);
            }
            
            public void WriteVector2(Vector2 value)
            {
                Write(value);
            }

            public void WriteColor(Color value)
            {
                Write(value);
            }

            // ReSharper disable once InconsistentNaming
            public void WriteMatrix3x2(Matrix3x2 value)
            {
                Write(value);
            }

            public void WriteRect(Rect value)
            {
                Write(value);
            }

            public void WriteNineSliceInfo(NineSliceInfo value)
            {
                Write(value);
            }

            public void WriteThreeSliceInfo(ThreeSliceInfo value)
            {
                Write(value);
            }

            public void WriteInt(int value)
            {
                Write(value);
            }

            public void WriteString(string value)
            {
                if (string.IsNullOrEmpty(value))
                {
                    WriteInt(0);
                    return;
                }

                int byteCount = Encoding.UTF8.GetByteCount(value);
                
                EnsureCapacity(4 + byteCount + 8);
                
                WriteInt(byteCount);
                
                Span<byte> destination = _span.Slice(_localPos, byteCount);
                Encoding.UTF8.GetBytes(value, destination);
                
                _localPos += byteCount;
                
                Align();
            }

            public void Submit()
            {
                if (_submitted) return;
                _parent.Commit(_localPos);
                _submitted = true;
            }

            public void Dispose()
            {
                if (!_submitted) Submit(); 
            }
        }

        public ref struct MemoryBufferReader
        {
            private readonly RendererMemoryBuffer _parent;
            private readonly Span<byte> _span;
            
            private int _localPos;
            
            public bool HasData => _localPos < _span.Length;
            
            public MemoryBufferReader(RendererMemoryBuffer parent, byte[] buffer, int sizeLimit)
            {
                _parent = parent;
                _span = buffer.AsSpan(0, sizeLimit);
                _localPos = 0;
            }

            public void Align(int alignment = 4)
            {
                _localPos = (_localPos + (alignment - 1)) & ~(alignment - 1);
            }

            public T Read<T>()where T : unmanaged
            {
                int size = Unsafe.SizeOf<T>();
                int alignment = Math.Min(size, 8);
    
                _localPos = (_localPos + (alignment - 1)) & ~(alignment - 1);
    
                T value = MemoryMarshal.Read<T>(_span.Slice(_localPos, size));
    
                _localPos += size;
                return value;
            }

            public byte ReadByte()
            {
                return _span[_localPos++];
            }

            public int ReadInt()
            {
                return Read<int>();
            }
            
            public float ReadFloat()
            {
                return Read<float>();
            }

            public bool ReadBool()
            {
                return Read<bool>();
            }
            
            public Vector2 ReadVector2()
            {
                return Read<Vector2>();
            }
            
            public Color ReadColor()
            {
                return Read<Color>();
            }

            // ReSharper disable once InconsistentNaming
            public Matrix3x2 ReadMatrix3x2()
            {
                return Read<Matrix3x2>();
            }

            public Rect ReadRect()
            {
                return Read<Rect>();
            }

            public NineSliceInfo ReadNineSliceInfo()
            {
                return Read<NineSliceInfo>();
            }
            
            public ThreeSliceInfo ReadThreeSliceInfo()
            {
                return Read<ThreeSliceInfo>();
            }
            
            public object? ReadObject()
            {
                return _parent.GetObjectReference(ReadInt());
            }
            
            public string ReadString()
            {
                int byteCount = Read<int>();
                if (byteCount == 0) return string.Empty;

                string value = Encoding.UTF8.GetString(_span.Slice(_localPos, byteCount));
        
                _localPos += byteCount;
        
                _localPos = (_localPos + (4 - 1)) & ~(4 - 1);
        
                return value;
            }
        }
        
        private readonly List<object?> _objectReferences = new (256); // List to hold object references, init it to 256, so that we can avoid resizing the list frequently
        private byte[] _buffer = new byte[1024 * 64]; // Allocating 64KB for now, but this can be adjusted as needed.
        private int _currentPosition = 0;

        public void EnsureCapacity(int requiredAdditionalSize)
        {
            if (_currentPosition + requiredAdditionalSize > _buffer.Length)
            {
                int newSize = _buffer.Length * 2;
                
                while(newSize < _currentPosition + requiredAdditionalSize)
                {
                    newSize *= 2;
                }
                
                byte[] newBuffer = new byte[newSize];
                
                Buffer.BlockCopy(_buffer, 0, newBuffer, 0, _currentPosition);
                
                _buffer = newBuffer;
            }
        }
        
        private const int DEFAULT_ALIGNMENT = 8;
        
        public MemoryBufferWriter CreateWriter(int estimatedSize)
        {
            _currentPosition = (_currentPosition + (7)) & ~(7);
            EnsureCapacity(estimatedSize);

            Span<byte> slice = _buffer.AsSpan(_currentPosition);
            return new MemoryBufferWriter(this, slice);
        }

        public MemoryBufferReader CreateReader()
        {
            return new MemoryBufferReader(this, _buffer, _currentPosition);
        }
        
        internal void Commit(int bytesWritten)
        {
            _currentPosition += bytesWritten;
        }

        internal Span<byte> GetFullSpanFrom(int position, int size)
        {
            EnsureCapacity(position + size);
            return _buffer.AsSpan(position, size);
        }

        internal int PushObjectReference(object? obj)
        {
            _objectReferences.Add(obj);
            return _objectReferences.Count - 1;
        }

        internal object? GetObjectReference(int index)
        {
            return _objectReferences[index];
        }
        
        public void Reset()
        {
            _currentPosition = 0;
            _objectReferences.Clear();
        }
        
    }
    
    public RendererMemoryBuffer MemoryBuffer { get; } = new();
    
    public SpriteBatch SpriteBatch { get; init; } = spriteBatch;
    public ShapeBatch ShapeBatch { get; init; } = shapeBatch;

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
    
    public void Execute()
    {
        _execClipStack.Clear();
        _execTransformStack.Clear();
        
        Matrix3x2 currentTransform = Matrix3x2.Identity;
        Rect? currentClip = null;
        
        BeginBatch(ref currentTransform, ref currentClip);
        var reader = MemoryBuffer.CreateReader();
        while (reader.HasData)
        {
            reader.Align(8);
            DrawCommandType type = (DrawCommandType)reader.ReadByte();

            switch (type)
            {
                case DrawCommandType.SetClip:
                {
                    var newClipping = reader.ReadRect();
                    _execClipStack.Push(currentClip);
                    currentClip = Rect.Intersect(currentClip, newClipping);
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
                    var transformMatrix = reader.ReadMatrix3x2();
                    _execTransformStack.Push(currentTransform);
                    currentTransform = transformMatrix * currentTransform;
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
                    var start = reader.ReadVector2();
                    var end = reader.ReadVector2();
                    var color = reader.ReadColor();
                    var thickness = reader.ReadFloat();
                    ShapeBatch.DrawLine(start.ToXnaFast(), end.ToXnaFast(), 0, color.ToMgColor(), color.ToMgColor(),
                        thickness);
                    break;
                }
                case DrawCommandType.DrawRectangle:
                {
                    var pos = reader.ReadVector2();
                    var size = reader.ReadVector2();
                    var color = reader.ReadColor();
                    var thickness = reader.ReadFloat();
                    var isFilled = reader.ReadBool();
                    if (isFilled)
                    {
                        ShapeBatch.FillRectangle(pos.ToXnaFast(), size.ToXnaFast(),
                            color.ToMgColor(), thickness, aaSize: 0);
                        break;
                    }

                    ShapeBatch.BorderRectangle(pos.ToXnaFast(), size.ToXnaFast(),
                        color.ToMgColor(), thickness, aaSize: 0);
                    break;
                }
                case DrawCommandType.DrawCircle:
                {
                    var center = reader.ReadVector2();
                    var radius = reader.ReadFloat();
                    var color = reader.ReadColor();
                    var thickness = reader.ReadFloat();
                    var aaSize = reader.ReadFloat();
                    var isFilled = reader.ReadBool();

                    if (isFilled)
                    {
                        ShapeBatch.FillCircle(center.ToXnaFast(), radius, color.ToMgColor(), aaSize);
                    }
                    else
                    {
                        ShapeBatch.BorderCircle(center.ToXnaFast(), radius, color.ToMgColor(), thickness, aaSize);
                    }
                    
                    break;
                }
                case DrawCommandType.DrawTexture:
                {
                    var pos = reader.ReadVector2();
                    var size = reader.ReadVector2();
                    var colorMask = reader.ReadColor();
                    var textureObject = reader.ReadObject();
                    if (textureObject is not Texture2D texture)
                    {
                        break;
                    }

                    var source = new RectangleF(Vector2.Zero, size.ToXnaFast());
                    
                    ShapeBatch.Draw(texture, pos.ToXnaFast(), source, colorMask.ToMgColor());
                    break;
                }
                case DrawCommandType.DrawText:
                {
                    var textLayoutObj = reader.ReadObject();
                    if (textLayoutObj is not RichTextLayout textLayout)
                    {
                        break;
                    }

                    var pos = reader.ReadVector2();
                    var color = reader.ReadColor();
                    // We should probably create an extensionMethod inside RichTextLayout so it could support ShapeBatch directly from textLayout.Draw() method.
                    ShapeBatch.DrawString(textLayout.Font, textLayout.Text, pos.ToXnaFast(), color.ToMgColor());
                    break;
                }
                case DrawCommandType.DrawTextWithString:
                {
                    var text = reader.ReadString();
                    var position = reader.ReadVector2();
                    var textColor = reader.ReadColor();
                    var fontSize = reader.ReadInt();
                    var spriteFont = reader.ReadObject();
                    if (spriteFont is not SpriteFontBase spriteFontObj)
                    {
                        break;
                    }
                    
                    ShapeBatch.DrawString(spriteFontObj, text, position.ToXnaFast(), textColor.ToMgColor());
                    break;
                }
                default:
                        break;
            }
        }
        EndBatch();
        
        MemoryBuffer.Reset();
        _currentTransform = Matrix3x2.Identity;
        _currentClip = null;
    }

    private void BeginBatch(ref Matrix3x2 transform, ref Rect? clip)
    {
        if (_isSpriteBatchBeginActive)
        #if DEBUG
            throw new System.InvalidOperationException("SpriteBatch is already active. Nested batches are not supported.");
        #else
            return;
        #endif
        _isSpriteBatchBeginActive = true;
        try
        {
            var rasterizerState = clip.HasValue ? _clippingRasterizerState : _defaultRasterizerState;

            if (clip.HasValue)
            {
                ShapeBatch.GraphicsDevice.ScissorRectangle = clip.Value.ToMGRect();
            }

            _currentTransform = transform;
            _currentClip = clip;
            ShapeBatch.Begin(view: transform.ToXna(), rasterizerState: rasterizerState);
        }
        catch (Exception ex)
        {
            _isSpriteBatchBeginActive = false;
            throw new InvalidOperationException("Error starting SpriteBatch", ex);
        }
    }

    private void EndBatch()
    {
        #if DEBUG
        if (!_isSpriteBatchBeginActive)
            throw new System.InvalidOperationException("SpriteBatch is not active. Cannot end batch.");
        _isSpriteBatchBeginActive = false;
        #endif
        ShapeBatch.End();
    }

    private void RestartBatch(ref Matrix3x2 transform, ref Rect? clip)
    {
        if (transform == _currentTransform && clip == _currentClip) return;
        EndBatch();
        BeginBatch(ref transform, ref clip);
    }

    public int PushClip(Rect clippingRect)
    {
        using var buffer = MemoryBuffer.CreateWriter(RenderSize.SetClip);
        buffer.WriteByte((byte)DrawCommandType.SetClip);
        buffer.WriteRect(clippingRect);
        
        _clipStack.Push(clippingRect);
        return _clipStack.Count;
    }

    public void PopClip(int clipId)
    {
        if (_clipStack.Count == 0)
            throw new System.InvalidOperationException("No clipping rectangle to pop.");
        if (clipId != _clipStack.Count)
            throw new System.InvalidOperationException("Clipping rectangles must be popped in the correct order (last pushed, first popped).");
        
        using var buffer = MemoryBuffer.CreateWriter(1);
        buffer.WriteByte((byte)DrawCommandType.UnsetClip);
        
        _clipStack.Pop();
    }

    public int PushTransform(Matrix3x2 transform)
    {
        using var buffer = MemoryBuffer.CreateWriter(RenderSize.SetTransform);
        buffer.WriteByte((byte)DrawCommandType.SetTransform);
        buffer.WriteMatrix3x2(transform);
        
        _transformStack.Push(transform);
        return _transformStack.Count;
    }

    public void PopTransform(int transformId)
    {
        if (_transformStack.Count == 0)
            throw new System.InvalidOperationException("No transformation to pop.");
        if (transformId != _transformStack.Count)
            throw new System.InvalidOperationException("Transformations must be popped in the correct order (last pushed, first popped).");
        
        using var buffer = MemoryBuffer.CreateWriter(1);
        buffer.WriteByte((byte)DrawCommandType.UnsetTransform);
        
        _transformStack.Pop();
    }

    public static class RenderSize
    {
        
        public static readonly int SetClip = ForClip();
        public static readonly int SetTransform = ForTransform();
        public static readonly int DrawRectangle = ForRect();
        public static readonly int DrawLine = ForLine();
        public static readonly int DrawCircle = ForCircle();
        public static readonly int DrawTexture = ForTexture();
        public static readonly int DrawText = ForText();
        public static readonly int DrawTextWithString = ForTextWithString();
        public static readonly int DrawSprite = ForSprite();
        public static readonly int DrawSpriteWithIndex = ForSpriteWithIndex();
        public static readonly int DrawNineSlice = ForNineSlice();
        public static readonly int DrawThreeSlice = ForThreeSlice();
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int AddAligned(int currentPos, int size)
        {
            int alignment = Math.Min(size, 8);
            currentPos = (currentPos + (alignment - 1)) & ~(alignment - 1);
            return currentPos + size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FinishAlign(int pos)
        {
            return (pos + 7) & ~7;
        }
        
        private static int ForClip()
        {
            int p = 1; // Byte flag
            p = AddAligned(p, Unsafe.SizeOf<Rect>());  // Clipping rect
            return FinishAlign(p);
        }

        private static int ForTransform()
        {
            int p = 1;
            p = AddAligned(p, Unsafe.SizeOf<Matrix3x2>());  // Transform matrix
            return FinishAlign(p);
        }

        private static int ForRect()
        {
            int p = 1; // Byte flag
            p = AddAligned(p, Unsafe.SizeOf<Vector2>()); // Position
            p = AddAligned(p, Unsafe.SizeOf<Vector2>()); // Size
            p = AddAligned(p, Unsafe.SizeOf<Color>());   // Color
            p = AddAligned(p, Unsafe.SizeOf<float>());   // Thickness
            p = AddAligned(p, Unsafe.SizeOf<bool>());    // Filled
            return FinishAlign(p);
        }

        private static int ForLine()
        {
            int p = 1; // Byte Flag
            p = AddAligned(p, Unsafe.SizeOf<Vector2>()); // Start point
            p = AddAligned(p, Unsafe.SizeOf<Vector2>()); // End point
            p = AddAligned(p, Unsafe.SizeOf<Color>());   // Color
            p = AddAligned(p, Unsafe.SizeOf<float>());   // Thickness
            return FinishAlign(p);
        }

        private static int ForCircle()
        {
            int p = 1; // Byte Flag
            p = AddAligned(p, Unsafe.SizeOf<Vector2>()); // Center
            p = AddAligned(p, Unsafe.SizeOf<float>());   // Radius
            p = AddAligned(p, Unsafe.SizeOf<Color>());   // Color
            p = AddAligned(p, Unsafe.SizeOf<float>());   // Thickness
            p = AddAligned(p, Unsafe.SizeOf<float>());   // aaSize
            p = AddAligned(p, Unsafe.SizeOf<bool>());    // IsFilled
            return FinishAlign(p);
        }

        private static int ForTexture()
        {
            int p = 1; // Byte Flag
            p = AddAligned(p, Unsafe.SizeOf<Vector2>());  // Position
            p = AddAligned(p, Unsafe.SizeOf<Vector2>());  // Size
            p = AddAligned(p, Unsafe.SizeOf<int>());      // Texture index
            p = AddAligned(p, Unsafe.SizeOf<Color>());    // Mask color
            return FinishAlign(p);
        }

        private static int ForText()
        {
            int p = 1; // Byte Flag
            p = AddAligned(p, Unsafe.SizeOf<int>()); // TextLayout object index
            p = AddAligned(p, Unsafe.SizeOf<Vector2>()); // Position
            p = AddAligned(p, Unsafe.SizeOf<Color>()); // Color
            return FinishAlign(p);
        }

        private static int ForTextWithString()
        {
            int p = 1; // Byte Flag
            p = AddAligned(p, 16);                   // String
            p = AddAligned(p, Unsafe.SizeOf<Vector2>()); // Position
            p = AddAligned(p, Unsafe.SizeOf<Color>());   // Text color
            p = AddAligned(p, Unsafe.SizeOf<int>());     // FontSize 
            p = AddAligned(p, Unsafe.SizeOf<int>());     // SpriteFontBase
            return FinishAlign(p);
        }

        private static int ForSpriteWithIndex()
        {
            int p = 1;  // Byte Flag
            p = AddAligned(p, Unsafe.SizeOf<Vector2>()); // Position
            p = AddAligned(p, Unsafe.SizeOf<Vector2>()); // Size
            p = AddAligned(p, Unsafe.SizeOf<int>());     // Sprite index
            p = AddAligned(p, Unsafe.SizeOf<Vector2>()); // Sprite size
            p = AddAligned(p, Unsafe.SizeOf<int>());     // Atlas Texture index
            p = AddAligned(p, Unsafe.SizeOf<Color>());   // Mask Color
            return FinishAlign(p);
        }
        
        private static int ForSprite()
        {
            int p = 1;  // Byte Flag
            p = AddAligned(p, Unsafe.SizeOf<Vector2>()); // Position
            p = AddAligned(p, Unsafe.SizeOf<Vector2>()); // Size
            p = AddAligned(p, Unsafe.SizeOf<Rect>());    // Source rect
            p = AddAligned(p, Unsafe.SizeOf<int>());     // Atlas Texture index
            p = AddAligned(p, Unsafe.SizeOf<Color>());   // Mask Color
            return FinishAlign(p);
        }

        private static int ForNineSlice()
        {
            int p = 1; // Byte flag
            p = AddAligned(p, Unsafe.SizeOf<Vector2>());          // Position
            p = AddAligned(p, Unsafe.SizeOf<Vector2>());          // Size
            p = AddAligned(p, Unsafe.SizeOf<int>());              // Texture index
            p = AddAligned(p, Unsafe.SizeOf<NineSliceInfo>());    // NineSlice info
            p = AddAligned(p, Unsafe.SizeOf<Color>());            // Mask Color
            return FinishAlign(p);
        }
        
        private static int ForThreeSlice()
        {
            int p = 1; // Byte flag
            p = AddAligned(p, Unsafe.SizeOf<Vector2>());          // Position
            p = AddAligned(p, Unsafe.SizeOf<Vector2>());          // Size
            p = AddAligned(p, Unsafe.SizeOf<int>());              // Texture index
            p = AddAligned(p, Unsafe.SizeOf<ThreeSliceInfo>());   // ThreeSlice info
            p = AddAligned(p, Unsafe.SizeOf<bool>());             // IsHorizontal
            p = AddAligned(p, Unsafe.SizeOf<Color>());            // Mask Color
            return FinishAlign(p);
        }
    }

    public void DrawRectangle(Vector2 position, Vector2 size, Color color, float thickness = 1, bool filled = false)
    {
        using var buffer = MemoryBuffer.CreateWriter(RenderSize.DrawRectangle);
        buffer.WriteByte((byte)DrawCommandType.DrawRectangle);
        buffer.WriteVector2(position);
        buffer.WriteVector2(size);
        buffer.WriteColor(color);
        buffer.WriteFloat(thickness);
        buffer.WriteBool(filled);
        
        // _drawCommands.Add(new DrawCommand(
        //     DrawCommandType.DrawRectangle,
        //     matrixData: PutRectangleData(position, size, color, thickness),
        //     boolData: filled
        // ));
    }

    public void DrawLine(Vector2 start, Vector2 end, Color color, float thickness = 1)
    {
        using var writer = MemoryBuffer.CreateWriter(RenderSize.DrawLine);
        writer.WriteByte((byte)DrawCommandType.DrawLine);
        writer.WriteVector2(start);
        writer.WriteVector2(end);
        writer.WriteColor(color);
        writer.WriteFloat(thickness);
    }

    public void DrawCircle(Vector2 center, float radius, Color color, float thickness = 1, float aaSize = 1.5f, bool filled = false)
    {
        using var writer = MemoryBuffer.CreateWriter(RenderSize.DrawCircle);
        writer.WriteByte((byte)DrawCommandType.DrawCircle);
        writer.WriteVector2(center);
        writer.WriteFloat(radius);
        writer.WriteColor(color);
        writer.WriteFloat(thickness);
        writer.WriteFloat(aaSize);
        writer.WriteBool(filled);
    }

    public void DrawTexture(Vector2 position, Vector2 size, Texture2D texture, Color? color = null)
    {
        using var writer = MemoryBuffer.CreateWriter(RenderSize.DrawTexture);
        writer.WriteByte((byte)DrawCommandType.DrawTexture);
        writer.WriteVector2(position);
        writer.WriteVector2(size);
        writer.WriteObject(texture);
        writer.WriteColor(color ?? Color.White);
    }

    public void DrawText(RichTextLayout textLayout, Vector2 position, Color? color = null)
    {
        using var writer = MemoryBuffer.CreateWriter(RenderSize.DrawText);
        writer.WriteByte((byte)DrawCommandType.DrawText);
        writer.WriteObject(textLayout);
        writer.WriteVector2(position);
        writer.WriteColor(color ?? Color.White);
    }

    public void DrawText(string text, Vector2 position, Color? color = null, int fontSize = 16, SpriteFontBase? font = null)
    {
        using var writer = MemoryBuffer.CreateWriter(RenderSize.DrawTextWithString);
        writer.WriteByte((byte)DrawCommandType.DrawTextWithString);
        writer.WriteString(text);
        writer.WriteVector2(position);
        writer.WriteColor(color ?? Color.White);
        writer.WriteInt(fontSize);
        writer.WriteObject(font);
    }

    public void DrawSprite(Vector2 position, Vector2 size, int spriteIndex, Vector2 spriteSize, Texture2D atlasTexture,
        Color? color = null)
    {
        using var writer = MemoryBuffer.CreateWriter(RenderSize.DrawSpriteWithIndex);
        writer.WriteByte((byte)DrawCommandType.DrawSpriteWithIndex);
        writer.WriteVector2(position);
        writer.WriteVector2(size);
        writer.WriteInt(spriteIndex);
        writer.WriteVector2(spriteSize);
        writer.WriteObject(atlasTexture);
        writer.WriteColor(color ?? Color.White);
    }

    public void DrawSprite(Vector2 position, Vector2 size, Rect sourceRect, Texture2D atlasTexture, Color? color = null)
    {
        using var writer = MemoryBuffer.CreateWriter(RenderSize.DrawSprite);
        writer.WriteByte((byte)DrawCommandType.DrawSprite);
        writer.WriteVector2(position);
        writer.WriteVector2(size);
        writer.WriteRect(sourceRect);
        writer.WriteObject(atlasTexture);
        writer.WriteColor(color ?? Color.White);
    }

    public void DrawNineSlice(Vector2 position, Vector2 size, Texture2D texture, NineSliceInfo info, Color? color = null)
    {
        using var writer = MemoryBuffer.CreateWriter(RenderSize.DrawNineSlice);
        writer.WriteByte((byte)DrawCommandType.DrawNineSlice);
        writer.WriteVector2(position);
        writer.WriteVector2(size);
        writer.WriteObject(texture);
        writer.Write(info);
        writer.WriteColor(color ?? Color.White);
    }

    public void DrawThreeSlice(Vector2 position, Vector2 size, Texture2D texture, ThreeSliceInfo info, bool isHorizontal,
        Color? color = null)
    {
        using var writer = MemoryBuffer.CreateWriter(RenderSize.DrawThreeSlice);
        writer.WriteByte((byte)DrawCommandType.DrawThreeSlice);
        writer.WriteVector2(position);
        writer.WriteVector2(size);
        writer.WriteObject(texture);
        writer.Write(info);
        writer.WriteBool(isHorizontal);
        writer.WriteColor(color ?? Color.White);
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