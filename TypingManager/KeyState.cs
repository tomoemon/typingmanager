using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Plugin;

namespace TypingManager
{
    public class KeyState : Plugin.IKeyState
    {
        // ��������Ă��邩 <������Ă���L�[�R�[�h, �l�͎g��Ȃ�>
        private Dictionary<int, int> down_key = new Dictionary<int, int>();

        // �O�񉟂���Ă����L�[
        private int last_down = 0;

        // �ЂƂO�ɉ�����Ă����L�[
        private int down = 0;

        // �ЂƂO�ɗ����ꂽ�L�[
        private int up = 0;

        // ���O��KeyDown��KeyUp�ŌĂ΂ꂽ�L�[�R�[�h
        private int called_key = 0;

        // called_key�̃L�[��
        private string called_key_name = "";

        private int VK_LMENU = (int)Keys.LMenu;
        private int VK_RMENU = (int)Keys.RMenu;
        private int VK_LSHIFT = (int)Keys.LShiftKey;
        private int VK_RSHIFT = (int)Keys.RShiftKey;
        private int VK_LCTRL = (int)Keys.LControlKey;
        private int VK_RCTRL = (int)Keys.RControlKey;

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
        /// ���񉟂��ꂽ�ۂɑ����ĉ�����Ă���̂��C�V���������ꂽ�̂���
        /// �������Ԃ����߂ɁC�e�L�[�̏������I����Ă���Ă΂��
        /// �O��̉����ꂽ�L�[�̏�Ԃ�ۑ�����
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
            get { return IsDown(VK_LMENU) || IsDown(VK_RMENU); }
        }

        public bool IsControl
        {
            get { return IsDown(VK_LCTRL) || IsDown(VK_RCTRL); }
        }

        public bool IsShift
        {
            get { return IsDown(VK_LSHIFT) || IsDown(VK_RSHIFT); }
        }
    }
}
