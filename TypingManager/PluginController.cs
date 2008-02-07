using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using Plugin;
using System.Windows.Forms;

namespace TypingManager
{
    public class PluginController : IStrokePluginController
    {
        /// <summary>
        /// プラグインの呼び出し順序はあまり気にしなくてよいので
        /// 辞書形式で保持しておく
        /// </summary>
        Dictionary<string, IStrokePlugin> plugin_dic;

        private Form main_form;

        public PluginController(Form form)
        {
            plugin_dic = new Dictionary<string, IStrokePlugin>();
            main_form = form;
        }

        public void Load()
        {
            //IPlugin型の名前を取得
            string ipluginName = typeof(IStrokePlugin).FullName;

            //インストールされているプラグインを調べる
            PluginInfo[] pis = PluginInfo.FindPlugins(ipluginName);

            //すべてのプラグインのインスタンスを作成する
            for (int i = 0; i < pis.Length; i++)
            {
                IStrokePlugin plugin = (IStrokePlugin)pis[i].CreateInstance();
                string name = plugin.GetAccessName();
                string author = plugin.GetAuthorName();
                string version = plugin.GetVersion();
                if (name != "" && author != "" && version != "")
                {
                    plugin.Controller = this;
                    plugin.MainForm = main_form;
                    plugin.Valid = true;
                    plugin_dic[name] = plugin;
                    Console.WriteLine("Plugin[{0}]: {1}", i, name);
                }
            }

        }

        /// <summary>キーが押されたときに呼び出される</summary>
        /// <param name="keycode">押されたキーの仮想キーコード</param>
        /// <param name="militime">キーが押された時間[ミリ秒]（OSが起動してからの経過時間）</param>
        /// <param name="app_path">キーが押されたアプリケーションのフルパス</param>
        /// <param name="app_title">キーが押されたウィンドウのタイトル</param>
        public void KeyDown(int keycode, int militime, string app_path, string app_title)
        {
            foreach (IStrokePlugin plugin in plugin_dic.Values)
            {
                if (plugin.Valid == true)
                {
                    plugin.KeyDown(keycode, militime, app_path, app_title);
                }
            }
        }

        /// <summary>キーが上がったときに呼び出される</summary>
        public void KeyUp(int keycode, int militime, string app_path, string app_title)
        {
            foreach (IStrokePlugin plugin in plugin_dic.Values)
            {
                if (plugin.Valid == true)
                {
                    plugin.KeyUp(keycode, militime, app_path, app_title);
                }
            }
        }

        public void Close()
        {
            foreach (IStrokePlugin plugin in plugin_dic.Values)
            {
                plugin.Close();
            }
        }

        public void Add(string name, IStrokePlugin plugin)
        {
            plugin_dic[name] = plugin;
        }

        /// <summary>
        /// メインメニューのプラグイン欄に表示するメニューを返す
        /// </summary>
        /// <param name="parent_menu"></param>
        public void AddMenu(ToolStripMenuItem parent_menu)
        {
            int i = 1;
            foreach (IStrokePlugin plugin in plugin_dic.Values)
            {
                string menu_name;
                if (i <= 9)
                {
                    menu_name = string.Format("{0}(&{1})", plugin.GetPluginName(), i);
                }
                else
                {
                    menu_name = plugin.GetPluginName();
                }
                ToolStripMenuItem item = new ToolStripMenuItem(menu_name);
                foreach (ToolStripMenuItem menu_item in plugin.GetToolStripMenu())
                {
                    item.DropDownItems.Add(menu_item);
                }
                parent_menu.DropDownItems.Add(item);
                i++;
            }
        }

        public List<IStrokePlugin> GetPluginList()
        {
            return new List<IStrokePlugin>(plugin_dic.Values);
        }

        public string GetSaveDir(string plugin_name)
        {
            if (plugin_dic.ContainsKey(plugin_name))
            {
                string dir = Path.GetDirectoryName(Application.ExecutablePath);
                string path = Path.Combine(Path.Combine(dir, LogDir.PLUGIN_DIR), plugin_name);
            }
            return "";
        }

        /// <summary>
        /// 基本的に他のプラグインから，別のプラグインの情報を得るために呼ばれる
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object GetInfo(string name)
        {
            if (plugin_dic.ContainsKey(name))
            {
                return plugin_dic[name].GetInfo();
            }
            return null;
        }
    }

    /// <summary>
    /// プラグインに関する情報
    /// </summary>
    public class PluginInfo
    {
        private string _location;
        private string _className;

        /// <summary>
        /// PluginInfoクラスのコンストラクタ
        /// </summary>
        /// <param name="path">アセンブリファイルのパス</param>
        /// <param name="cls">クラスの名前</param>
        private PluginInfo(string path, string cls)
        {
            _location = path;
            _className = cls;
        }

        /// <summary>
        /// アセンブリファイルのパス
        /// </summary>
        public string Location
        {
            get { return _location; }
        }

        /// <summary>
        /// クラスの名前
        /// </summary>
        public string ClassName
        {
            get { return _className; }
        }

        /// <summary>
        /// 有効なプラグインを探す
        /// </summary>
        /// <returns>有効なプラグインのPluginInfo配列</returns>
        public static PluginInfo[] FindPlugins(string ipluginName)
        {
            ArrayList plugins = new ArrayList();

            //プラグインフォルダ
            string folder = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath),
                                            LogDir.PLUGIN_DIR);

            if (!Directory.Exists(folder))
            {
                throw new ApplicationException(
                    "プラグインフォルダ\"" + folder + "\"が見つかりませんでした。"
                    );
            }
            //.dllファイルを探す
            string[] dlls = Directory.GetFiles(folder, "*.dll");

            foreach (string dll in dlls)
            {
                try
                {
                    //アセンブリとして読み込む
                    Assembly asm = Assembly.LoadFrom(dll);
                    foreach (Type t in asm.GetTypes())
                    {
                        //アセンブリ内のすべての型について、
                        //プラグインとして有効か調べる
                        if (t.IsClass && t.IsPublic && !t.IsAbstract &&
                            t.GetInterface(ipluginName) != null)
                        {
                            //PluginInfoをコレクションに追加する
                            plugins.Add(new PluginInfo(dll, t.FullName));
                        }
                    }
                }
                catch
                {
                    throw new ApplicationException(
                        "プラグインの読み込み中に問題が発生しました。"
                    );
                }
            }

            //コレクションを配列にして返す
            return (PluginInfo[])plugins.ToArray(typeof(PluginInfo));
        }

        /// <summary>
        /// プラグインクラスのインスタンスを作成する
        /// </summary>
        /// <returns>プラグインクラスのインスタンス</returns>
        public object CreateInstance()
        {
            try
            {
                //アセンブリを読み込む
                Assembly asm = Assembly.LoadFrom(this.Location);

                //クラス名からインスタンスを作成する
                return asm.CreateInstance(this.ClassName);
            }
            catch
            {
                return null;
            }
        }
    }
}
