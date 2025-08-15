#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
// 
// This file is part of RPG Creator and is distributed under the MIT License.
// You are free to use, modify, and distribute this file under the terms of the MIT License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence MIT.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence MIT.
// Voir LICENSE pour plus de détails.
// 
// Contact:
// => Mail: Ward727a@gmail.com
//    Please use this object: "RPG Creator [YourObject]"
// => Discord: ward727
// 
// For urgent inquiries, sending both an email and a message on Discord is highly recommended for a quicker response.
// 
// 
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.Core.Type.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.Core.Type.Objects
{
    public class Scene : BaseDrawable
    {
        public MapInstance? Map { get; private set; } = new MapInstance();

        protected override void _Draw(SpriteBatchExtend? sb)
        {
            if(!IsVisible)
                return;

            // Draw the scene here

            if(ClipChildren)
            {
                sb.ChildBegin(
                    sortMode: SpriteSortMode.Deferred,
                    blendState: BlendState.AlphaBlend,
                    samplerState: SamplerState.PointClamp,
                    depthStencilState: DepthStencilState.Default,
                    rasterizerState: sb.GraphicsDevice.RasterizerState
                );

                sb.GraphicsDevice.ScissorRectangle = Bounds;
            }

            foreach (var child in Children)
            {
                if (child.IsVisible)
                {
                    child.Draw(sb);
                }
            }

            if(ClipChildren)
                sb.ChildEnd();
        }

        protected override void _Update(GameTime gameTime)
        {
            foreach (var child in Children)
            {
                if(!child.IsActive)
                    continue;
                child.Update(gameTime);
            }
        }
    }
}
