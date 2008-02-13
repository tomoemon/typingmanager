using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Plugin
{
    public interface IKeyState
    {
        bool IsShift{get;}
        bool IsAlt{get;}
        bool IsControl{get;}
        int KeyCode { get;}
        string KeyName { get;}
        bool IsLastDown(Keys key);
        bool IsLastDown(int key);
        bool IsDown(Keys key);
        bool IsDown(int keycode);
        bool IsPush(Keys key);
        bool IsPush(int keycode);
    }
}
