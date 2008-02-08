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

        // ひとつ前に押されていたキー
        private int down = 0;

        // ひとつ前に離されたキー
        private int up = 0;

        // 直前のKeyDownかKeyUpで呼ばれたキーコード
        private int called_key = 0;

        // 押しっぱなしかどうか
        private bool pushed = false;

        private int VK_LMENU = (int)Keys.LMenu;
        private int VK_RMENU = (int)Keys.RMenu;
        private int VK_LSHIFT = (int)Keys.LShiftKey;
        private int VK_RSHIFT = (int)Keys.RShiftKey;
        private int VK_LCTRL = (int)Keys.LControlKey;
        private int VK_RCTRL = (int)Keys.RControlKey;

        public bool this[int key]
        {
            get { return down_key.ContainsKey(key); }
        }
        public int KeyCode
        {
            get { return called_key; }
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
            if (down == keycode)
            {
                pushed = false;
            }
            else
            {
                pushed = true;
            }
            down = keycode;
            called_key = keycode;
            down_key[keycode] = 1;
        }

        public void KeyUp(int keycode)
        {
            if (down_key.ContainsKey(keycode))
            {
                down_key.Remove(keycode);
            }
            up = keycode;
            called_key = keycode;
        }

        public bool GetKeyState(Keys key)
        {
            return this[(int)key];
        }

        public bool GetKeyState(int keycode)
        {
            return this[keycode];
        }

        public bool IsPushKey(Keys key)
        {
            return this[(int)key] && pushed;
        }

        public bool IsPushKey(int keycode)
        {
            return this[keycode] && pushed;
        }

        public bool IsAlt
        {
            get { return this[VK_LMENU] || this[VK_RMENU]; }
        }

        public bool IsControl
        {
            get { return this[VK_LCTRL] || this[VK_RCTRL]; }
        }

        public bool IsShift
        {
            get { return this[VK_LSHIFT] || this[VK_RSHIFT]; }
        }
    }
}
