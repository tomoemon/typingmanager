using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Plugin;

namespace TypingManager
{
    public class KeyboardProxyHook : IDisposable, IKeyboardHookBase
    {
        private Process proxyProcess;

        public event KeyboardHookedEventHandler KeyboardHooked;

        /*
        [DllImport("user32.dll", SetLastError = true)]
        private static extern short GetAsyncKeyState(int vkey);

        private const int VK_LSHIFT = VirtualKeyCode.VK_LSHIFT;
        private const int VK_RSHIFT = VirtualKeyCode.VK_RSHIFT;
        private const int VK_LMENU = VirtualKeyCode.VK_LMENU;
        private const int VK_RMENU = VirtualKeyCode.VK_RMENU;
        private const int VK_LCONTROL = VirtualKeyCode.VK_LCONTROL;
        private const int VK_RCONTROL = VirtualKeyCode.VK_RCONTROL;
        private bool last_lshift = false;
        private bool last_lmenu = false;
        private bool last_lcontrol = false;

        private Dictionary<int, bool> special_key_state = new Dictionary<int, bool>();

        private void GetKeyState()
        {
            for (int i = 0; i < 256; i++)
            {
                if ((GetAsyncKeyState(i) & 0x8000) > 0)
                {
                    Console.Write(1);
                }
                else
                {
                    Console.Write(0);
                }
            }
        }

        private int CheckSpecialKeyUp(int vkey)
        {
            if (vkey == VirtualKeyCode.VK_SHIFT)
            {
                bool last_lstate = special_key_state[VK_LSHIFT];
                bool last_rstate = special_key_state[VK_RSHIFT];
                special_key_state[VK_LSHIFT] = (GetAsyncKeyState(VK_LSHIFT) & 0x8000) > 0;
                special_key_state[VK_RSHIFT] = (GetAsyncKeyState(VK_RSHIFT) & 0x8000) > 0;
                
                if (last_lstate && !special_key_state[VK_LSHIFT])
                {
                    return VK_LSHIFT;
                }
                if (last_rstate && !special_key_state[VK_RSHIFT])
                {
                    return VK_RSHIFT;
                }
            }
            else if (vkey == VirtualKeyCode.VK_MENU)
            {
                
            }
            else if (vkey == VirtualKeyCode.VK_CONTROL)
            {
                
            }
            return vkey;
        }

        private int CheckSpecialKeyDown(int vkey)
        {
            if (vkey == VirtualKeyCode.VK_SHIFT)
            {
                bool last_lstate = special_key_state[VK_LSHIFT];
                bool last_rstate = special_key_state[VK_RSHIFT];
                special_key_state[VK_LSHIFT] = (GetAsyncKeyState(VK_LSHIFT) & 0x8000) > 0;
                special_key_state[VK_RSHIFT] = (GetAsyncKeyState(VK_RSHIFT) & 0x8000) > 0;

                if (!last_lstate && special_key_state[VK_LSHIFT])
                {
                    last_lshift = true;
                }
                if (!last_rstate && special_key_state[VK_RSHIFT])
                {
                    last_lshift = false;
                }
                if (last_lshift)
                {
                    return VK_LSHIFT;
                }
                return VK_RSHIFT;
            }
            else if (vkey == VirtualKeyCode.VK_MENU)
            {
                
            }
            else if (vkey == VirtualKeyCode.VK_CONTROL)
            {
                
            }
            return vkey;
        }
         */

        private void KeyHook(object sender, MessageReceivedEventArgs e)
        {
            int keycode = (int)e.Message.WParam;
            KeyboardMessage message;

            if (((int)e.Message.LParam & 0x80000000) > 0)
            {
                message = KeyboardMessage.KeyUp;
                Console.WriteLine(keycode);
                //keycode = CheckSpecialKeyUp(keycode);
            }
            else
            {
                message = KeyboardMessage.KeyDown;
                //keycode = CheckSpecialKeyDown(keycode);
            }

            KeyboardState state = new KeyboardState();
            state.KeyCode = (Keys)Enum.Parse(typeof(Keys), keycode.ToString());
            state.ScanCode = 0;
            state.Time = 0;
            KeyboardHookedEventArgs keyargs = new KeyboardHookedEventArgs(message, ref state);
            KeyboardHooked(this, keyargs);
        }

        public KeyboardProxyHook()
        {
            // イベントを取得
            string watch_window = Application.ExecutablePath.Replace(" ", "");
            MessageEvents.WindowTitle = watch_window;
            MessageEvents.WatchMessage(WinMessage.WM_KEYDOWN);
            MessageEvents.WatchMessage(WinMessage.WM_KEYUP);

            MessageEvents.MessageReceived += new EventHandler<MessageReceivedEventArgs>(KeyHook);
            Console.WriteLine(watch_window);

            /*
            // 初期状態ではすべて入力してない状態
            special_key_state[VirtualKeyCode.VK_LSHIFT] = false;
            special_key_state[VirtualKeyCode.VK_RSHIFT] = false;
            special_key_state[VirtualKeyCode.VK_LCONTROL] = false;
            special_key_state[VirtualKeyCode.VK_RCONTROL] = false;
            special_key_state[VirtualKeyCode.VK_LMENU] = false;
            special_key_state[VirtualKeyCode.VK_RMENU] = false;
            */

            // キーボードフックを行う別プロセスを起動
            string program = "_proxy.exe";
            string argument = "/proxy " + watch_window;

            proxyProcess = new Process();
            proxyProcess.StartInfo.FileName = program;	//起動するファイル名
            proxyProcess.StartInfo.Arguments = argument;	//起動時の引数
            proxyProcess.Start();
        }

        public void Dispose()
        {
            if (!proxyProcess.HasExited)
            {
                // メイン ウィンドウにクローズ メッセージを送信する
                if (!proxyProcess.CloseMainWindow())
                {
                    proxyProcess.Kill();  // 終了しなかった場合は強制終了する
                }
                proxyProcess.Close();
                proxyProcess.Dispose();
            }
        }
    }
}
