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

using System.Drawing;

namespace RPGCreator.SDK.Editor.Rendering;

public abstract class BaseMonogameViewport : IDisposable
{
    public Ulid Id { get; } = Ulid.NewUlid();
    
    public Size Size { get; set; }

    public event Func<IntPtr>? DoNewFrameAction;
    
    public event EventHandler? ViewportFocused;
    public event EventHandler? ViewportClosed;

    public event EventHandler<bool>? DrawingPausedChanged;
    public event EventHandler<bool>? UpdatingPausedChanged;

    public event EventHandler<TimeSpan>? Drawn;
    public event EventHandler<TimeSpan>? Updated;
    
    public event EventHandler<Size>? Resized;
    public event EventHandler? Disposed;
    
    public bool DrawFrameByFrame { get; set; } = false;

    public bool IsDrawingPaused { get; private set; } = true;
    public bool FrameAsked { get; private set; } = false;
    public bool IsUpdatingPaused { get; private set; } = true;
    
    public bool InternalIsDrawingPaused { get; private set; } = false;
    public bool InternalIsUpdatingPaused { get; private set; } = false;
    
    protected bool _inDrawing = false;

    public abstract void LoadContent(object graphicsDevice, object spriteBatch);
    
    public void DrawViewport(TimeSpan deltaTime)
    {
        if (InternalIsDrawingPaused || IsDrawingPaused) return;
        UpdatingFrame(deltaTime);
        Drawn?.Invoke(this, deltaTime);
    }

    public void UpdateViewport(TimeSpan deltaTime)
    {
        if (InternalIsUpdatingPaused || IsUpdatingPaused) return;
        UpdatingLoop(deltaTime);
        Updated?.Invoke(this, deltaTime);
    }

    public void DoNewFrame()
    {
        if (InternalIsDrawingPaused || IsDrawingPaused) return;
        FrameAsked = true;
        var ptr = DoNewFrameAction?.Invoke();
        if (ptr.HasValue)
        {
            UpdateAvaloniaControl(ptr.Value);
            AskForNewFrame();
        }
    }

    protected Action OnceUpdatedAction;
    
    public void OnceUpdatedDo(Action action)
    {
        OnceUpdatedAction = action;
    }
    
    public void PauseDrawing()
    {
        IsDrawingPaused = true;
        DrawingPausedChanged?.Invoke(this, true);
    }
    public void ResumeDrawing()
    {
        IsDrawingPaused = false;
        DrawingPausedChanged?.Invoke(this, false);
    }

    public void PauseUpdating()
    {
        IsUpdatingPaused = true;
        UpdatingPausedChanged?.Invoke(this, true);
    }
        
    public void ResumeUpdating()
    {
        IsUpdatingPaused = false;
        UpdatingPausedChanged?.Invoke(this, false);
    }
    
    public void Focus() => ViewportFocused?.Invoke(this, EventArgs.Empty);
    public void Close() => ViewportClosed?.Invoke(this, EventArgs.Empty);
    
    public void Resize(Size newSize)
    {

        PauseDrawing();
        PauseUpdating();
        Size = newSize;
        Resized?.Invoke(this, Size);
    }

    public abstract void SetNewRendertarget(object newRenderTarget);
    
    protected virtual void UpdatingFrame(TimeSpan deltaTime) { }
    protected virtual void UpdatingLoop(TimeSpan deltaTime) { }
    protected virtual void Disposing() { }
    protected virtual void AskForNewFrame() { }

    public abstract void UpdateAvaloniaControl(IntPtr bitmapControlAddress);

    public void Dispose()
    {
        Resized = null;
        Disposing();
        InternalIsDrawingPaused = true;
        InternalIsUpdatingPaused = true;
        Disposed?.Invoke(this, EventArgs.Empty);
    }
}