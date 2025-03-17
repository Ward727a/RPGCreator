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
    internal abstract class InterfacesMain
    {

        public Dictionary<string, object> metadata = new Dictionary<string, object>();

        static private bool _ShowingOverlay = false;
        static private string _OverlayTitle = "";

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
            if(_ShowingOverlay && _OverlayTitle != Title)
            {
                return _BaseFlags;
            } else
            {
                return ImGuiWindowFlags.None;
            }
        }

        public Vector2 Size = new();

        public void SetSize(float x = -1, float y = -1)
        {
            if (x != -1)
            {
                if (MinSize.X != -1 && x < MinSize.X)
                {
                    Size.X = MinSize.X;
                }
                else if(MaxSize.X != -1 && x > MaxSize.X)
                {
                    Size.X = MaxSize.X;
                }
                else
                {
                    Size.X = x;
                }
            }
            if (y != -1)
            {
                if (MinSize.Y != -1 && y < MinSize.Y)
                {
                    Size.Y = MinSize.Y;
                }
                else if (MaxSize.Y != -1 && y > MaxSize.Y)
                {
                    Size.Y = MaxSize.Y;
                }
                else
                {
                    Size.Y = y;
                }
            }
        }

        /// <summary>
        /// The minimum size for the interfaces.<br/>
        /// If one value is set to -1 then it will be ignored.
        /// </summary>
        public Vector2 MinSize = new(-1, -1);
        /// <summary>
        /// The maximum size for the interfaces.<br/>
        /// If one value is set to -1 then it will be ignored.
        /// </summary>
        public Vector2 MaxSize = new(-1, -1);
        public Vector2 Position = new();

        public bool opened = false;
        public string Title = "";

        protected GraphicsDevice graphics;
        protected InterfacesMain parent;
        protected List<InterfacesMain> Childs;
        
        private bool Visible = true;
        private bool Updated = true;

        public InterfacesMain(GraphicsDevice graphicsDevice)
        {
            graphics = graphicsDevice;
        }
        protected virtual void OnConstruct() { }

        public void Draw() { if (Visible) OnDraw(); }
        protected abstract void OnDraw();
        protected virtual void DrawChilds()
        {
            foreach (InterfacesMain child in Childs)
            {
                child.Draw();
            }
        }
        public void Update() { if (Updated) OnUpdate(); }
        protected abstract void OnUpdate();
        protected virtual void UpdateChilds()
        {
            foreach (InterfacesMain child in Childs)
            {
                child.Update();
            }
        }

        public void HandleClientSizeChanged() { if (Visible) OnClientSizeChanged(); }
        protected virtual void OnClientSizeChanged() { }
        public virtual void Show() { Visible = true; }
        public virtual void Hide() { Visible = false; }
        public virtual void Unfreeze() { Updated = true; }
        public virtual void Freeze() { Updated = false; }
        public void AddChild(InterfacesMain child)
        {
            child.parent = this;
            Childs.Add(child);
        }
        public void RemoveChild(InterfacesMain child)
        {
            child.parent = null;
            Childs.Remove(child);
        }
    
    }
}
