using ImGuiNET;
using RPGCreator.core.interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.helpers
{
    static class ImGui_Helper
    {
        private static Dictionary<int, ImFontPtr> AvailFonts = [];
        public enum ALIGNEMENT
        {
            LEFT,
            RIGHT,
            CENTER,
            CUSTOM
        }

        /// <summary>
        /// Align the next item in ratio to a string.
        /// </summary>
        /// <param name="text_to_align">The string to align.</param>
        /// <param name="align">The alignement type (If set to custom, you may specify a custom offset with <paramref name="customOffset"/>).</param>
        /// <param name="customOffset">The custom offset (0 = LEFT / 0.5 = CENTER / 1 = RIGHT). Only needed if <paramref name="align"/> is equal to CUSTOM</param>
        static public void AlignNextText(string text_to_align, ALIGNEMENT align, float customOffset = 0f)
        {
            ImGuiStylePtr style = ImGui.GetStyle();

            float size = ImGui.CalcTextSize(text_to_align).X + style.FramePadding.X * 2.0f;
            float available = ImGui.GetContentRegionAvail().X;

            float offset_to_add;
            switch(align)
            {
                case ALIGNEMENT.LEFT:
                    offset_to_add = 0f;
                    break;
                case ALIGNEMENT.CENTER:
                    offset_to_add = .5f;
                    break;
                case ALIGNEMENT.RIGHT:
                    offset_to_add = 1f;
                    break;
                default:
                    offset_to_add = customOffset;
                    break;
            }
            float offset = (available - size) * offset_to_add;
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);
        }
    
        static public void AddFont(int size)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            AvailFonts.Add(size, io.Fonts.AddFontFromFileTTF(BaseContent.GetImGuiFont(), size));
        }

        /// <summary>
        /// Get a loaded font.
        /// </summary>
        /// <param name="size">The font size wanted.</param>
        /// <returns></returns>
        static public ImFontPtr GetFont(int size)
        {
            if(AvailFonts.Count == 0)
            {
                Log.Logger.Fatal($"No loaded font size for ImGui!!");
                return null;
            }

            if(!AvailFonts.ContainsKey(size))
            {
                Log.Logger.Error($"Couldn't found font size {size}");
            }

            return AvailFonts.GetValueOrDefault(size, AvailFonts.First().Value);
        }


        // TODO:
        // I need to move all of the file selector thing to his own class, it would allow me to not have so many shitty workaround with parent.metadata, etc...

        public struct FILE_SELECTOR_PARAMETERS
        {
            /// <summary>
            /// Allow to precise what file can be selected / shown.
            /// </summary>
            public List<string> ExtensionFilters = [];
            /// <summary>
            /// Limit the user from going anywhere other than the starting folder.
            /// </summary>
            public bool LimitOnlyToStartFolder = false;
            /// <summary>
            /// Limit the user from going to other folder than the starting folder and his children folder.
            /// </summary>
            public bool LimitToStartFolderAndChildren = false;
            /// <summary>
            /// The starting folder, if left to default, it will be the software directory.
            /// </summary>
            public string StartFolder = BaseContent.Folders.GetSoftware();

            public FILE_SELECTOR_PARAMETERS()
            {
            }
        }

        private struct FILE_SELECTOR_DATA
        {
            public bool HasBeenInit = false;
            public string CurrentSelectedFile;
            public List<string> PreviousPath = [];
            public string CurrentPath;

            public FILE_SELECTOR_DATA()
            {
            }
        }

        public struct FILE_SELECTOR_RETURN
        {
            /// <summary>
            /// If clicked on "Cancel"
            /// </summary>
            public bool Canceled = false;
            /// <summary>
            /// If clicked on "Confirm"
            /// </summary>
            public bool Confirmed = false;
            /// <summary>
            /// Files selected, only setted when Confirmed = true.
            /// </summary>
            public List<string> SelectedFiles = [];
            /// <summary>
            /// Path selected, only setted when Confirmed = true.
            /// </summary>
            public string SelectedPath = "";

            public FILE_SELECTOR_RETURN()
            {
            }
        }

        static public bool FileSelector(InterfacesMain parent, string text_button, string window_title, out FILE_SELECTOR_RETURN response, FILE_SELECTOR_PARAMETERS parameters = default)
        {
            bool return_response = false;
            response = new();
            if (!parent.metadata.ContainsKey(text_button))
            {
                parent.metadata[text_button] = false;
                parent.metadata[$"{text_button}-data"] = new FILE_SELECTOR_DATA();
            }
            bool current_state = (bool)parent.metadata[text_button];
            FILE_SELECTOR_DATA data = (FILE_SELECTOR_DATA)parent.metadata[$"{text_button}-data"];
            if(ImGui.Button($"{text_button}-{parent.Title}"))
            {
                if (!current_state)
                {
                    current_state = true;
                }
            }

            if(current_state)
            {

                if(!data.HasBeenInit)
                {
                    data.CurrentPath = parameters.StartFolder;
                }

                string path = data.CurrentPath;

                ImGui.Begin($"{window_title}##file_selector-{parent.Title}", ImGuiWindowFlags.NoCollapse);

                if(ImGui.InputTextWithHint("##FilePath", "", ref path, 255))
                {
                    if (Directory.Exists(path))
                    {
                        if (parameters.LimitToStartFolderAndChildren && path.StartsWith(parameters.StartFolder))
                        {
                            data.PreviousPath.Add(data.CurrentPath);
                            data.CurrentPath = path;
                        }
                    }
                }

                ImGui.SeparatorText(path);
                ImGui.BeginChild("FolderExplorer"); // TODO Finish this | For this I need to finish the moving part before (see upper TODO).
                if(Directory.Exists(path))
                {
                    string[] Directories = Directory.GetDirectories(path);
                    string[] Files = Directory.GetFiles(path);

                    ImGui.BeginTable("folder-content", 2, ImGuiTableFlags.BordersInner);
                    ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 20);
                    ImGui.TableSetupColumn("", ImGuiTableColumnFlags.None);

                    foreach (string dir in Directories)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.Text("Dir.");
                        ImGui.TableNextColumn();
                        ImGui.Text(Path.GetFileName(dir));
                    }
                    foreach (string file in Files)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.Text("File");
                        ImGui.TableNextColumn();
                        ImGui.Text(Path.GetFileName(file));
                    }

                    ImGui.EndTable();
                }
                ImGui.EndChild();
                ImGui.SeparatorText("");

                if(ImGui.Button("Confirm"))
                {
                    response.Confirmed = true;
                    response.SelectedPath = path;
                    return_response = true;
                }

                if(ImGui.Button("Cancel"))
                {
                    response.Canceled = true;
                    return_response = true;
                }

                ImGui.End();
            }

            parent.metadata[text_button] = current_state;

            parent.metadata[$"{text_button}-data"] = data;

            return return_response;

        }
    }
}
