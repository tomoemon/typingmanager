using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Drawing;
using Plugin;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;

namespace TypingManager
{
    /// <summary>
    /// フィルタリングを行うプラグインを管理する
    /// KeyDownやKeyUpが呼び出された時に，それぞれの入力情報を
    /// 順番にすべてのフィルターに適用していく
    /// </summary>
    public class FilterController : IFilterPluginController
    {
        List<IFilterPlugin> filter_plugin_list = new List<IFilterPlugin>();
        Dictionary<string, int> plugin_order = new Dictionary<string, int>();
        private PluginController controller;

        public FilterController(PluginController _controller)
        {
            controller = _controller;
        }

        #region プロパティ...
        public int Count
        {
            get { return filter_plugin_list.Count; }
        }
        #endregion

        public List<IFilterPlugin> GetFilterPluginList()
        {
            return filter_plugin_list;
        }

        public void AddFilterPlugin(IFilterPlugin plugin)
        {
            plugin.FilterController = this;
            plugin_order[plugin.GetAccessName()] = filter_plugin_list.Count;
            filter_plugin_list.Add(plugin);
        }

        /// <summary>
        /// PluginControllerから呼ばれる関数
        /// 最初のフィルタープラグインを起動させる
        /// </summary>
        /// <param name="key_state"></param>
        /// <param name="militime"></param>
        /// <param name="app_path"></param>
        /// <param name="app_title"></param>
        public void KeyDown(IKeyState key_state, uint militime, string app_path, string app_title)
        {
            int first_order = 0;
            while (first_order < Count && !filter_plugin_list[first_order].Valid)
            {
                ++first_order;
            }
            if (first_order < Count)
            {
                filter_plugin_list[first_order].KeyDown(key_state, militime, app_path, app_title);
            }
            else
            {
                controller.FilteredKeyDown(key_state, militime, app_path, app_title);
            }
        }
        
        /// <summary>
        /// PluginControllerから呼ばれる関数
        /// 最初のフィルタープラグインを起動させる
        /// </summary>
        /// <param name="key_state"></param>
        /// <param name="militime"></param>
        /// <param name="app_path"></param>
        /// <param name="app_title"></param>
        public void KeyUp(IKeyState key_state, uint militime, string app_path, string app_title)
        {
            int first_order = 0;
            while (first_order < Count && !filter_plugin_list[first_order].Valid)
            {
                ++first_order;
            }
            if (first_order < Count)
            {
                filter_plugin_list[first_order].KeyUp(key_state, militime, app_path, app_title);
            }
            else
            {
                controller.FilteredKeyUp(key_state, militime, app_path, app_title);
            }
        }

        /// <summary>
        /// 各フィルタープラグインから呼ばれる
        /// 受け取ったデータは次のフィルターか実際のデータ記録プラグインに渡す
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="key_state"></param>
        /// <param name="militime"></param>
        /// <param name="app_path"></param>
        /// <param name="app_title"></param>
        public void FilteredKeyDown(IFilterPlugin plugin, IKeyState key_state, uint militime,
            string app_path, string app_title)
        {
            int next_order = plugin_order[plugin.GetAccessName()] + 1;
            while (next_order < Count && !filter_plugin_list[next_order].Valid)
            {
                ++next_order;
            }
            if (next_order < Count)
            {
                filter_plugin_list[next_order].KeyDown(key_state, militime, app_path, app_title);
            }
            else
            {
                controller.FilteredKeyDown(key_state, militime, app_path, app_title);
            }
        }

        /// <summary>
        /// 各フィルタープラグインから呼ばれる
        /// 受け取ったデータは次のフィルターか実際のデータ記録プラグインに渡す
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="key_state"></param>
        /// <param name="militime"></param>
        /// <param name="app_path"></param>
        /// <param name="app_title"></param>
        public void FilteredKeyUp(IFilterPlugin plugin, IKeyState key_state, uint militime,
            string app_path, string app_title)
        {
            int next_order = plugin_order[plugin.GetAccessName()] + 1;
            while (next_order < Count && !filter_plugin_list[next_order].Valid)
            {
                ++next_order;
            }
            if (next_order < Count)
            {
                filter_plugin_list[next_order].KeyUp(key_state, militime, app_path, app_title);
            }
            else
            {
                controller.FilteredKeyUp(key_state, militime, app_path, app_title);
            }
        }
    }

    public class PluginController : IStrokePluginController
    {
        /// <summary>
        /// ＜プラグインのアクセス名, リスト内のどこにあるか＞
        /// 名前で呼び出せるように辞書形式で保持
        /// </summary>
        Dictionary<string, IPluginBase> index_dic = new Dictionary<string, IPluginBase>();
        List<IStrokePlugin> stroke_plugin_list = new List<IStrokePlugin>();
        FilterController filter_controller;
        
        private Form main_form;

        public PluginController(Form form)
        {
            main_form = form;
            filter_controller = new FilterController(this);
        }


        public void Load()
        {
            LoadFilterPlugin();
            LoadStrokePlugin();
            LoadPluginConfig();
            Init();
        }

        private void LoadFilterPlugin()
        {
            // IFilterPlugin型の名前を取得
            string ipluginName = typeof(IFilterPlugin).FullName;

            // インストールされているプラグインを調べる
            PluginInfo[] pis = PluginInfo.FindPlugins(ipluginName);

            // すべてのプラグインのインスタンスを作成する
            for (int i = 0; i < pis.Length; i++)
            {
                IFilterPlugin plugin = (IFilterPlugin)pis[i].CreateInstance();
                AddFilterPlugin(plugin);
            }
        }

        private void LoadStrokePlugin()
        {
            // IStrokePlugin型の名前を取得
            string ipluginName = typeof(IStrokePlugin).FullName;

            // インストールされているプラグインを調べる
            PluginInfo[] pis = PluginInfo.FindPlugins(ipluginName);

            // すべてのプラグインのインスタンスを作成する
            for (int i = 0; i < pis.Length; i++)
            {
                IStrokePlugin plugin = (IStrokePlugin)pis[i].CreateInstance();
                AddStrokePlugin(plugin);
            }
        }

        /// <summary>キーが押されたときに呼び出される</summary>
        /// <param name="key_state">押されたキーの仮想キーコード</param>
        /// <param name="militime">キーが押された時間[ミリ秒]（OSが起動してからの経過時間）</param>
        /// <param name="app_path">キーが押されたアプリケーションのフルパス</param>
        /// <param name="app_title">キーが押されたウィンドウのタイトル</param>
        public void KeyDown(IKeyState key_state, uint militime, string app_path, string app_title)
        {
            filter_controller.KeyDown(key_state, militime, app_path, app_title);
        }

        /// <summary>
        /// キーが上がったときに呼び出される
        /// </summary>
        /// <param name="key_state"></param>
        /// <param name="militime"></param>
        /// <param name="app_path"></param>
        /// <param name="app_title"></param>
        public void KeyUp(IKeyState key_state, uint militime, string app_path, string app_title)
        {
            filter_controller.KeyUp(key_state, militime, app_path, app_title);
        }

        public void FilteredKeyDown(IKeyState key_state, uint militime, string app_path, string app_title)
        {
            foreach (IStrokePlugin plugin in stroke_plugin_list)
            {
                if (plugin.Valid == true)
                {
                    plugin.KeyDown(key_state, militime, app_path, app_title);
                }
            }
        }

        public void FilteredKeyUp(IKeyState key_state, uint militime, string app_path, string app_title)
        {
            foreach (IStrokePlugin plugin in stroke_plugin_list)
            {
                if (plugin.Valid == true)
                {
                    plugin.KeyUp(key_state, militime, app_path, app_title);
                }
            }
        }

        public void Init()
        {
            foreach (IPluginBase plugin in index_dic.Values)
            {
                plugin.Init();
            }
        }

        public void Close()
        {
            foreach (IPluginBase plugin in index_dic.Values)
            {
                plugin.Close();
            }
            SavePluginConfig();
        }

        public void AddFilterPlugin(IFilterPlugin plugin)
        {
            string name = plugin.GetAccessName();
            if (name != "" && !index_dic.ContainsKey(name))
            {
                plugin.Controller = this;
                plugin.MainForm = main_form;
                //plugin.Valid = false; // 起動時の有効・無効はプラグイン内部の設定に任せる
                filter_controller.AddFilterPlugin(plugin);
                index_dic[name] = plugin;
                Debug.WriteLine(string.Format("FilterPlugin: {0}", name));
            }
        }

        public void AddStrokePlugin(IStrokePlugin plugin)
        {
            string name = plugin.GetAccessName();
            if (name != "" && !index_dic.ContainsKey(name))
            {
                plugin.Controller = this;
                plugin.MainForm = main_form;
                //plugin.Valid = true; // 起動時の有効・無効はプラグイン内部の設定に任せる
                stroke_plugin_list.Add(plugin);
                index_dic[name] = plugin;
                Debug.WriteLine(string.Format("StrokePlugin: {0}", name));
            }
        }

        /// <summary>
        /// メインメニューのプラグイン欄に表示するメニューを返す
        /// </summary>
        /// <param name="parent_menu"></param>
        public void AddMenu(ToolStripMenuItem parent_menu)
        {
            int i = 1;
            foreach (IPluginBase plugin in stroke_plugin_list)
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
            foreach (IPluginBase plugin in filter_controller.GetFilterPluginList())
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

        public List<IStrokePlugin> GetStrokePluginList()
        {
            return stroke_plugin_list;
        }

        public List<IFilterPlugin> GetFilterPluginList()
        {
            return filter_controller.GetFilterPluginList();
        }

        /// <summary>
        /// プラグインのログを保存するディレクトリを返す
        /// 基本的にはlog/フォルダの中に「プラグイン名」フォルダを作成してその中に
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
        /// プラグインの設定を保存するディレクトリを返す
        /// 基本的にはconfig/フォルダの中に「プラグイン名」フォルダを作成してその中に
        /// </summary>
        /// <param name="plugin_name"></param>
        /// <returns></returns>
        public string GetConfigDir(string plugin_name)
        {
            if (index_dic.ContainsKey(plugin_name))
            {
                string path = Path.Combine(LogDir.CONFIG_DIR, plugin_name);
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
                return index_dic[name].GetInfo();
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
                writer.WriteStartElement("StrokePluginList");
                foreach (IPluginBase plugin in stroke_plugin_list)
                {
                    writer.WriteStartElement("Plugin");
                    writer.WriteAttributeString("plugin_name", "", plugin.GetPluginName());
                    writer.WriteAttributeString("access_name", "", plugin.GetAccessName());
                    writer.WriteAttributeString("valid", "", plugin.Valid.ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteStartElement("FilterPluginList");
                foreach (IPluginBase plugin in filter_controller.GetFilterPluginList())
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

                // 現在のStrokePluginの順番を読み出し
                Dictionary<string, int> plugin_order = new Dictionary<string, int>();
                for (int i = 0; i < stroke_plugin_list.Count; i++)
                {
                    plugin_order[stroke_plugin_list[i].GetAccessName()] = i;
                }

                // StrokePluginに関する設定の読み込み
                XmlNodeList node_list = doc.SelectNodes("PluginConfig/StrokePluginList/Plugin");
                Console.WriteLine("StrokePluginList ノード数:{0}", node_list.Count);
                for(int i=0; i<node_list.Count; i++)
                {
                    XmlNode plugin_node = node_list[i];
                    XmlAttributeCollection plugin_attrs = plugin_node.Attributes;
                    bool valid = bool.Parse(plugin_attrs["valid"].Value);
                    string plugin_name = plugin_attrs["plugin_name"].Value;
                    string access_name = plugin_attrs["access_name"].Value;
                    
                    if (index_dic.ContainsKey(access_name))
                    {
                        index_dic[access_name].Valid = valid;
                        if (i < plugin_order[access_name])
                        {
                            IStrokePlugin temp = stroke_plugin_list[i];
                            stroke_plugin_list[i] = stroke_plugin_list[plugin_order[access_name]];
                            stroke_plugin_list[plugin_order[access_name]] = temp;
                            /*
                            Console.WriteLine("{0}と{1}の交換", i, plugin_order[access_name]);
                            foreach (IStrokePlugin p in stroke_plugin_list)
                            {
                                Console.WriteLine(p.GetAccessName());
                            }
                             */
                        }
                    }
                }
                plugin_order.Clear();

                // 現在のFilterPluginの順番を読み出し
                List<IFilterPlugin> filter_plugin_list = filter_controller.GetFilterPluginList();
                for (int i = 0; i < filter_plugin_list.Count; i++)
                {
                    plugin_order[filter_plugin_list[i].GetAccessName()] = i;
                }

                // FilterPluginに関する設定の読み込み
                node_list = doc.SelectNodes("PluginConfig/FilterPluginList/Plugin");
                Console.WriteLine("FilterPluginList ノード数:{0}", node_list.Count);
                for (int i = 0; i < node_list.Count; i++)
                {
                    XmlNode plugin_node = node_list[i];
                    XmlAttributeCollection plugin_attrs = plugin_node.Attributes;
                    bool valid = bool.Parse(plugin_attrs["valid"].Value);
                    string plugin_name = plugin_attrs["plugin_name"].Value;
                    string access_name = plugin_attrs["access_name"].Value;

                    if (index_dic.ContainsKey(access_name))
                    {
                        index_dic[access_name].Valid = valid;
                        if (i < plugin_order[access_name])
                        {
                            IFilterPlugin temp = filter_plugin_list[i];
                            filter_plugin_list[i] = filter_plugin_list[plugin_order[access_name]];
                            filter_plugin_list[plugin_order[access_name]] = temp;
                        }
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
