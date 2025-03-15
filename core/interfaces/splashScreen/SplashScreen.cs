using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.core.config;
using RPGCreator.core.helpers;
using RPGCreator.core.plugins;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using System.Xml;
using MonoGame.Extended.Input;

namespace RPGCreator.core.interfaces.splashScreen
{
    class SplashScreen : InterfacesMain
    {
        enum LOADING_STATE
        {
            STARTING,
            STARTING_,
            LOADING_BASE,
            LOADING_BASE_,
            LOADING_FOLDERS,
            LOADING_FOLDERS_,
            CHECKING_PLUGINS,
            CHECKING_PLUGINS_,
            CHECKING_PROJECTS,
            CHECKING_PROJECTS_,
            DONE,
            DONE_,
            ERROR
        }

        public event EventHandler LoadingDone;

        LOADING_STATE state = LOADING_STATE.STARTING;
        string str_state = "";
        int loading_percentage = 0;
        List<string> loading_log = [];
        List<string> copyText = [];
        float auto_scroll_area = 4;

        // For CHECKING_PLUGINS state:
        int total_plugins_found = 0;
        int total_plugins_success = 0;
        List<string> plugin_unique_used = [];

        public SplashScreen(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
            Title = "Editor SplashScreen";
            Size = new(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);
            Position = new();
        }

        protected override void OnDraw()
        {
            Config.debug.b_BlockDebug = true;
            ImGui.SetNextWindowFocus();
            ImGui.SetNextFrameWantCaptureKeyboard(true);
            ImGui.SetNextFrameWantCaptureMouse(true);
            ImGui.Begin(Title, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar);
            ImGui.SetWindowSize(Size);
            ImGui.SetWindowPos(Position);
            switch(state)
            {
                case LOADING_STATE.STARTING:
                    {
                        str_state = "Starting loading...";
                        loading_log.Add($"Loading started at {DateTime.Now}, please wait.");
                        loading_percentage = 0;
                        state = LOADING_STATE.STARTING_;
                    } break;
                case LOADING_STATE.STARTING_:
                    state = LOADING_STATE.LOADING_BASE;
                    break;
                case LOADING_STATE.LOADING_BASE:
                    {
                        str_state = "Checking base content...";
                        if(BaseContent.GetGitignore() != "#Base Gitignore couldn't be found when generating it, please report it.")
                        {
                            loading_log.Add("Base Gitignore loaded...");
                        }

                        if(BaseContent.GetBasicFont() != null)
                        {
                            loading_log.Add("Basic font loaded...");
                        }

                        if(BaseContent.GetImGuiFont() != "")
                        {
                            loading_log.Add("ImGui font loaded...");
                        }
                        loading_percentage = 16;
                        state = LOADING_STATE.LOADING_FOLDERS;
                    } break;
                case LOADING_STATE.LOADING_FOLDERS:
                    {
                        str_state = "Loading base folders...";
                        loading_log.Add("-- Checking for folders in AppData --");
                        _CheckFolder(BaseContent.Folders.GetAppdata());
                        _CheckFolder(BaseContent.Folders.GetData());
                        _CheckFolder(BaseContent.Folders.GetConfig());
                        _CheckFolder(BaseContent.Folders.GetPlugins());
                        _CheckFolder(BaseContent.Folders.GetProjects());
                        _CheckFolder(BaseContent.Folders.GetInternals());
                        _CheckFolder(BaseContent.Folders.GetXSD());
                        _CheckFolder(BaseContent.Folders.GetXSDConfig());
                        _CheckFolder(BaseContent.Folders.GetXSDDev());
                        loading_log.Add("-- Checking for folders in the folder of the software --");
                        _CheckFolder(BaseContent.Folders.GetSoftware());
                        loading_percentage = 32;
                        state = LOADING_STATE.CHECKING_PLUGINS;
                    } break;
                case LOADING_STATE.CHECKING_PLUGINS:
                    {
                        str_state = "Checking for installed plugins...";
                        // First we need to check if the developers/plugin.xsd exist.
                        loading_log.Add("======================================");
                        loading_log.Add("Checking for schema file...");
                        string plugin_xsd = Path.Combine(BaseContent.Folders.GetXSDDev(), "plugin.xsd");
                        if(!File.Exists(plugin_xsd))
                        {
                            loading_log.Add("[ERROR] plugin schema file doesn't exist!");
                            state = LOADING_STATE.ERROR;
                            break;
                        }

                        XmlSchemaSet schemaSet = new();
                        schemaSet.Add(null, plugin_xsd);

                        XmlReaderSettings readerSettings = new();
                        readerSettings.Schemas.Add(schemaSet);
                        readerSettings.ValidationType = ValidationType.Schema;

                        loading_log.Add("======================================");
                        loading_log.Add($"Checking for plugins inside \"{BaseContent.Folders.GetPlugins()}\"...");
                        foreach(string path in Directory.GetDirectories(BaseContent.Folders.GetPlugins()))
                        {
                            if (_CheckPlugin(path, readerSettings))
                                continue;
                            else
                                break;
                        }
                        loading_percentage = 48;
                        state = LOADING_STATE.CHECKING_PROJECTS;
                    } break;
            }
            // ======================== Loading bar ========================
            ImGui_Helper.AlignNextText(str_state, ImGui_Helper.ALIGNEMENT.CUSTOM, 0.2f);
            ImGui.ProgressBar((float)(loading_percentage)/100, new Vector2(), str_state);
            ImGui.Text("Loading logs:");
            ImGui.BeginChild("logs", new(), ImGuiChildFlags.Borders, ImGuiWindowFlags.AlwaysVerticalScrollbar);

            int i = 0;
            foreach(string s in loading_log)
            {
                if(s.StartsWith("[ERROR]"))
                {
                    if (copyText.Count - 1 != i)
                    {
                        copyText.Add("Copy");
                    }
                    if (ImGui.Button($"{copyText[i]}##Copy{i}"))
                    {
                        TextCopy.ClipboardService.SetText(s);
                        copyText[i] = "Copied!";
                    }
                    if(!ImGui.IsItemHovered())
                    {
                        if (copyText[i] != "Copy")
                            copyText[i] = "Copy";
                    }
                    ImGui.SameLine();
                    ImGui.TextColored(new(1, 0, 0, 1), $">> {s}");
                    i++;
                    continue;
                }
                ImGui.TextWrapped($">> {s}");
            }
            ImGui.Text($"{ImGui.GetScrollY()}");
            if(ImGui.GetScrollY() >= ImGui.GetScrollMaxY() - auto_scroll_area)
            {
                ImGui.SetScrollHereY();
            }

            ImGui.EndChild();

            ImGui.End();
        }

        private void _CheckFolder(string path)
        {
            if (Directory.Exists(path))
            {
                loading_log.Add($"Folder {path} loaded.");
            } else
            {
                loading_log.Add($"[ERROR] Folder {path} couldn't be loaded!!");
                state = LOADING_STATE.ERROR;
            }
        }

        private bool _CheckPlugin(string path, XmlReaderSettings schema_reader)
        {
            loading_log.Add($"Found folder \"{path}\", checking for plugin config file...");

            string valid_path = "";

            {
                string base_config = Path.Combine(path, "plugin.xml");
                string custom_config = Path.Combine(path, $"{Path.GetFileName(path)}.xml");


                if (File.Exists(base_config))
                {
                    loading_log.Add("Found config \"plugin.xml\"");
                    valid_path = base_config;
                }
                else if (File.Exists(custom_config))
                {
                    loading_log.Add($"Found config \"{Path.GetFileName(path)}.xml\"");
                    valid_path = custom_config;
                }
                else
                {
                    loading_log.Add($"[ERROR] Folder \"{path}\" doesn't have any config file (either plugin.xml or {Path.GetFileName(path)}.xml)!!");
                    state = LOADING_STATE.ERROR;
                    return false;
                }
            }

            if(!_CheckConfig(valid_path, schema_reader))
            {
                state = LOADING_STATE.ERROR;
                return false;
            }

            return true;
        }

        private bool _CheckConfig(string config_path, XmlReaderSettings schema_reader)
        {
            XmlDocument xmlDoc = new();

            try
            {
                xmlDoc.Load(config_path);
            }
            catch (Exception ex)
            {
                loading_log.Add("[ERROR] XML File is either invalid, or there was an internal error. Please report with error message below!");
                loading_log.Add($"[ERROR] {ex.Message}");
                return false;
            }


            int error = 0;

            ValidationEventHandler handler = (_, e) =>
            {
                if (e.Severity == XmlSeverityType.Error || e.Severity == XmlSeverityType.Warning)
                {
                    loading_log.Add($"[ERROR] XML file {Path.GetFileName(config_path)} isn't a valid plugin config file.");
                    loading_log.Add($"[ERROR] {e.Message}");
                    error++;
                }
            };

            schema_reader.ValidationEventHandler += handler;

            using (XmlReader reader = XmlReader.Create(new System.IO.StringReader(xmlDoc.InnerXml), schema_reader))
            {
                while (reader.Read())
                {
                    if (reader.Name == "unique")
                    {
                        string value = reader.ReadElementContentAsString();
                        if (!plugin_unique_used.Contains(value))
                        {
                            plugin_unique_used.Add(value);
                        }
                        else
                        {
                            loading_log.Add($"[ERROR] A plugin with the unique name \"{value}\" already exist. Please report this to the PLUGIN developer.");
                            loading_log.Add($"[ERROR] A temporary solution could be to edit the unique with another one, but be aware that it could break other plugin!");
                            error++;
                        }
                    }
                }
            }
            if (error != 0)
            {
                return false;
            }
            schema_reader.ValidationEventHandler -= handler;
            return true;
        }

        protected override void OnUpdate()
        {
            throw new NotImplementedException();
        }

        public void HandleClientSizeChanged()
        {
            Size = new(graphics.Viewport.Width, graphics.Viewport.Height);
        }
    }
}
