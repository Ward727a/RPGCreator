using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.config
{
    // Manage all configuration related to logs
    class ConfigLog
    {

        // Folder where logs will be stored
        public string folderName = "";

        // File name
        public string fileName = "";

        public static Vector4 col_verbose = new(.25f, .25f, .25f, 1);
        public static Vector4 col_debug = new(.59f, .68f, .89f, 1);
        public static Vector4 col_info = new(.59f, .59f, .59f, 1);
        public static Vector4 col_warn = new(1, .64f, 0, 1);
        public static Vector4 col_err = new(.6f, 0, 0, 1);
        public static Vector4 col_fatal = new(1, 0, 0, 1);

        public bool b_logging = true;

    }
}
