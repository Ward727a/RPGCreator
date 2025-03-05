using Serilog;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.Types.Resources
{
    class ResourcesImages : Resource
    {

        Image image = null;
        Size Size = new();

        public ResourcesImages(string path = "")
        {
            Path = new(path);
            Type = ResourcesTypes.IMAGES;

            if (!File.Exists(path))
            {
                Log.Logger.Error($"Tried loading image at path \"{path}\", but it doesn't exist.");
                return;
            }

            image = Image.Load(path);
            Size = image.Size;
        }
    }
}
