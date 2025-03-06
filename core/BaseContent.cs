using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RPGCreatorLib.ContentPipeline.TXT;
using Serilog;
using System;
using System.Reflection.Metadata;

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
        /// <summary>
        /// This allow the class to load content from the MGCB Pipeline.<br/>
        /// This should be called in the main process (Game.cs).
        /// </summary>
        /// <param name="content">The ContentManager to use .Load from</param>
        static public void LoadBaseContent(ContentManager content)
        {
            Gitignore = content.Load<TXTAsset>("BaseContent/.gitignore").Content;
        }

        #region NullTexture
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
        #endregion NullTexture

        #region GitIgnore
        static private string Gitignore = string.Empty;

        static public string GetGitignore()
        {
            if(Gitignore == string.Empty)
            {
                Log.Logger.Error("Gitignore is actually empty. Maybe the LoadBaseContent wasn't called?");
                return "#Base Gitignore couldn't be found when generating it, please report it.";
            }

            return Gitignore;
        }
        #endregion GitIgnore
    }
}
