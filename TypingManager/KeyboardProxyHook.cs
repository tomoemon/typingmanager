using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using Plugin;

namespace TypingManager
{
    public class KeyboardProxyHook : IDisposable, IKeyboardHookBase
    {
        public event KeyboardHookedEventHandler KeyboardHooked;

        [DllImport(PROXY_DLL_32, EntryPoint = "SetHook")]
        private extern static bool SetHook32(IntPtr hwnd);
        
        [DllImport(PROXY_DLL_32, EntryPoint = "ResetHook")]
        private extern static void ResetHook32();

        [DllImport(PROXY_DLL_64, EntryPoint = "SetHook")]
        private extern static bool SetHook64(IntPtr hwnd);

        [DllImport(PROXY_DLL_64, EntryPoint = "ResetHook")]
        private extern static void ResetHook64();

        private static bool SetHook(IntPtr hwnd)
        {
            if (System.Environment.Is64BitOperatingSystem)
            {
                return SetHook64(hwnd);
            }
            else
            {
                return SetHook32(hwnd);
            }
        }

        private static void ResetHook()
        {
            if (System.Environment.Is64BitOperatingSystem)
            {
                ResetHook64();
            }
            else
            {
                ResetHook32();
            }
        }


        public const string PROXY_DLL_32 = "KeyboardHookDll32.dll";
        public const string PROXY_DLL_64 = "KeyboardHookDll64.dll";

        public static bool IsExists()
        {
            return File.Exists(PROXY_DLL_32) && File.Exists(PROXY_DLL_64);
        }

        private void KeyUp(MessageReceivedEventArgs e)
        {
            int keycode = (int)e.Message.WParam;
            KeyboardMessage message = KeyboardMessage.KeyUp;

            KeyboardState state = new KeyboardState();
            state.KeyCode = (Keys)Enum.Parse(typeof(Keys), keycode.ToString());
            state.ScanCode = 0;
            state.Time = 0;
            KeyboardHookedEventArgs keyargs = new KeyboardHookedEventArgs(message, ref state);
            KeyboardHooked(this, keyargs);
        }

        private void KeyDown(MessageReceivedEventArgs e)
        {
            int keycode = (int)e.Message.WParam;
            KeyboardMessage message = KeyboardMessage.KeyDown;

            KeyboardState state = new KeyboardState();
            state.KeyCode = (Keys)Enum.Parse(typeof(Keys), keycode.ToString());
            state.ScanCode = 0;
            state.Time = 0;
            KeyboardHookedEventArgs keyargs = new KeyboardHookedEventArgs(message, ref state);
            KeyboardHooked(this, keyargs);
        }

        public KeyboardProxyHook()
        {
            SetHook(MessageEvents.WindowHandle);
            MessageEvents.WatchMessage(WinMessage.WM_KEYDOWN, this.KeyDown);
            MessageEvents.WatchMessage(WinMessage.WM_KEYUP, this.KeyUp);
        }

        public void Dispose()
        {
            ResetHook();
        }
    }
}
