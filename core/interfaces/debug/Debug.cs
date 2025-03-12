using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.core.config;
using RPGCreator.core.debug;
using RPGCreator.core.helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.interfaces.debug
{
    class Debug : InterfacesMain
    {
        protected string _top_tip_text = "To hide this, press CTRL + ALT + D";
        public Debug(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
        }

        protected override void OnDraw()
        {
            if (!Config.debug.b_Menu)
            {
                if (Config.debug.b_Logger)
                {
                    ImDebug.Logger.RenderLogger();
                }
                if(Config.debug.b_InternMetrics)
                {
                    ImGui.ShowMetricsWindow();
                }
                return;
            };

            Position = new(0, 0);
            Size = new(graphics.Viewport.Width, graphics.Viewport.Height);

            ImGui.Begin("DebugMenuBackground", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar);

            ImGui.SetWindowPos(Position);
            ImGui.SetWindowSize(Size);
            ImGui.SameLine();
            ImGui_Helper.AlignNextText(_top_tip_text, ImGui_Helper.ALIGNEMENT.CENTER);
            ImGui.Text(_top_tip_text);

            ImGui.End();

            ImGui.SetNextWindowFocus();
            ImGui.Begin("DebugMenu", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);
            ImGui.SetWindowPos(new(20, 30));
            ImGui.SetWindowSize(new(Size.X - 40, Size.Y - 50));
            if (ImGui.Button("X Close menu"))
            {
                Config.debug.b_Menu = false;
            }

            if (ImGui.Button("Toggle logger"))
            {
                Config.debug.b_Logger = !Config.debug.b_Logger;
            }
            ImGui.SameLine();
            ImGui.Text($"Logger is: {(Config.debug.b_Logger ? "shown" : "hidden")}");

            if(ImGui.Button("Toggle internal ImGui Metrics"))
            {
                Config.debug.b_InternMetrics = !Config.debug.b_InternMetrics;
            }
            ImGui.SameLine();
            ImGui.Text($"Intern Metric is: {(Config.debug.b_InternMetrics ? "shown" : "hidden")}");

            ImGui.End();
        }

        protected override void OnUpdate()
        {
            throw new NotImplementedException();
        }
    }
}
