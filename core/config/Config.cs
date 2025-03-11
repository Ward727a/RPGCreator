using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.config
{
    static class Config
    {
        public static ConfigEditor editor = new();
        public static ConfigLog log = new();
        public static ConfigDebug debug = new();
        public static ConfigProject currentProject = new();
        public static ConfigPlugins plugins = new();
    }
}
