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
using Microsoft.Xna.Framework.Graphics;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RPGCreator.Core.Type.Assets
{
    public class ImageAsset : BaseAsset
    {


        internal Image _Image;
        protected string _ImagePathCached;
        protected Texture2D _TextureCache;
        public string ImagePath { get; set; }
        public int Width;
        public int Height;

        public ImageAsset() : base()
        { }

        public ImageAsset(string Name) : base(Name)
        {
        }

        public override void FromBase()
        {
            base.FromBase();
            if (!AssetsTypeHelper.IsImageCompatible(Type))
            {
                throw new InvalidOperationException($"Asset type is not image-compatible. Actual type: {Type}");
            }

            XElement data = AssetData;

            string file_path = data.Element("file_path")?.Value ?? "UNKNOWN";

            if (string.IsNullOrEmpty(file_path) || file_path == "UNKNOWN")
            {
                throw new Exception($"File path is null or empty.");
            }

            if (!File.Exists(file_path))
            {
                throw new Exception($"File at {file_path} doesn't exist.");
            }

            try
            {
                Image image = Image.Load(file_path);

                ImagePath = file_path;
                _Image = image;
                Width = image.Width;
                Height = image.Height;
            }
            catch (Exception e)
            {
                throw new Exception($"File at {file_path} is not a valid image.", e);
            }
        }

        public virtual Texture2D GetTexture(GraphicsDevice device)
        {
            if(device == null)
            {
                throw new ArgumentNullException(nameof(device), "GraphicsDevice cannot be null.");
            }

            if (_Image == null)
            {
                throw new InvalidOperationException("Image is not loaded.");
            }

            if(_TextureCache != null && _ImagePathCached == ImagePath)
            {
                return _TextureCache;
            }

            try
            {
                // Convert ImageSharp image to Texture2D
                using (var ms = new MemoryStream())
                {
                    _Image.SaveAsPng(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    _TextureCache = Texture2D.FromStream(device, ms);
                    _ImagePathCached = ImagePath;
                    return _TextureCache;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to convert Image to Texture2D.", e);
            }
        }
    }
}
