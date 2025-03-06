using Autofac.Core;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.core.types.objects;
using Serilog;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.types.objects.resources
{
    class ResourcesImages : Resource
    {

        public Image Image = null;
        public Size Size = new();
        private Texture2D Texture = null;

        public ResourcesImages(string path = "")
        {
            Path = path;
            Type = ResourcesTypes.IMAGES;

            if (!Path.IsValid)
            {
                Log.Logger.Error($"Tried loading Image at path \"{path}\", but it doesn't exist.");
                return;
            }

            Image = Image.Load(path);
            Size = Image.Size;
        }

        public override ResourcesImages Duplicate()
        {
            ResourcesImages RI = (ResourcesImages)base.Duplicate();

            RI.Size = Size;
            RI.Image = Image;

            return RI;
        }

        public override string ToString()
        {
            return $"#{ID}# ResourceImage(Path: '{Path.Path}', {Size})";
        }

        public Texture2D GetTexture2D()
        {
            if (!Path.IsValid)
            {
                return BaseContent.GetNullTexture();
            }
            Texture ??= Texture2D.FromFile(Game1.GetGraphicDevice(), GetPath());
            return Texture;
        }
    }
}
