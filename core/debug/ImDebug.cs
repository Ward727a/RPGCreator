using ImGuiNET;
using RPGCreator.core.config;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace RPGCreator.core.debug
{
    static class ImDebug
    {
        public static class Logger
        {
            struct LogEntry(string message, Vector4 color, int level)
            {
                public string message = message;
                public Vector4 col = color;
                public int level = level;
            }

            private readonly static Queue<LogEntry> logs = new();
            private readonly static int MAX_LOG_ENTRIES = 1000;
            private readonly static int MIN_X_SIZE = 470;

            // 0 = all
            // 1 = Fatal
            // 2 = Error
            // 3 = Warn
            // 4 = Info
            // 5 = Debug
            // 6 = Verbose
            private static int log_show_choice = 0;
            private static string search_value = "";

            public static void RenderLogger()
            {
                ImGui.Begin("Logger");
                if (ImGui.GetWindowSize().X < MIN_X_SIZE) // Check if the size is at least the minimum size, or else radio button will go outside the box
                {
                    ImGui.SetWindowSize(new Vector2(MIN_X_SIZE, ImGui.GetWindowSize().Y));
                }

                // Allow selection of log level
                RenderButton("All", 0);
                RenderButton("Fatal", 1);
                RenderButton("Error", 2);
                RenderButton("Warn", 3);
                RenderButton("Info", 4);
                RenderButton("Debug", 5);
                ImGui.RadioButton("Verbose", ref log_show_choice, 6);

                // Allow searching in logs
                ImGui.InputText("Search", ref search_value, 255);
                ImGui.Separator();

                foreach (LogEntry log in logs)
                {
                    if ((log.message.Contains(search_value, StringComparison.CurrentCultureIgnoreCase) || search_value == "") && (log_show_choice == 0 || log.level == log_show_choice))
                    {
                        ImGui.TextColored(log.col, log.message);
                    }
                }

                if (!Config.debug.b_ImGui)
                {
                    ImGui.Text("Debug with ImGui disabled!");
                }

                ImGui.End();
            }

            // Just a small wrapper to not have to repeat those 2 lines
            private static void RenderButton(string label, int value)
            {
                ImGui.RadioButton(label, ref log_show_choice, value);
                ImGui.SameLine();
            }

            public static void AddLog(string msg, Vector4 color, int level)
            {
                if (logs.Count >= MAX_LOG_ENTRIES)
                    logs.Dequeue();

                logs.Enqueue(new LogEntry(msg, color, level));
            }

            public static void AddWarn(string msg)
            {
                AddLog(msg, ConfigLog.col_warn, 3);
            }

            public static void AddFatal(string msg)
            {
                AddLog(msg, ConfigLog.col_fatal, 1);
            }

            public static void AddInfo(string msg)
            {
                AddLog(msg, ConfigLog.col_info, 4);
            }

            public static void AddError(string msg)
            {
                AddLog(msg, ConfigLog.col_err, 2);
            }

            public static void AddVerbose(string msg)
            {
                AddLog(msg, ConfigLog.col_verbose, 6);
            }

            public static void AddDebug(string msg)
            {
                AddLog(msg, ConfigLog.col_debug, 5);
            }
        }
    }
}
