using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.core.config;
using RPGCreator.core.helpers;
using RPGCreator.core.interfaces.launcher.parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.interfaces.launcher
{
    class Launcher : InterfacesMain
    {
        protected ProjectManager projectManager;
        protected PluginsList pluginsList;
        public About aboutPart;

        public Launcher(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
            aboutPart = new(graphicsDevice);
            pluginsList = new(graphicsDevice, aboutPart.Size.Y);
            projectManager = new(graphicsDevice, aboutPart.Size.Y, pluginsList.Size.X);
        }

        protected override void OnUpdate()
        {
        }

        protected override void OnDraw()
        {
            aboutPart.Draw();
            pluginsList.Draw();
            projectManager.Draw();
        }

        protected override void OnClientSizeChanged()
        {
            aboutPart.UpdateSize();
            pluginsList.UpdateSize(aboutPart.Size.Y);
            projectManager.UpdateSize(aboutPart.Size.Y, pluginsList.Size.X);
        }
    }
}
