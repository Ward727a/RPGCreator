using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using RPGCreator.core.helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.interfaces.launcher.parts
{
    class PluginsItem : InterfacesMain
    {
        private bool ChangedCursor = false;
        public bool outdated = true;
        public bool IsCollapsed = false;
        public PluginsItem(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
            Title = "PluginsPart-Test";
        }

        protected override void OnDraw()
        {
            ImGui.BeginChild(Title, new(0, 0), ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY);
            ImGui.BeginGroup();

            if (outdated)
                ImGui.TextColored(new(1, 0.4f, .4f, 1), "Test plugin");
            else
                ImGui.Text("Test plugin");

            ImGui.SameLine();
            ImGui_Helper.AlignNextText("V1.0.0", ImGui_Helper.ALIGNEMENT.RIGHT);
            ImGui.Text("V1.0.0");
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
                if (outdated)
                    ImGui.TextColored(new(1, .4f, .4f, 1), "Outdated");
                else
                    ImGui.TextColored(new(1, 0.6f, 0.6f, 1), "Disabled");
                
                if (IsCollapsed)
                    ImGui.Text("Click to show more information.");
                else
                    ImGui.Text("Click to collapse this plugin information.");

                if(outdated)
                {
                    ImGui.SeparatorText("Error");
                    ImGui.Text("This plugin is outdated and can't be used.");
                }

                ImGui.EndTooltip();
                MouseExtended.SetCursor(Microsoft.Xna.Framework.Input.MouseCursor.Hand);
                ChangedCursor = true;
            } else if(ChangedCursor)
            {
                MouseExtended.SetCursor(Microsoft.Xna.Framework.Input.MouseCursor.Arrow);
                ChangedCursor = false;
            }

            if (!IsCollapsed)
            {
                ImGui.Separator();
                ImGui.Text("Context");
            }
            ImGui.EndChild();
        }

        protected override void OnUpdate()
        {
            throw new NotImplementedException();
        }
    }
}
