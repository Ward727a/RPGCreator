using Microsoft.Xna.Framework.Graphics;
using System;

/*
 * All content here need to be hardcoded or inside the MGCB, No file path is allowed!!
 */

namespace RPGCreator.core
{
    /// <summary>
    /// This contains all base content for the engine like NullTexture.<br/><br/>
    /// All variables here are static, to only be allocated one time in memory.<br/><br/>
    /// </summary>
    class BaseContent
    {
        static private Texture2D NullTexture = null;

        static public Texture2D GetNullTexture()
        {
            if (NullTexture == null)
            {
                NullTexture = new Texture2D(Game1.GetGraphicDevice(), 32, 32);
                Microsoft.Xna.Framework.Color[] data = new Microsoft.Xna.Framework.Color[32 * 32];
                Array.Fill(data, Microsoft.Xna.Framework.Color.DeepPink);
                NullTexture.SetData(data);
            }

            return NullTexture;
        }
    }
}
