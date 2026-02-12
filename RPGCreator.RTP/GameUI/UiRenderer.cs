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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;
using RPGCreator.RTP.GameUI.BaseControls;
using RPGCreator.RTP.GameUI.BaseControls.Containers;

namespace RPGCreator.RTP.GameUI;

public class UiRenderer
{
    public GraphicsDevice GraphicsDevice { get; private set; }
    
    private readonly RasterizerState _clippingRasterizerState = new RasterizerState { ScissorTestEnable = true };
    
    public void Initialize(GraphicsDevice graphicsDevice)
    {
        GraphicsDevice = graphicsDevice;
    }
    
    public void DrawDebugBounds(SpriteBatch sb, BaseControl control)
    {
        if (!control.AbsoluteVisibility) return;

        var color = GetDebugColor(control);
        
        if(!control.ShouldDrawn)
            color = new Color(color.R*.8f, color.G*.8f, color.B*.8f, (0.5f));
        
        sb.DrawRectangle(control.GlobalsBounds, color, 1f);
        
        sb.DrawCircle(control.GlobalsBounds.Location.ToVector2() + control.Origin, 2f, 8, color, 2f);
        
        Rectangle currentScissor = Rectangle.Empty;
        bool skipDueToClip = false;
        
        if(control is ContainerControl { ClipsToBounds: true })
        {
            currentScissor = GraphicsDevice.ScissorRectangle;
            var newScissor = Rectangle.Intersect(currentScissor, control.GlobalsBounds);
            if (!newScissor.IsEmpty)
            {
                sb.End();
                GraphicsDevice.ScissorRectangle = newScissor;
                sb.Begin(rasterizerState: _clippingRasterizerState);
            }
            else
            {
                skipDueToClip = true;
            }
        }

        if (!skipDueToClip && control is ContainerControl container)
        {
            foreach (var child in container.Children)
            {
                DrawDebugBounds(sb, child);
            }

            if (container.ClipsToBounds)
            {
                sb.End();
                GraphicsDevice.ScissorRectangle = currentScissor;
                sb.Begin(rasterizerState: _clippingRasterizerState);
            }
        }
    }

    public Color GetDebugColor(BaseControl control)
    {
        var typeName = control.GetType().Name;
        var hash = typeName.GetHashCode();
        return new Color((hash & 0xFF0000) >> 16, (hash & 0x00FF00) >> 8, hash & 0x0000FF);
    }
}