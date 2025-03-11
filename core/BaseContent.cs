using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using RPGCreatorLib.ContentPipeline.TXT;
using Serilog;
using System;
using System.IO;
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

        static private void _ErrorMessage(string target, string actually_is)
        {
            Log.Logger.Error($"{target} is actually {actually_is}. Maybe the LoadBaseContent wasn't called?");
        }

        /// <summary>
        /// This allow the class to load content from the MGCB Pipeline.<br/>
        /// This should be called in the main process (Game.cs).
        /// </summary>
        /// <param name="content">The ContentManager to use .Load from</param>
        static public void LoadBaseContent(ContentManager content)
        {
            Gitignore = content.Load<TXTAsset>("BaseContent/.gitignore").Content;
            BasicFont = content.Load<BitmapFont>("Fonts/OpenSans-Regular");
            ImGuiFont = Path.Combine(content.RootDirectory, "Fonts/Roboto-Regular.ttf");
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
                _ErrorMessage("Gitignore", "empty");
                return "#Base Gitignore couldn't be found when generating it, please report it.";
            }

            return Gitignore;
        }
        #endregion GitIgnore

        #region BaseFont
        static private BitmapFont BasicFont;

        static public BitmapFont GetBasicFont()
        {
            if(BasicFont == null)
            {
                _ErrorMessage("BasicFont", "null");
                return null;
            }
            return BasicFont;
        }
        #endregion BaseFont

        #region ImGuiFont
        static private string ImGuiFont;

        static public string GetImGuiFont()
        {
            if(ImGuiFont == string.Empty || !File.Exists(ImGuiFont))
            {
                _ErrorMessage("ImGuiFont", "empty");
                return "";
            }
            return ImGuiFont;
        }
        #endregion ImGuiFont
    }
}
