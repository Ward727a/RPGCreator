using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using RPGCreator.core.helpers;
using RPGCreator.core.plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.interfaces.launcher.parts
{
    class PluginsItem : InterfacesMain
    {
        public BasePlugin plugin;

        private bool ChangedCursor = false;
        public bool IsCollapsed = false;
        public PluginsItem(GraphicsDevice graphicsDevice, string plugin_unique) : base(graphicsDevice)
        {
            Title = $"PluginsPart-{plugin_unique}";
            plugin = new(plugin_unique);
        }

        protected override void OnDraw()
        {
            ImGui.BeginChild(Title, new(0, 0), ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY, GetBaseFlags());
            ImGui.BeginGroup();

            if (plugin.IsOutdated())
                ImGui.TextColored(new(1, 0.4f, .4f, 1), plugin.Name);
            else
                ImGui.Text(plugin.Name);

            ImGui.SameLine();
            ImGui_Helper.AlignNextText($"V{plugin.PluginVersion}", ImGui_Helper.ALIGNEMENT.RIGHT);
            ImGui.Text($"V{plugin.PluginVersion}");
            ImGui.EndGroup();

            if(ImGui.IsItemClicked())
            {
                IsCollapsed = !IsCollapsed;
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Plugin state:");
                ImGui.SameLine();
                if (plugin.IsOutdated())
                    ImGui.TextColored(new(1, .4f, .4f, 1), "Outdated");
                else
                    ImGui.TextColored(new(1, 0.6f, 0.6f, 1), "Disabled");
                
                if (IsCollapsed)
                    ImGui.Text("Click to show more information.");
                else
                    ImGui.Text("Click to collapse this plugin information.");

                if(plugin.IsOutdated())
                {
                    ImGui.SeparatorText("Error");
                    ImGui.Text("This plugin is outdated and can't be used.");
                }

                ImGui.EndTooltip();
                MouseExtended.SetCursor(MouseCursor.Hand);
                ChangedCursor = true;
            } else if(ChangedCursor)
            {
                MouseExtended.SetCursor(MouseCursor.Arrow);
                ChangedCursor = false;
            }

            if (!IsCollapsed)
            {
                ImGui.Separator();

                if(plugin.Description.Length > 120)
                    ImGui.TextWrapped(plugin.Description.Remove(120));
                else
                    ImGui.TextWrapped(plugin.Description);
            }
            ImGui.EndChild();
        }

        protected override void OnUpdate()
        {
            throw new NotImplementedException();
        }
    }
}
