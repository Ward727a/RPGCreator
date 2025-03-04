using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.Types
{
    class Resource
    {
        public enum ResourcesTypes
        {
            SCRIPT,
            IMAGES,
            AUDIOS,
            VIDEOS,
            FONTS,
            SHADERS,
            TEXTURES,
            LOCALES, // For translations
            DATA, // For data files (SQLite)
            PLUGINS,
            CUSTOM,
            MAX
        }

        static Dictionary<string, int> CustomResourcesTypes = [];

        public string Path;
        public ResourcesTypes Type = ResourcesTypes.MAX;
        public int CustomType = -1;

        public static int GetCustomType(string idx)
        {
            if (CustomResourcesTypes.ContainsKey(idx))
            {
                return CustomResourcesTypes[idx];
            }
            Log.Logger.Error($"Custom type \"{idx}\" not found.");
            return -1;
        }

        public static int AddCustomType(string idx)
        {
            if (!CustomResourcesTypes.ContainsKey(idx))
            {
                if(CustomResourcesTypes.TryAdd(idx, CustomResourcesTypes.Count))
                {
                    return CustomResourcesTypes[idx];
                } else
                {
                    Log.Logger.Error($"Unknown error when trying to add custom type \"{idx}\".");
                }
            }
            Log.Logger.Error($"Custom type \"{idx}\" already exist.");
            return -1;
        }
    }
}