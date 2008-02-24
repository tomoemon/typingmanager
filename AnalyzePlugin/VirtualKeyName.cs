using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin
{
    public static class VirtualKeyName
    {
        // <キーコード, キーの名前>
        static Dictionary<int, string> key_name;

        public static string GetKeyName(int index)
        {
            if (key_name.ContainsKey(index))
            {
                return key_name[index];
            }
            return "";
        }

        static VirtualKeyName()
        {
            key_name = new Dictionary<int, string>();

            // 0から9
            for (int i = (int)'0'; i <= (int)'9'; i++)
            {
                key_name[i] = new string((char)i, 1);
            }

            // aからz
            for (int i = (int)'A'; i <= (int)'Z'; i++)
            {
                key_name[i] = new string((char)i, 1);
            }

            // テンキーの数字
            for (int i = 0x60; i <= 0x69; i++)
            {
                key_name[i] = string.Format("テンキー[{0}]", i - 0x60);
            }
            key_name[0x6A] = "テンキー[*]";
            key_name[0x6B] = "テンキー[+]";
            key_name[0x6C] = "テンキー[Enter]";
            key_name[0x6D] = "テンキー[-]";
            key_name[0x6E] = "テンキー[.]";
            key_name[0x6F] = "テンキー[/]";

            // F1からF24
            for (int i = 1; i <= 24; i++)
            {
                key_name[0x70 + i - 1] = string.Format("F{0}", i);
            }

            // その他
            key_name[0x08] = "BackSpace";
            key_name[0x09] = "Tab";
            key_name[0x0d] = "Enter";
            key_name[0x10] = "Shift";
            key_name[0x11] = "Ctrl";
            key_name[0x12] = "Alt";
            key_name[0x13] = "Pause";
            key_name[0x1b] = "Esc";
            key_name[0x1c] = "変換";
            key_name[0x1d] = "無変換";
            key_name[0x20] = "Space";
            key_name[0x21] = "PageUp";
            key_name[0x22] = "PageDown";
            key_name[0x23] = "End";
            key_name[0x24] = "Home";
            key_name[0x25] = "←";
            key_name[0x26] = "↑";
            key_name[0x27] = "→";
            key_name[0x28] = "↓";
            key_name[0x2c] = "PrintScreen";
            key_name[0x2d] = "Insert";
            key_name[0x2e] = "Delete";
            key_name[0x90] = "NumLock";
            key_name[0x91] = "ScrollLock";
            key_name[0x5b] = "左Win";
            key_name[0x5c] = "右Win";
            key_name[0xa0] = "左Shift";
            key_name[0xa1] = "右Shift";
            key_name[0xa2] = "左Ctrl";
            key_name[0xa3] = "右Ctrl";
            key_name[0xa4] = "左Alt";
            key_name[0xa5] = "右Alt";
            key_name[0xba] = ":";
            key_name[0xbb] = ";";
            key_name[0xbc] = ",";
            key_name[0xbd] = "-";
            key_name[0xbe] = ".";
            key_name[0xbf] = "/";
            key_name[0xc0] = "@";
            key_name[0xdb] = "[";
            key_name[0xdc] = "\\";
            key_name[0xdd] = "]";
            key_name[0xde] = "^";
            key_name[0xe2] = "_";
        }
    }

    public static class VirtualKeyCode
    {
        public const int VK_LBUTTON = 0x1;
        public const int VK_RBUTTON = 0x2;
        public const int VK_CANCEL = 0x3;
        public const int VK_MBUTTON = 0x4;
        public const int VK_XBUTTON1 = 0x5;
        public const int VK_XBUTTON2 = 0x6;
        public const int VK_BACK = 0x8;
        public const int VK_TAB = 0x9;
        public const int VK_CLEAR = 0x0C;
        public const int VK_RETURN = 0x0D;
        public const int VK_SHIFT = 0x10;
        public const int VK_CONTROL = 0x11;
        public const int VK_MENU = 0x12;
        public const int VK_PAUSE = 0x13;
        public const int VK_CAPITAL = 0x14;
        public const int VK_KANA = 0x15;
        public const int VK_HANGUEL = 0x15;
        public const int VK_HANGUL = 0x15;
        public const int VK_JUNJA = 0x17;
        public const int VK_FINAL = 0x18;
        public const int VK_HANJA = 0x19;
        public const int VK_KANJI = 0x19;
        public const int VK_ESCAPE = 0x1B;
        public const int VK_CONVERT = 0x1C;
        public const int VK_NONCONVERT = 0x1D;
        public const int VK_ACCEPT = 0x1E;
        public const int VK_MODECHANGE = 0x1F;
        public const int VK_SPACE = 0x20;
        public const int VK_PRIOR = 0x21;
        public const int VK_NEXT = 0x22;
        public const int VK_END = 0x23;
        public const int VK_HOME = 0x24;
        public const int VK_LEFT = 0x25;
        public const int VK_UP = 0x26;
        public const int VK_RIGHT = 0x27;
        public const int VK_DOWN = 0x28;
        public const int VK_SELECT = 0x29;
        public const int VK_PRINT = 0x2A;
        public const int VK_EXECUTE = 0x2B;
        public const int VK_SNAPSHOT = 0x2C;
        public const int VK_INSERT = 0x2D;
        public const int VK_DELETE = 0x2E;
        public const int VK_HELP = 0x2F;
        public const int VK_LWIN = 0x5B;
        public const int VK_RWIN = 0x5C;
        public const int VK_APPS = 0x5D;
        public const int VK_SLEEP = 0x5F;
        public const int VK_NUMPAD0 = 0x60;
        public const int VK_NUMPAD1 = 0x61;
        public const int VK_NUMPAD2 = 0x62;
        public const int VK_NUMPAD3 = 0x63;
        public const int VK_NUMPAD4 = 0x64;
        public const int VK_NUMPAD5 = 0x65;
        public const int VK_NUMPAD6 = 0x66;
        public const int VK_NUMPAD7 = 0x67;
        public const int VK_NUMPAD8 = 0x68;
        public const int VK_NUMPAD9 = 0x69;
        public const int VK_MULTIPLY = 0x6A;
        public const int VK_ADD = 0x6B;
        public const int VK_SEPARATOR = 0x6C;
        public const int VK_SUBTRACT = 0x6D;
        public const int VK_DECIMAL = 0x6E;
        public const int VK_DIVIDE = 0x6F;
        public const int VK_F1 = 0x70;
        public const int VK_F2 = 0x71;
        public const int VK_F3 = 0x72;
        public const int VK_F4 = 0x73;
        public const int VK_F5 = 0x74;
        public const int VK_F6 = 0x75;
        public const int VK_F7 = 0x76;
        public const int VK_F8 = 0x77;
        public const int VK_F9 = 0x78;
        public const int VK_F10 = 0x79;
        public const int VK_F11 = 0x7A;
        public const int VK_F12 = 0x7B;
        public const int VK_F13 = 0x7C;
        public const int VK_F14 = 0x7D;
        public const int VK_F15 = 0x7E;
        public const int VK_F16 = 0x7F;
        public const int VK_F17 = 0x80;
        public const int VK_F18 = 0x81;
        public const int VK_F19 = 0x82;
        public const int VK_F20 = 0x83;
        public const int VK_F21 = 0x84;
        public const int VK_F22 = 0x85;
        public const int VK_F23 = 0x86;
        public const int VK_F24 = 0x87;
        public const int VK_NUMLOCK = 0x90;
        public const int VK_SCROLL = 0x91;
        public const int VK_LSHIFT = 0xA0;
        public const int VK_RSHIFT = 0xA1;
        public const int VK_LCONTROL = 0xA2;
        public const int VK_RCONTROL = 0xA3;
        public const int VK_LMENU = 0xA4;
        public const int VK_RMENU = 0xA5;
        public const int VK_BROWSER_BACK = 0xA6;
        public const int VK_BROWSER_FORWARD = 0xA7;
        public const int VK_BROWSER_REFRESH = 0xA8;
        public const int VK_BROWSER_STOP = 0xA9;
        public const int VK_BROWSER_SEARCH = 0xAA;
        public const int VK_BROWSER_FAVORITES = 0xAB;
        public const int VK_BROWSER_HOME = 0xAC;
        public const int VK_VOLUME_MUTE = 0xAD;
        public const int VK_VOLUME_DOWN = 0xAE;
        public const int VK_VOLUME_UP = 0xAF;
        public const int VK_MEDIA_NEXT_TRACK = 0xB0;
        public const int VK_MEDIA_PREV_TRACK = 0xB1;
        public const int VK_MEDIA_STOP = 0xB2;
        public const int VK_MEDIA_PLAY_PAUSE = 0xB3;
        public const int VK_LAUNCH_MAIL = 0xB4;
        public const int VK_LAUNCH_MEDIA_SELECT = 0xB5;
        public const int VK_LAUNCH_APP1 = 0xB6;
        public const int VK_LAUNCH_APP2 = 0xB7;
        public const int VK_OEM_1 = 0xBA;
        public const int VK_OEM_PLUS = 0xBB;
        public const int VK_OEM_COMMA = 0xBC;
        public const int VK_OEM_MINUS = 0xBD;
        public const int VK_OEM_PERIOD = 0xBE;
        public const int VK_OEM_2 = 0xBF;
        public const int VK_OEM_3 = 0xC0;
        public const int VK_OEM_4 = 0xDB;
        public const int VK_OEM_5 = 0xDC;
        public const int VK_OEM_6 = 0xDD;
        public const int VK_OEM_7 = 0xDE;
        public const int VK_OEM_8 = 0xDF;
        public const int VK_OEM_102 = 0xE2;
        public const int VK_PROCESSKEY = 0xE5;
        public const int VK_PACKET = 0xE7;
        public const int VK_ATTN = 0xF6;
        public const int VK_CRSEL = 0xF7;
        public const int VK_EXSEL = 0xF8;
        public const int VK_EREOF = 0xF9;
        public const int VK_PLAY = 0xFA;
        public const int VK_ZOOM = 0xFB;
        public const int VK_NONAME = 0xFC;
        public const int VK_PA1 = 0xFD;
        public const int VK_OEM_CLEAR = 0xFE;
    }
}
