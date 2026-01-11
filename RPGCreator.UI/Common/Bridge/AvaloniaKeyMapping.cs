using System.Collections.Generic;
using Avalonia.Input;
using RPGCreator.SDK.Inputs;

namespace RPGCreator.UI.Common.Bridge;

public static class AvaloniaKeyMapping
{
    private static readonly Dictionary<Key, KeyboardKeys> _mapping = new()
    {
        [Key.A] = KeyboardKeys.A, [Key.B] = KeyboardKeys.B, [Key.C] = KeyboardKeys.C,
        [Key.D] = KeyboardKeys.D, [Key.E] = KeyboardKeys.E, [Key.F] = KeyboardKeys.F,
        [Key.G] = KeyboardKeys.G, [Key.H] = KeyboardKeys.H, [Key.I] = KeyboardKeys.I,
        [Key.J] = KeyboardKeys.J, [Key.K] = KeyboardKeys.K, [Key.L] = KeyboardKeys.L,
        [Key.M] = KeyboardKeys.M, [Key.N] = KeyboardKeys.N, [Key.O] = KeyboardKeys.O,
        [Key.P] = KeyboardKeys.P, [Key.Q] = KeyboardKeys.Q, [Key.R] = KeyboardKeys.R,
        [Key.S] = KeyboardKeys.S, [Key.T] = KeyboardKeys.T, [Key.U] = KeyboardKeys.U,
        [Key.V] = KeyboardKeys.V, [Key.W] = KeyboardKeys.W, [Key.X] = KeyboardKeys.X,
        [Key.Y] = KeyboardKeys.Y, [Key.Z] = KeyboardKeys.Z,

        [Key.D0] = KeyboardKeys.D0, [Key.D1] = KeyboardKeys.D1, [Key.D2] = KeyboardKeys.D2,
        [Key.D3] = KeyboardKeys.D3, [Key.D4] = KeyboardKeys.D4, [Key.D5] = KeyboardKeys.D5,
        [Key.D6] = KeyboardKeys.D6, [Key.D7] = KeyboardKeys.D7, [Key.D8] = KeyboardKeys.D8,
        [Key.D9] = KeyboardKeys.D9,

        [Key.NumPad0] = KeyboardKeys.NumPad0, [Key.NumPad1] = KeyboardKeys.NumPad1,
        [Key.NumPad2] = KeyboardKeys.NumPad2, [Key.NumPad3] = KeyboardKeys.NumPad3,
        [Key.NumPad4] = KeyboardKeys.NumPad4, [Key.NumPad5] = KeyboardKeys.NumPad5,
        [Key.NumPad6] = KeyboardKeys.NumPad6, [Key.NumPad7] = KeyboardKeys.NumPad7,
        [Key.NumPad8] = KeyboardKeys.NumPad8, [Key.NumPad9] = KeyboardKeys.NumPad9,
        [Key.Multiply] = KeyboardKeys.Multiply, [Key.Add] = KeyboardKeys.Add,
        [Key.Subtract] = KeyboardKeys.Subtract, [Key.Decimal] = KeyboardKeys.Decimal,
        [Key.Divide] = KeyboardKeys.Divide,

        [Key.F1] = KeyboardKeys.F1, [Key.F2] = KeyboardKeys.F2, [Key.F3] = KeyboardKeys.F3,
        [Key.F4] = KeyboardKeys.F4, [Key.F5] = KeyboardKeys.F5, [Key.F6] = KeyboardKeys.F6,
        [Key.F7] = KeyboardKeys.F7, [Key.F8] = KeyboardKeys.F8, [Key.F9] = KeyboardKeys.F9,
        [Key.F10] = KeyboardKeys.F10, [Key.F11] = KeyboardKeys.F11, [Key.F12] = KeyboardKeys.F12,
        [Key.F13] = KeyboardKeys.F13, [Key.F14] = KeyboardKeys.F14, [Key.F15] = KeyboardKeys.F15, 
        [Key.F16] = KeyboardKeys.F16, [Key.F17] = KeyboardKeys.F17, [Key.F18] = KeyboardKeys.F18,
        [Key.F19] = KeyboardKeys.F19, [Key.F20] = KeyboardKeys.F20, [Key.F21] = KeyboardKeys.F21,
        [Key.F22] = KeyboardKeys.F22, [Key.F23] = KeyboardKeys.F23, [Key.F24] = KeyboardKeys.F24,

        [Key.Escape] = KeyboardKeys.Escape,
        [Key.Tab] = KeyboardKeys.Tab,
        [Key.CapsLock] = KeyboardKeys.CapsLock,
        [Key.Space] = KeyboardKeys.Space,
        [Key.Enter] = KeyboardKeys.Enter,
        [Key.Back] = KeyboardKeys.Back,
        [Key.Insert] = KeyboardKeys.Insert,
        [Key.Delete] = KeyboardKeys.Delete,
        [Key.Home] = KeyboardKeys.Home,
        [Key.End] = KeyboardKeys.End,
        [Key.PageUp] = KeyboardKeys.PageUp,
        [Key.PageDown] = KeyboardKeys.PageDown,

        [Key.Left] = KeyboardKeys.Left,
        [Key.Right] = KeyboardKeys.Right,
        [Key.Up] = KeyboardKeys.Up,
        [Key.Down] = KeyboardKeys.Down,

        [Key.LeftShift] = KeyboardKeys.LeftShift,
        [Key.RightShift] = KeyboardKeys.RightShift,
        [Key.LeftCtrl] = KeyboardKeys.LeftControl,
        [Key.RightCtrl] = KeyboardKeys.RightControl,
        [Key.LeftAlt] = KeyboardKeys.LeftAlt,
        [Key.RightAlt] = KeyboardKeys.RightAlt,
        [Key.LWin] = KeyboardKeys.LeftWindows,
        [Key.RWin] = KeyboardKeys.RightWindows,
        
        // Touche média
        [Key.MediaPlayPause] = KeyboardKeys.MediaPlayPause,
        [Key.MediaStop] = KeyboardKeys.MediaStop,
        [Key.MediaNextTrack] = KeyboardKeys.MediaNextTrack,
        [Key.MediaPreviousTrack] = KeyboardKeys.MediaPreviousTrack,
        
        // Touche volume
        [Key.VolumeUp] = KeyboardKeys.VolumeUp,
        [Key.VolumeDown] = KeyboardKeys.VolumeDown,
        [Key.VolumeMute] = KeyboardKeys.VolumeMute,
        
        // Touche verrouillage
        [Key.NumLock] = KeyboardKeys.NumLock,
        [Key.CapsLock] = KeyboardKeys.CapsLock,
        
        // Touche ponctuation commune
        [Key.OemPlus] = KeyboardKeys.OemPlus,
        [Key.OemMinus] = KeyboardKeys.OemMinus,
        [Key.OemComma] = KeyboardKeys.OemComma,
        [Key.OemPeriod] = KeyboardKeys.OemPeriod,
        [Key.OemQuestion] = KeyboardKeys.OemQuestion,
        [Key.OemSemicolon] = KeyboardKeys.OemSemicolon,
        [Key.OemQuotes] = KeyboardKeys.OemQuotes,
        [Key.OemOpenBrackets] = KeyboardKeys.OemOpenBrackets,
        [Key.OemCloseBrackets] = KeyboardKeys.OemCloseBrackets,
        [Key.OemPipe] = KeyboardKeys.OemPipe,
        [Key.OemTilde] = KeyboardKeys.OemTilde,
        [Key.OemEnlw] = KeyboardKeys.OemEnlW,
        [Key.OemBackslash] = KeyboardKeys.OemBackslash,
        [Key.Oem8] = KeyboardKeys.Oem8,
        [Key.OemAuto] = KeyboardKeys.OemAuto,
        [Key.OemCopy] = KeyboardKeys.OemCopy,
        [Key.OemClear] = KeyboardKeys.OemClear,
        [Key.ImeProcessed] = KeyboardKeys.ProcessKey,
        [Key.Attn] = KeyboardKeys.Attn,
        [Key.CrSel] = KeyboardKeys.Crsel,
        [Key.ExSel] = KeyboardKeys.Exsel,
        [Key.EraseEof] = KeyboardKeys.EraseEof,
        [Key.Play] = KeyboardKeys.Play,
        [Key.Zoom] = KeyboardKeys.Zoom,
        [Key.Pa1] = KeyboardKeys.Pa1,
        [Key.LaunchApplication1] = KeyboardKeys.LaunchApplication1,
        [Key.LaunchApplication2] = KeyboardKeys.LaunchApplication2,
        [Key.LaunchMail] = KeyboardKeys.LaunchMail,
        [Key.SelectMedia] = KeyboardKeys.SelectMedia,
        [Key.BrowserBack] = KeyboardKeys.BrowserBack,
        [Key.BrowserForward] = KeyboardKeys.BrowserForward,
        [Key.BrowserRefresh] = KeyboardKeys.BrowserRefresh,
        [Key.BrowserStop] = KeyboardKeys.BrowserStop,
        [Key.BrowserSearch] = KeyboardKeys.BrowserSearch,
        [Key.BrowserFavorites] = KeyboardKeys.BrowserFavorites,
        [Key.BrowserHome] = KeyboardKeys.BrowserHome,
        [Key.Scroll] = KeyboardKeys.Scroll,
        [Key.Separator] = KeyboardKeys.Separator,
        [Key.Apps] = KeyboardKeys.Apps,
        [Key.Sleep] = KeyboardKeys.Sleep,
        [Key.Select] = KeyboardKeys.Select,
        [Key.Print] = KeyboardKeys.Print,
        [Key.Execute] = KeyboardKeys.Execute,
        [Key.Help] = KeyboardKeys.Help,
        [Key.ImeConvert] = KeyboardKeys.ImeConvert,
        [Key.ImeNonConvert] = KeyboardKeys.ImeNoConvert,
        [Key.KanaMode] = KeyboardKeys.Kana,
        [Key.KanjiMode] = KeyboardKeys.Kanji,
        [Key.None] = KeyboardKeys.None,
        
        
        // Autres touches
        [Key.PrintScreen] = KeyboardKeys.PrintScreen,
        [Key.Pause] = KeyboardKeys.Pause,
    };

    public static bool TryMapKey(Key avaloniaKey, out KeyboardKeys sdkKey)
    {
        return _mapping.TryGetValue(avaloniaKey, out sdkKey);
    }
}