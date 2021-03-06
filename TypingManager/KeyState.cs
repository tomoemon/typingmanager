using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Plugin;

namespace TypingManager
{
    public class KeyState : Plugin.IKeyState
    {
        // 今押されているか <押されているキーコード, 値は使わない>
        private Dictionary<int, int> down_key = new Dictionary<int, int>();

        // 前回押されていたキー
        private int last_down = 0;

        // ひとつ前に押されていたキー
        private int down = 0;

        // ひとつ前に離されたキー
        private int up = 0;

        // 直前のKeyDownかKeyUpで呼ばれたキーコード
        private int called_key = 0;

        // called_keyのキー名
        private string called_key_name = "";

        private const int VK_LMENU = (int)Keys.LMenu;
        private const int VK_RMENU = (int)Keys.RMenu;
        private const int VK_MENU = (int)Keys.Menu;
        private const int VK_LSHIFT = (int)Keys.LShiftKey;
        private const int VK_RSHIFT = (int)Keys.RShiftKey;
        private const int VK_SHIFT = (int)Keys.ShiftKey;
        private const int VK_LCTRL = (int)Keys.LControlKey;
        private const int VK_RCTRL = (int)Keys.RControlKey;
        private const int VK_CTRL = (int)Keys.ControlKey;

        public int KeyCode
        {
            get { return called_key; }
        }
        public string KeyName
        {
            get { return called_key_name; }
        }
        public int UpKey
        {
            get { return up; }
        }
        public int DownKey
        {
            get { return down; }
        }

        public void KeyDown(int keycode)
        {
            down = keycode;
            called_key = keycode;
            called_key_name = VirtualKeyName.GetKeyName(keycode);
            down_key[keycode] = 1;
        }

        public void KeyUp(int keycode)
        {
            if (down_key.ContainsKey(keycode))
            {
                down_key.Remove(keycode);
            }
            if (keycode == last_down)
            {
                last_down = 0;
            }
            up = keycode;
            called_key = keycode;
            called_key_name = VirtualKeyName.GetKeyName(keycode);
        }

        /// <summary>
        /// 次回押された際に続けて押されているのか，新しく押されたのかを
        /// 正しく返すために，各キーの処理が終わってから呼ばれる
        /// 前回の押されたキーの状態を保存する
        /// </summary>
        public void SetDownState()
        {
            last_down = down;   
        }

        public bool IsDown(Keys key)
        {
            return down_key.ContainsKey((int)key);
        }

        public bool IsDown(int keycode)
        {
            return down_key.ContainsKey(keycode);
        }

        public bool IsLastDown(Keys key)
        {
            return (int)key == last_down;
        }

        public bool IsLastDown(int keycode)
        {
            return keycode == last_down;
        }

        public bool IsPush(Keys key)
        {
            return IsDown(key) && !IsLastDown(key);
        }

        public bool IsPush(int keycode)
        {
            return IsDown(keycode) && !IsLastDown(keycode);
        }

        public bool IsAlt
        {
            get { return IsDown(VK_LMENU) || IsDown(VK_RMENU) || IsDown(VK_MENU); }
        }

        public bool IsControl
        {
            get { return IsDown(VK_LCTRL) || IsDown(VK_RCTRL) || IsDown(VK_CTRL); }
        }

        public bool IsShift
        {
            get { return IsDown(VK_LSHIFT) || IsDown(VK_RSHIFT) || IsDown(VK_SHIFT); }
        }
    }
}
