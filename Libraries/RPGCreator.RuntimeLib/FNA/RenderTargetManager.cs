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

using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Shared.Types;

namespace RPGCreator.RuntimeLib.FNA;

/// <summary>
/// A quick dirty class to manage render targets.<br/>
/// This will be improved later on, but for now it's doing its job.
/// </summary>
public class RenderTargetManager : IDisposable
{
    private readonly GraphicsDevice _graphicsDevice;
    
    private readonly Dictionary<StringName, RenderTarget2D> _targets = new();
    
    public RenderTargetManager(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
    }

    /// <summary>
    /// Retrieves an existing <see cref="RenderTarget2D"/> associated with the specified name,
    /// or creates and associates a new one if it does not exist.
    /// </summary>
    /// <param name="name">The unique name identifying the render target to retrieve or create.</param>
    /// <param name="width">The width of the new render target to create if it does not already exist.</param>
    /// <param name="height">The height of the new render target to create if it does not already exist.</param>
    /// <returns>The <see cref="RenderTarget2D"/> instance associated with the specified name.</returns>
    public RenderTarget2D GetOrCreateTarget2D(StringName name, int width = 1, int height = 1)
    {
        if (_targets.TryGetValue(name, out var target))
            return target;

        var newTarget = new RenderTarget2D(_graphicsDevice, width, height);
        _targets[name] = newTarget;
        return newTarget;
    }

    /// <summary>
    /// Switches the current render target associated with the specified name to a new RenderTarget2D instance.
    /// If a render target with the specified name does not already exist, it will be added.
    /// </summary>
    /// <param name="name">The unique identifier for the render target being switched or added.</param>
    /// <param name="newTarget">The new RenderTarget2D instance to associate with the specified name.</param>
    /// <remarks>
    /// This doesn't dispose of the old render target, so be sure to do that yourself if necessary.
    /// </remarks>
    public RenderTarget2D SwitchTarget2D(StringName name, RenderTarget2D newTarget)
    {
        _targets[name] = newTarget;
        return newTarget;
    }

    /// <summary>
    /// Replaces the existing <see cref="RenderTarget2D"/> associated with the specified name and disposes of the old one.
    /// </summary>
    /// <param name="name">The unique name identifying the render target to replace.</param>
    /// <param name="newTarget">The new <see cref="RenderTarget2D"/> to associate with the specified name.</param>
    public RenderTarget2D SwitchAndDestroyTarget2D(StringName name, RenderTarget2D newTarget)
    {
        if (_targets.TryGetValue(name, out var target))
        {
            target.Dispose();
        }

        _targets[name] = newTarget;
        return newTarget;
    }

    /// <summary>
    /// Destroys the render target associated with the specified name and removes it from the internal collection.
    /// If the render target exists, it will be properly disposed before removal.
    /// </summary>
    /// <param name="name">The unique identifier of the render target to be destroyed.</param>
    /// <returns>True if the render target was successfully removed; otherwise, false.</returns>
    public bool DestroyTarget2D(StringName name)
    {
        if (_targets.TryGetValue(name, out var target))
            target.Dispose();
        
        return _targets.Remove(name);
    }

    public void Dispose()
    {
        foreach (var renderTarget2D in _targets)
        {
            renderTarget2D.Value.Dispose();
        }

        _targets.Clear();
    }
}