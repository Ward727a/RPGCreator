#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
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
// 
// 
#endregion
using Avalonia.Media.Imaging;
using Microsoft.Xna.Framework.Graphics;
using SixLabors.ImageSharp;
using System.Xml.Linq;

/*
 * ImageAsset.cs
 * =============
 * This file defines the ImageAsset class, which represents an image asset in the RPG Creator engine.
 * It includes properties for the image path, width, height, and methods to load the image
 *
 * DevNote:
 * This class should be refactored to use the SkiaSharp library for better performance and compatibility.
 * [Ward727, 30/07/2025]
 * 
 */

namespace RPGCreator.Core.Types.Assets
{
    public class ImageAsset : BaseAsset
    {
        public event EventHandler? ImageChanged;

        internal Image? _Image;
        protected string _ImagePathCached;
        protected Texture2D? _TextureCache;
        protected Bitmap? _BitmapCache;
        private string _ImagePath;
        public string ImagePath { 
            get => _ImagePath; 
            set
            {
                if (_ImagePath != value)
                {
                    _ImagePath = value;
                    _ImagePathCached = string.Empty; // Reset cache when path changes
                    _TextureCache = null; // Reset texture cache
                    _BitmapCache = null; // Reset bitmap cache
                    _Image = null; // Reset image
                    try
                    {
                        Image image = Image.Load(_ImagePath);
                        _Image = image;
                        ImageWidth = image.Width;
                        ImageHeight = image.Height;
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error loading image from path '{value}': {e.Message}");
                        Console.ResetColor();
                    }
                    ImageChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }

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

            string file_path;

            if(data == null)
            {
                file_path = ImagePath;
            } else
            {
                file_path = data.Element("file_path")?.Value ?? "UNKNOWN";
            }


            if (string.IsNullOrEmpty(file_path) || file_path == "UNKNOWN")
            {
                throw new Exception($"File path is null or empty.");
            }

            if (!File.Exists(file_path))
            {
                throw new Exception($"File at {file_path} doesn't exist.");
            }

            ImagePath = file_path;
        }

        protected void LoadImageFromPath(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                throw new ArgumentException("Invalid image path.", nameof(path));
            }

            try
            {
                _Image = Image.Load(path);
                ImageWidth = _Image.Width;
                ImageHeight = _Image.Height;
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to load image from path '{path}'.", e);
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
                FromBase();
                //throw new InvalidOperationException("Image is not loaded.");
            }

            if(_TextureCache != null && _ImagePathCached == ImagePath)
            {
                return _TextureCache;
            }

            try
            {
                // Convert ImageSharp image to Texture2D
                using var ms = new MemoryStream();
                _Image.SaveAsPng(ms);
                ms.Seek(0, SeekOrigin.Begin);
                _TextureCache = Texture2D.FromStream(device, ms);
                _ImagePathCached = ImagePath;
                return _TextureCache;
            }
            catch (Exception e)
            {
                throw new Exception("Failed to convert Image to Texture2D.", e);
            }
        }
        
        public virtual Bitmap GetBitmap(bool forceReload = false)
        {
            if(_BitmapCache != null)
            {
                return _BitmapCache;
            }
            _BitmapCache = new Bitmap(ImagePath);
            return _BitmapCache;
        }

    }
}
