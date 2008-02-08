using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using Plugin;

namespace DetailLogPlugin
{
    public class DetailTrigger : IComparable<DetailTrigger>, ICloneable
    {
        private string path = "";
        private string comment = "";
        private TriggerSequence start_seq = new TriggerSequence();
        private TriggerSequence end_seq = new TriggerSequence();

        public DetailTrigger(string _path, string _comment, string start, string end)
        {
            Reset();
            path = _path;
            comment = _comment;
            start_seq.SetSaveFormat(start);
            end_seq.SetSaveFormat(end);
        }

        public DetailTrigger()
        {
            Reset();
        }

        public void Reset()
        {
            Start.Reset();
            End.Reset();
        }

        public bool IsStart(IKeyState state)
        {
            return Start.IsInvoke(state);
        }

        public bool IsEnd(IKeyState state)
        {
            return End.IsInvoke(state);
        }

        #region プロパティ...
        public TriggerSequence Start
        {
            get { return start_seq; }
        }
        public TriggerSequence End
        {
            get { return end_seq; }
        }
        public string Path
        {
            get { return path; }
            set { path = value; }
        }
        public string Comment
        {
            get { return comment; }
            set { comment = value; }
        }
        #endregion

        public int CompareTo(DetailTrigger other)
        {
            if (path == other.Path &&
                start_seq.CompareTo(other.Start) == 0)
            {
                Console.WriteLine("false in DetailTrigger");
                return 0;
            }
            return -1;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public class TriggerController
    {
        public const string TARGET_ALL_PROCESS = "*";
        public const int ALL_PROCESS_ID = -10;

        // <プロセスのフルパス, プロセスごとに登録されたトリガのリスト>
        private Dictionary<string, List<DetailTrigger>> trigger_dic;

        private bool logging = false;
        private string logging_path = "";
        private string last_input_path = "";

        #region プロパティ...
        public bool Logging
        {
            get { return logging; }
            set { logging = value; }
        }
        public string LoggingPath
        {
            get { return logging_path; }
            set { logging_path = value; }
        }
        #endregion

        public TriggerController()
        {
            trigger_dic = new Dictionary<string, List<DetailTrigger>>();
        }

        public void DebugShow()
        {
            foreach (string key in trigger_dic.Keys)
            {
                Console.WriteLine(key);
                foreach (DetailTrigger trigger in trigger_dic[key])
                {
                    Console.WriteLine(" -path:{0}, comment:{1}, start:{2}, end:{3}",
                        trigger.Path, trigger.Comment, trigger.Start.ToString(), trigger.End.ToString());
                }
            }
        }

        public List<DetailTrigger> GetAllTrigers()
        {
            List<DetailTrigger> result = new List<DetailTrigger>();
            foreach (string path in trigger_dic.Keys)
            {
                result.AddRange(trigger_dic[path]);
            }
            return result;
        }

        public bool Add(DetailTrigger new_trigger)
        {
            string app_path = new_trigger.Path;
            if (!trigger_dic.ContainsKey(app_path))
            {
                trigger_dic[app_path] = new List<DetailTrigger>();
            }
            for (int i = 0; i < trigger_dic[app_path].Count; i++)
            {
                if (trigger_dic[app_path][i].CompareTo(new_trigger) == 0)
                {
                    return false;
                }
            }
            trigger_dic[app_path].Add(new_trigger);
            DebugShow();
            return true;
        }

        public bool Remove(DetailTrigger remove_trigger)
        {
            string app_path = remove_trigger.Path;
            if (trigger_dic.ContainsKey(app_path))
            {
                int remove_index = -1;
                for (int i = 0; i < trigger_dic[app_path].Count; i++)
                {
                    if (trigger_dic[app_path][i] == remove_trigger)
                    {
                        remove_index = i;
                    }
                }
                if (remove_index != -1)
                {
                    trigger_dic[app_path].RemoveAt(remove_index);
                    DebugShow();
                    return true;
                }
            }
            return false;
        }

        public DetailTrigger IsStart(string app_path, IKeyState key_state)
        {
            if (app_path != last_input_path)
            {
                AllReset();
            }

            if (trigger_dic.ContainsKey(TARGET_ALL_PROCESS))
            {
                for (int i = 0; i < trigger_dic[TARGET_ALL_PROCESS].Count; i++)
                {
                    DetailTrigger trigger = trigger_dic[TARGET_ALL_PROCESS][i];
                    if (trigger.IsStart(key_state))
                    {
                        logging_path = TARGET_ALL_PROCESS;
                        return trigger;
                    }
                }
            }
            if (trigger_dic.ContainsKey(app_path))
            {
                Console.WriteLine("{0} トリガーチェック", app_path);
                for (int i = 0; i < trigger_dic[app_path].Count; i++)
                {
                    DetailTrigger trigger = trigger_dic[app_path][i];
                    if (trigger.IsStart(key_state))
                    {
                        logging_path = app_path;
                        return trigger;
                    }
                }
            }
            last_input_path = app_path;
            return null;
        }

        public DetailTrigger IsEnd(string app_path, IKeyState key_state)
        {
            if (LoggingPath == TriggerController.TARGET_ALL_PROCESS)
            {
                app_path = TARGET_ALL_PROCESS;
            }
            else
            {
                if (app_path != LoggingPath)
                {
                    AllReset();
                    return null;
                }
                else if (app_path != last_input_path)
                {
                    AllReset();
                }
            }
            if (trigger_dic.ContainsKey(app_path))
            {
                for (int i = 0; i < trigger_dic[app_path].Count; i++)
                {
                    DetailTrigger trigger = trigger_dic[app_path][i];
                    if (trigger.IsEnd(key_state))
                    {
                        logging_path = "";
                        return trigger;
                    }
                }
            }
            last_input_path = app_path;
            return null;
        }

        private void AllReset()
        {
            foreach (string app_path in trigger_dic.Keys)
            {
                foreach (DetailTrigger trigger in trigger_dic[app_path])
                {
                    trigger.Reset();
                }
            }
        }

        public void Save()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("\t");
            XmlWriter writer = XmlWriter.Create(StrokeTimeLog.TRIGGER_FILE, settings);
            try
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("DetailLogTrigger");
                foreach (string path in trigger_dic.Keys)
                {
                    writer.WriteStartElement("Process");
                    writer.WriteAttributeString("path", "", path);
                    foreach (DetailTrigger trigger in trigger_dic[path])
                    {
                        writer.WriteStartElement("Trigger");
                        writer.WriteAttributeString("comment", "", trigger.Comment);
                        writer.WriteAttributeString("start", "", trigger.Start.GetSaveFormat());
                        writer.WriteAttributeString("end", "", trigger.End.GetSaveFormat());
                        writer.WriteAttributeString("start_text", "", trigger.Start.GetViewFormat());
                        writer.WriteAttributeString("end_text", "", trigger.End.GetViewFormat());
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            finally
            {
                writer.Close();
            }
        }

        public void Load()
        {
            if (File.Exists(StrokeTimeLog.TRIGGER_FILE))
            {
                string xml = "";
                using (StreamReader sr = new StreamReader(StrokeTimeLog.TRIGGER_FILE))
                {
                    xml = sr.ReadToEnd();
                }

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);

                XmlNodeList node_list = doc.SelectNodes("//Trigger");
                foreach (XmlNode trigger_node in node_list)
                {
                    XmlNode process_node = trigger_node.ParentNode;
                    XmlAttributeCollection process_attrs = process_node.Attributes;
                    string process_path = process_attrs["path"].Value;

                    XmlAttributeCollection trigger_attrs = trigger_node.Attributes;
                    string comment = trigger_attrs["comment"].Value;
                    string start = trigger_attrs["start"].Value;
                    string end = trigger_attrs["end"].Value;
                    DetailTrigger trigger = new DetailTrigger(process_path, comment, start, end);
                    Add(trigger);
                }
            }
        }
    }
}
