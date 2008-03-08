using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using Plugin;

namespace CountPerKey
{
    public class CountMain : IStrokePlugin
    {
        private bool valid = true;
        private IPluginController controller;
        private Form main_form;

        private const string LOG_NAME = "count.xml";
        private string log_dir = "";
        private bool form_open = false;
        private CountForm form;

        private DateTime start_date;
        private DateTime last_update;
        private Dictionary<int, int> total_key_count = new Dictionary<int, int>();
        private Dictionary<int, int> today_key_count = new Dictionary<int, int>();

        #region IPluginBase メンバ

        #region プロパティ...
        public IPluginController Controller
        {
            get { return controller; }
            set { controller = value; }
        }

        public System.Windows.Forms.Form MainForm
        {
            get { return main_form; }
            set { main_form = value; }
        }

        public bool Valid
        {
            get { return valid; }
            set { valid = value; }
        }
        public bool FormOpen
        {
            get { return form_open; }
            set { form_open = value; }
        }
        public Dictionary<int, int> TodayKey
        {
            get { return today_key_count; }
        }
        public Dictionary<int, int> TotalKey
        {
            get { return total_key_count; }
        }
        public int TotalDay
        {
            get
            {
                int day = (int)(Math.Ceiling(last_update.Subtract(start_date).TotalDays)) + 1;
                if (day < 1)
                {
                    day = 1;
                }
                return day;
            }
        }
        public DateTime LastUpdate
        {
            get { return last_update; }
        }
        public DateTime StartDate
        {
            get { return start_date; }
        }
        #endregion

        #region プラグイン基本情報...
        public string GetAccessName()
        {
            return "count_per_key";
        }

        public string GetAuthorName()
        {
            return "tomoemon";
        }

        public string GetComment()
        {
            return "キーごとの打鍵数を数えます";
        }

        public object GetInfo()
        {
            return null;
        }

        public string GetPluginName()
        {
            return "キー別打鍵数";
        }

        public string GetVersion()
        {
            return "0.0.1";
        }
#endregion

        public List<System.Windows.Forms.ToolStripMenuItem> GetToolStripMenu()
        {
            List<ToolStripMenuItem> menu_item = new List<ToolStripMenuItem>();
            ToolStripMenuItem item = new ToolStripMenuItem("表示(&C)...");
            item.Click += new EventHandler(item_Click);
            menu_item.Add(item);
            return menu_item;
        }

        public void Close()
        {
            Save();
        }

        public void Init()
        {
            log_dir = Controller.GetSaveDir(GetAccessName());
            Load();
        }

        public bool IsHasConfigForm()
        {
            return true;
        }

        public void KeyDown(IKeyState keystate, uint militime, string app_path, string app_title)
        {
        }

        public void KeyUp(IKeyState keystate, uint militime, string app_path, string app_title)
        {
            DateTime now = DateTime.Now;
            if (now.Day != last_update.Day)
            {
                last_update = now;
                today_key_count.Clear();
                if (FormOpen)
                {
                    form.FormDataLoad();
                }
            }
            Console.WriteLine("TotalDay:{0}", TotalDay);

            if (!total_key_count.ContainsKey(keystate.KeyCode))
            {
                total_key_count[keystate.KeyCode] = 0;
            }
            if (!today_key_count.ContainsKey(keystate.KeyCode))
            {
                today_key_count[keystate.KeyCode] = 0;
            }
            total_key_count[keystate.KeyCode]++;
            today_key_count[keystate.KeyCode]++;

            if (FormOpen)
            {
                form.FormDataUpdate(keystate.KeyCode);
            }
        }

        public void ShowConfigForm()
        {
            if (!FormOpen)
            {
                FormOpen = true;
                form = new CountForm(this);
                form.Location = MainForm.Location;
                form.Show();
            }
        }
        #endregion

        public void Reset()
        {
            start_date = DateTime.Now;
            last_update = DateTime.Now;
            today_key_count.Clear();
            total_key_count.Clear();
        }

        private string GetFileName()
        {
            return Path.Combine(log_dir, LOG_NAME);
        }

        private void item_Click(object sender, EventArgs e)
        {
            ShowConfigForm();
        }

        private void Load()
        {
            string filename = GetFileName();
            last_update = DateTime.Now;
            if (File.Exists(filename))
            {
                string xml = "";
                using (StreamReader sr = new StreamReader(filename))
                {
                    xml = sr.ReadToEnd();
                }

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);

                start_date = DateTime.Parse(doc.SelectSingleNode("/CountKeyLog/StartDate").InnerText);
                last_update = DateTime.Parse(doc.SelectSingleNode("CountKeyLog/LastUpdate").InnerText);

                XmlNode today_node = doc.SelectSingleNode("/CountKeyLog/TodayLog");
                LoadKeyLog(today_node, today_key_count);
                XmlNode total_node = doc.SelectSingleNode("/CountKeyLog/TotalLog");
                LoadKeyLog(total_node, total_key_count);
            }
            else
            {
                start_date = DateTime.Now;
            }
        }

        private void LoadKeyLog(XmlNode parent, Dictionary<int,int> data)
        {
            XmlNodeList node_list = parent.SelectNodes("//Key");
            foreach (XmlNode key_node in node_list)
            {
                XmlAttributeCollection attrs = key_node.Attributes;
                int keycode = int.Parse(attrs["keycode"].Value);
                int stroke = int.Parse(attrs["stroke"].Value);
                data[keycode] = stroke;
            }
        }

        private void Save()
        {
            string filename = GetFileName();

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("\t");
            XmlWriter writer = XmlWriter.Create(filename, settings);
            try
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("CountKeyLog");
                writer.WriteElementString("StartDate", start_date.ToString());
                writer.WriteElementString("LastUpdate", last_update.ToString());

                writer.WriteStartElement("TodayLog");
                foreach(int keycode in today_key_count.Keys)
                {
                    int key_stroke = today_key_count[keycode];
                    writer.WriteStartElement("Key");
                    writer.WriteAttributeString("keycode", "", keycode.ToString());
                    writer.WriteAttributeString("stroke", "", key_stroke.ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                writer.WriteStartElement("TotalLog");
                foreach (int keycode in total_key_count.Keys)
                {
                    int key_stroke = total_key_count[keycode];
                    writer.WriteStartElement("Key");
                    writer.WriteAttributeString("keycode", "", keycode.ToString());
                    writer.WriteAttributeString("stroke", "", key_stroke.ToString());
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
    }
}
