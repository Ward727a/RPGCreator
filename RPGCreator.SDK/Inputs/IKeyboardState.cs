using RPGCreator.SDK.GlobalState;

namespace RPGCreator.SDK.Inputs;

/// <summary>
/// Keyboard keys enumeration.<br/>
/// FROM: Microsoft.Xna.Framework.Input (MonoGame).
/// </summary>
public enum KeyboardKeys
{
    /// <summary>Reserved.</summary>
    None = 0,
    /// <summary>BACKSPACE key.</summary>
    Back = 8,
    /// <summary>TAB key.</summary>
    Tab = 9,
    /// <summary>ENTER key.</summary>
    Enter = 13, // 0x0000000D
    /// <summary>PAUSE key.</summary>
    Pause = 19, // 0x00000013
    /// <summary>CAPS LOCK key.</summary>
    CapsLock = 20, // 0x00000014
    /// <summary>Kana key on Japanese keyboards.</summary>
    Kana = 21, // 0x00000015
    /// <summary>Kanji key on Japanese keyboards.</summary>
    Kanji = 25, // 0x00000019
    /// <summary>ESC key.</summary>
    Escape = 27, // 0x0000001B
    /// <summary>IME Convert key.</summary>
    ImeConvert = 28, // 0x0000001C
    /// <summary>IME NoConvert key.</summary>
    ImeNoConvert = 29, // 0x0000001D
    /// <summary>SPACEBAR key.</summary>
    Space = 32, // 0x00000020
    /// <summary>PAGE UP key.</summary>
    PageUp = 33, // 0x00000021
    /// <summary>PAGE DOWN key.</summary>
    PageDown = 34, // 0x00000022
    /// <summary>END key.</summary>
    End = 35, // 0x00000023
    /// <summary>HOME key.</summary>
    Home = 36, // 0x00000024
    /// <summary>LEFT ARROW key.</summary>
    Left = 37, // 0x00000025
    /// <summary>UP ARROW key.</summary>
    Up = 38, // 0x00000026
    /// <summary>RIGHT ARROW key.</summary>
    Right = 39, // 0x00000027
    /// <summary>DOWN ARROW key.</summary>
    Down = 40, // 0x00000028
    /// <summary>SELECT key.</summary>
    Select = 41, // 0x00000029
    /// <summary>PRINT key.</summary>
    Print = 42, // 0x0000002A
    /// <summary>EXECUTE key.</summary>
    Execute = 43, // 0x0000002B
    /// <summary>PRINT SCREEN key.</summary>
    PrintScreen = 44, // 0x0000002C
    /// <summary>INS key.</summary>
    Insert = 45, // 0x0000002D
    /// <summary>DEL key.</summary>
    Delete = 46, // 0x0000002E
    /// <summary>HELP key.</summary>
    Help = 47, // 0x0000002F
    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard.
    /// </summary>
    D0 = 48, // 0x00000030
    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard.
    /// </summary>
    D1 = 49, // 0x00000031
    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard.
    /// </summary>
    D2 = 50, // 0x00000032
    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard.
    /// </summary>
    D3 = 51, // 0x00000033
    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard.
    /// </summary>
    D4 = 52, // 0x00000034
    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard.
    /// </summary>
    D5 = 53, // 0x00000035
    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard.
    /// </summary>
    D6 = 54, // 0x00000036
    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard.
    /// </summary>
    D7 = 55, // 0x00000037
    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard.
    /// </summary>
    D8 = 56, // 0x00000038
    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard.
    /// </summary>
    D9 = 57, // 0x00000039
    /// <summary>A key.</summary>
    A = 65, // 0x00000041
    /// <summary>B key.</summary>
    B = 66, // 0x00000042
    /// <summary>C key.</summary>
    C = 67, // 0x00000043
    /// <summary>D key.</summary>
    D = 68, // 0x00000044
    /// <summary>E key.</summary>
    E = 69, // 0x00000045
    /// <summary>F key.</summary>
    F = 70, // 0x00000046
    /// <summary>G key.</summary>
    G = 71, // 0x00000047
    /// <summary>H key.</summary>
    H = 72, // 0x00000048
    /// <summary>I key.</summary>
    I = 73, // 0x00000049
    /// <summary>J key.</summary>
    J = 74, // 0x0000004A
    /// <summary>K key.</summary>
    K = 75, // 0x0000004B
    /// <summary>L key.</summary>
    L = 76, // 0x0000004C
    /// <summary>M key.</summary>
    M = 77, // 0x0000004D
    /// <summary>N key.</summary>
    N = 78, // 0x0000004E
    /// <summary>O key.</summary>
    O = 79, // 0x0000004F
    /// <summary>P key.</summary>
    P = 80, // 0x00000050
    /// <summary>Q key.</summary>
    Q = 81, // 0x00000051
    /// <summary>R key.</summary>
    R = 82, // 0x00000052
    /// <summary>S key.</summary>
    S = 83, // 0x00000053
    /// <summary>T key.</summary>
    T = 84, // 0x00000054
    /// <summary>U key.</summary>
    U = 85, // 0x00000055
    /// <summary>V key.</summary>
    V = 86, // 0x00000056
    /// <summary>W key.</summary>
    W = 87, // 0x00000057
    /// <summary>X key.</summary>
    X = 88, // 0x00000058
    /// <summary>Y key.</summary>
    Y = 89, // 0x00000059
    /// <summary>Z key.</summary>
    Z = 90, // 0x0000005A
    /// <summary>Left Windows key.</summary>
    LeftWindows = 91, // 0x0000005B
    /// <summary>Right Windows key.</summary>
    RightWindows = 92, // 0x0000005C
    /// <summary>Applications key.</summary>
    Apps = 93, // 0x0000005D
    /// <summary>Computer Sleep key.</summary>
    Sleep = 95, // 0x0000005F
    /// <summary>Numeric keypad 0 key.</summary>
    NumPad0 = 96, // 0x00000060
    /// <summary>Numeric keypad 1 key.</summary>
    NumPad1 = 97, // 0x00000061
    /// <summary>Numeric keypad 2 key.</summary>
    NumPad2 = 98, // 0x00000062
    /// <summary>Numeric keypad 3 key.</summary>
    NumPad3 = 99, // 0x00000063
    /// <summary>Numeric keypad 4 key.</summary>
    NumPad4 = 100, // 0x00000064
    /// <summary>Numeric keypad 5 key.</summary>
    NumPad5 = 101, // 0x00000065
    /// <summary>Numeric keypad 6 key.</summary>
    NumPad6 = 102, // 0x00000066
    /// <summary>Numeric keypad 7 key.</summary>
    NumPad7 = 103, // 0x00000067
    /// <summary>Numeric keypad 8 key.</summary>
    NumPad8 = 104, // 0x00000068
    /// <summary>Numeric keypad 9 key.</summary>
    NumPad9 = 105, // 0x00000069
    /// <summary>Multiply key.</summary>
    Multiply = 106, // 0x0000006A
    /// <summary>Add key.</summary>
    Add = 107, // 0x0000006B
    /// <summary>Separator key.</summary>
    Separator = 108, // 0x0000006C
    /// <summary>Subtract key.</summary>
    Subtract = 109, // 0x0000006D
    /// <summary>Decimal key.</summary>
    Decimal = 110, // 0x0000006E
    /// <summary>Divide key.</summary>
    Divide = 111, // 0x0000006F
    /// <summary>F1 key.</summary>
    F1 = 112, // 0x00000070
    /// <summary>F2 key.</summary>
    F2 = 113, // 0x00000071
    /// <summary>F3 key.</summary>
    F3 = 114, // 0x00000072
    /// <summary>F4 key.</summary>
    F4 = 115, // 0x00000073
    /// <summary>F5 key.</summary>
    F5 = 116, // 0x00000074
    /// <summary>F6 key.</summary>
    F6 = 117, // 0x00000075
    /// <summary>F7 key.</summary>
    F7 = 118, // 0x00000076
    /// <summary>F8 key.</summary>
    F8 = 119, // 0x00000077
    /// <summary>F9 key.</summary>
    F9 = 120, // 0x00000078
    /// <summary>F10 key.</summary>
    F10 = 121, // 0x00000079
    /// <summary>F11 key.</summary>
    F11 = 122, // 0x0000007A
    /// <summary>F12 key.</summary>
    F12 = 123, // 0x0000007B
    /// <summary>F13 key.</summary>
    F13 = 124, // 0x0000007C
    /// <summary>F14 key.</summary>
    F14 = 125, // 0x0000007D
    /// <summary>F15 key.</summary>
    F15 = 126, // 0x0000007E
    /// <summary>F16 key.</summary>
    F16 = 127, // 0x0000007F
    /// <summary>F17 key.</summary>
    F17 = 128, // 0x00000080
    /// <summary>F18 key.</summary>
    F18 = 129, // 0x00000081
    /// <summary>F19 key.</summary>
    F19 = 130, // 0x00000082
    /// <summary>F20 key.</summary>
    F20 = 131, // 0x00000083
    /// <summary>F21 key.</summary>
    F21 = 132, // 0x00000084
    /// <summary>F22 key.</summary>
    F22 = 133, // 0x00000085
    /// <summary>F23 key.</summary>
    F23 = 134, // 0x00000086
    /// <summary>F24 key.</summary>
    F24 = 135, // 0x00000087
    /// <summary>NUM LOCK key.</summary>
    NumLock = 144, // 0x00000090
    /// <summary>SCROLL LOCK key.</summary>
    Scroll = 145, // 0x00000091
    /// <summary>Left SHIFT key.</summary>
    LeftShift = 160, // 0x000000A0
    /// <summary>Right SHIFT key.</summary>
    RightShift = 161, // 0x000000A1
    /// <summary>Left CONTROL key.</summary>
    LeftControl = 162, // 0x000000A2
    /// <summary>Right CONTROL key.</summary>
    RightControl = 163, // 0x000000A3
    /// <summary>Left ALT key.</summary>
    LeftAlt = 164, // 0x000000A4
    /// <summary>Right ALT key.</summary>
    RightAlt = 165, // 0x000000A5
    /// <summary>Browser Back key.</summary>
    BrowserBack = 166, // 0x000000A6
    /// <summary>Browser Forward key.</summary>
    BrowserForward = 167, // 0x000000A7
    /// <summary>Browser Refresh key.</summary>
    BrowserRefresh = 168, // 0x000000A8
    /// <summary>Browser Stop key.</summary>
    BrowserStop = 169, // 0x000000A9
    /// <summary>Browser Search key.</summary>
    BrowserSearch = 170, // 0x000000AA
    /// <summary>Browser Favorites key.</summary>
    BrowserFavorites = 171, // 0x000000AB
    /// <summary>Browser Start and Home key.</summary>
    BrowserHome = 172, // 0x000000AC
    /// <summary>Volume Mute key.</summary>
    VolumeMute = 173, // 0x000000AD
    /// <summary>Volume Down key.</summary>
    VolumeDown = 174, // 0x000000AE
    /// <summary>Volume Up key.</summary>
    VolumeUp = 175, // 0x000000AF
    /// <summary>Next Track key.</summary>
    MediaNextTrack = 176, // 0x000000B0
    /// <summary>Previous Track key.</summary>
    MediaPreviousTrack = 177, // 0x000000B1
    /// <summary>Stop Media key.</summary>
    MediaStop = 178, // 0x000000B2
    /// <summary>Play/Pause Media key.</summary>
    MediaPlayPause = 179, // 0x000000B3
    /// <summary>Start Mail key.</summary>
    LaunchMail = 180, // 0x000000B4
    /// <summary>Select Media key.</summary>
    SelectMedia = 181, // 0x000000B5
    /// <summary>Start Application 1 key.</summary>
    LaunchApplication1 = 182, // 0x000000B6
    /// <summary>Start Application 2 key.</summary>
    LaunchApplication2 = 183, // 0x000000B7
    /// <summary>The OEM Semicolon key on a US standard keyboard.</summary>
    OemSemicolon = 186, // 0x000000BA
    /// <summary>For any country/region, the '+' key.</summary>
    OemPlus = 187, // 0x000000BB
    /// <summary>For any country/region, the ',' key.</summary>
    OemComma = 188, // 0x000000BC
    /// <summary>For any country/region, the '-' key.</summary>
    OemMinus = 189, // 0x000000BD
    /// <summary>For any country/region, the '.' key.</summary>
    OemPeriod = 190, // 0x000000BE
    /// <summary>The OEM question mark key on a US standard keyboard.</summary>
    OemQuestion = 191, // 0x000000BF
    /// <summary>The OEM tilde key on a US standard keyboard.</summary>
    OemTilde = 192, // 0x000000C0
    /// <summary>Green ChatPad key.</summary>
    ChatPadGreen = 202, // 0x000000CA
    /// <summary>Orange ChatPad key.</summary>
    ChatPadOrange = 203, // 0x000000CB
    /// <summary>The OEM open bracket key on a US standard keyboard.</summary>
    OemOpenBrackets = 219, // 0x000000DB
    /// <summary>The OEM pipe key on a US standard keyboard.</summary>
    OemPipe = 220, // 0x000000DC
    /// <summary>The OEM close bracket key on a US standard keyboard.</summary>
    OemCloseBrackets = 221, // 0x000000DD
    /// <summary>
    /// The OEM singled/double quote key on a US standard keyboard.
    /// </summary>
    OemQuotes = 222, // 0x000000DE
    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard.
    /// </summary>
    Oem8 = 223, // 0x000000DF
    /// <summary>
    /// The OEM angle bracket or backslash key on the RT 102 key keyboard.
    /// </summary>
    OemBackslash = 226, // 0x000000E2
    /// <summary>IME PROCESS key.</summary>
    ProcessKey = 229, // 0x000000E5
    /// <summary>OEM Copy key.</summary>
    OemCopy = 242, // 0x000000F2
    /// <summary>OEM Auto key.</summary>
    OemAuto = 243, // 0x000000F3
    /// <summary>OEM Enlarge Window key.</summary>
    OemEnlW = 244, // 0x000000F4
    /// <summary>Attn key.</summary>
    Attn = 246, // 0x000000F6
    /// <summary>CrSel key.</summary>
    Crsel = 247, // 0x000000F7
    /// <summary>ExSel key.</summary>
    Exsel = 248, // 0x000000F8
    /// <summary>Erase EOF key.</summary>
    EraseEof = 249, // 0x000000F9
    /// <summary>Play key.</summary>
    Play = 250, // 0x000000FA
    /// <summary>Zoom key.</summary>
    Zoom = 251, // 0x000000FB
    /// <summary>PA1 key.</summary>
    Pa1 = 253, // 0x000000FD
    /// <summary>CLEAR key.</summary>
    OemClear = 254, // 0x000000FE
}

public static class KeyboardKeysExtensions
{
    /// <summary>
    /// Indicates whether the specified key is a modifier key (SHIFT, CONTROL, ALT).
    /// </summary>
    public static bool IsModifierKey(this KeyboardKeys key)
    {
        return key == KeyboardKeys.LeftShift || key == KeyboardKeys.RightShift ||
               key == KeyboardKeys.LeftControl || key == KeyboardKeys.RightControl ||
               key == KeyboardKeys.LeftAlt || key == KeyboardKeys.RightAlt;
    }
    
    public static char? ToChar(this KeyboardKeys key, bool shiftPressed)
    {
        if (key >= KeyboardKeys.A && key <= KeyboardKeys.Z)
        {
            char c = (char)('A' + (key - KeyboardKeys.A));
            return shiftPressed ? c : char.ToLower(c);
        }
        
        if (key >= KeyboardKeys.D0 && key <= KeyboardKeys.D9)
        {
            char c = (char)('0' + (key - KeyboardKeys.D0));
            if (shiftPressed)
            {
                return key switch
                {
                    KeyboardKeys.D1 => '!',
                    KeyboardKeys.D2 => '@',
                    KeyboardKeys.D3 => '#',
                    KeyboardKeys.D4 => '$',
                    KeyboardKeys.D5 => '%',
                    KeyboardKeys.D6 => '^',
                    KeyboardKeys.D7 => '&',
                    KeyboardKeys.D8 => '*',
                    KeyboardKeys.D9 => '(',
                    _ => c
                };
            }
            return c;
        }

        return null;
    }
}

public readonly ref struct RawKeyboardData
{
    public readonly ReadOnlySpan<KeyboardKeys> PressedKeys;
    public readonly bool CapsLock;
    public readonly bool NumLock;

    public RawKeyboardData(ReadOnlySpan<KeyboardKeys> pressedKeys, bool capsLock, bool numLock)
    {
        PressedKeys = pressedKeys;
        CapsLock = capsLock;
        NumLock = numLock;
    }
}

public interface IKeyboardState : IState
{
    event Action<char> TextInput;
    event Action<KeyboardKeys> KeyDown;
    event Action<KeyboardKeys> KeyUp;
    
    void Update(RawKeyboardData data);

    void UpdateInput(char text);

    /// <summary>
    /// Indicates whether the specified key is currently pressed.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool IsKeyPressed(KeyboardKeys key);
    /// <summary>
    /// Indicates whether the specified key is currently released.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool IsKeyReleased(KeyboardKeys key);
    
    /// <summary>
    /// Indicates whether the specified key was pressed at the last update, but is released now.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool WasKeyPressed(KeyboardKeys key);
    /// <summary>
    /// Indicates whether the specified key was released at the last update, but is pressed now.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool WasKeyReleased(KeyboardKeys key);
    
    /// <summary>
    /// Indicates whether the specified key was just pressed in this update.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool WasKeyJustPressed(KeyboardKeys key);
    /// <summary>
    /// Indicates whether the specified key was just released in this update.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool WasKeyJustReleased(KeyboardKeys key);
    
    public ReadOnlySpan<KeyboardKeys> GetPressedKeys();
    
    /// <summary>
    /// Indicates whether the CAPS LOCK is currently active.
    /// </summary>
    public bool IsCapsLockActive { get; }
    /// <summary>
    /// Indicates whether the NUM LOCK is currently active.
    /// </summary>
    public bool IsNumLockActive { get; }
}