using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace TypingManager
{
    public class KeyboardProxyHook : IDisposable, IKeyboardHookBase
    {
        private Process proxyProcess;

        public event KeyboardHookedEventHandler KeyboardHooked;

        private void KeyHook(object sender, MessageReceivedEventArgs e)
        {
            KeyboardMessage message;
            if (((int)e.Message.LParam & 0x80000000) > 0)
            {
                message = KeyboardMessage.KeyUp;
                Debug.WriteLine((int)e.Message.WParam);
            }
            else
            {
                message = KeyboardMessage.KeyDown;
            }

            KeyboardState state = new KeyboardState();
            state.KeyCode = (Keys)Enum.Parse(typeof(Keys), ((int)e.Message.WParam).ToString());
            state.ScanCode = 0;
            state.Time = 0;
            KeyboardHookedEventArgs keyargs = new KeyboardHookedEventArgs(message, ref state);
            KeyboardHooked(this, keyargs);
        }

        public KeyboardProxyHook()
        {
            string watch_window = Application.ExecutablePath.Replace(" ", "");
            MessageEvents.WindowTitle = watch_window;
            MessageEvents.WatchMessage(WinMessage.WM_KEYDOWN);
            MessageEvents.WatchMessage(WinMessage.WM_KEYUP);

            MessageEvents.MessageReceived += new EventHandler<MessageReceivedEventArgs>(KeyHook);
            Debug.WriteLine(watch_window);

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
