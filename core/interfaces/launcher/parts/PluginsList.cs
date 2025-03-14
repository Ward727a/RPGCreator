using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.core.config;
using RPGCreator.core.helpers;
using RPGCreator.core.io.datas;
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
        List<PluginsItem> items = [];
        int EnabledPlugins = 0;
        public PluginsList(GraphicsDevice graphicsDevice, float AboutSizeY) : base(graphicsDevice)
        {
            Title = "Launcher-PluginsPart";
            MinSize = new(250, -1);
            MaxSize = new(400, -1);
            SetSize((graphicsDevice.Viewport.Width / 450) * 100, graphicsDevice.Viewport.Height - AboutSizeY);
            Position = new(graphicsDevice.Viewport.Width - Size.X, 0);

            EnabledPlugins = ConfigFile.plugins.GetEnabledPluginsCount();

            foreach(string plugin_unique in ConfigFile.plugins.GetPluginsUnique())
            {
                items.Add(new(graphicsDevice, plugin_unique));
            }

        }

        protected override void OnDraw()
        {

            ImGui.Begin(Title, ref opened, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBringToFrontOnFocus);
            ImGui.SetWindowPos(Position);
            ImGui.SetWindowSize(Size);

            ImGui.PushFont(ImGui_Helper.GetFont(16));
            ImGui_Helper.AlignNextText($"Plugins ({items.Count} | {EnabledPlugins})", ImGui_Helper.ALIGNEMENT.CENTER);
            ImGui.Text($"Plugins ({items.Count} | {EnabledPlugins})");
            ImGui.PopFont();

            ImGui.Separator();
            foreach (PluginsItem item in items)
            {
                item.Draw();
            }

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
