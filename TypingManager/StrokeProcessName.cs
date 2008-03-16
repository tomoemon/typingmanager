using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;

namespace TypingManager
{
    public class ProcessNameInfo
    {
        // 大文字・小文字がもとのままのパス
        public string original_path;

        // 比較用にすべて小文字にしたパス
        public string path;
        public string name;
        public int id;

        // これまでのすべての合計打鍵数
        public int total;
        public ProcessNameInfo(string _original_path, string _path, string _name, int _id)
        {
            original_path = _original_path;
            path = _path;
            name = _name;
            id = _id;
        }
        public ProcessNameInfo() { }
    }

    public class StrokeProcessName : Plugin.BaseStrokePlugin, Plugin.IProcessNameData
    {
        public const string NO_TARGET = "null";

        // app_path（すべて小文字にしたもの） : app_info
        private Dictionary<string, ProcessNameInfo> path_dic;

        // app_id : app_path（すべて小文字にしたもの）
        private Dictionary<int, string> id_dic;
        private int next_id;

        #region プロパティ...
        public Dictionary<string, ProcessNameInfo> ProcessDic
        {
            get { return path_dic; }
        }
        public int TotalApp
        {
            get { return path_dic.Count - 1; }
        }
        #endregion


        #region BaseStrokePluginの実装上書き
        /// <summary>プラグインの名前を返すこと</summary>
        public override string GetPluginName() { return "プロセスID管理"; }

        /// <summary>プラグインの名前を返すこと</summary>
        public override string GetAccessName() { return "process_name"; }

        /// <summary>プラグインに関する簡単な説明を書くこと</summary>
        public override string GetComment() { return "プロセス名とID情報を管理します"; }

        /// <summary>プラグイン作者の名前を返すこと</summary>
        public override string GetAuthorName() { return "tomoemon"; }

        /// <summary>プラグインのバージョンを書くこと</summary>
        public override string GetVersion() { return "0.0.1"; }

        public override object GetInfo()
        {
            return (Plugin.IProcessNameData)this;
        }

        public override void AutoSave()
        {
            Save();
        }
        public override void Close()
        {
            Save();
        }
        #endregion


        public StrokeProcessName()
        {
            base.Valid = true;
            path_dic = new Dictionary<string, ProcessNameInfo>();
            id_dic = new Dictionary<int, string>();
            Add(NO_TARGET, NO_TARGET);
            next_id = 1;
        }

        /// <summary>
        /// すべて小文字にしたパスを返す
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetPath(int id)
        {
            if (id_dic.ContainsKey(id))
            {
                return id_dic[id];
            }
            return "";
        }

        /// <summary>
        /// 大文字・小文字がそのままのパスを返す
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetOriginalPath(int id)
        {
            if (id_dic.ContainsKey(id))
            {
                string small_path = id_dic[id];
                if (path_dic.ContainsKey(small_path))
                {
                    return path_dic[small_path].original_path;
                }
            }
            return "";
        }

        public string GetName(int id)
        {
            if (id_dic.ContainsKey(id))
            {
                return path_dic[id_dic[id]].name;
            }
            return "";
        }

        public string GetName(string path)
        {
            if (path_dic.ContainsKey(path))
            {
                return path_dic[path].name;
            }
            return "";
        }

        public int GetID(string path)
        {
            string small_path = path.ToLower();
            if (path_dic.ContainsKey(small_path))
            {
                return path_dic[small_path].id;
            }
            return -1;
        }

        public void SetName(string path, string name)
        {
            string small_path = path.ToLower();
            if (path_dic.ContainsKey(small_path))
            {
                path_dic[small_path].name = name;
            }
        }

        public List<int> GetProcessList()
        {
            return new List<int>(id_dic.Keys);
        }

        public int GetTotal(int id)
        {
            if (id_dic.ContainsKey(id))
            {
                return path_dic[id_dic[id]].total;
            }
            return 0;
        }

        public void Stroke(string app_path)
        {
            string small_path = app_path.ToLower();
            path_dic[small_path].total++;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app_path">オリジナルのパス名（大文字・小文字がそのまま）</param>
        /// <param name="app_name">パス名から拡張子を除いたファイル名</param>
        /// <returns></returns>
        public int Add(string app_path, string app_name)
        {
            ProcessNameInfo info;
            int regist_id = next_id;
            string small_path = app_path.ToLower();

            if (path_dic.ContainsKey(small_path))
            {
                info = path_dic[small_path];
                info.name = app_name;
                regist_id = info.id;
            }
            else
            {
                info = new ProcessNameInfo(app_path, small_path, app_name, next_id);
                path_dic[small_path] = info;
                id_dic[next_id] = small_path;
                next_id++;
            }
            return regist_id;
        }

        public void Save(string filename)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("\t");
            XmlWriter writer = XmlWriter.Create(filename, settings);
            try
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("ProcessNameList");
                int process_num = path_dic.Count - 1;
                writer.WriteElementString("ProcessNum", process_num.ToString());
                writer.WriteElementString("NextID", next_id.ToString());
                
                writer.WriteStartElement("ProcessList");
                foreach (ProcessNameInfo info in path_dic.Values)
                {
                    writer.WriteStartElement("Process");
                    writer.WriteAttributeString("id", "", info.id.ToString());
                    writer.WriteAttributeString("path", "", info.original_path.ToString());
                    writer.WriteAttributeString("name", "", info.name.ToString());
                    writer.WriteAttributeString("total", "", info.total.ToString());
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

        public void Save()
        {
            Save(Plugin.LogDir.PROCESS_FILE);
        }

        public void Load(string filename)
        {
            if (File.Exists(filename))
            {
                string xml = "";
                StreamReader sr = new StreamReader(filename);
                xml = sr.ReadToEnd();
                sr.Dispose();

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);

                // NextIDは現在登録してあるProcessの中の一番大きいID+1とする
                //XmlNode id_node = doc.SelectSingleNode("/ProcessNameList/NextID");
                //next_id = int.Parse(id_node.InnerText);
                next_id = 0;

                XmlNodeList node_list = doc.SelectNodes("/ProcessNameList/ProcessList/Process");
                foreach (XmlNode info_node in node_list)
                {
                    XmlAttributeCollection attrs = info_node.Attributes;
                    ProcessNameInfo info = new ProcessNameInfo();
                    info.id = int.Parse(attrs["id"].Value);
                    info.original_path = attrs["path"].Value;
                    info.path = attrs["path"].Value.ToLower();
                    info.name = attrs["name"].Value;
                    //Debug.WriteLine(info.name);
                    info.total = int.Parse(attrs["total"].Value);
                    
                    id_dic[info.id] = info.path;
                    path_dic[info.path] = info;
                    if (info.id > next_id)
                    {
                        next_id = info.id;
                    }
                }
                next_id += 1;
            }
        }

        public void Load()
        {
            Load(Plugin.LogDir.PROCESS_FILE);    
        }
    }
}
