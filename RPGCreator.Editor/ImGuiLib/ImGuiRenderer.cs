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

// ORIGINAL FILES CAN BE FOUND AT: https://github.com/cave-bjornson/ImGuiRenderer.Hexa.Net.MonoGame/tree/main/ImguiRenderer.Hexa.Net.MonoGame
// EDITED TO BE USED WITH FNA

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.RuntimeLib.FNA;
using RPGCreator.SDK;
using RPGCreator.SDK.Common.Logging;
using SDL3;

namespace RPGCreator.Editor.ImGuiLib;

public class ImGuiRenderer
{
    // Graphics
    private readonly GraphicsDevice _graphicsDevice;
    private BasicEffect? _effect;
    private readonly RasterizerState _rasterizerState;

    private byte[] _vertexData = null!;
    private VertexBuffer _vertexBuffer = null!;
    private int _vertexBufferSize;

    private byte[] _indexData = null!;
    private IndexBuffer _indexBuffer = null!;
    private int _indexBufferSize;

    // Textures
    private readonly Dictionary<ImTextureID, TextureInfo> _textures;
    private int _nextTexId = 1;

    public ImGuiRenderer(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);

        ImGuiContextPtr context = ImGui.CreateContext();
        ImGui.SetCurrentContext(context);

        _graphicsDevice = game.GraphicsDevice;
        _textures = new Dictionary<ImTextureID, TextureInfo>();

        _rasterizerState = new RasterizerState()
        {
            CullMode = CullMode.None,
            DepthBias = 0,
            FillMode = FillMode.Solid,
            MultiSampleAntiAlias = false,
            ScissorTestEnable = true,
            SlopeScaleDepthBias = 0
        };

        SetupBackendCapabilities();
        LoadFonts();
        RebuildFontAtlas();
    }

    private void SetupBackendCapabilities()
    {
        ImGuiIOPtr io = ImGui.GetIO();
        io.BackendFlags |= ImGuiBackendFlags.RendererHasTextures;
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;

        // Set up platform IO for texture management
        ImGuiPlatformIOPtr platformIO = ImGui.GetPlatformIO();

        if (_graphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
        {
            platformIO.RendererTextureMaxWidth = 2048;
            platformIO.RendererTextureMaxHeight = 2048;
        }
        else
        {
            platformIO.RendererTextureMaxWidth = 4096;
            platformIO.RendererTextureMaxHeight = 4096;
        }
    }

    private unsafe void LoadFonts()
    {
        ImGuiIOPtr io = ImGui.GetIO();
        var interFont = Path.Combine(RpgEnv.Path.ExeAssetsFolder, "Fonts", "Inter",
            "Inter-VariableFont_opsz,wght.ttf");
        var iconFont = Path.Combine(RpgEnv.Path.ExeAssetsFolder, "Fonts", "RPGCreator-Icon",
            "RPGCreator-Icon.ttf");
        io.Fonts.AddFontFromFileTTF(interFont);
        
        ImFontConfig* config = ImGui.ImFontConfig(); 
        config->MergeMode = 1; // Set this on '1' so ImGui merge fonts.
        config->PixelSnapH = 1;
        
        uint[] iconRanges = [0xF000, 0xF015, 0];
        
        fixed (uint* rangePtr = iconRanges)
        {
            io.Fonts.AddFontFromFileTTF(iconFont, 16.0f, config, rangePtr);
        }
    }
    public unsafe void RebuildFontAtlas()
    {
        var io = ImGui.GetIO();

        byte* pixelData = io.Fonts.TexData.Pixels;
        int width = io.Fonts.TexData.Width;
        int height = io.Fonts.TexData.Height;

        if (pixelData == null || width == 0 || height == 0)
            return;

        var fontTexture = new Texture2D(
            _graphicsDevice, 
            width, 
            height, 
            false, 
            SurfaceFormat.Color
        );
        fontTexture.Name = "FontTexture";

        fontTexture.SetDataPointerEXT(0, null, (IntPtr)pixelData, width * height * 4);

        var fontTextureId = BindTexture(fontTexture);

        io.Fonts.TexData.SetTexID(fontTextureId.GetTexID()); 
        io.Fonts.TexData.SetStatus(ImTextureStatus.Ok);
    }

    public virtual unsafe ImTextureRef BindTexture(Texture2D texture)
    {
        IntPtr texId = new IntPtr(_nextTexId++);

        TextureInfo textureInfo = new TextureInfo()
        {
            Texture = texture,
            IsManaged = false,
        };

        _textures[texId] = textureInfo;

        return new ImTextureRef(null, texId);
    }

    public virtual TextureInfo? UnbindTexture(ImTextureRef textureRef)
    {
        if (_textures.TryGetValue(textureRef.TexID, out TextureInfo? textureInfo))
        {
            if (textureInfo.IsManaged)
            {
                textureInfo.Texture.Dispose();
            }

            _textures.Remove(textureRef.TexID);
            
            return textureInfo;
        }

        return null;
    }

    private readonly Dictionary<ImGuiMouseCursor, IntPtr> _sdlCursors = new();
    private void UpdateMouseCursor()
    {
        ImGuiIOPtr io = ImGui.GetIO();
        if ((io.ConfigFlags & ImGuiConfigFlags.NoMouseCursorChange) != 0)
        {
            return;
        }

        ImGuiMouseCursor imguiCursor = ImGui.GetMouseCursor();
        if (imguiCursor == ImGuiMouseCursor.None)
        {
            SDL.SDL_ShowCursor();
            return;
        }
    
        SDL.SDL_SystemCursor sdlSystemCursor = imguiCursor switch
        {
            ImGuiMouseCursor.TextInput => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_TEXT,
            ImGuiMouseCursor.ResizeAll => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_NWSE_RESIZE,
            ImGuiMouseCursor.ResizeNs => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_NS_RESIZE,
            ImGuiMouseCursor.ResizeEw => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_EW_RESIZE,
            ImGuiMouseCursor.ResizeNesw => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_NESW_RESIZE,
            ImGuiMouseCursor.ResizeNwse => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_NWSE_RESIZE,
            ImGuiMouseCursor.Hand => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_POINTER,
            ImGuiMouseCursor.NotAllowed => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_NOT_ALLOWED,
            _ => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_DEFAULT,
        };

        if (!_sdlCursors.TryGetValue(imguiCursor, out IntPtr cursorPtr))
        {
            cursorPtr = SDL.SDL_CreateSystemCursor(sdlSystemCursor);
            _sdlCursors[imguiCursor] = cursorPtr;
        }

        if (cursorPtr != IntPtr.Zero)
        {
            SDL.SDL_SetCursor(cursorPtr);
        }
    }
    
    public virtual void UpdateTexture(ImTextureDataPtr textureData)
    {
        switch (textureData.Status)
        {
            case ImTextureStatus.WantCreate:
                CreateTexture(textureData);
                break;

            case ImTextureStatus.WantUpdates:
                UpdateTextureData(textureData);
                break;

            case ImTextureStatus.WantDestroy:
                DestroyTexture(textureData);
                break;

            case ImTextureStatus.Ok:
                // Nothing to do
                break;
        }
    }

    private unsafe void CreateTexture(ImTextureDataPtr textureData)
    {
        SurfaceFormat format =
            textureData.Format == ImTextureFormat.Rgba32 ? SurfaceFormat.Color : SurfaceFormat.Alpha8;
        Texture2D texture = new Texture2D(_graphicsDevice, textureData.Width, textureData.Height, false, format);
        texture.Name = "TextureCreatedByImGuiRenderer";

        if (textureData.Pixels != null)
        {
            int pixelCount = textureData.Width * textureData.Height;
            int bytesPerPixel = textureData.Format == ImTextureFormat.Rgba32 ? 4 : 1;
            int dataSize = pixelCount * bytesPerPixel;

            byte[] managedData = new byte[dataSize];
            Marshal.Copy(new IntPtr(textureData.Pixels), managedData, 0, dataSize);
            texture.SetData(managedData);
        }

        TextureInfo textureInfo = new TextureInfo()
        {
            Texture = texture,
            IsManaged = true,
        };

        _textures[textureData.TexID] = textureInfo;
        textureData.SetStatus(ImTextureStatus.Ok);
    }

    private unsafe void UpdateTextureData(ImTextureDataPtr textureData)
    {
        IntPtr texId = textureData.GetTexID();
        if (!_textures.TryGetValue(texId, out var textureInfo))
        {
            return;
        }

        var texture = textureInfo.Texture;

        // Check if the texture's dimensions or format have changed
        var newFormat =
            textureData.Format == ImTextureFormat.Rgba32 ? SurfaceFormat.Color : SurfaceFormat.Alpha8;
        if (texture.Width != textureData.Width || texture.Height != textureData.Height || texture.Format != newFormat)
        {
            texture.Dispose();
            texture = new Texture2D(_graphicsDevice, textureData.Width, textureData.Height, false, newFormat);
            texture.Name = "TextureUpdatedByImGuiRenderer";
            textureInfo.Texture = texture;
        }

        if (textureData.Pixels != null)
        {
            var updates = textureData.Updates;
            var bytesPerPixel = textureData.Format == ImTextureFormat.Rgba32 ? 4 : 1;
            var sourcePitch = textureData.Width * bytesPerPixel;

            for (var i = 0; i < updates.Size; i++)
            {
                var update = updates[i];
                var updatePitch = update.W * bytesPerPixel;
                var dataSize = updatePitch * update.H;

                var managedData = ArrayPool<byte>.Shared.Rent(dataSize);

                try
                {
                    var sourcePtr = (byte*)textureData.Pixels;

                    fixed (byte* destPtr = managedData)
                    {
                        for (var row = 0; row < update.H; row++)
                        {
                            var currentSourceY = update.Y + row;
                            var sourceByteIndex = (currentSourceY * sourcePitch) + (update.X * bytesPerPixel);
                            var destByteIndex = row * updatePitch;

                            Buffer.MemoryCopy(
                                sourcePtr + sourceByteIndex, 
                                destPtr + destByteIndex, 
                                dataSize - destByteIndex, 
                                updatePitch
                            );
                        }

                        texture.SetDataPointerEXT(
                            0, 
                            new Rectangle(update.X, update.Y, update.W, update.H), 
                            (IntPtr)destPtr, 
                            dataSize
                        );
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(managedData);
                }
            }
            // Old code, kept for reference and if new has problem
            //
            // int pixelCount = textureData.Width * textureData.Height;
            // int bytesPerPixel = textureData.Format == ImTextureFormat.Rgba32 ? 4 : 1;
            // int dataSize = pixelCount * bytesPerPixel;
            //
            // byte[] managedData = new byte[dataSize];
            // Marshal.Copy(new IntPtr(textureData.Pixels), managedData, 0, dataSize);
            // texture.SetData(managedData);
        }
        
        textureData.SetStatus(ImTextureStatus.Ok);
    }

    private void DestroyTexture(ImTextureDataPtr textureData)
    {
        IntPtr texId = textureData.GetTexID();
        if (_textures.TryGetValue(texId, out TextureInfo textureInfo))
        {
            if (textureInfo.IsManaged)
            {
                textureInfo.Texture?.Dispose();
            }

            _textures.Remove(texId);
        }
    }

    public virtual void BeforeLayout(GameTime gameTime)
    {
        ImGuiIOPtr io = ImGui.GetIO();
        if (gameTime.ElapsedGameTime.TotalSeconds > 0)
        {
            io.DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        else
        {
            io.DeltaTime = 1.0f / 60.0f;
        }
        ImGui.NewFrame();
        
    }

    public virtual void AfterLayout()
    {
        ImGui.Render();

        unsafe
        {
            ImDrawDataPtr drawData = ImGui.GetDrawData();
            ProcessTextureUpdates(drawData);
            RenderDrawData(drawData);
        }

        UpdateMouseCursor();
    }

    private unsafe void ProcessTextureUpdates(ImDrawDataPtr drawData)
    {
        if (drawData.Textures.Data == null) return;

        for (int i = 0; i < drawData.Textures.Size; i++)
        {
            ImTextureDataPtr textureData = drawData.Textures.Data[i];
            UpdateTexture(textureData);
        }
    }

    protected virtual Effect UpdateEffect(Texture2D texture)
    {
        ImGuiIOPtr io = ImGui.GetIO();

        _effect ??= new BasicEffect(_graphicsDevice);

        _effect.World = Matrix.Identity;
        _effect.View = Matrix.Identity;
        _effect.Projection =
            Matrix.CreateOrthographicOffCenter(0.0f, io.DisplaySize.X, io.DisplaySize.Y, 0.0f, -1.0f, 1.0f);
        _effect.TextureEnabled = true;
        _effect.Texture = texture;
        _effect.VertexColorEnabled = true;

        return _effect;
    }

    private unsafe void RenderDrawData(ImDrawData* drawData)
    {
        // Cache states so they can be restored when we're done.
        Viewport lastViewport = _graphicsDevice.Viewport;
        Rectangle lastScissorBox = _graphicsDevice.ScissorRectangle;
        RasterizerState lastRasterizer = _graphicsDevice.RasterizerState;
        DepthStencilState lastDepthStencil = _graphicsDevice.DepthStencilState;
        Color lastBlendFactor = _graphicsDevice.BlendFactor;
        BlendState lastBlendState = _graphicsDevice.BlendState;
        SamplerState lastSamplerState = _graphicsDevice.SamplerStates[0];

        // Setup render state: 
        // - alpha-blending enabled
        _graphicsDevice.BlendFactor = Color.White;
        _graphicsDevice.BlendState = BlendState.NonPremultiplied;

        // - No face culling  
        // - Scissor testing enabled
        _graphicsDevice.RasterizerState = _rasterizerState;

        // - Depth read-only (testing enabled, writes disabled)
        _graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

        // - Point filtering for textures (no interpolation)
        _graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

        // Handle cases of screen coordinates != from framebuffer coordinates (e.g. retina displays)
        drawData->ScaleClipRects(ImGui.GetIO().DisplayFramebufferScale);

        // Setup projection
        _graphicsDevice.Viewport = new Viewport(0, 0, _graphicsDevice.PresentationParameters.BackBufferWidth,
            _graphicsDevice.PresentationParameters.BackBufferHeight);

        UpdateBuffers(drawData);

        RenderCommandLists(drawData);
        
        ImGui.UpdatePlatformWindows();
        ImGui.RenderPlatformWindowsDefault();

        // Restore modified state
        _graphicsDevice.Viewport = lastViewport;
        _graphicsDevice.ScissorRectangle = lastScissorBox;
        _graphicsDevice.RasterizerState = lastRasterizer;
        _graphicsDevice.DepthStencilState = lastDepthStencil;
        _graphicsDevice.BlendState = lastBlendState;
        _graphicsDevice.BlendFactor = lastBlendFactor;
        _graphicsDevice.SamplerStates[0] = lastSamplerState;
    }

    private unsafe void UpdateBuffers(ImDrawData* drawData)
    {
        if (drawData->TotalVtxCount == 0)
        {
            return;
        }

        // Expand buffers if we need more room
        if (drawData->TotalVtxCount > _vertexBufferSize)
        {
            _vertexBuffer?.Dispose();

            _vertexBufferSize = (int)(drawData->TotalVtxCount * 1.5f);
            _vertexBuffer = new VertexBuffer(_graphicsDevice, DrawVertDeclaration.Declaration, _vertexBufferSize,
                BufferUsage.None);
            _vertexData = new byte[_vertexBufferSize * DrawVertDeclaration.Size];
        }

        if (drawData->TotalIdxCount > _indexBufferSize)
        {
            _indexBuffer?.Dispose();

            _indexBufferSize = (int)(drawData->TotalIdxCount * 1.5f);
            _indexBuffer = new IndexBuffer(_graphicsDevice, IndexElementSize.SixteenBits, _indexBufferSize,
                BufferUsage.None);
            _indexData = new byte[_indexBufferSize * sizeof(ushort)];
        }

        // Copy ImGui's vertices and indices to a set of managed byte arrays
        int vtxOffset = 0;
        int idxOffset = 0;

        for (int n = 0; n < drawData->CmdListsCount; n++)
        {
            ImDrawList* cmdList = drawData->CmdLists.Data[n];

            fixed (void* vtxDstPtr = &_vertexData[vtxOffset * DrawVertDeclaration.Size])
            {
                fixed (void* idxDstPtr = &_indexData[idxOffset * sizeof(ushort)])
                {
                    Buffer.MemoryCopy(cmdList->VtxBuffer.Data, vtxDstPtr, _vertexData.Length,
                        cmdList->VtxBuffer.Size * DrawVertDeclaration.Size);
                    Buffer.MemoryCopy(cmdList->IdxBuffer.Data, idxDstPtr, _indexData.Length,
                        cmdList->IdxBuffer.Size * sizeof(ushort));
                }
            }

            vtxOffset += cmdList->VtxBuffer.Size;
            idxOffset += cmdList->IdxBuffer.Size;
        }

        // Copy the managed byte arrays to the gpu vertex- and index buffers
        _vertexBuffer.SetData(_vertexData, 0, drawData->TotalVtxCount * DrawVertDeclaration.Size);
        _indexBuffer.SetData(_indexData, 0, drawData->TotalIdxCount * sizeof(ushort));
    }

    private unsafe void RenderCommandLists(ImDrawData* drawData)
    {
        _graphicsDevice.SetVertexBuffer(_vertexBuffer);
        _graphicsDevice.Indices = _indexBuffer;

        int vtxOffset = 0;
        int idxOffset = 0;

        for (int n = 0; n < drawData->CmdListsCount; n++)
        {
            ImDrawList* cmdList = drawData->CmdLists.Data[n];

            for (int cmdi = 0; cmdi < cmdList->CmdBuffer.Size; cmdi++)
            {
                ImDrawCmd* drawCmd = &cmdList->CmdBuffer.Data[cmdi];

                if (drawCmd->ElemCount == 0)
                {
                    continue;
                }

                // In v1.92, we need to handle ImTextureRef instead of ImTextureID
                ImTextureRef textureRef = drawCmd->TexRef;
                ImTextureID texId = textureRef.GetTexID();
                if (!_textures.TryGetValue(texId, out TextureInfo textureInfo))
                {
                    textureInfo = new TextureInfo()
                    {
                        IsManaged = false,
                        Texture = Texture2D.GetDefaultTexture(_graphicsDevice),
                    };
                    
                    Logger.Error($"Could not find a texture with id '{texId}', please check your bindings!");
                }

                _graphicsDevice.ScissorRectangle = new Rectangle(
                    (int)drawCmd->ClipRect.X,
                    (int)drawCmd->ClipRect.Y,
                    (int)(drawCmd->ClipRect.Z - drawCmd->ClipRect.X),
                    (int)(drawCmd->ClipRect.W - drawCmd->ClipRect.Y)
                );

                Effect effect = UpdateEffect(textureInfo.Texture);

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();

#pragma warning disable CS0618 // // FNA does not expose an alternative method.
                    _graphicsDevice.DrawIndexedPrimitives(
                        primitiveType: PrimitiveType.TriangleList,
                        baseVertex: (int)drawCmd->VtxOffset + vtxOffset,
                        minVertexIndex: 0,
                        numVertices: cmdList->VtxBuffer.Size,
                        startIndex: (int)drawCmd->IdxOffset + idxOffset,
                        primitiveCount: (int)drawCmd->ElemCount / 3
                    );
#pragma warning restore CS0618
                }
            }

            vtxOffset += cmdList->VtxBuffer.Size;
            idxOffset += cmdList->IdxBuffer.Size;
        }
    }

    private bool _disposed;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        _effect?.Dispose();

        // Clean up managed textures
        foreach (TextureInfo textureInfo in _textures.Values)
        {
            if (textureInfo.IsManaged)
            {
                textureInfo.Texture?.Dispose();
            }
        }

        _textures.Clear();

        ImGui.DestroyContext();
    }
}

public sealed class TextureInfo
{
    public Texture2D Texture { get; set; }
    public bool IsManaged { get; init; }
}

public static class DrawVertDeclaration
{
    public static readonly VertexDeclaration Declaration;

    public static readonly int Size;

    static unsafe DrawVertDeclaration()
    {
        Size = sizeof(ImDrawVert);

        VertexElement position = new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0);
        VertexElement uv = new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0);
        VertexElement color = new VertexElement(16, VertexElementFormat.Color, VertexElementUsage.Color, 0);

        Declaration = new VertexDeclaration(Size, position, uv, color);
    }
}