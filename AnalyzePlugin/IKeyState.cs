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
        bool IsPushKey(Keys key);
        bool IsPushKey(int keycode);
        bool GetKeyState(int keycode);
        bool GetKeyState(Keys key);
    }
}
