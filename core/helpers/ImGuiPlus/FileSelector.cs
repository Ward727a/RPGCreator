using ImGuiNET;
using MonoGame.Extended.Input;
using RPGCreator.core.interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.helpers.ImGuiPlus
{
    class FileSelector
    {
        public string ButtonContent = "";
        public string ParentTitle = "";
        public string WindowTitle = "";
        public FILE_SELECTOR_PARAMETERS Parameters = new();

        // ------------------------------------------------ //

        public event EventHandler OnOpen;
        public event EventHandler<FILE_SELECTOR_RETURN> OnConfirmed;
        public event EventHandler OnCanceled;

        // ------------------------------------------------ //

        public bool IsOpened = false;
        public List<string> CurrentSelectedFile = [];
        public List<string> PreviousPath = [];
        public string CurrentPath;
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

        public struct FILE_SELECTOR_RETURN
        {
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

        private FileModal

        public FileSelector(InterfacesMain parent, string text_button, string window_title, FILE_SELECTOR_PARAMETERS parameters = default)
        {

            ButtonContent = text_button;
            ParentTitle = parent.Title;
            WindowTitle = window_title;
            Parameters = parameters;

            if(parameters.StartFolder == string.Empty)
            {
                CurrentPath = BaseContent.Folders.GetSoftware();
            } else
            {
                CurrentPath = parameters.StartFolder;
            }
        }
    
    
        public void Draw()
        {
            if(ImGui.Button($"{ButtonContent}##{ParentTitle}"))
            {
                if(!IsOpened)
                {
                    OnOpen?.Invoke(this, null);
                    IsOpened = true;
                }
            }

            if(IsOpened)
            {
                DrawModal();
            }
        }

        private void DrawModal()
        {
            string path = CurrentPath;

            ImGui.Begin($"{WindowTitle}##file_selector-{ParentTitle}", ImGuiWindowFlags.NoCollapse);
            Vector2 CurrentWindowSize = ImGui.GetWindowSize() - new Vector2(0 , 150);

            float Start_Y = ImGui.GetFrameHeightWithSpacing()*2;
            if (ImGui.InputTextWithHint("##FilePath", "", ref path, 1023))
            {
                if (Directory.Exists(path))
                {

                    if(Parameters.LimitToStartFolderAndChildren && path.StartsWith(Parameters.StartFolder))
                    {
                        PreviousPath.Add(CurrentPath);
                        CurrentPath = path;
                    }

                }
            }

            ImGui.SeparatorText(path);

            float End_Y = ImGui.GetContentRegionAvail().Y - Start_Y;

            ImGui.BeginChild("##TestAA", new(0, End_Y));
            if(Directory.Exists(path))
            {
                string[] Directories = Directory.GetDirectories(path);
                string[] Files = Directory.GetFiles(path);

                ImGui.BeginTable("folder-content", 2, ImGuiTableFlags.BordersInner);
                ImGui.TableSetupColumn("type", ImGuiTableColumnFlags.WidthFixed, 20);
                ImGui.TableSetupColumn("path", ImGuiTableColumnFlags.None);

                foreach (string dir in Directories)
                {
                    ImGui.TableNextRow();
                    if (CurrentSelectedFile.Contains(dir))
                    {
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg1, ImGui.ColorConvertFloat4ToU32(new(1, 0, 0, 1)));
                    }
                    ImGui.TableNextColumn();
                    ImGui.Text("Dir.");
                    ImGui.TableNextColumn();
                    if(ImGui.Selectable(Path.GetFileName(dir)))
                    {
                        if (KeyboardExtended.GetState().IsShiftDown())
                        {
                            CurrentSelectedFile.Add(dir);
                        }
                        else
                        {
                            CurrentSelectedFile.Clear();
                            CurrentSelectedFile.Add(dir);
                        }
                    }
                }
                foreach (string file in Files)
                {
                    ImGui.TableNextRow();
                    if (CurrentSelectedFile.Contains(file))
                    {
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, ImGui.ColorConvertFloat4ToU32(new(.3f, .3f, .7f, .4f)));
                    }
                    ImGui.TableNextColumn();
                    ImGui.Text("File");
                    ImGui.TableNextColumn();
                    if(ImGui.Selectable(Path.GetFileName(file)))
                    {
                        if(KeyboardExtended.GetState().IsShiftDown())
                        {
                            CurrentSelectedFile.Add(file);
                        }
                        else
                        {
                            CurrentSelectedFile.Clear();
                            CurrentSelectedFile.Add(file);
                        }
                    }
                }

                ImGui.EndTable();
            }
            ImGui.EndChild();
            ImGui.SeparatorText("");

            if (ImGui.Button("Confirm"))
            {
                FILE_SELECTOR_RETURN return_data = new()
                {
                    SelectedFiles = CurrentSelectedFile,
                    SelectedPath = CurrentPath
                };

                OnConfirmed?.Invoke(this, return_data);
                IsOpened = false;
                // Add confirm event here.
            }
            ImGui.SameLine();
            if(ImGui.Button("Cancel"))
            {
                OnCanceled?.Invoke(this, null);
                // Add response event here.
            }
            ImGui.End();
        }
    
        public void OpenModal()
        {
            IsOpened = true;
        }

        public void CloseModal()
        {
            IsOpened = false;
        }
    }
}
