using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.helpers
{
    class SpriteBatchExtended : SpriteBatch
    {
        public bool IsBegin { get; private set; }

        public new void Begin(SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Effect effect = null, Matrix? transformMatrix = null)
        {
            base.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);
            IsBegin = true;
        }

        public new void End()
        {
            base.End();
            IsBegin = false;
        }

        public SpriteBatchExtended(GraphicsDevice graphicsDevice) : base(graphicsDevice, 0)
        {
            
        }
    }
}
