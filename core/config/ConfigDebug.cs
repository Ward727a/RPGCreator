using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.config
{
    // Manage all configuration related to debuging
    class ConfigDebug
    {

        // Block the debug menu
        public bool b_BlockDebug = false;

        // Enable/Disable ImGui
        public bool b_ImGui = true;

        // Show/Hide debugger menu
        public bool b_Menu = false;

        // Show/Hide logger
        public bool b_Logger = false;

        // Show/Hide internal ImGui Metrics
        public bool b_InternMetrics = false;
    }
}
