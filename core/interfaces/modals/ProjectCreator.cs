using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.core.helpers;
using RPGCreator.core.helpers.ImGuiPlus;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.interfaces.modals
{
    class ProjectCreator : InterfacesMain
    {

        public string project_name = "";
        
        private float left_margin = 150;
        private float top_margin = 100;

        private FileSelector SelectorPath;

        public ProjectCreator(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
            MinSize = new(250, 250);
            Title = "Project creator";
            float vWidth = graphicsDevice.Viewport.Width;
            float vHeight = graphicsDevice.Viewport.Height;
            float sizeX = (vWidth - left_margin * 2);
            float sizeY = (vHeight - top_margin * 2);
            SetSize((sizeX), (sizeY));
            Position = new(left_margin, top_margin);

            FileSelector.FILE_SELECTOR_PARAMETERS param = new();
            param.StartFolder = "E:\\_dev\\RPGCreator\\Content\\BaseContent\\Licenses";
            SelectorPath = new(this, "Select the project location...", "Project location...", param);

            SelectorPath.OnOpen += (_, _) =>
            {
                Lock();
            };

            SelectorPath.OnConfirmed += (_, _) =>
            {
                Unlock();
            };

            Hide();
        }

        public override void Show()
        {
            base.Show();
            float vWidth = graphics.Viewport.Width;
            float vHeight = graphics.Viewport.Height;
            float sizeX = (vWidth - left_margin * 2);
            float sizeY = (vHeight - top_margin * 2);
            SetSize((sizeX), (sizeY));
            Position = new(left_margin, top_margin);

            SetOverlay(this);
        }

        public override void Hide()
        {
            base.Hide();

            RemoveOverlay();
        }

        protected override void OnDraw()
        {
            ImGui.Begin(Title, GetBaseFlags() | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | _SelfFlags);

            if(ImGui.GetWindowSize() != Size)
                ImGui.SetWindowSize(Size);
            if(ImGui.GetWindowPos() != Position)
                ImGui.SetWindowPos(Position);

            ImGui.InputTextWithHint("Project name", "Type here your project name...", ref project_name, 255);

            SelectorPath.Draw();

            ImGui.End();
        }

        protected override void OnUpdate()
        {
            throw new NotImplementedException();
        }
    }
}
