using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.config
{
    // Manage all configuration related to logs
    class ConfigLog
    {
        public struct LogSetting
        {
            public Color prefix_color;
            public Color text_color;
            public bool toggle;
            public string prefix;

            public LogSetting(Color _prefix_color, Color _text_color, bool _toggle, string _prefix)
            {
                prefix_color = _prefix_color;
                text_color = _text_color;
                toggle = _toggle;
                prefix = _prefix;
            }

            public LogSetting(LogSetting setting)
            {
                prefix_color = setting.prefix_color;
                text_color = setting.text_color;
                toggle = setting.toggle;
                prefix = setting.prefix;
            }
        }

        // Folder where logs will be stored
        public string folderName = "";

        // File name
        public string fileName = "";

        // Config for each log type available
        public LogSetting info = new(Color.SteelBlue, Color.LightSteelBlue, true, "[INFO] ");
        public LogSetting normal = new(Color.Gray, Color.LightGray, true, "[NORMAL] ");
        public LogSetting warning = new(Color.DarkOrange, Color.Orange, true, "[WARNING] ");
        public LogSetting error = new(Color.DarkRed, Color.Red, true, "[ERROR] ");
        public LogSetting success = new(Color.DarkOliveGreen, Color.ForestGreen, true, "[SUCCESS] ");
        public LogSetting debug = new(Color.DarkMagenta, Color.Magenta, true, "[DEBUG] ");

        public bool b_logging = true;

    }
}
