using RPGCreator.core.Types;
using System;
using System.Collections.Generic;
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

        public static Vector4 col_verbose = Color.DarkGray.Value;
        public static Vector4 col_debug = Color.LightMagenta.Value;
        public static Vector4 col_info = Color.LightGray.Value;
        public static Vector4 col_warn = Color.Yellow.Value;
        public static Vector4 col_err = Color.LightRed.Value;
        public static Vector4 col_fatal = Color.Red.Value;

        public bool b_logging = true;

    }
}
