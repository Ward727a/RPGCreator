using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.interfaces.modals
{
    internal class OkModal : BaseModal
    {
        public bool CloseOnConfirmed = true;
        public bool CloseOnLoseFocus = false;

        public new bool IsModal = true;
        public new bool CanBeCollapsed = false;
        public new bool CanBeResized = false;
        public new bool CanBeMoved = false;

        public string Title = "";
        public string Message = "";

        public event EventHandler OnConfirmed;

        public OkModal(GraphicsDevice graphics, InterfacesMain parent, string title, string message) : base(graphics, parent)
        {
            Title = title;
            Message = message;
        }

        public override void CloseModal()
        {
            IsOpened = false;
            _OnClose();
        }

        public override void ShowModal()
        {
            IsOpened = true;
            _OnOpen();
        }

        protected override void OnDraw()
        {
            BeginModal();

            DrawModalBody();

            EndModal();
        }

        protected void BeginModal()
        {
            ImGuiWindowFlags flags = ImGuiWindowFlags.None;
            CreateGuiWinFlags(ref flags);

            ImGui.Begin($"{Title}##{ID}", flags);

            ImGui.SetWindowSize(Size);
            ImGui.SetWindowPos(Position);

        }

        protected virtual void DrawModalBody()
        {
            // Put the content of the modal here.

            ImGui.TextWrapped(Message);

            ImGui.Separator();

            if(ImGui.Button("OK"))
            {
                RaiseConfirmed();
            }

        }

        protected void EndModal()
        {
            if (!ImGui.IsWindowFocused())
            {
                if(CloseOnLoseFocus)
                {
                    CloseModal();
                }
            }
            ImGui.End();
        }

        protected void RaiseConfirmed()
        {
            OnConfirmed?.Invoke(this, EventArgs.Empty);

            if(CloseOnConfirmed)
            {
                CloseModal();
            }
        }

        protected override void OnUpdate()
        {
            throw new NotImplementedException();
        }
    }
}
