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

using Arch.Buffer;
using Arch.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.RuntimeLib.ECS.Components;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.Scenes;

namespace RPGCreator.RuntimeLib.ECS.System;

public class SkyRenderSystem(World world, CommandBuffer commandBuffer, ISceneRender sceneRender, GraphicsDevice graphicsDevice) : RpgcBaseSystem(world, commandBuffer, sceneRender)
{
    private QueryDescription skyQuery = new QueryDescription().WithAll<SkyComponent>();
    private Texture2D? SkyTexture;

    public override void Initialize()
    {
        SkyTexture = new Texture2D(graphicsDevice, 1, 1);
        SkyTexture.SetData([Color.White]);
        SkyTexture.Name = "SkyTexture";
    }

    public override void Update(in double t)
    {
        if (SkyTexture is null)
            return;

        World.Query(in skyQuery, (ref SkyComponent skyC) =>
        {
            var drawSizeX = skyC.MapSize.X * 32f;
            var drawSizeY = skyC.MapSize.Y * 32f;
            
            var color = Color.FromNonPremultiplied((int)Math.Floor(skyC.Color.X * 255), (int)Math.Floor(skyC.Color.Y * 255), (int)Math.Floor(skyC.Color.Z * 255), 255);

            sceneRender.SubmitSprite(SkyTexture, new Vector2(0, 0), new Rectangle(0, 0, (int)drawSizeX, (int)drawSizeY), color, -1f, int.MinValue);
        });
    }

    public override void Dispose()
    {
        SkyTexture?.Dispose();
        SkyTexture = null;
        GC.SuppressFinalize(this);
    }
}