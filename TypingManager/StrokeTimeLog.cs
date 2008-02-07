using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Xml;
using System.IO;

namespace TypingManager
{
    public struct Stroke
    {
        // ���z�L�[
        private int vkey;

        // �X�L�����R�[�h
        private int scan;

        // �L�[�̖��O
        private string key_name;

        // �L�[�������ԁi�ڍ׃��O���J�n���Ă���o�߂����~���b�j
        private int down_time;

        // �L�[���㎞��
        private int up_time;

        #region �v���p�e�B...
        public int VKey
        {
            get { return vkey; }
        }
        public int ScanCode
        {
            get { return scan; }
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

        public Stroke(int vkey, int scan, int down_time, int up_time)
        {
            this.vkey = vkey;
            this.scan = scan;
            this.down_time = down_time;
            this.up_time = up_time;
            this.key_name = VirtualKeyName.GetKeyName(vkey);
        }

        public override string ToString()
        {
            string res = string.Format("{0},{1},{2},{3}", vkey, scan, down_time, up_time);
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
                //Debug.WriteLine(tag_list[tag_list.Count - 1]);
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

    public class StrokeTimeLog
    {
        // �����ꂽ�L�[�Ɖ����ꂽ���Ԃ�ێ����鎫��
        // ���̃L�[���オ�������ɓ��g�ݍ��킹��Stroke�C���X�^���X�����
        private Dictionary<int, int> down_dic = new Dictionary<int, int>();

        // ��̃L�[�ɂ��ĉ����ꂽ���ԂƗ��������Ԃ��y�A�ɂ����C���X�^���X�̃��X�g
        private List<Stroke> stroke_list = new List<Stroke>();

        // �ڍ׃��M���O���J�n���Ă���ŏ��̑Ō����s��ꂽ����
        private int start_time;

        // ���ݏڍ׃��M���O����
        private bool logging;

        private DetailLogInfo info;

        #region �v���p�e�B...
        public bool Logging
        {
            get{return logging;}
        }
        public DetailLogInfo Info
        {
            get { return info; }
        }
        #endregion

        public StrokeTimeLog()
        {
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
        }

        public void LoggingEnd()
        {
            if (logging)
            {
                logging = false;

                // Down���������ɕ��בւ�
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
                    Debug.WriteLine(s.ToString());
                }
                 * */
            }
        }

        public void KeyDown(int vkey, int scan, int query_time)
        {
            if (!Logging) return;

            if (start_time == 0)
            {
                start_time = query_time;
            }

            // �L�[���グ���Ȃ��܂܍Ăщ����ꂽ�ꍇ�͑O��̃L�[�����ɂ��Ă�
            // ���������ԂƓ����ɗ������ƍl����Stroke�C���X�^���X�𐶐�����
            if (down_dic.ContainsKey(scan))
            {
                stroke_list.Add(new Stroke(vkey, scan, down_dic[scan], down_dic[scan]));
            }
            down_dic[scan] = query_time - start_time;
        }

        public void KeyUp(int vkey, int scan, int query_time)
        {
            if (!Logging) return;

            if (down_dic.ContainsKey(scan))
            {
                stroke_list.Add(new Stroke(vkey, scan, down_dic[scan], query_time - start_time));
                down_dic.Remove(scan);
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
                    writer.WriteAttributeString("scan_code", "", st.ScanCode.ToString());
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
            Save(LogDir.DETAIL_XML_FILE(DateTime.Now));
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
                        sw.Write(st.ScanCode); sw.Write(",");
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
                    int scan = int.Parse(key_attrs["scan_code"].Value);
                    int down = int.Parse(key_attrs["down"].Value);
                    int up = int.Parse(key_attrs["up"].Value);
                    stroke_list.Add(new Stroke(vkey, scan, down, up));
                }
            }
        }
    }
}
