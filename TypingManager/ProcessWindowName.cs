using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace TypingManager
{
    public class ProcessWindowName
    {
        const int MAX_TITLE_LEN = 512;
        const int MAX_MODULE_LEN = 256;

        // OpenProcess関数でアクセスフラグを設定するための定数
        const int SYNCHRONIZE = 0x00100000;
        const int STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        const int PROCESS_TERMINATE = 0x0001;
        const int PROCESS_CREATE_THREAD = 0x0002;
        const int PROCESS_SET_SESSIONID = 0x0004;
        const int PROCESS_VM_OPERATION = 0x0008;
        const int PROCESS_VM_READ = 0x0010;
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_DUP_HANDLE = 0x0040;
        const int PROCESS_CREATE_PROCESS = 0x0080;
        const int PROCESS_SET_QUOTA = 0x0100;
        const int PROCESS_SET_INFORMATION = 0x0200;
        const int PROCESS_QUERY_INFORMATION = 0x0400;
        const int PROCESS_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFF;

        [DllImport("user32.dll")]
        extern static IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        extern static int GetWindowText(IntPtr hWnd, StringBuilder lpStr, int nMaxCount);

        [DllImport("user32.dll")]
        extern static int GetWindowThreadProcessId(IntPtr hWnd, ref IntPtr ProcessId);

        [DllImport("psapi.dll")]
        extern static int GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, StringBuilder lpStr, int nSize);

        /// <summary>
        /// この呼び出しの結果、プロセスの最初のモジュールのハンドルが hModule 変数に格納されます。
        /// 実際にはプロセスには名前がありませんが、プロセス内の最初のモジュールはそのプロセスの
        /// 実行可能モジュールになることを覚えておいてください。これで、返されたモジュールの
        /// ハンドル (hModule) を GetModuleFileNameEx() API または GetModuleBaseName() API で使用して、
        /// プロセスの実行可能モジュールのフルパス名または簡単なモジュール名を取得できます。
        /// どちらの関数でも、プロセスへのハンドル、モジュールへのハンドル、名前が返される
        /// バッファへのポインタ、バッファのサイズがパラメータとして使用されます。 
        /// </summary>
        /// <param name="hProcess"></param>
        /// <param name="hModule"></param>
        /// <param name="size"></param>
        /// <param name="cbReturn"></param>
        /// <returns></returns>
        [DllImport("psapi.dll")]
        extern static bool EnumProcessModules(IntPtr hProcess, ref IntPtr hModule, int size, ref int cbReturn);

        [DllImport("kernel32.dll")]
        extern static IntPtr OpenProcess(int dwDesireAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        extern static bool CloseHandle(IntPtr hObject);

        static StringBuilder title_text = new StringBuilder(MAX_TITLE_LEN);
        static StringBuilder mod_path = new StringBuilder(MAX_MODULE_LEN);

        /// <summary>
        /// 最前面にあるウィンドウのタイトル文字列を取得
        /// </summary>
        /// <returns></returns>
        public static string GetFrontWindowTitle()
        {
            // 最前面ウィンドウの hwnd を取得
            IntPtr hwnd = GetForegroundWindow();

            // タイトルバー文字列を取得
            GetWindowText(hwnd, title_text, title_text.Capacity);

            return title_text.ToString();
        }

        /// <summary>
        /// 最前面にあるウィンドウを生成したプロセスのフルパスを取得
        /// </summary>
        /// <returns></returns>
        public static string GetFrontProcessName()
        {
            IntPtr hwnd = GetForegroundWindow();
            IntPtr proc_id = IntPtr.Zero;

            // アクティブウィンドウのハンドルからプロセスIDを取得
            GetWindowThreadProcessId(hwnd, ref proc_id);

            // プロセスIDからプロセスハンドルを取得
            IntPtr proc_handle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ,
                false, proc_id.ToInt32());

            if (proc_handle != IntPtr.Zero)
            {
                // プロセスの実行モジュールハンドルを取得
                IntPtr mod_handle = IntPtr.Zero;
                int return_size = 0;
                bool bModules = EnumProcessModules(proc_handle, ref mod_handle,
                    Marshal.SizeOf(mod_handle), ref return_size);

                if (bModules)
                {
                    // 実行モジュールのフルパスを取得
                    int path_len = GetModuleFileNameEx(proc_handle, mod_handle, mod_path, mod_path.Capacity);
                    /*
                    Debug.Write("ProcID=" + proc_id.ToString() + ", ");
                    Debug.Write("ProcHandle=" + proc_handle.ToString() + ", ");
                    Debug.Write("ModHandle=" + mod_handle.ToString() + ", ");
                    Debug.Write("ModName=" + mod_path.ToString() + "\n");
                    */
                    CloseHandle(proc_handle);
                    return mod_path.ToString();
                }
            }
            return "";
        }
    }
}
