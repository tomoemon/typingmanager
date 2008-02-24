using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Reflection;

namespace TypingManager
{
    static class Program
    {
        private static Mutex _mutex;

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            /*
            AnalyzeTool plugin = PythonTool.Create();
            Console.WriteLine("author name:{0}", plugin.GetAuthorName());
            Console.WriteLine("template:{0}", plugin.GetInputTemplate());
            Console.WriteLine("analyze result:{0}", plugin.Run());
             */
            //Mutexクラスの作成
            string asm_name = Assembly.GetExecutingAssembly().GetName().Name;
            string cur_dir = Path.GetDirectoryName(Application.ExecutablePath);

            // Mutexの名前には'\'が入っているとダメなのでパス名に使えない'/'に置換
            string mutex_name = asm_name + "_" + cur_dir.Replace('\\', '/');
            
            _mutex = new Mutex(false, mutex_name);
            
            //ミューテックスの所有権を要求する
            if (_mutex.WaitOne(0, false) == false)
            {
                //テキストファイルリソースを取り出す
                string txt = Properties.Resources.ApplicationName;

                //すでに起動していると判断して終了
                MessageBox.Show(string.Format("{0}は同じディレクトリですでに起動中です",txt), txt);
                return;
            }

            try
            {
                MessageFilter.ChangeWindowMessageFilter(WinMessage.WM_KEYDOWN, FilterType.MSGFLT_ADD);
                MessageFilter.ChangeWindowMessageFilter(WinMessage.WM_KEYUP, FilterType.MSGFLT_ADD);
            }
            catch
            {
                // WindowsVista以外だとChangeWindowMessageFilter関数が見つからないので例外が発生
                // でも，Vista以外ではこの関数は呼び出さなくていいので問題なし．
            }


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
