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
using MonoGame.Extended;
using RPGCreator.Core.Inputs.Mouse;
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.Core.Type.Assets;
using RPGCreator.Core.Type.Map.Interactable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.Core.Type.Map
{
    public class Tile(Tileset tileset, Rectangle uv) : BaseInteractable
    {
        public Tileset Tileset { get; private set; } = tileset;
        public Rectangle UV { get; private set; } = uv;
        
        public bool SelectedTile { get; set; } = false;
        private int TaskSelectedID = -1;

        protected override void _Draw(SpriteBatchExtend sb)
        {
            sb.Draw(Tileset.GetTexture(sb.GraphicsDevice), Position, UV, Color.White);
        }

        protected override void _Update(GameTime gameTime)
        {
            CheckMouse();
        }

        protected override void OnClick()
        {
            if(Parent is MapLayer layer)
            {
                layer.SelectTile(this);
                Selected();
            }
        }

        public virtual void Selected()
        {
            // TODO
        }
    }
}
