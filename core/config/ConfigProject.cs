using RPGCreator.core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.config
{
    class ConfigProject
    {
        public struct ProjectData(string name)
        {
            public string Name = name;
            public string ShortDescription = "";
            public string Description = "";
            public ResourcePath IconPath;

        }

        public ProjectData Info = new("NoNamedDefined");
        public string BasePath = "";
    }
}
