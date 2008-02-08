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

            // F1からF24
            for (int i = 1; i <= 24; i++)
            {
                key_name[0x70 + i - 1] = string.Format("F{0}", i);
            }

            // その他
            key_name[0x08] = "BackSpace";
            key_name[0x09] = "Tab";
            key_name[0x0d] = "Enter";
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
}
