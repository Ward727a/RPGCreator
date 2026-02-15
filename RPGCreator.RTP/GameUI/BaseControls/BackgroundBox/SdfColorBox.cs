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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.RTP.GameUI.Layers;

namespace RPGCreator.RTP.GameUI.BaseControls.BackgroundBox;

public class SdfColorBox : SimpleColorBox
{

    private static Effect _sdfEffect = null!;
    
    public static void LoadSdfEffect(GraphicsDevice graphicsDevice)
    {
        var shaderBytes = BaseLayer.GetShaderByteCode("RoundedRect.glsl.mgfxo");

        _sdfEffect = new Effect(graphicsDevice, shaderBytes);
    }
    
    #region Events
    
    public event EventHandler<ControlPropertyChangingEventArgs<Color>>? OnBorderColorChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<Color>>? OnBorderColorChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<float>>? OnBorderThicknessChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<float>>? OnBorderThicknessChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<float>>? OnRadiusChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<float>>? OnRadiusChanged;
    
    #endregion

    protected override string _Name { get; set; } = "SdfColorBox";
    
    public float Radius
    {
        get;
        set
        {
            if (Equals(value, field)) return;
            var old = field;
            OnRadiusChanging?.Invoke(this, new (old, value));
            field = value;
            OnRadiusChanged?.Invoke(this, new (old, value));
            Invalidate();
        }
    } = 0f;

    public float BorderThickness
    {
        get;
        set
        {
            if (Equals(value, field)) return;
            var old = field;
            OnBorderThicknessChanging?.Invoke(this, new (old, value));
            field = value;
            OnBorderThicknessChanged?.Invoke(this, new (old, value));
            Invalidate();
        }
    } = 0f;

    public Color BorderColor
    {
        get;
        set
        {
            if (Equals(value, field)) return;
            var old = field;
            OnBorderColorChanging?.Invoke(this, new (old, value));
            field = value;
            OnBorderColorChanged?.Invoke(this, new (old, value));
            Invalidate();
        }
    } = Color.Black;

    public override void Draw(TimeSpan deltaTime, bool shouldEndDraw = true)
    {
        if (!AbsoluteVisibility || !ShouldDrawn || OwningLayer == null) return;
        _sdfEffect.Parameters["Size"].SetValue(new Vector2(GlobalsBounds.Width, GlobalsBounds.Height));
        _sdfEffect.Parameters["Radius"].SetValue(Radius);
        _sdfEffect.Parameters["Thickness"]?.SetValue(BorderThickness);
        _sdfEffect.Parameters["OutlineColor"]?.SetValue(BorderColor.ToVector4());
        
        Renderer.SpriteBatch.End();

        {
            Renderer.SpriteBatch.Begin(effect: _sdfEffect, rasterizerState: OwningLayer!.ClippingRasterizerState);

            Renderer.SpriteBatch.Draw(Renderer.PixelTexture, GlobalsBounds, BackgroundColor * (AbsoluteAlpha / 255f));

            Renderer.SpriteBatch.End();
        }

        Renderer.SpriteBatch.Begin(rasterizerState: new RasterizerState { ScissorTestEnable = true });
        DirectBaseDraw(deltaTime, false);
        
        if (shouldEndDraw)
            EndContainerDraw();
    }
}