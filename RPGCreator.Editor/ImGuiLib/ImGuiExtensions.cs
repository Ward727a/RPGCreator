// RPG Creator - Open-source RPG Engine.
// (c) 2026 Ward
// 
// This file is part of RPG Creator and is distributed under the Apache 2.0 License.
// You are free to use, modify, and distribute this file under the terms of the Apache 2.0 License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence Apache 2.0.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence Apache 2.0.
// Voir LICENSE pour plus de détails.
// 
// Contact:
// => Mail: Ward727a@gmail.com
//    Please use this object: "RPG Creator [YourObject]"
// => Discord: ward727
// 
// For urgent inquiries, sending both an email and a message on Discord is highly recommended for a quicker response.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Hexa.NET.ImGui;
using RPGCreator.SDK.SDLHelper;
using SDL3;

namespace RPGCreator.Editor.ImGuiLib;

public static class ImGuiExt
{
    public enum CenteringDirection
    {
        None,
        Horizontal,
        Vertical,
        Both = Horizontal | Vertical,
    }


    public enum ButtonType
    {
        Normal,
        Success,
        Warning,
        Error
    }


    public struct FileDialogParameters(IntPtr windowHandle, Action<string[]> onFileSelected)
    {
        public IntPtr WindowHandle = windowHandle;
        public Action<string[]> OnFileSelected = onFileSelected;
        public string? DefaultLocation = null; // If put to null, it will use the last location used by the user.
        public bool AllowMany = false;
        public List<(string name, string[] extensions)> Filters = [];
    }

    public struct FolderDialogParameters(IntPtr windowHandle, Action<string[]> onFolderSelected)
    {
        public IntPtr WindowHandle = windowHandle;
        public Action<string[]> OnFolderSelected = onFolderSelected;
        public string? DefaultLocation = null;
        public bool AllowMany = false;
    }

    public static class Colors
    {
        public static readonly Vector4 Error = new(1.0f, 0.3f, 0.3f, 1.0f);
        public static readonly Vector4 Warning = new(1.0f, 0.8f, 0.2f, 1.0f);
        public static readonly Vector4 Success = new(0.2f, 0.8f, 0.2f, 1.0f);
    }

    extension(ImGui)
    {

        public static unsafe void TextEllipsis(string text, float maxWidth = 0f)
        {
            var availWidth = maxWidth > 0f ? maxWidth : ImGui.GetContentRegionAvail().X;
            var textSize = ImGui.CalcTextSize(text);

            if (textSize.X <= availWidth)
            {
                ImGui.TextUnformatted(text);
                return;
            }

            var pos = ImGui.GetCursorScreenPos();
            var posMax = new Vector2(pos.X + availWidth, pos.Y + textSize.Y);

            ImGuiP.RenderTextEllipsis(
                ImGui.GetWindowDrawList(),
                pos,
                posMax,
                posMax.X,
                text,
                (byte*)null,
                &textSize);

            ImGui.Dummy(textSize with { X = availWidth });
            ImGui.SetItemTooltip(text);
        }
        
        public static ImGuiDockNodeFlags ImGuiDockNodeFlags_NoTabBar => (ImGuiDockNodeFlags)(1 << 12);
        public static void CenteredText(string text, CenteringDirection direction, bool wrap = false)
        {
            var textSize = ImGui.CalcTextSize(text);
            var availSize = ImGui.GetContentRegionAvail();
            switch (direction)
            {
                case CenteringDirection.Both:
                {
                    var posX = (availSize.X - textSize.X) * 0.5f;
                    if (posX > 0.0f)
                    {
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + posX);
                    }

                    var posY = (availSize.Y - textSize.Y) * 0.5f;
                    if (posY > 0.0f)
                    {
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + posY);
                    }

                    goto default;
                }
                case CenteringDirection.Horizontal:
                {
                    var posX = (availSize.X - textSize.X) * 0.5f;
                    if (posX > 0.0f)
                    {
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + posX);
                    }

                    goto default;
                }
                case CenteringDirection.Vertical:
                {
                    var posY = (availSize.Y - textSize.Y) * 0.5f;
                    if (posY > 0.0f)
                    {
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + posY);
                    }

                    goto default;
                }

                case CenteringDirection.None:
                default:
                    if(wrap)
                        ImGui.TextWrapped(text);
                    else
                        ImGui.Text(text);
                    break;
            }
        }

        /// <summary>
        /// Return if we are current in a table.
        /// </summary>
        /// <returns><c>True</c> if in table; <c>False</c> otherwise.</returns>
        public static unsafe bool InTable()
        {
            return ImGuiP.GetCurrentTable().Handle != null;
        }
        
        public static bool CenteredButton(string text, CenteringDirection direction)
        {
            var availSize = ImGui.GetContentRegionAvail();
            var textSize = ImGui.CalcTextSize(text);
            var buttonSize = ImGui.CalcButtonSize(text);

            switch (direction)
            {
                case CenteringDirection.Both:
                {
                    var posX = (availSize.X - buttonSize.X) * 0.5f;
                    if (posX > 0.0f)
                    {
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + posX);
                    }

                    var posY = (availSize.Y - textSize.Y) * 0.5f;
                    if (posY > 0.0f)
                    {
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + posY);
                    }

                    break;
                }
                case CenteringDirection.Horizontal:
                {
                    var posX = (availSize.X - buttonSize.X) * 0.5f;
                    if (posX > 0.0f)
                    {
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + posX);
                    }

                    break;
                }
                case CenteringDirection.Vertical:
                {
                    var posY = (availSize.Y - textSize.Y) * 0.5f;
                    if (posY > 0.0f)
                    {
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + posY);
                    }

                    break;
                }

                case CenteringDirection.None:
                default:
                    break;
            }

            return ImGui.Button(text, buttonSize);
        }

        public static unsafe void OpenFileDialogButton(string label, FileDialogParameters parameters)
        {
            if (ImGui.Button(label))
            {
                List<SDL.SDL_DialogFileFilter> filters =
                [
                    .. parameters.Filters.Select(filter =>
                        SdlDialogFileFilterBuilder.CreateFilter(filter.name, filter.extensions))
                ];

                SDL.SDL_ShowOpenFileDialog(
                    callback: (_, filelist, _) =>
                    {
                        if (filelist != IntPtr.Zero)
                        {
                            var ptr = (IntPtr*)filelist;
                            var selectedPaths = new List<string>();

                            while (*ptr != IntPtr.Zero)
                            {
                                var path = Marshal.PtrToStringUTF8(*ptr);
                                if (!string.IsNullOrEmpty(path))
                                {
                                    selectedPaths.Add(path);
                                }

                                ptr++;
                            }

                            if (selectedPaths.Count > 0)
                            {
                                parameters.OnFileSelected?.Invoke([.. selectedPaths]);
                            }
                        }
                    },
                    userdata: IntPtr.Zero,
                    window: parameters.WindowHandle,
                    filters: [..filters],
                    nfilters: filters.Count,
                    default_location: parameters.DefaultLocation,
                    allow_many: parameters.AllowMany
                );

                foreach (var filter in filters)
                {
                    var sdlDialogFileFilter = filter;
                    SdlDialogFileFilterBuilder.FreeFilter(ref sdlDialogFileFilter);
                }
            }
        }

        public static unsafe void OpenFolderDialogButton(string label, FolderDialogParameters parameters)
        {
            if (ImGui.Button(label))
            {
                SDL.SDL_ShowOpenFolderDialog(
                    callback: (_, filelist, _) =>
                    {
                        if (filelist != IntPtr.Zero)
                        {
                            var ptr = (IntPtr*)filelist;
                            var selectedPaths = new List<string>();

                            while (*ptr != IntPtr.Zero)
                            {
                                var path = Marshal.PtrToStringUTF8(*ptr);
                                if (!string.IsNullOrEmpty(path))
                                {
                                    selectedPaths.Add(path);
                                }

                                ptr++;
                            }

                            if (selectedPaths.Count > 0)
                            {
                                parameters.OnFolderSelected?.Invoke([.. selectedPaths]);
                            }
                        }
                    },
                    userdata: IntPtr.Zero,
                    window: parameters.WindowHandle,
                    default_location: parameters.DefaultLocation,
                    allow_many: parameters.AllowMany
                );
            }
        }

        public static Vector2 CalcButtonSize(string buttonLabel)
        {
            var textSize = ImGui.CalcTextSize(buttonLabel);
            var padding = ImGui.GetStyle().FramePadding;
            return new Vector2(textSize.X + padding.X * 2.0f, textSize.Y + padding.Y * 2.0f);
        }

        public static bool Button(string label, ButtonType type = ButtonType.Normal)
        {
            bool SuccessBtn()
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, .5f, 0.2f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(.3f, .6f, .3f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(.2f, .65f, .2f, 1.0f));
                var pressed = ImGui.Button(label);
                ImGui.PopStyleColor(3);
                return pressed;
            }

            bool WarningBtn()
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(.5f, .5f, .2f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(.6f, .6f, .3f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(.65f, .65f, .2f, 1.0f));
                var pressed = ImGui.Button(label);
                ImGui.PopStyleColor(3);
                return pressed;
            }

            bool ErrorBtn()
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(.5f, 0.2f, 0.2f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(.7f, 0.3f, 0.3f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(.5f, 0.2f, 0.2f, 1.0f));
                var pressed = ImGui.Button(label);
                ImGui.PopStyleColor(3);
                return pressed;
            }

            return type switch
            {
                ButtonType.Normal => ImGui.Button(label),
                ButtonType.Success => SuccessBtn(),
                ButtonType.Warning => WarningBtn(),
                ButtonType.Error => ErrorBtn(),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
        
        public static void TextColoredWrapped(Vector4 color, string text)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, color);
            ImGui.TextWrapped(text);
            ImGui.PopStyleColor();
        }

        public static void Separator(bool vertical = false)
        {
            if(!vertical)
                ImGui.Separator();
            else
            {
                var pos = ImGui.GetCursorScreenPos();
                var height = ImGui.GetFrameHeight(); 
                
                var separatorColor = ImGui.GetColorU32(ImGuiCol.Separator);
            
                ImGui.GetWindowDrawList().AddLine(
                    new Vector2(pos.X, pos.Y), 
                    pos with { Y = pos.Y + height }, 
                    separatorColor
                );
            
                ImGui.Dummy(new Vector2(0, 0));
            }
        }

        public static void IndeterminateProgressBar(string id, Vector2 size, Vector4 fillColor, Vector4 bgColor)
        {
            var cursorPos = ImGui.GetCursorScreenPos();

            ImGui.Dummy(size);

            var drawList = ImGui.GetWindowDrawList();

            var bbMin = cursorPos;
            var bbMax = cursorPos + size;

            var bgU32 = ImGui.ColorConvertFloat4ToU32(bgColor);
            var fillU32 = ImGui.ColorConvertFloat4ToU32(fillColor);

            var rounding = size.Y * 0.5f;

            drawList.AddRectFilled(bbMin, bbMax, bgU32, rounding);

            var time = (float)ImGui.GetTime();
            var barWidth = size.X;
            var indicatorWidth = barWidth * 0.3f;

            var speed = 2.0f;
            var t = (MathF.Sin(time * speed) + 1f) * 0.5f;

            var xMin = bbMin.X + t * (barWidth - indicatorWidth);
            var xMax = xMin + indicatorWidth;

            var indicatorMin = new Vector2(xMin, bbMin.Y);
            var indicatorMax = new Vector2(xMax, bbMax.Y);

            drawList.AddRectFilled(indicatorMin, indicatorMax, fillU32, rounding);
        }

        public static ImGuiKey SDLKeyToImGuiKey(SDL.SDL_Keycode keycode)
        {
            return keycode switch
            {
                SDL.SDL_Keycode.SDLK_TAB => ImGuiKey.Tab,
                SDL.SDL_Keycode.SDLK_LEFT => ImGuiKey.LeftArrow,
                SDL.SDL_Keycode.SDLK_RIGHT => ImGuiKey.RightArrow,
                SDL.SDL_Keycode.SDLK_UP => ImGuiKey.UpArrow,
                SDL.SDL_Keycode.SDLK_DOWN => ImGuiKey.DownArrow,
                SDL.SDL_Keycode.SDLK_PAGEUP => ImGuiKey.PageUp,
                SDL.SDL_Keycode.SDLK_PAGEDOWN => ImGuiKey.PageDown,
                SDL.SDL_Keycode.SDLK_HOME => ImGuiKey.Home,
                SDL.SDL_Keycode.SDLK_END => ImGuiKey.End,
                SDL.SDL_Keycode.SDLK_INSERT => ImGuiKey.Insert,
                SDL.SDL_Keycode.SDLK_DELETE => ImGuiKey.Delete,
                SDL.SDL_Keycode.SDLK_BACKSPACE => ImGuiKey.Backspace,
                SDL.SDL_Keycode.SDLK_SPACE => ImGuiKey.Space,
                SDL.SDL_Keycode.SDLK_RETURN => ImGuiKey.Enter,
                SDL.SDL_Keycode.SDLK_ESCAPE => ImGuiKey.Escape,
                SDL.SDL_Keycode.SDLK_A => ImGuiKey.A,
                SDL.SDL_Keycode.SDLK_B => ImGuiKey.B,
                SDL.SDL_Keycode.SDLK_C => ImGuiKey.C,
                SDL.SDL_Keycode.SDLK_D => ImGuiKey.D,
                SDL.SDL_Keycode.SDLK_E => ImGuiKey.E,
                SDL.SDL_Keycode.SDLK_F => ImGuiKey.F,
                SDL.SDL_Keycode.SDLK_G => ImGuiKey.G,
                SDL.SDL_Keycode.SDLK_H => ImGuiKey.H,
                SDL.SDL_Keycode.SDLK_I => ImGuiKey.I,
                SDL.SDL_Keycode.SDLK_J => ImGuiKey.J,
                SDL.SDL_Keycode.SDLK_K => ImGuiKey.K,
                SDL.SDL_Keycode.SDLK_L => ImGuiKey.L,
                SDL.SDL_Keycode.SDLK_M => ImGuiKey.M,
                SDL.SDL_Keycode.SDLK_N => ImGuiKey.N,
                SDL.SDL_Keycode.SDLK_O => ImGuiKey.O,
                SDL.SDL_Keycode.SDLK_P => ImGuiKey.P,
                SDL.SDL_Keycode.SDLK_Q => ImGuiKey.Q,
                SDL.SDL_Keycode.SDLK_R => ImGuiKey.R,
                SDL.SDL_Keycode.SDLK_S => ImGuiKey.S,
                SDL.SDL_Keycode.SDLK_T => ImGuiKey.T,
                SDL.SDL_Keycode.SDLK_U => ImGuiKey.U,
                SDL.SDL_Keycode.SDLK_V => ImGuiKey.V,
                SDL.SDL_Keycode.SDLK_W => ImGuiKey.W,
                SDL.SDL_Keycode.SDLK_X => ImGuiKey.X,
                SDL.SDL_Keycode.SDLK_Y => ImGuiKey.Y,
                SDL.SDL_Keycode.SDLK_Z => ImGuiKey.Z,
                SDL.SDL_Keycode.SDLK_F1 => ImGuiKey.F1,
                SDL.SDL_Keycode.SDLK_F2 => ImGuiKey.F2,
                SDL.SDL_Keycode.SDLK_F3 => ImGuiKey.F3,
                SDL.SDL_Keycode.SDLK_F4 => ImGuiKey.F4,
                SDL.SDL_Keycode.SDLK_F5 => ImGuiKey.F5,
                SDL.SDL_Keycode.SDLK_F6 => ImGuiKey.F6,
                SDL.SDL_Keycode.SDLK_F7 => ImGuiKey.F7,
                SDL.SDL_Keycode.SDLK_F8 => ImGuiKey.F8,
                SDL.SDL_Keycode.SDLK_F9 => ImGuiKey.F9,
                SDL.SDL_Keycode.SDLK_F10 => ImGuiKey.F10,
                SDL.SDL_Keycode.SDLK_F11 => ImGuiKey.F11,
                SDL.SDL_Keycode.SDLK_F12 => ImGuiKey.F12,
                SDL.SDL_Keycode.SDLK_F13 => ImGuiKey.F13,
                SDL.SDL_Keycode.SDLK_F14 => ImGuiKey.F14,
                SDL.SDL_Keycode.SDLK_F15 => ImGuiKey.F15,
                SDL.SDL_Keycode.SDLK_F16 => ImGuiKey.F16,
                SDL.SDL_Keycode.SDLK_F17 => ImGuiKey.F17,
                SDL.SDL_Keycode.SDLK_F18 => ImGuiKey.F18,
                SDL.SDL_Keycode.SDLK_F19 => ImGuiKey.F19,
                SDL.SDL_Keycode.SDLK_F20 => ImGuiKey.F20,
                SDL.SDL_Keycode.SDLK_F21 => ImGuiKey.F21,
                SDL.SDL_Keycode.SDLK_F22 => ImGuiKey.F22,
                SDL.SDL_Keycode.SDLK_F23 => ImGuiKey.F23,
                SDL.SDL_Keycode.SDLK_F24 => ImGuiKey.F24,

                SDL.SDL_Keycode.SDLK_LCTRL => ImGuiKey.LeftCtrl,
                SDL.SDL_Keycode.SDLK_RCTRL => ImGuiKey.RightCtrl,
                SDL.SDL_Keycode.SDLK_LSHIFT => ImGuiKey.LeftShift,
                SDL.SDL_Keycode.SDLK_RSHIFT => ImGuiKey.RightShift,
                SDL.SDL_Keycode.SDLK_LALT => ImGuiKey.LeftAlt,
                SDL.SDL_Keycode.SDLK_RALT => ImGuiKey.RightAlt,
                SDL.SDL_Keycode.SDLK_LHYPER => ImGuiKey.LeftSuper,
                SDL.SDL_Keycode.SDLK_RHYPER => ImGuiKey.RightSuper,
                SDL.SDL_Keycode.SDLK_MENU => ImGuiKey.Menu,
                SDL.SDL_Keycode.SDLK_APOSTROPHE => ImGuiKey.Apostrophe,
                SDL.SDL_Keycode.SDLK_COMMA => ImGuiKey.Comma,
                SDL.SDL_Keycode.SDLK_MINUS => ImGuiKey.Minus,
                SDL.SDL_Keycode.SDLK_PERIOD => ImGuiKey.Period,
                SDL.SDL_Keycode.SDLK_SLASH => ImGuiKey.Slash,
                SDL.SDL_Keycode.SDLK_SEMICOLON => ImGuiKey.Semicolon,
                SDL.SDL_Keycode.SDLK_EQUALS => ImGuiKey.Equal,
                SDL.SDL_Keycode.SDLK_LEFTBRACKET => ImGuiKey.LeftBracket,
                SDL.SDL_Keycode.SDLK_BACKSLASH => ImGuiKey.Backslash,
                SDL.SDL_Keycode.SDLK_RIGHTBRACKET => ImGuiKey.RightBracket,
                SDL.SDL_Keycode.SDLK_GRAVE => ImGuiKey.GraveAccent,
                SDL.SDL_Keycode.SDLK_CAPSLOCK => ImGuiKey.CapsLock,
                SDL.SDL_Keycode.SDLK_SCROLLLOCK => ImGuiKey.ScrollLock,
                SDL.SDL_Keycode.SDLK_NUMLOCKCLEAR => ImGuiKey.NumLock,
                SDL.SDL_Keycode.SDLK_PRINTSCREEN => ImGuiKey.PrintScreen,
                SDL.SDL_Keycode.SDLK_PAUSE => ImGuiKey.Pause,
                SDL.SDL_Keycode.SDLK_AC_BACK => ImGuiKey.AppBack,
                SDL.SDL_Keycode.SDLK_AC_FORWARD => ImGuiKey.AppForward,
                SDL.SDL_Keycode.SDLK_KP_0 => ImGuiKey.Keypad0,
                SDL.SDL_Keycode.SDLK_KP_1 => ImGuiKey.Keypad1,
                SDL.SDL_Keycode.SDLK_KP_2 => ImGuiKey.Keypad2,
                SDL.SDL_Keycode.SDLK_KP_3 => ImGuiKey.Keypad3,
                SDL.SDL_Keycode.SDLK_KP_4 => ImGuiKey.Keypad4,
                SDL.SDL_Keycode.SDLK_KP_5 => ImGuiKey.Keypad5,
                SDL.SDL_Keycode.SDLK_KP_6 => ImGuiKey.Keypad6,
                SDL.SDL_Keycode.SDLK_KP_7 => ImGuiKey.Keypad7,
                SDL.SDL_Keycode.SDLK_KP_8 => ImGuiKey.Keypad8,
                SDL.SDL_Keycode.SDLK_KP_9 => ImGuiKey.Keypad9,
                SDL.SDL_Keycode.SDLK_KP_DECIMAL => ImGuiKey.KeypadDecimal,
                SDL.SDL_Keycode.SDLK_KP_DIVIDE => ImGuiKey.KeypadDivide,
                SDL.SDL_Keycode.SDLK_KP_MULTIPLY => ImGuiKey.KeypadMultiply,
                SDL.SDL_Keycode.SDLK_KP_MINUS => ImGuiKey.KeypadSubtract,
                SDL.SDL_Keycode.SDLK_KP_PLUS => ImGuiKey.KeypadAdd,
                SDL.SDL_Keycode.SDLK_KP_ENTER => ImGuiKey.KeypadEnter,
                SDL.SDL_Keycode.SDLK_KP_EQUALS => ImGuiKey.Equal,

                _ => ImGuiKey.None
            };
        }
    }
}