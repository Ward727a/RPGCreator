using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.core.helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.interfaces.launcher.parts
{
    class PluginsList : InterfacesMain
    {
        public PluginsList(GraphicsDevice graphicsDevice, float AboutSizeY) : base(graphicsDevice)
        {
            Title = "Launcher-PluginsPart";
            MinSize = new(250, -1);
            MaxSize = new(400, -1);
            SetSize((graphicsDevice.Viewport.Width / 450) * 100, graphicsDevice.Viewport.Height - AboutSizeY);
            Position = new(graphicsDevice.Viewport.Width - Size.X, 0);
        }

        protected override void OnDraw()
        {

            ImGui.Begin(Title, ref opened, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar);
            ImGui.SetWindowPos(Position);
            ImGui.SetWindowSize(Size);

            ImGui.PushFont(ImGui_Helper.GetFont(16));
            ImGui_Helper.AlignNextText("Plugins installed", ImGui_Helper.ALIGNEMENT.CENTER);
            ImGui.Text("Plugins installed");
            ImGui.PopFont();

            ImGui.Separator();

            ImGui.Text("Try");
            ImGui.SameLine();
            ImGui.TextColored(new(.8f, .2f, .2f, 1), "Test of colored text");

            ImGui.End();
        }

        protected override void OnUpdate()
        {
            throw new NotImplementedException();
        }

        public void UpdateSize(float AboutSizeY)
        {
            SetSize((graphics.Viewport.Width / 450) * 100);
            Position.X = graphics.Viewport.Width - Size.X;
            SetSize(y: graphics.Viewport.Height - AboutSizeY);
        }
    }
}
