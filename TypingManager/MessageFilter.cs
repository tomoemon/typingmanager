using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace TypingManager
{
    public class WinMessage
    {
        public const int WM_KEYDOWN = 0x100;
        public const int WM_KEYUP = 0x101;
    }

    public enum FilterType
    {
        MSGFLT_ADD = 1,
        MSGFLT_REMOVE = 2,
    }

    public class MessageFilter
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ChangeWindowMessageFilter(uint msg, FilterType type);
    }
}
