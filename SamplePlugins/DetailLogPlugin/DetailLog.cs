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
    public class Stroke
    {
        // ���z�L�[
        private int vkey;

        // �L�[�̖��O
        private string key_name;

        // Alt��Ctrl,Shift��������Ă�����
        // Alt�̂݉�����Ă���Ƃ��F"A"
        // Alt��Ctrl��������Ă���Ƃ��F"AC"
        // Alt,Ctrl,Shift��������Ă���Ƃ��F"ACS"
        private string modify = "";

        // �L�[�������ԁi�ڍ׃��O���J�n���Ă���o�߂����~���b�j
        private uint down_time;

        // �L�[���㎞��
        private uint up_time;

        #region �v���p�e�B...
        public int VKey
        {
            get { return vkey; }
        }
        public string KeyModify
        {
            get { return modify; }
            set { modify = value; }
        }
        public uint DownTime
        {
            get { return down_time; }
        }
        public uint UpTime
        {
            get { return up_time; }
        }
        public string KeyName
        {
            get { return key_name; }
        }
        #endregion

        public Stroke(IKeyState key, uint down_time, uint up_time)
        {
            this.vkey = key.KeyCode;
            this.down_time = down_time;
            this.up_time = up_time;
            this.key_name = VirtualKeyName.GetKeyName(this.vkey);
            if (key.IsAlt)
            {
                modify = "A";
            }
            if (key.IsControl)
            {
                modify += "C";
            }
            if (key.IsShift)
            {
                modify += "S";
            }
        }

        public Stroke(int vkey, uint down_time, uint up_time, string modify)
        {
            this.vkey = vkey;
            this.down_time = down_time;
            this.up_time = up_time;
            this.modify = modify;
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

        // ���̃��M���O�ɕt�����R�����g�i�^�O�������j
        public string Comment;

        // ���̃��M���O�ɂ����^�O
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

    public class DetailLog : Plugin.BaseStrokePlugin
    {
        public const string PLUGIN_NAME = "detail_log";
        public static string PLUGIN_LOG_DIR = "";
        public static string PLUGIN_CONFIG_DIR = "";
        public static string DETAIL_XML_FILE(DateTime date)
            { return Path.Combine(PLUGIN_LOG_DIR, date.ToString("yyyyMMdd_HHmmss") + ".xml"); }
        public static string TRIGGER_FILE
        {
            get { return Path.Combine(PLUGIN_CONFIG_DIR, "trigger.xml"); }
        }
        public static string COMMENT_FILE
        {
            get { return Path.Combine(PLUGIN_CONFIG_DIR, "comment.txt"); }
        }
        public static string CSV_DIR
        {
            get { return Path.Combine(PLUGIN_LOG_DIR, "csv"); }
        }

        // <�����ꂽ�L�[,�����ꂽ����>
        // ���̃L�[���オ�������ɓ��g�ݍ��킹��Stroke�C���X�^���X�����
        private Dictionary<int, uint> down_dic = new Dictionary<int, uint>();

        // ��̃L�[�ɂ��ĉ����ꂽ���ԂƗ��������Ԃ��y�A�ɂ����C���X�^���X�̃��X�g
        private List<Stroke> stroke_list = new List<Stroke>();

        // �ڍ׃��M���O���J�n���Ă���ŏ��̑Ō����s��ꂽ����
        private uint start_time;

        private DetailLogInfo info;

        private DetailLogForm form;
        
        // ���݃t�H�[�����J���Ă��邩
        private bool form_open;

        // ����܂łɓ��͂����R�����g�̃��X�g
        private List<string> comment_history = new List<string>();

        // �ڍ׃��O�̃g���K�R���g���[��
        private TriggerController trigger_controller = new TriggerController();

        // �ڍ׃��O�擾�ΏۂƂ���v���Z�X�����擾���邽��
        private IProcessNameData process_name;

        #region �v���p�e�B...
        public bool Logging
        {
            get { return TriggerCtrl.Logging; }
            set { TriggerCtrl.Logging = value; }
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
        public TriggerController TriggerCtrl
        {
            get { return trigger_controller; }
        }
        public IProcessNameData ProcessName
        {
            get { return process_name; }
        }
        #endregion

        #region BaseStrokePlugin�̎����㏑��
        /// <summary>�v���O�C���̖��O��Ԃ�����</summary>
        public override string GetAccessName() { return PLUGIN_NAME; }

        public override string GetPluginName() { return "�ڍ׃��O�擾"; }

        /// <summary>�v���O�C���Ɋւ���ȒP�Ȑ�������������</summary>
        public override string GetComment() { return "��Ō����Ƃ̏ڍׂȃ��O���L�^���܂�"; }

        /// <summary>�v���O�C����҂̖��O��Ԃ�����</summary>
        public override string GetAuthorName() { return "tomoemon"; }

        /// <summary>�v���O�C���̃o�[�W��������������</summary>
        public override string GetVersion() { return "0.0.1"; }

        public override void KeyDown(IKeyState key_state, uint militime, string app_path, string app_title)
        {
            if (FormOpen)
            {
                form.KeyStrokeDown(key_state, militime);
            }
            app_path = app_path.ToLower();
            //Console.WriteLine("StrokeTimeLog.KeyDown: {0}", app_path);
            if (!Logging)
            {
               // Console.WriteLine("Trigger IsStart");
                DetailTrigger trigger = TriggerCtrl.IsStart(app_path, key_state);
                if (trigger != null)
                {
                    // ���̑Ō����烍�O��L���ɂ��邽�ߍ���true�ɂ��邾����return
                    LoggingStart(trigger.Path, trigger.Comment);
                }
                return;
            }
            else
            {
                if (TriggerCtrl.LoggingPath == TriggerController.TARGET_ALL_PROCESS)
                {
                    app_path = TriggerController.TARGET_ALL_PROCESS;
                }
                else if (app_path != TriggerCtrl.LoggingPath)
                {
                    // �Ώۃv���Z�X�𖾎����Ă���ꍇ�͕ʂ̃v���Z�X��Ώۂɂ����Ō���
                    // �L�^���Ȃ�
                    return;
                }
                if (TriggerCtrl.IsEnd(app_path, key_state) != null)
                {
                    LoggingEnd();
                    return;
                }
            }

            if (start_time == 0)
            {
                start_time = militime;
            }

            // �L�[���グ���Ȃ��܂܍Ăщ����ꂽ�ꍇ�͑O��̃L�[�����ɂ��Ă�
            // ���������ԂƓ����ɗ������ƍl����Stroke�C���X�^���X�𐶐�����
            if (down_dic.ContainsKey(key_state.KeyCode))
            {
                stroke_list.Add(
                    new Stroke(key_state, down_dic[key_state.KeyCode],
                        down_dic[key_state.KeyCode]));
            }
            //Console.WriteLine("DetailLog keydown:{0}", key_state.KeyCode);
            down_dic[key_state.KeyCode] = (uint)(militime - start_time);
        }

        public override void KeyUp(IKeyState key_state, uint militime, string app_path, string app_title)
        {
            if (FormOpen)
            {
                form.KeyStrokeUp(key_state, militime);
            }
            if (!Logging) return;

            if (down_dic.ContainsKey(key_state.KeyCode))
            {
                stroke_list.Add(
                    new Stroke(key_state, down_dic[key_state.KeyCode],
                    militime - start_time));
                down_dic.Remove(key_state.KeyCode);
            }
        }

        public override void Init()
        {
            PLUGIN_LOG_DIR = Controller.GetSaveDir(GetAccessName());
            PLUGIN_CONFIG_DIR = Controller.GetConfigDir(GetAccessName());
            LogDirectoryCheck();
            process_name = (IProcessNameData)Controller.GetInfo("process_name");
            TriggerCtrl.Load();
            LoadCommentHistory();
        }

        public override void Close()
        {
            LoggingEnd();
            TriggerCtrl.Save();
            SaveCommentHistory();
        }

        public override List<ToolStripMenuItem> GetToolStripMenu()
        {
            List<ToolStripMenuItem> menu_item = new List<ToolStripMenuItem>();
            ToolStripMenuItem item = new ToolStripMenuItem("�ݒ�(&C)...");
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
                //Console.WriteLine("x={0}, y={1}", MainForm.Location.X, MainForm.Location.Y);
                form.Location = MainForm.Location;
                form.Show();
            }
        }

        void item_Click(object sender, EventArgs e)
        {
            ShowConfigForm();
        }
        #endregion

        public DetailLog()
        {
            form_open = false;
            start_time = 0;
            info = new DetailLogInfo();
        }

        /// <summary>
        /// ���̃v���O�C���Ŏg���f�B���N�g�����쐬����
        /// �v���O�C���p�̃f�B���N�g���̓��C���A�v���P�[�V�����ō쐬�ς�
        /// </summary>
        private void LogDirectoryCheck()
        {
            if (!Directory.Exists(CSV_DIR))
            {
                Directory.CreateDirectory(CSV_DIR);
            }
        }

        public void LoggingStart(string path, string comment)
        {
            TriggerCtrl.LoggingPath = path;
            Logging = true;
            start_time = 0;
            stroke_list.Clear();
            info.Reset();
            info.Date = DateTime.Now;
            info.SetComment(comment);

            if (comment_history.IndexOf(comment) == -1)
            {
                comment_history.Add(comment);
                if (comment_history.Count > DetailLogForm.MAX_COMBOBOX_HISTORY)
                {
                    comment_history.RemoveAt(0);
                }
            }
            
            // �t�H�[�����\�����Ă���ꍇ�͊J�n�{�^���𖳌��ɂ���
            if (FormOpen)
            {
                form.Text = DetailLogForm.WINDOW_TITLE + string.Format(" �v����:[{0}]", path);
                form.StartButton.Enabled = false;
                form.EndButon.Enabled = true;
            }
        }

        public void LoggingEnd()
        {
            if (Logging)
            {
                Logging = false;

                // Down���������ɕ��בւ�
                stroke_list.Sort(
                    delegate(Stroke s1, Stroke s2)
                    {
                        return (int)(s1.DownTime - s2.DownTime);
                    }
                );
                Save();
                /*
                foreach (Stroke s in stroke_list)
                {
                    Console.WriteLine(s.ToString());
                }
                 * */
                // �t�H�[�����\�����Ă���ꍇ�͊J�n�{�^����L���ɂ���
                if (FormOpen)
                {
                    form.Text = DetailLogForm.WINDOW_TITLE;
                    form.StartButton.Enabled = true;
                    form.EndButon.Enabled = false;
                }
            }
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
                    writer.WriteAttributeString("modify", "", st.KeyModify);
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
            Console.WriteLine(DETAIL_XML_FILE(DateTime.Now));
            Save(DETAIL_XML_FILE(DateTime.Now));
        }

        /// <summary>
        /// �ڍ׃��O�ɂ����R�����g�̗�����ۑ�����
        /// </summary>
        private void SaveCommentHistory()
        {
            using (StreamWriter sw = new StreamWriter(COMMENT_FILE))
            {
                //Console.WriteLine("Comment Save");
                foreach (string text in comment_history)
                {
                    Console.WriteLine(text);
                    sw.WriteLine(text);
                }
            }
        }

        /// <summary>
        /// �ڍ׃��O�ɂ���R�����g�̗����t�@�C����ǂݍ���
        /// </summary>
        private void LoadCommentHistory()
        {
            if (File.Exists(COMMENT_FILE))
            {
                using (StreamReader sr = new StreamReader(COMMENT_FILE))
                {
                    //Console.WriteLine("Comment Load");
                    string line;
                    while ((line = sr.ReadLine()) != null) // 1�s���ǂݏo���B
                    {
                        //Console.WriteLine(line);
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
                        sw.Write(st.KeyModify); sw.Write(",");
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
                    uint down = uint.Parse(key_attrs["down"].Value);
                    uint up = uint.Parse(key_attrs["up"].Value);
                    string modify = "";
                    XmlAttribute modify_attr = key_attrs["modify"];
                    if (modify_attr != null)
                    {
                        modify = modify_attr.Value;
                    }
                    stroke_list.Add(new Stroke(vkey, down, up, modify));
                }
            }
        }
    }
}
