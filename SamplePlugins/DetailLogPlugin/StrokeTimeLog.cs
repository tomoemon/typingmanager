using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using Plugin;

namespace DetailLogPlugin
{
    public struct Stroke
    {
        // 仮想キー
        private int vkey;

        // キーの名前
        private string key_name;

        // キー押下時間（詳細ログを開始してから経過したミリ秒）
        private int down_time;

        // キー押上時間
        private int up_time;

        #region プロパティ...
        public int VKey
        {
            get { return vkey; }
        }
        public int DownTime
        {
            get { return down_time; }
        }
        public int UpTime
        {
            get { return up_time; }
        }
        public string KeyName
        {
            get { return key_name; }
        }
        #endregion

        public Stroke(int vkey, int down_time, int up_time)
        {
            this.vkey = vkey;
            this.down_time = down_time;
            this.up_time = up_time;
            this.key_name = VirtualKeyName.GetKeyName(vkey);
        }

        public override string ToString()
        {
            string res = string.Format("{0},{1},{2},{3}", vkey, down_time, up_time);
            return res;
        }
    }

    public class DetailLogInfo
    {
        public string FileName;

        // 今のロギングに付けたコメント（タグを除く）
        public string Comment;

        // 今のロギングにつけたタグ
        public List<string> Tag;

        public DateTime Date;

        public DetailLogInfo()
        {
            Tag = new List<string>();
        }

        public void Reset()
        {
            Tag.Clear();
        }

        public void SetComment(string comment)
        {
            Regex r = new Regex(@"\[([^]]+)\]");
            MatchCollection matches = r.Matches(comment);
            foreach (Match m in matches)
            {
                Tag.Add(m.Groups[1].Value);
                //Console.WriteLine(tag_list[tag_list.Count - 1]);
            }
            Comment = Regex.Replace(comment, @"\[[^]]*\]", "").Trim();
        }

        public string TagConcat(string separator, bool bracket)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Tag.Count; i++)
            {
                if (i != 0)
                {
                    sb.Append(separator);
                }
                if (bracket) sb.Append("[");
                sb.Append(Tag[i]);
                if (bracket) sb.Append("]");
            }
            return sb.ToString();
        }

        public string TagConcat(string separator)
        {
            return TagConcat(separator, false);
        }
    }

    public class StrokeTimeLog : Plugin.BaseStrokePlugin
    {
        // 押されたキーと押された時間を保持する辞書
        // そのキーが上がった時に二つを組み合わせてStrokeインスタンスを作る
        private Dictionary<int, int> down_dic = new Dictionary<int, int>();

        // 一つのキーについて押された時間と離した時間をペアにしたインスタンスのリスト
        private List<Stroke> stroke_list = new List<Stroke>();

        // 詳細ロギングを開始してから最初の打鍵が行われた時間
        private int start_time;

        // 現在詳細ロギング中か
        private bool logging;

        private DetailLogInfo info;


        private DetailLogForm form;
        
        // 現在フォームを開いているか
        private bool form_open;

        private List<string> comment_history = new List<string>();


        #region プロパティ...
        public bool Logging
        {
            get{return logging;}
        }
        public DetailLogInfo Info
        {
            get { return info; }
        }
        public bool FormOpen
        {
            get { return form_open; }
            set { form_open = value; }
        }
        public List<string> Comment
        {
            get { return comment_history; }
        }
        #endregion

        #region BaseStrokePluginの実装上書き
        /// <summary>プラグインの名前を返すこと</summary>
        public override string GetAccessName() { return "detail_log"; }

        public override string GetPluginName() { return "詳細ログ取得"; }

        /// <summary>プラグインに関する簡単な説明を書くこと</summary>
        public override string GetComment() { return "一打鍵ごとの詳細なログを記録します"; }

        /// <summary>プラグイン作者の名前を返すこと</summary>
        public override string GetAuthorName() { return "tomoemon"; }

        /// <summary>プラグインのバージョンを書くこと</summary>
        public override string GetVersion() { return "0.0.1"; }

        public override void  KeyDown(int keycode, int militime, string app_path, string app_title)
        {
            if (!Logging) return;

            if (start_time == 0)
            {
                start_time = militime;
            }

            // キーが上げられないまま再び押された場合は前回のキー押下については
            // 押した時間と同時に離したと考えてStrokeインスタンスを生成する
            if (down_dic.ContainsKey(keycode))
            {
                stroke_list.Add(new Stroke(keycode, down_dic[keycode], down_dic[keycode]));
            }
            down_dic[keycode] = militime - start_time;
        }

        public override void KeyUp(int keycode, int militime, string app_path, string app_title)
        {
            if (!Logging) return;

            if (down_dic.ContainsKey(keycode))
            {
                stroke_list.Add(new Stroke(keycode, down_dic[keycode], militime - start_time));
                down_dic.Remove(keycode);
            }
        }

        public override void Close()
        {
            LoggingEnd();
            Save();
            SaveCommentHistory();
        }

        public override List<ToolStripMenuItem> GetToolStripMenu()
        {
            List<ToolStripMenuItem> menu_item = new List<ToolStripMenuItem>();
            ToolStripMenuItem item = new ToolStripMenuItem("設定(&C)...");
            item.Click += new EventHandler(item_Click);
            menu_item.Add(item);
            return menu_item;
        }

        public override bool IsHasConfigForm()
        {
            return true;
        }

        public override void ShowConfigForm()
        {
            if (!FormOpen)
            {
                FormOpen = true;
                form = new DetailLogForm(this);
                Console.WriteLine("x={0}, y={1}", MainForm.Location.X, MainForm.Location.Y);
                form.Location = MainForm.Location;
                form.Show();
            }
        }

        void item_Click(object sender, EventArgs e)
        {
            ShowConfigForm();
        }
        #endregion

        public StrokeTimeLog()
        {
            form_open = false;
            logging = false;
            start_time = 0;
            info = new DetailLogInfo();
        }

        public void LoggingStart(string comment)
        {
            logging = true;
            start_time = 0;
            stroke_list.Clear();
            info.Reset();
            info.Date = DateTime.Now;
            info.SetComment(comment);
            comment_history.Add(comment);
        }

        public void LoggingEnd()
        {
            if (logging)
            {
                logging = false;

                // Downが早い順に並べ替え
                stroke_list.Sort(
                    delegate(Stroke s1, Stroke s2)
                    {
                        return s1.DownTime - s2.DownTime;
                    }
                );
                Save();
                /*
                foreach (Stroke s in stroke_list)
                {
                    Console.WriteLine(s.ToString());
                }
                 * */
            }
        }

        public bool Save(string filename)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("\t");
            XmlWriter writer = XmlWriter.Create(filename, settings);
            try
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("DetailLog");
                writer.WriteElementString("Tag", info.TagConcat(","));
                writer.WriteElementString("Comment", info.Comment);
                writer.WriteElementString("Date", info.Date.ToString());
                writer.WriteStartElement("StrokeList");
                foreach (Stroke st in stroke_list)
                {
                    writer.WriteStartElement("Key");
                    writer.WriteAttributeString("key_name", "", st.KeyName);
                    writer.WriteAttributeString("vkey_code", "", st.VKey.ToString());
                    writer.WriteAttributeString("down", "", st.DownTime.ToString());
                    writer.WriteAttributeString("up", "", st.UpTime.ToString());
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
            return true;
        }

        public void Save()
        {
            string filename = Controller.GetSaveDir(this.GetPluginName());
            Save(LogDir.DETAIL_XML_FILE(DateTime.Now));
        }

        /// <summary>
        /// 詳細ログにつけたコメントの履歴を保存する
        /// </summary>
        private void SaveCommentHistory()
        {
            using (StreamWriter sw = new StreamWriter(Plugin.LogDir.COMMENT_FILE))
            {
                foreach (string text in comment_history)
                {
                    sw.WriteLine(text);
                }
            }
        }

        /// <summary>
        /// 詳細ログにつけるコメントの履歴ファイルを読み込む
        /// </summary>
        private void LoadCommentHistory()
        {
            if (File.Exists(LogDir.COMMENT_FILE))
            {
                using (StreamReader sr = new StreamReader(Plugin.LogDir.COMMENT_FILE))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null) // 1行ずつ読み出し。
                    {
                        comment_history.Add(line);
                    }
                }
            }
        }

        public bool SaveCSV(string filename)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(filename))
                {
                    foreach (Stroke st in stroke_list)
                    {
                        sw.Write(st.KeyName); sw.Write(",");
                        sw.Write(st.VKey); sw.Write(",");
                        sw.Write(st.DownTime); sw.Write(",");
                        sw.Write(st.UpTime); sw.Write("\n");
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public void Load(string filename)
        {
            if (File.Exists(filename))
            {
                stroke_list.Clear();
                string xml = "";
                using (StreamReader sr = new StreamReader(filename))
                {
                    xml = sr.ReadToEnd();
                }

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);

                XmlNode tag_node = doc.SelectSingleNode("/DetailLog/Tag");
                info.Tag.AddRange(tag_node.InnerText.Split(new char[] { ',' }));
                XmlNode comment_node = doc.SelectSingleNode("/DetailLog/Comment");
                info.Comment = comment_node.InnerText;
                XmlNode date_node = doc.SelectSingleNode("/DetailLog/Date");
                info.Date = DateTime.Parse(date_node.InnerText);

                XmlNodeList node_list = doc.SelectNodes("/DetailLog/StrokeList/Key");
                foreach (XmlNode key_node in node_list)
                {
                    XmlAttributeCollection key_attrs = key_node.Attributes;
                    int vkey = int.Parse(key_attrs["vkey_code"].Value);
                    int down = int.Parse(key_attrs["down"].Value);
                    int up = int.Parse(key_attrs["up"].Value);
                    stroke_list.Add(new Stroke(vkey, down, up));
                }
            }
        }
    }
}
