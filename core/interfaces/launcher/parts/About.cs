using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.interfaces.launcher.parts
{
    class About : InterfacesMain
    {
        public About(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
            Title = "Launcher-AboutPart";
            SetSize(graphicsDevice.Viewport.Width, 30);
            Position.X = 0;
            Position.Y = (graphicsDevice.Viewport.Height - Size.Y);
        }

        public void UpdateSize()
        {
            SetSize(graphics.Viewport.Width);
            Position.Y = (graphics.Viewport.Height - Size.Y);
        }

        protected override void OnDraw()
        {
            ImGui.Begin(Title, ref opened, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBringToFrontOnFocus);
            ImGui.SetWindowPos(Position);
            ImGui.SetWindowSize(Size);
            ImGui.End();
        }

        protected override void OnUpdate()
        {

        }
    }
}
