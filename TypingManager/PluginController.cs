using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using Plugin;
using System.Windows.Forms;
using System.Xml;

namespace TypingManager
{
    public class PluginController : IStrokePluginController
    {
        /// <summary>
        /// <プラグインのアクセス名, リスト内のどこにあるか>
        /// 名前で呼び出せるように辞書形式で保持
        /// </summary>
        Dictionary<string, int> index_dic = new Dictionary<string, int>();

        List<IStrokePlugin> plugin_list = new List<IStrokePlugin>();

        private Form main_form;

        public PluginController(Form form)
        {
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
                    plugin_list.Add(plugin);
                    index_dic[name] = plugin_list.Count - 1;
                    GetSaveDir(name);   // プラグイン用ディレクトリの作成
                    Console.WriteLine("Plugin[{0}]: {1}", i, name);
                }
            }
            LoadPluginConfig();
        }

        /// <summary>キーが押されたときに呼び出される</summary>
        /// <param name="keycode">押されたキーの仮想キーコード</param>
        /// <param name="militime">キーが押された時間[ミリ秒]（OSが起動してからの経過時間）</param>
        /// <param name="app_path">キーが押されたアプリケーションのフルパス</param>
        /// <param name="app_title">キーが押されたウィンドウのタイトル</param>
        public void KeyDown(IKeyState key_state, int militime, string app_path, string app_title)
        {
            foreach (IStrokePlugin plugin in plugin_list)
            {
                if (plugin.Valid == true)
                {
                    plugin.KeyDown(key_state, militime, app_path, app_title);
                }
            }
        }

        /// <summary>キーが上がったときに呼び出される</summary>
        public void KeyUp(IKeyState key_state, int militime, string app_path, string app_title)
        {
            foreach (IStrokePlugin plugin in plugin_list)
            {
                if (plugin.Valid == true)
                {
                    plugin.KeyUp(key_state, militime, app_path, app_title);
                }
            }
        }

        public void Init()
        {
            foreach (IStrokePlugin plugin in plugin_list)
            {
                plugin.Init();
            }
        }

        public void Close()
        {
            foreach (IStrokePlugin plugin in plugin_list)
            {
                plugin.Close();
            }
            SavePluginConfig();
        }

        public void Add(IStrokePlugin plugin)
        {
            string name = plugin.GetAccessName();
            if (name != "")
            {
                plugin.Valid = true;
                plugin_list.Add(plugin);
                index_dic[name] = plugin_list.Count - 1;
            }
        }

        /// <summary>
        /// メインメニューのプラグイン欄に表示するメニューを返す
        /// </summary>
        /// <param name="parent_menu"></param>
        public void AddMenu(ToolStripMenuItem parent_menu)
        {
            int i = 1;
            foreach (IStrokePlugin plugin in plugin_list)
            {
                if (plugin.GetToolStripMenu() == null) continue;

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
            return plugin_list;
        }

        /// <summary>
        /// プラグインのログを保存するディレクトリを返す
        /// 基本的にはlogフォルダの中に「プラグイン名」フォルダを作成してその中に
        /// </summary>
        /// <param name="plugin_name"></param>
        /// <returns></returns>
        public string GetSaveDir(string plugin_name)
        {
            if (index_dic.ContainsKey(plugin_name))
            {
                string path = Path.Combine(LogDir.LOG_DIR, plugin_name);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
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
            if (index_dic.ContainsKey(name))
            {
                return plugin_list[index_dic[name]].GetInfo();
            }
            return null;
        }

        private void SavePluginConfig()
        {
            string filename = LogDir.PLUGIN_CONFIG_FILE;

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("\t");
            XmlWriter writer = XmlWriter.Create(filename, settings);
            try
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("PluginConfig");
                writer.WriteStartElement("PluginList");
                foreach (IStrokePlugin plugin in plugin_list)
                {
                    writer.WriteStartElement("Plugin");
                    writer.WriteAttributeString("plugin_name", "", plugin.GetPluginName());
                    writer.WriteAttributeString("access_name", "", plugin.GetAccessName());
                    writer.WriteAttributeString("valid", "", plugin.Valid.ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            finally
            {
                writer.Close();
            }
        }

        private void LoadPluginConfig()
        {
            string filename = LogDir.PLUGIN_CONFIG_FILE;

            if (File.Exists(filename))
            {
                string xml = "";
                using (StreamReader sr = new StreamReader(filename))
                {
                    xml = sr.ReadToEnd();
                }

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);

                XmlNodeList node_list = doc.SelectNodes("//Plugin");
                foreach (XmlNode plugin_node in node_list)
                {
                    XmlAttributeCollection plugin_attrs = plugin_node.Attributes;
                    bool valid = bool.Parse(plugin_attrs["valid"].Value);
                    string plugin_name = plugin_attrs["plugin_name"].Value;
                    string access_name = plugin_attrs["access_name"].Value;
                    if (index_dic.ContainsKey(access_name))
                    {
                        plugin_list[index_dic[access_name]].Valid = valid;
                    }
                }
            }
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
