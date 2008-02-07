using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;

namespace TypingManager
{
    public class ModuleIcon
    {
        // SHGetFileInfo関数
        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        // SHGetFileInfo関数で使用するフラグ
        public const uint SHGFI_ICON = 0x100; // アイコン・リソースの取得
        public const uint SHGFI_LARGEICON = 0x0; // 大きいアイコン
        public const uint SHGFI_SMALLICON = 0x1; // 小さいアイコン

        public static Icon GetIcon(string path, uint icon_type)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            IntPtr hSuccess = SHGetFileInfo(path, 0,
                ref shinfo, (uint)Marshal.SizeOf(shinfo), icon_type);
            if (hSuccess != IntPtr.Zero)
            {
                Icon appIcon = Icon.FromHandle(shinfo.hIcon);
                return appIcon;
            }
            return null;
        }

        // SHGetFileInfo関数で使用する構造体
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };
    }
}
