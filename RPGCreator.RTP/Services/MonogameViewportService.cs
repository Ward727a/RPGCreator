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
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using RPGCreator.RTP.Viewport;
using RPGCreator.SDK.Editor.Rendering;
using RPGCreator.SDK.EditorUiService;
using RPGCreator.SDK.Logging;
using Size = System.Drawing.Size;

namespace RPGCreator.RTP.Services;

public sealed class MonogameViewportService : IMonogameViewport
{
    public RenderCore _core { get; private set; }
    public bool IsCoreReady { get; private set; } = false;
    public event Action? OnCoreReady;
    private Dictionary<string, BaseGameViewport> ViewportsMap { get; } = new();
    private readonly List<BaseGameViewport> _activeViewports = [];

    private Queue<(string ViewportId, IntPtr bitmapControlAddress, Size InitialSize)> _pendingViewports = new();
    private Queue<(string ViewportId, int Width, int Height)> _pendingResizes = new();
    
    public void Initialize()
    {
        _core = new RenderCore(this);
        _core.DeviceReady += CoreReady;
    
    }

    private void CoreReady()
    {
        IsCoreReady = true;
        
        while (_pendingViewports.Count > 0)
        {
            var (viewportId, bitmapControlAddress, initialSize) = _pendingViewports.Dequeue();
            CreateNewViewport(viewportId, bitmapControlAddress, initialSize);
        }
        while (_pendingResizes.Count > 0)
        {
            var (viewportId, width, height) = _pendingResizes.Dequeue();
            ResizeViewport(viewportId, width, height);
        }

        OnCoreReady?.Invoke();
    }
    
    public void CreateNewViewport(string viewportId, IntPtr bitmapControlAddress, Size initialSize)
    {
        if (ViewportsMap.ContainsKey(viewportId))
        {
            throw new Exception($"Viewport with ID '{viewportId}' already exists.");
        }

        if (!IsCoreReady)
        {
            _pendingViewports.Enqueue((viewportId, bitmapControlAddress, initialSize));
            return;
        }

        var viewport = new GameViewport(_core.CreateNewRenderTarget2D(initialSize.Width, initialSize.Height));
        _core.LoadContent(viewport);
        viewport.Size = initialSize;
        viewport.UpdateAvaloniaControl(bitmapControlAddress);
        _activeViewports.Add(viewport);
        viewport.ResumeUpdating();
        viewport.ResumeDrawing();
        ViewportsMap[viewportId] = viewport;
        
        viewport.Resized += (sender, newSize) =>
        {
            if (sender is GameViewport vp)
            {
                vp.RenderTarget?.Dispose();
                vp.RenderTarget = _core.CreateNewRenderTarget2D(newSize.Width, newSize.Height);
                vp.ResumeUpdating();
                vp.ResumeDrawing();
            }
        };
    }

    public void ResizeViewport(string viewportId, int width, int height)
    {
        if (!IsCoreReady)
        {
            _pendingResizes.Enqueue((viewportId, width, height));
            return;
        }
        
        if (!ViewportsMap.TryGetValue(viewportId, out var viewport))
        {
            throw new Exception($"Viewport with ID '{viewportId}' not found.");
        }
        
        viewport.Resize(new Size(width, height));
    }

    public BaseGameViewport GetViewport(string viewportId)
    {
        if (!ViewportsMap.TryGetValue(viewportId, out var viewport))
        {
            throw new Exception($"Viewport with ID '{viewportId}' not found.");
        }

        return viewport;
    }

    public void DestroyViewport(string viewportId)
    {
        if (!ViewportsMap.TryGetValue(viewportId, out var viewport))
        {
            throw new Exception($"Viewport with ID '{viewportId}' not found.");
        }
        
        viewport.Dispose();
        _activeViewports.Remove(viewport);
        ViewportsMap.Remove(viewportId);
    }
    private bool _isTicking = false;

    public void Tick()
    {
    }

    public void AttachToWindow(IntPtr avaloniaWindowHandle)
    {
        // return;
    }

    internal ReadOnlySpan<BaseGameViewport> GetAllViewports()
    {
        return CollectionsMarshal.AsSpan(_activeViewports);
    }
}