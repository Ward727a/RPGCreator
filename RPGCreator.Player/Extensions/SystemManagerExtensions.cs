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
using RPGCreator.SDK;
using RPGCreator.SDK.ECS.Systems;

namespace RPGCreator.Player.Extensions;

public static class SystemManagerExtensions
{
    public static void Draw(this SystemManager self, TimeSpan deltaTime, SpriteBatch spriteBatch)
    {        
        var drawingSystems = self.GetDrawingSystems();
        
        spriteBatch.Begin(
            SpriteSortMode.Deferred, 
            BlendState.AlphaBlend, 
            SamplerState.PointClamp, 
            DepthStencilState.None, 
            RasterizerState.CullNone, // Force le rendu même si MonoGame hésite
            null, 
            transformMatrix: RuntimeServices.CameraService.GetViewMatrix().ToXnaFast()
        );
        foreach (var drawingSystem in drawingSystems)
        {
            drawingSystem.Update(deltaTime);
        }
        var render = RuntimeServices.RenderService;
        render.DrawDebugLine(new System.Numerics.Vector2(-10000, 0), new System.Numerics.Vector2(10000, 0), 2f, Color.Red.ToSystemFast());
        render.DrawDebugLine(new System.Numerics.Vector2(0, -10000), new Vector2(0, 10000).ToNumerics(), 2f, Color.Green.ToSystemFast());
        
        spriteBatch.End();
    }
}