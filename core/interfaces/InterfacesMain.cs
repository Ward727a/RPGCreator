using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.interfaces
{
    internal abstract class InterfacesMain(GraphicsDevice graphicsDevice) : InterfaceBase(graphicsDevice)
    {
        static private bool _ShowingOverlay = false;
        static private string _OverlayTitle = "";

        protected ImGuiWindowFlags _SelfFlags = ImGuiWindowFlags.None;
        static private ImGuiWindowFlags _BaseFlags = ImGuiWindowFlags.None;

        protected static void SetOverlay(InterfacesMain overlayObject)
        {
            _ShowingOverlay = true;
            _OverlayTitle = overlayObject.Title;
            _BaseFlags |= ImGuiWindowFlags.NoInputs;
            _BaseFlags |= ImGuiWindowFlags.NoBringToFrontOnFocus;
        }

        protected static void RemoveOverlay()
        {
            _ShowingOverlay = false;
            _OverlayTitle = null;
            _BaseFlags = ImGuiWindowFlags.None;
        }

        protected static bool HasOverlay()
        {
            return _ShowingOverlay;
        }

        protected static string OverlayParent()
        {
            return _OverlayTitle;
        }

        protected ImGuiWindowFlags GetBaseFlags()
        {
            ImGuiWindowFlags return_flags = ImGuiWindowFlags.None;
            if(_ShowingOverlay && _OverlayTitle != Title)
            {
                return_flags = _BaseFlags;
                return_flags |= _SelfFlags;
            }

            return return_flags;
        }

        public void Lock()
        {
            if (!_SelfFlags.HasFlag(ImGuiWindowFlags.NoMouseInputs))
                _SelfFlags |= ImGuiWindowFlags.NoMouseInputs;
        }

        public void Unlock()
        {
            if (_SelfFlags.HasFlag(ImGuiWindowFlags.NoMouseInputs))
                _SelfFlags &= ~ImGuiWindowFlags.NoMouseInputs;
        }

        public bool opened = false;
        public string Title = "";

        protected virtual void OnConstruct() { }
    
    }
}
