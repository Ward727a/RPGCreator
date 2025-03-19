using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.core.types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.interfaces
{
    internal abstract class InterfaceBase : BaseObject
    {
        public bool CanBeCollapsed = false;
        public bool CanBeResized = false;
        public bool CanBeMoved = false;
        public bool HasTitlebar = false;
        public bool CanHaveScrollbar = false;
        public bool CanScrollWithMouse = false;
        public bool HasBackground = false;
        public bool CanSaveSettings = false;
        public bool HasMouseInputs = false;
        public bool FocusOnAppear = false;
        public bool ToFrontOnFocus = false;
        public bool KeyboardNavigation = false;
        public bool NavigationOnFocus = false;
        public bool HasUnsavedDoc = false;
        public bool CanDock = false;
        public bool HasDecoration = false;
        public bool HasInputs = false;
        public bool IsChildWindow = false;
        public bool IsTooltip = false;
        public bool IsPopup = false;
        public bool IsModal = false;
        public bool IsChildMenu = false;
        public bool IsDockHost = false;
        public bool IsMenuBar = false;
        public bool ShowHScroll = false;
        public bool ShowVScroll = false;
        public bool AutoResize = false;

        private bool Visible = true;
        private bool Updated = true;

        public Vector2 Size = new();

        public void SetSize(float x = -1, float y = -1)
        {
            if (x != -1)
            {
                if (MinSize.X != -1 && x < MinSize.X)
                {
                    Size.X = MinSize.X;
                }
                else if (MaxSize.X != -1 && x > MaxSize.X)
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

        protected GraphicsDevice graphics;
        internal InterfaceBase(GraphicsDevice device)
        {
            graphics = device;
        }

        public void Draw() { if (Visible) OnDraw(); }
        protected abstract void OnDraw();
        public void Update() { if (Updated) OnUpdate(); }
        protected abstract void OnUpdate();
        public void HandleClientSizeChanged() { if (Visible) OnClientSizeChanged(); }
        protected virtual void OnClientSizeChanged() { }
        public virtual void Show() { Visible = true; }
        public virtual void Hide() { Visible = false; }
        public virtual void Unfreeze() { Updated = true; }
        public virtual void Freeze() { Updated = false; }

        /// <summary>
        /// This use the provided paremeters of the base class <see cref="InterfaceBase"/> to combine flags that are ready to use.<br/>
        /// Each parameters can be edited on the fly to modify those flags and then edit the style of the window.
        /// </summary>
        /// <param name="flags"></param>
        protected void CreateGuiWinFlags(ref ImGuiWindowFlags flags)
        {
            _AddWindowFlags(ref flags, !CanBeCollapsed, ImGuiWindowFlags.NoCollapse);
            _AddWindowFlags(ref flags, !CanBeMoved, ImGuiWindowFlags.NoMove);
            _AddWindowFlags(ref flags, !CanBeResized, ImGuiWindowFlags.NoResize);
            _AddWindowFlags(ref flags, !HasTitlebar, ImGuiWindowFlags.NoTitleBar);
            _AddWindowFlags(ref flags, !CanHaveScrollbar, ImGuiWindowFlags.NoScrollbar);
            _AddWindowFlags(ref flags, !CanScrollWithMouse, ImGuiWindowFlags.NoScrollWithMouse);
            _AddWindowFlags(ref flags, !HasBackground, ImGuiWindowFlags.NoBackground);
            _AddWindowFlags(ref flags, !CanSaveSettings, ImGuiWindowFlags.NoSavedSettings);
            _AddWindowFlags(ref flags, !HasMouseInputs, ImGuiWindowFlags.NoMouseInputs);
            _AddWindowFlags(ref flags, !FocusOnAppear, ImGuiWindowFlags.NoFocusOnAppearing);
            _AddWindowFlags(ref flags, !ToFrontOnFocus, ImGuiWindowFlags.NoBringToFrontOnFocus);
            _AddWindowFlags(ref flags, !KeyboardNavigation, ImGuiWindowFlags.NoNavInputs);
            _AddWindowFlags(ref flags, !NavigationOnFocus, ImGuiWindowFlags.NoNavFocus);
            _AddWindowFlags(ref flags, HasUnsavedDoc, ImGuiWindowFlags.UnsavedDocument);
            _AddWindowFlags(ref flags, !CanDock, ImGuiWindowFlags.NoDocking);
            _AddWindowFlags(ref flags, !HasDecoration, ImGuiWindowFlags.NoDecoration);
            _AddWindowFlags(ref flags, !HasInputs, ImGuiWindowFlags.NoInputs);
            _AddWindowFlags(ref flags, IsChildWindow, ImGuiWindowFlags.ChildWindow);
            _AddWindowFlags(ref flags, IsTooltip, ImGuiWindowFlags.Tooltip);
            _AddWindowFlags(ref flags, IsPopup, ImGuiWindowFlags.Popup);
            _AddWindowFlags(ref flags, IsModal, ImGuiWindowFlags.Modal);
            _AddWindowFlags(ref flags, IsChildMenu, ImGuiWindowFlags.ChildMenu);
            _AddWindowFlags(ref flags, IsDockHost, ImGuiWindowFlags.DockNodeHost);
            _AddWindowFlags(ref flags, IsMenuBar, ImGuiWindowFlags.MenuBar);
            _AddWindowFlags(ref flags, ShowHScroll, ImGuiWindowFlags.AlwaysHorizontalScrollbar);
            _AddWindowFlags(ref flags, ShowVScroll, ImGuiWindowFlags.AlwaysVerticalScrollbar);
            _AddWindowFlags(ref flags, AutoResize, ImGuiWindowFlags.AlwaysAutoResize);
        }

        private void _AddWindowFlags(ref ImGuiWindowFlags flags, bool condition, ImGuiWindowFlags flag_to_add)
        {
            if(condition && !flags.HasFlag(flag_to_add)) flags |= flag_to_add;
        }
    }
}
