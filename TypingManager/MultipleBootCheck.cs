using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace TypingManager
{
    public class MultipleBootCheck
    {
        // 外部プロセスのメイン・ウィンドウを起動するためのWin32 API
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);
		
        // 外部プロセスのウィンドウを最前面に表示する
		public static void WakeupWindow(IntPtr hWnd) 
		{ 
			// メイン・ウィンドウが最小化されていれば元に戻す
			if (IsIconic(hWnd))
			{
				ShowWindowAsync(hWnd, SW_RESTORE);
			}

			// メイン・ウィンドウを最前面に表示する
			SetForegroundWindow(hWnd);
		}

        // ShowWindowAsync関数のパラメータに渡す定義値
		private const int SW_RESTORE = 9;  // 画面を元の大きさに戻す

		/// <summary>
        /// 実行中のプロセスと同じProcessNameを持つプロセスを取得する
		/// </summary>
		/// <returns></returns>
		public static Process GetPreviousProcess()
		{
			Process curProcess = Process.GetCurrentProcess(); 
            // ProcessNameは実行ファイル名の.exeを抜いたもの
			Process[] allProcesses = Process.GetProcessesByName (curProcess.ProcessName); 

			foreach (Process checkProcess in allProcesses) 
			{ 
				// 自分自身のプロセスIDは無視する
				if (checkProcess.Id != curProcess.Id) 
				{
                    string prev = checkProcess.MainModule.FileName;
                    string cur = curProcess.MainModule.FileName;

					// プロセスのフルパスを比較して同じアプリケーションか検証
					if (String.Compare(prev, cur, true) == 0)
					{
						// 同じフルパス名のプロセスを取得
						return checkProcess;
					}
				}  
			}

			// 同じアプリケーションのプロセスが見つからない！  
			return null;  
		}
    }
}
