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
using RPGCreator.SDK.GameUI.Interfaces;

namespace RPGCreator.SDK.Editor.Rendering;

public abstract class BaseUiViewport : BaseMonogameViewport
{
    public abstract IUiManager UiManager { get; }
}

public abstract class BaseMonogameViewport : IDisposable
{
    public Ulid Id { get; } = Ulid.NewUlid();
    
    public SDK.Types.Size Size { get; set; }

    public event Func<IntPtr>? DoNewFrameAction;
    
    public event EventHandler? ViewportFocused;
    public event EventHandler? ViewportClosed;

    public event EventHandler<bool>? DrawingPausedChanged;
    public event EventHandler<bool>? UpdatingPausedChanged;

    public event EventHandler<TimeSpan>? Drawn;
    public event EventHandler<TimeSpan>? Updated;
    
    public event EventHandler<SDK.Types.Size>? Resized;
    public event EventHandler? Disposed;
    
    public bool DrawFrameByFrame { get; set; } = false;

    public bool IsDrawingPaused { get; private set; } = true;
    public bool FrameAsked { get; private set; } = false;
    public bool IsUpdatingPaused { get; private set; } = true;
    
    public bool InternalIsDrawingPaused { get; private set; } = false;
    public bool InternalIsUpdatingPaused { get; private set; } = false;

    public string IdControlLockedIn { get; private set; } = string.Empty;

    protected bool _inDrawing = false;

    public abstract void LoadContent(object graphicsDevice, object spriteBatch, object? shapeBatch = null);

    /// <summary>
    /// This is used to lock the viewport to a specific image control.<br/>
    /// By doing that, we ensure that the viewport will only get inputs event if the mouse is inside the image control.
    /// </summary>
    /// <param name="imageControlName">The image control name where the viewport is drawn and should be locked.</param>
    public void LockToImageControl(string imageControlName)
    {
        IdControlLockedIn = imageControlName;
    }

    /// <summary>
    /// Unlock the viewport from the image control.<br/>
    /// This should be quite rare to happen, as without it, if we put the mouse at '0, 0' in viewport A, viewport B without image control lock, will also get it.
    /// </summary>
    public void UnlockFromImageControl()
    {
        IdControlLockedIn = string.Empty;
    }

    /// <summary>
    /// Check if the mouse is inside the image control locked in.
    /// </summary>
    /// <returns>
    /// Return true if the mouse is inside the image control locked in, false otherwise.
    /// </returns>
    public bool IsInsideImageControl()
    {
        if (string.IsNullOrWhiteSpace(IdControlLockedIn))
            return true;

        if (GlobalStates.ViewportMouseState.InObject is string insideControl)
        {
            return insideControl == IdControlLockedIn;
        }

        return false;
    }
    
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
    
    public void Resize(SDK.Types.Size newSize)
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