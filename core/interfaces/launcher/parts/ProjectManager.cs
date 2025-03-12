using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.core.config;
using RPGCreator.core.helpers;
using RPGCreator.core.io.datas;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace RPGCreator.core.interfaces.launcher.parts
{
    class ProjectManager : InterfacesMain
    {
        protected int _project_number = 0;
        protected bool _is_project_loaded = false;
        protected string _project_search_input = "";

        public ProjectManager(GraphicsDevice graphicsDevice, float AboutSizeY, float PluginSizeX) : base(graphicsDevice)
        {
            Title = "Launcher-ProjectsManager";
            SetSize((graphicsDevice.Viewport.Width - PluginSizeX), graphicsDevice.Viewport.Height - AboutSizeY);
            Position = new(0, 0);
            LoadProject();
        }

        public override void Show()
        {
            base.Show();
            if(!_is_project_loaded)
            {
                LoadProject();
            }
        }

        private void LoadProject()
        {

            _project_number = Projects.GetProjectCount();
            _is_project_loaded = true;
        }

        protected override void OnDraw()
        {
            ImGui.Begin(Title, ref opened, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBringToFrontOnFocus);
            ImGui.SetWindowPos(Position);
            ImGui.SetWindowSize(Size);

            // =================== Projects Manager =======================
            ImGui.PushFont(ImGui_Helper.GetFont(16));
            ImGui_Helper.AlignNextText($"Projects Manager", ImGui_Helper.ALIGNEMENT.CENTER);
            ImGui.Text($"Projects Manager");
            ImGui.PopFont();

            // ------------------------------------------------------------
            ImGui.Separator();

            // [New Project] [Search] [Open folder] [Settings] ============
            ImGui.Button("New Project", new Vector2());
            ImGui.SameLine();
            ImGui.InputTextWithHint("##ProjectSearch", "Search for a project...", ref _project_search_input, 255);
            ImGui.SameLine();
            //ImGui_Helper.AlignNextText("Open folder Settings    ", ImGui_Helper.ALIGNEMENT.RIGHT);
            if(ImGui.Button("Open folder"))
            {
                //TODO: Replace this by a more "multi-platform" way | This only work on windows.
                Process.Start("explorer.exe", @$"{BaseContent.Folders.GetProjects()}");
            }
            ImGui.SameLine();
            ImGui.Button("Settings");

            // ------------------------------------------------------------
            ImGui.Separator();

            ImGui.BeginChild("ProjectsList", new Vector2(), ImGuiChildFlags.None, ImGuiWindowFlags.AlwaysVerticalScrollbar);

            if (!_is_project_loaded)
            {
                // ======================== Loading bar ========================
                ImGui_Helper.AlignNextText("Loading projects...", ImGui_Helper.ALIGNEMENT.CUSTOM, 0.2f);
                ImGui.ProgressBar((float)(ImGui.GetTime() * -0.25f), new Vector2(), "Loading projects...");
            } else
            {
                if(_project_number == 0)
                {
                    ImGui_Helper.AlignNextText("You don't have any project (for now).", ImGui_Helper.ALIGNEMENT.CENTER);
                    ImGui.Text("You don't have any project (for now).");

                    ImGui_Helper.AlignNextText("Create a new project", ImGui_Helper.ALIGNEMENT.CENTER);
                    ImGui.Button("Create a new project");
                } else
                {
                    ImGui.Text("This part is still WIP.");
                }
            }

                ImGui.EndChild();

            ImGui.End();
        }

        protected override void OnUpdate()
        {
            throw new NotImplementedException();
        }

        public void UpdateSize(float AboutSizeY, float PluginSizeX)
        {
            SetSize((graphics.Viewport.Width - PluginSizeX), graphics.Viewport.Height - AboutSizeY);
        }
    }
}
